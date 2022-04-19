using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace Telepathy
{
    public static class TelepathyUtils
    {
        //Special override of the method to handle expiring the solution in copy/paste scenarios.
        //If scheduleNew is false it just does the same thing as the default method.
        public static void connectMatchingParams(GH_Document doc,bool scheduleNew)
        {
            connectMatchingParams(doc);
           if(scheduleNew) Grasshopper.Instances.ActiveCanvas.Document.ScheduleSolution(10);
        }

        //the main method that does the work - checks all receivers and senders for matches,
        //and rewires accordingly.
        public static void connectMatchingParams(GH_Document doc)
        {
            //Get all the activeobjects. We have to call the
            //Grasshopper.Instances.ActiveCanvas.Document rather than the 
            //doc that's passed into the method because
            //in copy-paste scenarios, that's actually a VIRTUAL doc with no other
            //objects in it, whereas this one is the actually active document.
            var activeObjects = Grasshopper.Instances.ActiveCanvas.Document.ActiveObjects();
           //get all the remote receiver params
            var allReceivers = activeObjects.Where(x => x is Param_RemoteReceiver).Cast<Param_RemoteReceiver>().ToList();

            //get all the remote sender params
            var allSenders = activeObjects.Where(x => x is Param_RemoteSender).Cast<Param_RemoteSender>().ToList();


            //for each receiver...
            foreach (Param_RemoteReceiver receiver in allReceivers)
            {
                //wire up that receiver to all matching senders
                ProcessReceiver(allSenders, receiver);
            }
         
        }


       // this method wires up a receiver to all matching senders. 
        public static void ProcessReceiver(List<Param_RemoteSender> allSenders, Param_RemoteReceiver receiver)
        {
            //get the key
            string key = receiver.NickName;
            //stop if it's empty
            if (string.IsNullOrEmpty(key)) return;

            //check if the existing sources match the key, throw em out otherwise
            List<IGH_Param> sourcesToRemove = new List<IGH_Param>();
            foreach (IGH_Param param in receiver.Sources)
            {
                //if the source does not match, remove it
                if (!LikeOperator.LikeString(param.NickName, key, Microsoft.VisualBasic.CompareMethod.Binary))
                {
                    sourcesToRemove.Add(param);
                }
            }

            //a custom method to remove all sources at once - calling RemoveSource in a loop
            //was giving me trouble because it kept expiring the solution repeatedly.
            RemoveSources(receiver, sourcesToRemove);



            //get all the senders whose nickname matches the key
            var matchingSenders = allSenders.Where(s => LikeOperator.LikeString(s.NickName,key,Microsoft.VisualBasic.CompareMethod.Binary));

            //for all the matching senders
            foreach (Param_RemoteSender sender in matchingSenders)
            {
                //if the matching sender is not currently a source, add it
                if (!receiver.Sources.Contains(sender))
                {
                    receiver.AddSource(sender);

                }


            }
        }

        


        //this method safely handles removing multiple sources at a time. 
        public static void RemoveSources(IGH_Param target, List<IGH_Param> sources)
        {
            foreach(IGH_Param source in sources){
                if (source == null) continue;
                if (!target.Sources.Contains(source))
                {
                    continue;
                }
                target.Sources.Remove(source);
                source.Recipients.Remove(target);
                
            }
            if (sources.Count > 0)
            {
                target.OnObjectChanged(GH_ObjectEventType.Sources);
                target.ExpireSolution(false);
            }
           

        }

        // utility method to get the last added key for the purposes of the .. shortcut
        internal static string GetLastUsedKey(GH_Document doc)
        {
            return GetAllKeys(doc).Last();
        }

        //retrieve all keys in the current document
        public static List<string> GetAllKeys(GH_Document doc)
        {
            var allKeys = doc.ActiveObjects().Where(o => o is RemoteParam).Select(o => o.NickName).Distinct().ToList();
            return allKeys;
        }

        // iterate over all the remoteParams in the doc and find and replace text in their names. 
        internal static void FindReplace(string find, string replace, bool forceExact)
        {
            //get all the remote params
            var allKeys = Grasshopper.Instances.ActiveCanvas.Document.ActiveObjects().Where(o => o is RemoteParam);
           //for all the remote params
            foreach (var key in allKeys)
            {
                // if the key matches the string
                if (forceExact ? key.NickName == find : key.NickName.Contains(find))
                {
                    //replace the text with the new string
                    key.NickName = key.NickName.Replace(find,replace);
                    //clean up component display
                    key.Attributes.ExpireLayout();
                }
            }
        }
    }

   
}
