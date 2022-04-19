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
    //Class derives all its behavior from remoteParam, which does the bulk of the work, and just overrides a few functions for the specific things.
   public class Param_RemoteSender : RemoteParam
    {
      
       //constructor - calls the base, sets a default nickname key
       public Param_RemoteSender()
           : base()
       {
           nicknameKey = "unset"; //better default value here?
           base.NickName = nicknameKey;
           base.Hidden = true;
       }

        //Maintain internal private nickname key. I could probably just call on the nickname property itself, not sure.
        protected string nicknameKey = "";

       

        // First piece of magic - whenever the nickname is set, it calls the util method 
        // to check that all matching params are connected properly 
        public override string NickName
        {
            get
            {
                nicknameKey = base.NickName;
                return nicknameKey;
            }
            set
            {
                nicknameKey = value;
                base.NickName = nicknameKey;

                //best practice to do this as a schedule solution so that you're not rewiring stuff mid-solution. May not be 
                //strictly necessary here since a nickname change isn't a solution event, but, hey.
                GH_Document doc = this.OnPingDocument();
                // This check fixes a bug where during clustering it would try to access a document 
                // that didn't exist. Seems to be all better now.
                if (doc != null) 
                {
                    doc.ScheduleSolution(10, TelepathyUtils.connectMatchingParams);
                }
            }
        }

       // here's all the basic stuff for overriding the name/description of the param. 
       // Normally we'd pass a new IGH_ComponentDescription through the base, but since 
       // we're extending an existing param class we have to do it manually.
       #region Overriding Name and Description
       public override string TypeName => "Remote Sender";


        public override string Category
       {
           get => "Params";
           set => base.Category = value;
       }
       public override string SubCategory
       {
           get => "Telepathy";
           set => base.SubCategory = value;
       }

       public override string Name
       {
           get => "Remote Sender";
           set => base.Name = value;
       }
       #endregion

     
  

       //make sure it has a new guid so no conflicts w existing components   
       public override Guid ComponentGuid => new Guid("{ADA99447-8A42-4C8E-BAA4-C8EF36A372B6}");

        protected override Bitmap Icon => Properties.Resources.sender;
    }
}
