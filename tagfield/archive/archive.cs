using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parameters
{

    public static class last_added_file
    {
        public static System.Drawing.Bitmap file;
        public static UInt64 archiveID;
        public static UInt64 picID;
        public static string filename;
        public static bool ready;
    }

    public static class archive_parameters
    {
        public static Int64 seedSizeMB = 126;
    }
}

namespace TDA
{

    public enum Option { include, exclude };

    public struct Querry
    {
        public UInt64 tag;
        public Option tagOption;
    }
    
    public struct QuerryMatch
    {
        public UInt64 PicID;
        public UInt64 ArchiveID;
        public LinkedList<UInt64> tags;
    }

    public delegate System.IO.Compression.ZipArchive ReopenSelf(UInt64 ArchiveID);
    public delegate System.IO.Stream GetMetafile(UInt64 ArchiveID);

    public class Archive
    {

        public struct PicInfo
        {
            public UInt64 ID;
            public string fileName;
            public LinkedList<UInt64> tags;
        }

        public bool initialized;
        public bool PicInfoReady;
        private UInt64 nextPicID;
        private System.IO.Compression.ZipArchive file;
        private LinkedList<PicInfo> Content;

        public Archive()
        {
            PicInfoReady = false;
            initialized = false;
        }

        public int Initialize(System.IO.Stream archiveFile, System.IO.Stream metaFile)
        {
            this.initialized = true;
            this.file = new System.IO.Compression.ZipArchive(archiveFile, System.IO.Compression.ZipArchiveMode.Update);
            if (!this.PicInfoReady)
            {
                this.Content = new LinkedList<PicInfo>();
                this.readMetaFromFile(metaFile);
                this.PicInfoReady = true;
            }
            metaFile.Close();
            return (0);
        }

        public int Initialize(System.IO.Stream archiveFile)
        {
            this.initialized = true;
            if (!this.PicInfoReady) return (1);
            this.file = new System.IO.Compression.ZipArchive(archiveFile, System.IO.Compression.ZipArchiveMode.Update);
            return (0);
        }

        public int Uninitialize()
        {
            this.initialized = false;
            this.file.Dispose();
            return (0);
        }

        public static LinkedList<UInt64> GetTagInfoClone(LinkedList<UInt64> Origin)
        {
            return (new LinkedList<UInt64>(Origin));
        }

        public Int64 GetWeightMB()
        {
            if (!initialized) return (0);
            Int64 Target = 0;
            if (file.Entries.Count == 0) return (0);
            foreach (System.IO.Compression.ZipArchiveEntry entry in file.Entries)
            {
                Target = Target + entry.CompressedLength;
            }
            Target = (Target / (1024 * 1024));
            return (Target);
        }

        public int addFile(string filepath, ReopenSelf reopener, UInt64 MyID, GetMetafile writer)
        {
            if ((!this.initialized) || (!this.PicInfoReady)) return (2);
            if (nextPicID == UInt64.MaxValue) return (1);
            file.CreateEntry(nextPicID.ToString());
            this.file.Dispose();
            this.file = reopener(MyID);
            System.IO.Stream strw = file.GetEntry(nextPicID.ToString()).Open();
            System.IO.FileInfo FileReader = new System.IO.FileInfo(filepath);
            System.IO.FileStream strr = FileReader.OpenRead();
            strr.CopyTo(strw);
            strw.Flush();
            this.file.Dispose();
            this.file = reopener(MyID);
            PicInfo inserter = new PicInfo();
            inserter.tags = new LinkedList<UInt64>();
            inserter.ID = nextPicID;
            string name_of_file = filepath.Substring(filepath.LastIndexOf('\\') + 1);
            inserter.fileName = name_of_file.Replace(' ', '_');
            Content.AddLast(inserter);
            parameters.last_added_file.picID = nextPicID;
            System.IO.FileStream last_added_image = FileReader.OpenRead();
            parameters.last_added_file.file = new System.Drawing.Bitmap(last_added_image);
            last_added_image.Close();
            parameters.last_added_file.filename = inserter.fileName;
            parameters.last_added_file.ready = true;
            System.IO.Stream WriteMetaStream = writer(MyID);
            nextPicID++;
            this.writeMetaToFile(WriteMetaStream);
            WriteMetaStream.Dispose();
            return (0);
        }

        public int deleteFile(UInt64 ID)
        {
            if (Content.Count == 0) return (1);
            bool breaker = false;
            LinkedListNode<PicInfo> node = Content.First;
            while (node != Content.Last)
            {
                if (node.Value.ID == ID)
                {
                    Content.Remove(node);
                    breaker = true;
                }
                node = node.Next;
            }
            if (!breaker) return (2);
            file.GetEntry(ID.ToString()).Delete();
            return (0);
        }

