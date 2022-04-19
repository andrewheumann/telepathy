using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Telepathy
{
    public partial class FindReplaceForm : Form
    {
        public FindReplaceForm()
        {
            InitializeComponent();
        }


        // do nothing on cancel
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // on replace all, call the find replace utility on all text.
        private void ReplaceAllClick(object sender, EventArgs e)
        {
            TelepathyUtils.FindReplace(this.textBox1.Text, this.textBox2.Text, forceExactCbx.Checked);
            this.Close();
        }

       
    }
}
