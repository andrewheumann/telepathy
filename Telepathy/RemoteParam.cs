using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.HTML;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Linq;

namespace Telepathy
{
    // This class handles all the behavior that's shared between the Sender and Receiver. 
    // It inherits from Param_GenericObject so that it behaves like a simple data parameter 
    // in most cases, and implements IGH_InitCodeAware so that it can take Sender=something 
    // and Receiver=something double-click shortcut codes.
    public abstract class RemoteParam : Param_GenericObject, IGH_InitCodeAware
    {
       

        // This lets us accept "init code" strings from the user in the double click menu, so 
        // if you type RemoteSender=MyData it will come pre-named accordingly.
        public void SetInitCode(string code)
        {

            if (code == "..")
            {

                this.NickName = TelepathyUtils.GetLastUsedKey(Grasshopper.Instances.ActiveCanvas.Document);
                return;
            }
            try
            {
                this.NickName = code;
            }
            catch
            {

            }
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            // this is to handle copy paste scenarios - we call a special version of the util that also triggers a solution afterwards to "clean up"
            document.ScheduleSolution(5, callback => TelepathyUtils.connectMatchingParams(callback, true));

        }

        

        //override component layout
        public override void CreateAttributes()
        {
            base.Attributes = new RemoteParamAttributes(this);
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }

        //add the Keys + find/replace menu
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem recentKeyMenu = GH_DocumentObject.Menu_AppendItem(menu, "Keys");
            foreach (string key in TelepathyUtils.GetAllKeys(Grasshopper.Instances.ActiveCanvas.Document).OrderBy(s => s))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    System.Windows.Forms.ToolStripMenuItem keyitem = GH_DocumentObject.Menu_AppendItem(recentKeyMenu.DropDown, key, new EventHandler(Menu_KeyClicked));
                }
            }
            ToolStripMenuItem findReplaceMenu = GH_DocumentObject.Menu_AppendItem(menu, "Find/Replace key names", new EventHandler(Menu_FindReplaceClicked));
            base.AppendAdditionalMenuItems(menu);
        }

        //trigger this when find and replace clicked - launch the form.
        private void Menu_FindReplaceClicked(object sender, EventArgs e)
        {
            FindReplaceForm fr_form = new FindReplaceForm();
            fr_form.ShowDialog();
        }

        //respond to keys menu selection
        private void Menu_KeyClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem keyItem = (System.Windows.Forms.ToolStripMenuItem)sender;
            this.NickName = keyItem.Text;
            this.Attributes.ExpireLayout();
        }
    }
}