        public LinkedList<QuerryMatch> searchThisArchive(LinkedList<Querry> Request)
        {
            if (this.initialized)
            {
                LinkedList<QuerryMatch> Target = new LinkedList<QuerryMatch>();
                QuerryMatch inserter;
                foreach (PicInfo Pic in Content)
                {
                    inserter = new QuerryMatch();
                    inserter.PicID = Pic.ID;
                    inserter.tags = Pic.tags;
                    Target.AddLast(inserter);
                }
                foreach (Querry search in Request)
                {
                    if (Target.Count == 0) break;
                    if (search.tagOption == Option.include) Target = applyInclusiveQuerry(Target, search.tag);
                    if (search.tagOption == Option.exclude) Target = applyExclusiveQuerry(Target, search.tag);
                }
                return (Target);
            }
            else return null;
        }

        public int removeTagFromPic(UInt64 PicID, UInt64 TagID)
        {
            if (!this.PicInfoReady) return (2);
            bool removed = false;
            foreach(PicInfo pic in Content)
            {
                if (pic.ID == PicID)
                {
                    removed = pic.tags.Remove(TagID);
                    break;
                }
            }
            if (!removed) return (1);
            return (0);
        }

        public int addTagToPic(UInt64 PicID, UInt64 TagID, System.IO.Stream MetaFile)
        {
            if (!this.PicInfoReady) return (2);
            bool added = false;
            foreach (PicInfo pic in Content)
            {
                if (pic.ID == PicID)
                {
                    pic.tags.AddLast(TagID);
                    added = true;
                    break;
                }
            }
            if (!added) return (1);
            this.writeMetaToFile(MetaFile);
            MetaFile.Close();
            return (0);
        }

        private LinkedList<QuerryMatch> applyInclusiveQuerry(LinkedList<QuerryMatch> Target, UInt64 TargetTag)
        {
            LinkedListNode<QuerryMatch> searcher = Target.First;
            bool apply_next = true;
            bool tag_met = false;
            for (int i = 1; i < Target.Count; i++)
            {
                apply_next = true;
                tag_met = false;
                if (i < (Target.Count - 1)) apply_next = false;
                foreach (UInt64 tag in searcher.Value.tags)
                {
                    if (tag == TargetTag) tag_met = true;
                }
                if(!tag_met)
                {
                    if (searcher != Target.Last)
                    {
                        apply_next = false;
                        searcher = searcher.Next;
                        Target.Remove(searcher.Previous);
                    }
                    else
                    {
                        Target.Remove(searcher);
                    }
                }
                if (searcher == Target.Last) break;
                if (apply_next) searcher = searcher.Next;
            }
            return (Target);
        }

        private LinkedList<QuerryMatch> applyExclusiveQuerry(LinkedList<QuerryMatch> Target, UInt64 TargetTag)
        {
            LinkedListNode<QuerryMatch> searcher = Target.First;
            bool apply_next = true;
            for (int i = 1; i < Target.Count; i++)
            {
                apply_next = true;
                foreach (UInt64 tag in searcher.Value.tags)
                {
                    if (tag == TargetTag)
                    {
                        if(searcher != Target.Last)
                        {
                            apply_next = false;
                            searcher = searcher.Next;
                            Target.Remove(searcher.Previous);
                        }
                        else
                        {
                            Target.Remove(searcher);
                        }
                        break;
                    }
                }
                if (searcher == Target.Last) break;
                if (apply_next) searcher = searcher.Next;
            }
            return (Target);
        }

        private int readMetaFromFile(System.IO.Stream metaFile)
        {
            System.IO.StreamReader strr = new System.IO.StreamReader(metaFile);
            string line = strr.ReadLine();
            if (line == null) return (1);
            nextPicID = UInt64.Parse(line);
            while ((line = strr.ReadLine()) != null)
            {
                string[] vallues = line.Split(' ');
                PicInfo inserter = new PicInfo();
                inserter.tags = new LinkedList<UInt64>();
                inserter.ID = UInt64.Parse(vallues[0]);
                if (vallues.Length >= 2) inserter.fileName = vallues[1];
                for (int i = 2; i < vallues.Length; i++)
                {
                    inserter.tags.AddLast(UInt64.Parse(vallues[i]));
                }
                Content.AddLast(inserter);
            }
            strr.Dispose();
            metaFile.Close();
            return (0);
        }

        private int writeMetaToFile(System.IO.Stream str)
        {
            System.IO.StreamWriter strw = new System.IO.StreamWriter(str);
            strw.WriteLine(nextPicID);
            foreach (PicInfo node in Content)
            {
                strw.Write(node.ID.ToString());
                if (node.fileName != null) strw.Write(" " + node.fileName);
                foreach (UInt64 tag in node.tags)
                {
                    strw.Write(" " + tag.ToString());
                }
                strw.WriteLine();
            }
            strw.Flush();
            str.Close();
            return (0);
        }
    }
}
