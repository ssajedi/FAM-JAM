using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.ExtensibleStorage;

using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.DirectoryServices;
using System.Reflection;

namespace RevitPlugIn
{
    public class FamilyManager : IExternalEventHandler
    {

        public ExternalEvent _event;

        static UIApplication _application;

        public FamilyManager(UIApplication app, Document doc)
        {

            _event = ExternalEvent.Create(this);

            _application = app;
        }

        static Autodesk.Revit.DB.Document _doc
        {
            get
            {
                return _application.ActiveUIDocument.Document;
            }
        }

        public void Execute(UIApplication app)
        {

            Transaction tran = new Transaction(_doc, "FamJam");

            try
            {

                DrawCurvedBeam(tran);

            }
            catch (Exception ex)
            {
                if (tran.HasStarted()) { tran.RollBack(); }
                MessageBox.Show("Fail to create family: " + ex.Message + ex.StackTrace);
                return;
            }


        }

        public string GetName()
        {
            return "FamJam";
        }


        public void DrawCurvedBeam(Transaction tran)
        {

        


            tran.Commit();

        }
    }
}
