using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Reflection;

using System.Windows.Media.Imaging;
using revit_llm;


namespace RevitPlugIn
{
    public delegate void SelectionChanged();

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class MainEntry : IExternalApplication
    {
        const string TabName = "AECHeck";

        const string docPanelName = "Documents";

        const string modelingPanelName = "Modeling";

        Assembly assem = Assembly.GetAssembly(typeof(MainEntry));

        public static MainEntry thisApp = null;

        public static Document doc;

        public event SelectionChanged ElementSelectionChanged;

        internal UIControlledApplication _controlledApplication;

        internal MainWindow famJam;

        public string AssemblyDirctionry
        {
            get
            {
                return assem.CodeBase.Replace("RevitPlugIn.dll", "").Replace(@"file:///", "");
            }
        }


        public Result OnStartup(UIControlledApplication application)
        {



            try
            {
                //GetSelectionChangedEvent();

                _controlledApplication = application;

                CreateTab(application);

                thisApp = this;

                _controlledApplication = application;

                application.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;
                application.ControlledApplication.DocumentCreated += ControlledApplication_DocumentCreated;
                application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
                application.ControlledApplication.DocumentSavedAs += ControlledApplication_DocumentSavedAs;



                // _familyDirMgr.RootFolder = @"Z:\___BIM\FAMILY\REVIT FAMILY\RST" + " " + application.ControlledApplication.VersionNumber;



                return Result.Succeeded;


            }
            catch (Exception ex)
            {
                TaskDialog.Show("Fail to add Tools Tab", ex.ToString());
                return Result.Failed;
            }



        }


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentSavedAs(object sender, Autodesk.Revit.DB.Events.DocumentSavedAsEventArgs e)
        {
            if (e.Document == null) { return; }
            doc = e.Document;
        }

        private void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            doc = e.Document;
        }

        private void ControlledApplication_DocumentCreated(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs e)
        {
            if (e.Document == null) { return; }
            doc = e.Document;
        }

        private void ControlledApplication_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            if (e.GetDocument() == null) { return; }
            doc = e.GetDocument();
        }


        private void CreateTab(UIControlledApplication application)
        {

            application.CreateRibbonTab(TabName);

            RibbonPanel docPanel = application.CreateRibbonPanel(TabName, docPanelName);

            RibbonPanel modelingPanel = application.CreateRibbonPanel(TabName, modelingPanelName);

        

            PushButtonData pbtndtFamJamTool = new PushButtonData("FamJam", "FamJam Tool", AssemblyDirctionry + "RevitPlugIn.dll", "RevitPlugIn.FamJam");

            PushButton pbtnFamJam = docPanel.AddItem(pbtndtFamJamTool) as PushButton;

            pbtnFamJam.ToolTip = "FamJam Tool Developed in AECTech LA";


        }

    }
}
