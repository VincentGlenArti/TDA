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

    public partial class DiscController : Form
    {

        private TDA.Disc file;
        private string filepath;
        private bool newfile;
        private LinkedList<MainTagNode> Tags;
        private LinkedList<ActiveTagNode> ActiveTags;
        private LinkedList<TDA.PictureNode> Pictures;
        private string search;
        private int PageSize;

        public DiscController(string filepath, bool newfile)
        {
            this.filepath = filepath;
            this.newfile = newfile;
            InitializeComponent();
        }

        private void DiscController_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectedItem = this.comboBox1.Items[0];
            this.ApplyLocalization();
            try
            {
                this.file = new TDA.Disc(filepath, newfile);
            }
            catch(Exception EXC_DISC_LOAD)
            {
                MessageBox.Show(lang.ERROR_DISC_LOAD + EXC_DISC_LOAD.Message,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                this.Close();
                return;
            }
            this.ActiveTags = new LinkedList<ActiveTagNode>();
            this.Tags = new LinkedList<MainTagNode>();
            this.search = "";
            this.SearchTagField(search);
            this.UpdateActiveTagArea();
        }

        private void SearchTagField(string request)
        {
            LinkedList<TDA.TagField.TagNode> result = this.file.searchTags(request);
            MainTagNode inserter;
            Tags.Clear();
            foreach(TDA.TagField.TagNode node in result)
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
            foreach(ActiveTagNode node in ActiveTags)
            {
                selector = Tags.First;
                while(true)
                {
                    if(selector.Value.basetag.ID == node.basetag.ID)
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
            listView3.Clear();
            if (this.ActiveTags.Count == 0)
            {
                listView3.Items.Add(lang.NO_ACTIVE_TAGS);
                listView3.Update();
                return;
            }
            foreach (ActiveTagNode node in ActiveTags)
            {
                listView3.Items.Add(node.ToString());
            }
            listView3.Update();
            CommenceFileSearch();
        }

        private void CommenceFileSearch()
        {
            this.Pictures = new LinkedList<TDA.PictureNode>();
            if (ActiveTags.Count != 0)
            {
                LinkedList<TDA.Querry> Search = new LinkedList<TDA.Querry>();
                foreach (ActiveTagNode TN in ActiveTags)
                {
                    TDA.Querry Inserter = new TDA.Querry();
                    Inserter.tag = TN.basetag.ID;
                    Inserter.tagOption = TN.option;
                    Search.AddLast(Inserter);
                }
                this.file.Search(Search);
                Pictures = this.file.GetPage(this.PageSize, 0);
            }
        }

        private void UpdateTagArea()
        {
            listView2.Clear();
            if (this.Tags.Count == 0)
            {
                listView2.Items.Add(lang.NO_TAGS);
                listView2.Update();
                return;
            }
            foreach(MainTagNode node in Tags)
            {
                if (!node.active) listView2.Items.Add(node.basetag.name);
            }
            if (listView2.Items.Count == 0)
            {
                listView2.Items.Add(lang.All_tags_are_active);
            }
            listView2.Update();
        }

        private void ApplyLocalization()
        {
            this.label2.Text = lang.ItemsPerPage;
            this.tabPage1.Text = lang.Tab1;
            this.tabPage2.Text = lang.Tab2;
            this.discToolStripMenuItem.Text = lang.MENU_0_Disc;
            this.newToolStripMenuItem.Text = lang.MENU_0_New;
            this.openToolStripMenuItem.Text = lang.MENU_0_Open;
            this.closeToolStripMenuItem.Text = lang.MENU_0_Close;
            this.settingsToolStripMenuItem.Text = lang.MENU_1_Settings;
            this.openSettingsToolStripMenuItem.Text = lang.MENU_1_OpenSettings;
            this.fileToolStripMenuItem.Text = lang.MENU_2_File;
            this.addFileToolStripMenuItem.Text = lang.MENU_2_Add;
            this.removeFileToolStripMenuItem.Text = lang.MENU_2_Remove;
            this.searchFileToolStripMenuItem.Text = lang.MENU_2_Search;
            this.tagToolStripMenuItem.Text = lang.MENU_3_Tag;
            this.addTagToolStripMenuItem.Text = lang.MENU_3_Add;
            this.removeTagToolStripMenuItem.Text = lang.MENU_3_Remove;
            this.searchTagToolStripMenuItem.Text = lang.MENU_3_Search;
            this.helpToolStripMenuItem.Text = lang.MENU_4_Help;
            this.howToUseToolStripMenuItem.Text = lang.MENU_4_HowToUse;
            this.aboutToolStripMenuItem.Text = lang.MENU_4_About;
            this.includeToolStripMenuItem.Text = lang.RMB_CLICK_1_Include;
            this.excludeToolStripMenuItem.Text = lang.RMB_CLICK_1_Exclude;
            this.deleteToolStripMenuItem.Text = lang.RMB_CLICK_1_Delete;
            this.addNewToolStripMenuItem.Text = lang.RMB_CLICK_1_AddNew;
            this.removeToolStripMenuItem.Text = lang.RMB_CLICK_2_Remove;
            //this.changeSearchOptionToolStripMenuItem.Text = lang.RMB_CLICK_2_ChangeSearchOption;
            this.contextMenuStrip1.Items[0].Text = lang.This_is_not_a_tag;
            //this.searchToolStripMenuItem.Text = lang.MENU
        }

        private void arminToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            string selection = ((ListView)sender).SelectedItems[0].Text;
            if (selection != lang.NO_TAGS)
            {
                contextMenuStrip1.Items[0].Text = selection;
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void listView3_ItemActivate(object sender, EventArgs e)
        {
            string selection = ((ListView)sender).SelectedItems[0].Text;
            if (selection != lang.NO_ACTIVE_TAGS.ToString())
            {
                contextMenuStrip2.Items[0].Text = selection;
                contextMenuStrip2.Show(Cursor.Position);
            }
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

        private void includeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selection = contextMenuStrip1.Items[0].Text;
            if ((!TDA.TagField.charCheck(selection)) || (selection == lang.NO_TAGS) || (Tags.Count == 0) || (selection == lang.This_is_not_a_tag)) return;
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
            this.contextMenuStrip1.Items[0].Text = lang.This_is_not_a_tag;
            this.UpdateTagArea();
            this.UpdateActiveTagArea();
        }

        private void excludeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selection = contextMenuStrip1.Items[0].Text;
            if ((!TDA.TagField.charCheck(selection)) || (selection == lang.NO_TAGS) || (Tags.Count == 0) || (selection == lang.This_is_not_a_tag)) return;
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
            inserter.option = TDA.Option.exclude;
            ActiveTags.AddLast(inserter);
            this.contextMenuStrip1.Items[0].Text = lang.This_is_not_a_tag;
            this.UpdateTagArea();
            this.UpdateActiveTagArea();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selection = contextMenuStrip1.Items[0].Text;
            if ((!TDA.TagField.charCheck(selection)) || (selection == lang.NO_TAGS) || (Tags.Count == 0) || (selection == lang.This_is_not_a_tag)) return;
            LinkedListNode<MainTagNode> node = Tags.First;
            while (true)
            {
                if (node.Value.basetag.name == selection)
                {
                    int returner = this.file.DeleteTag(node.Value.basetag);
                    if (returner == 1)
                    {
                        MessageBox.Show(lang.ERROR_DID_NOT_DELETE,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                        return;
                    }
                    Tags.Remove(node);
                    this.UpdateTagArea();
                    return;
                }
                if (node == Tags.Last) break;
                node = node.Next;
            }
            MessageBox.Show(lang.ERROR_TAG_MISSING,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
        }

        private void AddTag()
        {
            NewTagAdder popup = new NewTagAdder();
            popup.ShowDialog();
            if (!popup.Resultative) return;
            int result = this.file.AddTag(popup.ResultTag);
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

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.AddTag();
        }

        private void removeTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Do not delete;
        }

        private void addTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.AddTag();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selection = contextMenuStrip2.Items[0].Text;
            if (selection == lang.NO_ACTIVE_TAGS || ActiveTags.Count == 0) return;
            selection = selection.Substring(2);
            UInt64 TargetID;
            LinkedListNode<ActiveTagNode> ActiveSelector = ActiveTags.First;
            while(true)
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
            while(true)
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

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string fileToOpen = openFileDialog1.FileName;
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileToOpen);
                Int64 fileLengthMB = (fileInfo.Length / (1024 * 1024));
                if (fileLengthMB > parameters.archive_parameters.seedSizeMB)
                {
                    MessageBox.Show(lang.ERROR_FILE_TOO_LARGE,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                }
                else
                {
                    try
                    {
                        this.file.addFileToDisc(fileLengthMB, fileToOpen);
                    }
                    catch (Exception EXC)
                    {
                        MessageBox.Show(lang.ERROR_FILE_LOAD + EXC.Message,
                               lang.ERROR_MESSAGEBOX_HEADER,
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
                        return;
                    }
                    FileTagAdder.TagsAdded = false;
                    FileTagAdder addTagsToNewFile = new FileTagAdder(this.file);
                    addTagsToNewFile.ShowDialog();
                    if (FileTagAdder.TagsAdded)
                    {
                        this.SearchTagField(this.search);
                    }
                }
                this.CommenceFileSearch();
            }
        }

    }
}
