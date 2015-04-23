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
    public partial class NewTagAdder : Form
    {

        public string ResultTag;
        public bool Resultative;

        public NewTagAdder()
        {
            InitializeComponent();
            Resultative = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string result = this.textBox1.Text;
            if(!TDA.TagField.charCheck(result))
            {
                MessageBox.Show(lang.ERROR_CHARCHECK,
                                lang.ERROR_MESSAGEBOX_HEADER,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
                return;
            }
            ResultTag = result;
            Resultative = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NewTagAdder_Load(object sender, EventArgs e)
        {
            this.button1.Text = lang.NEWTAGADDER_ADD_BUTTON;
            this.button2.Text = lang.NEWTAGADDER_CANCEL;
            this.Text = lang.NEWTAGADDER_HEADER;
        }

    }
}
