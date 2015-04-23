using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscController
{
    public partial class FileTagAdder : Form
    {

        private TDA.Disc parrent;
        private string search;
        private UInt64 ArchiveID;
        private UInt64 PicID;
        private System.Drawing.Bitmap Picture;
        private string filename;
        private LinkedList<MainTagNode> Tags;
        private LinkedList<ActiveTagNode> ActiveTags;
        public static bool TagsAdded;

        public FileTagAdder(TDA.Disc parrent)
        {
            this.parrent = parrent;
            this.search = "";
            this.Tags = new LinkedList<MainTagNode>();
            this.ActiveTags = new LinkedList<ActiveTagNode>();
            InitializeComponent();
        }

        private int LoadLastImage()
        {
            if (!parameters.last_added_file.ready) return (1);
            PicID = parameters.last_added_file.picID;
            ArchiveID = parameters.last_added_file.archiveID;
            Picture = parameters.last_added_file.file;
            filename = parameters.last_added_file.filename;
            this.pictureBox1.Image = Picture;
            this.Text = filename;
            parameters.last_added_file.ready = false;
            return (0);
        }

        private void SearchTagField(string request)
        {
            LinkedList<TDA.TagField.TagNode> result = this.parrent.searchTags(request);
            MainTagNode inserter;
            Tags.Clear();
            this.listView1.Items.Clear();
            foreach (TDA.TagField.TagNode node in result)
            {
                inserter = new MainTagNode();
                inserter.basetag = node;
                inserter.active = false;
                Tags.AddLast(inserter);
            }
            this.ActivateTags();
            this.UpdateTagArea();
        }

        private void ActivateTags()
        {
            if (ActiveTags.Count == 0 || Tags.Count == 0) return;
            LinkedListNode<MainTagNode> selector;
            MainTagNode changer;
            bool next = true;
            foreach (ActiveTagNode node in ActiveTags)
            {
                selector = Tags.First;
                while (true)
                {
                    if (selector.Value.basetag.ID == node.basetag.ID)
                    {
                        changer = selector.Value;
                        changer.active = true;
                        Tags.AddBefore(selector, changer);
                        if (selector != Tags.Last)
                        {
                            selector = selector.Next;
                            next = false;
                            Tags.Remove(selector.Previous);
                        }
                        else Tags.Remove(selector);
                    }
                    if (!next)
                    {
                        next = true;
                        continue;
                    }
                    if (selector == Tags.Last) break;
                    selector = selector.Next;
                }
            }
        }

        private void UpdateActiveTagArea()
        {
            listView2.Clear();
            if (this.ActiveTags.Count == 0)
            {
                listView2.Items.Add(lang.FileTagAdder_No_Active_Tags);
                listView2.Update();
                return;
            }
            foreach (ActiveTagNode node in ActiveTags)
            {
                listView2.Items.Add(node.ToString());
            }
            listView2.Update();
        }

        private void UpdateTagArea()
        {
            listView1.Clear();
            if (this.Tags.Count == 0)
            {
                listView1.Items.Add(lang.NO_TAGS);
                listView1.Update();
                return;
            }
            foreach (MainTagNode node in Tags)
            {
                if (!node.active) listView1.Items.Add(node.basetag.name);
            }
            if (listView1.Items.Count == 0)
            {
                listView1.Items.Add(lang.FileTagAdder_All_Tags_Are_Active);
            }
            listView1.Update();
        }

        private void ApplyLocalization()
        {
            this.tabPage1.Text = lang.FileTagAdder_TagsTab;
            this.tabPage2.Text = lang.FileTagAdder_ActiveTagsTab;
            this.button1.Text = lang.FileTagAdder_CloseButton;
            this.button2.Text = lang.FileTagAdder_AddTagsAndCloseButton;
            this.button3.Text = lang.FileTagAdder_NewTagButton;
        }

        private void FileTagAdder_Load(object sender, EventArgs e)
        {
            this.SearchTagField(search);
            this.UpdateActiveTagArea();
            this.ApplyLocalization();
            int returner = this.LoadLastImage();
            if (returner != 0) this.Close();
        }

        private void AddTag()
        {
            NewTagAdder popup = new NewTagAdder();
            popup.ShowDialog();
            if (!popup.Resultative) return;
            FileTagAdder.TagsAdded = true;
            int result = this.parrent.AddTag(popup.ResultTag);
            switch (result)
            {
                case (0):
                    {
                        this.SearchTagField(this.search);
                        break;
                    }
                case (1):
                    {
                        MessageBox.Show(lang.ERROR_CHARCHECK,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        break;
                    }
                case (2):
                    {
                        MessageBox.Show(lang.ERROR_TAG_EXISTS,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        break;
                    }
                case (3):
                    {
                        MessageBox.Show(lang.ERROR_TOO_MANY_TAGS,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        break;
                    }
                default:
                    {
                        MessageBox.Show(lang.ERROR_UNKNOWN,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        break;
                    }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.AddTag();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach(ActiveTagNode node in this.ActiveTags)
            {
                this.parrent.TagFile(ArchiveID, PicID, node.basetag.ID);
            }
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private ActiveTagNode getInserterForActiveTags(string selection)
        {
            MainTagNode selector = new MainTagNode();
            selector.basetag.name = lang.NO_TAGS;
            LinkedListNode<MainTagNode> node = Tags.First;
            bool next = true;
            while (true)
            {
                if (node.Value.basetag.name == selection)
                {
                    selector = node.Value;
                    if (selector.active == true)
                    {
                        MessageBox.Show(lang.ERROR_TAG_ACTIVE,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        selector.basetag.name = null;
                        break;
                    }
                    else
                    {
                        selector.active = true;
                        Tags.AddBefore(node, selector);
                        if (node != Tags.Last)
                        {
                            node = node.Next;
                            Tags.Remove(node.Previous);
                            next = false;
                        }
                        else Tags.Remove(node);
                        break;
                    }
                }
                if (!next)
                {
                    next = true;
                    continue;
                }
                if (node == Tags.Last) break;
                node = node.Next;
            }
            ActiveTagNode inserter = new ActiveTagNode();
            inserter.basetag = selector.basetag;
            return (inserter);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            string selection = ((ListView)sender).SelectedItems[0].Text;
            if (selection != lang.NO_TAGS && selection != lang.FileTagAdder_All_Tags_Are_Active)
            {
                ActiveTagNode inserter = getInserterForActiveTags(selection);
                if (inserter.basetag.name == null) return;
                if (inserter.basetag.name == lang.NO_TAGS)
                {
                    MessageBox.Show(lang.ERROR_TAG_MISSING,
                                    lang.ERROR_MESSAGEBOX_HEADER,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation,
                                    MessageBoxDefaultButton.Button1);
                    return;
                }
                inserter.option = TDA.Option.include;
                ActiveTags.AddLast(inserter);
                this.UpdateTagArea();
                this.UpdateActiveTagArea();
            }
        }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            string selection = ((ListView)sender).SelectedItems[0].Text;
            if (selection == lang.FileTagAdder_No_Active_Tags || ActiveTags.Count == 0) return;
            selection = selection.Substring(2);
            UInt64 TargetID;
            LinkedListNode<ActiveTagNode> ActiveSelector = ActiveTags.First;
            while (true)
            {
                if (ActiveSelector.Value.basetag.name == selection)
                {
                    TargetID = ActiveSelector.Value.basetag.ID;
                    ActiveTags.Remove(ActiveSelector);
                    break;
                }
                if (ActiveSelector == ActiveTags.Last)
                {
                    MessageBox.Show(lang.ERROR_TAG_MISSING,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                    return;
                }
                ActiveSelector = ActiveSelector.Next;
            }
            if (Tags.Count == 0) return;
            LinkedListNode<MainTagNode> MainSelector = Tags.First;
            while (true)
            {
                if (MainSelector.Value.basetag.ID == TargetID)
                {
                    MainTagNode inserter = MainSelector.Value;
                    inserter.active = false;
                    Tags.AddBefore(MainSelector, inserter);
                    Tags.Remove(MainSelector);
                    break;
                }
                if (MainSelector == Tags.Last) break;
                MainSelector = MainSelector.Next;
            }
            this.UpdateActiveTagArea();
            this.UpdateTagArea();
        }

    }
}
