using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace parameters
{
    public static partial class disc_parameters
    {
        public static string tagfield_entryname = "tagfield";
        public static string archivelist_entryname = "archivelist";
    }
}

namespace TDA
{

    public class PictureNode
    {
        public PictureNode() { }

        public Bitmap preview;
        public UInt64 ArchiveID;
        public UInt64 PictureID;
        public LinkedList<UInt64> Tags;
    }

    public class Disc
    {

        private string filepath;
        private System.IO.Compression.ZipArchive file;
        private TagField tags;
        private ArchiveList archivelist;
        private LinkedList<QuerryMatch> SearchResult;

        public int NumberOfPicsFound
        {
            get
            {
                if (SearchResult == null) return 0;
                else return SearchResult.Count;
            }
        }

        public int NumberOfPages(int PageSize)
        {
            if (PageSize <= 0) return 0;
            else return (NumberOfPicsFound / PageSize + (NumberOfPicsFound % PageSize == 0 ? 0 : 1));
        }

        public LinkedList<PictureNode> GetPage(int PageSize, int PageID)
        {
            LinkedList<PictureNode> Target = new LinkedList<PictureNode>();
            if (SearchResult != null)
            {
                if (NumberOfPages(PageSize) > PageID)
                {
                    int PageBegin = PageSize * PageID,
                        PageEnd = PageSize * (PageID + 1);
                    for(int i = PageBegin; i < PageEnd && i < SearchResult.Count; i++)
                    {
                        PictureNode inserter = new PictureNode();
                        inserter.ArchiveID = SearchResult.ElementAt(i).ArchiveID;
                        inserter.PictureID = SearchResult.ElementAt(i).PicID;
                        inserter.Tags = SearchResult.ElementAt(i).tags;
                        Target.AddLast(inserter);
                    }
                }
            }
            return Target;
        }

        public Disc(string filepath, bool newDisc)
        {
            this.SearchResult = null;
            this.filepath = filepath;
            if (newDisc) this.createNewDisc();
            this.loadDataFromFile();
        }

        public int addFileToDisc(Int64 filesize, string fileToAddPath)
        {
            ReopenParrent reopener = new ReopenParrent(this.ReopenAndReturn);
            ReopenSelf reopenerArchive = new ReopenSelf(this.ArchiveReopenSelfAndReturn);
            GetWriteStream writer = new GetWriteStream(this.GetArchiveListStream);
            GetMetafile metawriter = new GetMetafile(this.GetArchiveMetaStream);
            this.archivelist.addFileToArchive(filesize, fileToAddPath, this.file, reopener, reopenerArchive, writer, metawriter);
            this.ReopenFile();
            return (0);
        }

        public int TagFile(UInt64 ArchiveID, UInt64 PicID, UInt64 TagID)
        {
            GetMetafile metawriter = new GetMetafile(this.GetArchiveMetaStream);
            int returner = this.archivelist.TagFile(ArchiveID, PicID, TagID, metawriter);
            this.ReopenFile();
            if (returner != 0) return (returner);
            return (0);
        }

        public int AddTag(string newtag)
        {
            int returner = this.tags.addTag(newtag);
            if (returner == 0)
            {
                System.IO.Compression.ZipArchiveEntry tags_file = file.GetEntry(parameters.disc_parameters.tagfield_entryname);
                this.UpdateTags();
            }
            return(returner);
        }

        public LinkedList<QuerryMatch> Search(LinkedList<Querry> Search)
        {
            return this.archivelist.SearchForPictures(Search, new ReopenSelf(this.ArchiveReopenSelfAndReturn));
        }

        public int DeleteTag(TagField.TagNode tag)
        {
            int returner = this.tags.removeTag(tag.name);
            this.UpdateTags();
            //if returner == 0 remove from files
            return (returner);
        }

        public int loadDataFromFile()
        {
            file = System.IO.Compression.ZipFile.Open(filepath, System.IO.Compression.ZipArchiveMode.Update);
            this.readTags();
            this.getArchiveData();
            this.getArchives();
            return (0);
        }

