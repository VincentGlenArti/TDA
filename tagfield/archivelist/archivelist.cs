using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parameters
{
    public static partial class archivelist_parameters
    {
        public static string archive_extension = ".arc";
        public static string archive_meta_extension = ".meta";
    }
}

namespace TDA
{

    public delegate System.IO.Compression.ZipArchive ReopenParrent();
    public delegate System.IO.Stream GetWriteStream();

    public class ArchiveList
    {

        private UInt64 nextArchiveID;

        public struct ArchiveListNode
        {
            public UInt64 ID;
            public Archive file;
        }

        private LinkedList<ArchiveListNode> archives;

        public LinkedList<QuerryMatch> SearchForPictures(LinkedList<Querry> Search, System.IO.Compression.ZipArchive parrent)
        {
            LinkedList<QuerryMatch> Target = new LinkedList<QuerryMatch>();
            foreach (ArchiveListNode Archive in this.archives)
            {
                if (!Archive.file.initialized) Archive.file.Initialize(parrent.GetEntry(Archive.ID.ToString() + parameters.archivelist_parameters.archive_extension).Open());
                Target.Concat<QuerryMatch>(Archive.file.searchThisArchive(Search));
                Archive.file.Uninitialize();
            }
            return Target;
        }

        public int getArchiveNumber()
        {
            return (archives.Count);
        }

        public ArchiveList()
        {
            archives = new LinkedList<ArchiveListNode>();
        }

        public UInt64 returnNextID()
        {
            return (nextArchiveID);
        }

        private int addArchive(string newfile, System.IO.Compression.ZipArchive parrent, ReopenParrent reopener, ReopenSelf reopenerArchive, GetMetafile writer)
        {
            if (nextArchiveID == UInt64.MaxValue) return (1);
            ArchiveListNode inserter = new ArchiveListNode();
            inserter.ID = nextArchiveID;
            inserter.file = new Archive();
            System.IO.Compression.ZipArchive ArchiveCreater = System.IO.Compression.ZipFile.Open
                (inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension,
                System.IO.Compression.ZipArchiveMode.Create);
            ArchiveCreater.Dispose();
            parrent.CreateEntry(inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension);
            parrent.CreateEntry(inserter.ID.ToString() + parameters.archivelist_parameters.archive_meta_extension);
            parrent = reopener();
            System.IO.StreamWriter ArchiveWriter = new System.IO.StreamWriter(parrent.GetEntry(inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension).Open());
            System.IO.StreamReader ArchiveReader = new System.IO.StreamReader(inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension);
            ArchiveWriter.Write(ArchiveReader.ReadToEnd());
            ArchiveReader.Dispose();
            ArchiveWriter.Flush();
            ArchiveWriter.Dispose();
            System.IO.File.Delete(inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension);
            parrent = reopener();
            inserter.file.Initialize(parrent.GetEntry(inserter.ID.ToString() + parameters.archivelist_parameters.archive_extension).Open(),
                                                parrent.GetEntry(inserter.ID.ToString() + parameters.archivelist_parameters.archive_meta_extension).Open());
            inserter.file.addFile(newfile, reopenerArchive, inserter.ID, writer);
            inserter.file.Uninitialize();
            archives.AddLast(inserter);
            nextArchiveID++;
            return (0);
        }

        private int deleteArchive(UInt64 targetID)
        {
            bool deleted = false;
            LinkedListNode<ArchiveListNode> node = archives.First;
            for (int i = 0; i < archives.Count; i++)
            {
                if (node.Value.ID == targetID)
                {
                    archives.Remove(node);
                    deleted = true;
                    break;
                }
                node = node.Next;
            }
            if (deleted) return (0);
            return (1);
        }

        public int TagFile(UInt64 ArchiveID, UInt64 PicID, UInt64 TagID, GetMetafile MetaWriter)
        {
            bool success = false;
            foreach(ArchiveListNode node in archives)
            {
                if(node.ID == ArchiveID)
                {
                    int returner = node.file.addTagToPic(PicID, TagID, MetaWriter(node.ID));
                    if (returner == 0)
                    {
                        success = true;
                        break;
                    }
                    else return (returner + 1);
                }
            }
            if (success) return (0);
            else return (1);
        }

        public int addFileToArchive(Int64 filesize, string newfile, System.IO.Compression.ZipArchive parrent, ReopenParrent reopener, ReopenSelf reopenerArchive, GetWriteStream writer, GetMetafile metawriter)
        {
            bool inserted = false;
            int returner;
            foreach(ArchiveListNode node in archives)
            {
                if (!node.file.initialized)
                {
                    node.file.Initialize(parrent.GetEntry(node.ID.ToString() + parameters.archivelist_parameters.archive_extension).Open());
                }
                if ((node.file.GetWeightMB() + filesize) <= parameters.archive_parameters.seedSizeMB)
                {
                    returner = node.file.addFile(newfile, reopenerArchive, node.ID, metawriter);
                    if (returner == 0)
                    {
                        parameters.last_added_file.archiveID = node.ID;
                        inserted = true;
                        node.file.Uninitialize();
                        break;
                    }
                }
                node.file.Uninitialize();
            }
            if (!inserted)
            {
                this.addArchive(newfile, parrent, reopener, reopenerArchive, metawriter);
            }
            this.writeToFile(writer());
            return (0);
        }

        public int initializeArchives(System.IO.Compression.ZipArchive parrent)
        {
            foreach(ArchiveListNode node in archives)
            {
                if (!node.file.PicInfoReady)
                {
                    node.file.Initialize(parrent.GetEntry(node.ID.ToString() + parameters.archivelist_parameters.archive_extension).Open(),
                                            parrent.GetEntry(node.ID.ToString() + parameters.archivelist_parameters.archive_meta_extension).Open());
                    node.file.Uninitialize();
                }
            }
            return (0);
        }

        public int readFromFile(System.IO.Stream str)
        {
            System.IO.StreamReader strr = new System.IO.StreamReader(str);
            string line = strr.ReadLine();
            if (line == null) return (1);
            nextArchiveID = UInt64.Parse(line);
            ArchiveListNode inserter;
            while ((line = strr.ReadLine()) != null)
            {
                inserter = new ArchiveListNode();
                inserter.ID = UInt64.Parse(line);
                inserter.file = new Archive();
                archives.AddLast(inserter);
            }
            strr.Dispose();
            return (0);
        }

        public int writeToFile(System.IO.Stream str)
        {
            System.IO.StreamWriter strw = new System.IO.StreamWriter(str);
            strw.WriteLine(nextArchiveID);
            foreach(ArchiveListNode node in archives)
            {
                strw.WriteLine(node.ID.ToString());
            }
            strw.Dispose();
            return (0);
        }

    }
}
