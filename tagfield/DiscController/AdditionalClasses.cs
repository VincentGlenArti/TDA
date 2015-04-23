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
    public enum PopupWindowState { OK, None };

    public struct ActiveTagNode
    {
        public TDA.TagField.TagNode basetag;
        public TDA.Option option;

        public override string ToString()
        {
            string Target = "";
            if (this.option == TDA.Option.include) Target = Target + "+ ";
            else if (this.option == TDA.Option.exclude) Target = Target + "- ";
            Target = Target + basetag.name;
            return (Target);
        }
    }

    public struct MainTagNode
    {
        public TDA.TagField.TagNode basetag;
        public bool active;
    }

}