        public LinkedList<TagField.TagNode> searchTags(string request)
        {
            return (this.tags.returnSearch(request));
        }

        private int getArchives()
        {
            if (archivelist.getArchiveNumber() == 0) return(1);
            archivelist.initializeArchives(this.file);
            return (0);
        }

        private int getArchiveData()
        {
            System.IO.Compression.ZipArchiveEntry archivelist_file = file.GetEntry(parameters.disc_parameters.archivelist_entryname);
            archivelist = new ArchiveList();
            int returner = archivelist.readFromFile(archivelist_file.Open());
            return (returner);
        }

        private int readTags()
        {
            System.IO.Compression.ZipArchiveEntry tags_file = file.GetEntry(parameters.disc_parameters.tagfield_entryname);
            tags = new TagField();
            int returner = tags.readFromFile(tags_file.Open());
            return (returner);
        }

        private int UpdateTags()
        {
            System.IO.Compression.ZipArchiveEntry tags_file = file.GetEntry(parameters.disc_parameters.tagfield_entryname);
            tags_file.Delete();
            file.CreateEntry(parameters.disc_parameters.tagfield_entryname);
            tags_file = file.GetEntry(parameters.disc_parameters.tagfield_entryname);
            tags.writeToFile(tags_file.Open());
            this.ReopenFile();
            return (0);
        }

        private System.IO.Stream GetArchiveListStream()
        {
            return (this.file.GetEntry(parameters.disc_parameters.archivelist_entryname).Open());
        }

        private System.IO.Compression.ZipArchive ReopenAndReturn()
        {
            this.ReopenFile();
            return (this.file);
        }

        private System.IO.Stream GetArchiveMetaStream(UInt64 ArchiveID)
        {
            System.IO.Stream target = this.file.GetEntry(ArchiveID.ToString() + parameters.archivelist_parameters.archive_meta_extension).Open();
            return (target);
        }

        private System.IO.Compression.ZipArchive ArchiveReopenSelfAndReturn(UInt64 ArchiveID)
        {
            return ( new System.IO.Compression.ZipArchive (this.file.GetEntry(ArchiveID.ToString() + parameters.archivelist_parameters.archive_extension).Open(),
                System.IO.Compression.ZipArchiveMode.Update) );
        }

        private void ReopenFile()
        {
            file.Dispose();
            System.IO.FileInfo reopener = new System.IO.FileInfo(filepath);
            file = new System.IO.Compression.ZipArchive(reopener.Open(System.IO.FileMode.Open), System.IO.Compression.ZipArchiveMode.Update, false, null);
        }

        private int createNewDisc()
        {
            System.IO.Compression.ZipArchive file2 = System.IO.Compression.ZipFile.Open(filepath, System.IO.Compression.ZipArchiveMode.Create);
            file2.Dispose();
            file = System.IO.Compression.ZipFile.Open(filepath, System.IO.Compression.ZipArchiveMode.Update);
            file.CreateEntry(parameters.disc_parameters.tagfield_entryname);
            System.IO.Compression.ZipArchiveEntry entry_tf = file.GetEntry(parameters.disc_parameters.tagfield_entryname);
            System.IO.StreamWriter stream_tf = new System.IO.StreamWriter(entry_tf.Open());
            stream_tf.WriteLine(((UInt64)0).ToString());
            stream_tf.Close();
            file.CreateEntry(parameters.disc_parameters.archivelist_entryname);
            System.IO.Compression.ZipArchiveEntry entry_arcl = file.GetEntry(parameters.disc_parameters.archivelist_entryname);
            System.IO.StreamWriter stream_arcl = new System.IO.StreamWriter(entry_arcl.Open());
            stream_arcl.WriteLine(((UInt64)0).ToString());
            stream_arcl.Close();
            file.Dispose();
            return (0);
        }

    }
}
