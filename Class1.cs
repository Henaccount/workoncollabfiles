using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.ProcessPower.ProjectManagerUI;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using Autodesk.ProcessPower.DataObjects.Synchronization;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Checkout.Class1))]

namespace Checkout
{
    public class Class1
    {
        [CommandMethod("workoncollabfiles", CommandFlags.Session)]
        public async void workoncollabfiles()
        {

            PlantProject project = PlantApplication.CurrentProject;
            var pipingPart = project.ProjectParts["Piping"];
            var ds = project.DocumentService;
            PnPSyncConflicts conflicts;

            ds.DownloadHeadRevision(pipingPart, true, false, null, out conflicts);
            System.Collections.Generic.List<PnPProjectDrawing> files = pipingPart.GetPnPDrawingFiles();

            foreach (var file in files)
            {
                ds.CheckOut(file, string.Empty, true, null, out conflicts);

                // work on the file
                string strDwgPath = file.ResolvedFilePath;
                DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;

                Document docToWorkOn = acDocMgr.Open(strDwgPath, false);

                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = docToWorkOn;

                //Editor ed = docToWorkOn.Editor;

                using (docToWorkOn.LockDocument())
                {
                    //do common plant api stuff here
                    //...

                    //if you want to execute commands, do like this:
                    await AcadApp.DocumentManager.ExecuteInCommandContextAsync(async (obj) =>
                    {
                        await AcadApp.DocumentManager.MdiActiveDocument.Editor.CommandAsync("_Plantshowall");
                        await AcadApp.DocumentManager.MdiActiveDocument.Editor.CommandAsync("_UnIsolateObjects");
                        await AcadApp.DocumentManager.MdiActiveDocument.Editor.CommandAsync("LAYTHW");
                        await AcadApp.DocumentManager.MdiActiveDocument.Editor.CommandAsync("LAYON");

                        await AcadApp.DocumentManager.MdiActiveDocument.Editor.CommandAsync("_PlantSteelSetRep", "_H");
                        //audit?
                        //plantaudit?
                        //purge?
                    }, null);
                }

                docToWorkOn.CloseAndSave(strDwgPath);
                //docToWorkOn.Database.SaveAs(strDwgPath, true, DwgVersion.Current, docToWorkOn.Database.SecurityParameters);
                //docToWorkOn.CloseAndDiscard();

                // Checkin
                //                                            
                ds.CheckIn(file, string.Empty, false, null, null, out conflicts);

            }




        }
    }
}