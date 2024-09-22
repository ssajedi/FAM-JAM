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
using System.Security.Cryptography;
using System.Windows.Controls;
using System.IO;

namespace revit_llm
{
    public class FamJamManager : IExternalEventHandler
    {

        public ExternalEvent _event;

        static UIApplication _application;

        public List<Furniture> Furnitures = new List<Furniture>();

      public  static string relativeFolder;

        public FamJamManager(UIApplication app, Document doc, string rootFolder)
        {
            //MessageBox.Show("hERE2");
            _event = ExternalEvent.Create(this);
            //MessageBox.Show("hERE3");
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

                DoAction(tran);

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

        int numberOfInstancesEARow = 5;

        //int totNumber = 11;

        double spacing = 5;

        public void DoAction(Transaction tran)
        {

            tran.Start();

            string[] rfaFiles = Directory.GetFiles(relativeFolder, "*.rfa");
            MessageBox.Show("Folder: "+ relativeFolder);
            MessageBox.Show("Total count: " + GetAllFamilies().Count.ToString());

            foreach (var file in GetAllFamilies())
            {
                Family family;
                if (_doc.LoadFamily(file, out family))
                {
                    //TaskDialog.Show("Load Family", $"Successfully loaded family: {Path.GetFileName(filePath)}");
                }
                else
                {
                    // TaskDialog.Show("Load Family", $"Failed to load family: {Path.GetFileName(filePath)}");
                }
            }


            var location = new XYZ();

            var familySymbols = GetFrameTypes(_doc);
            MessageBox.Show("here 2");
            foreach (var familySymbol in familySymbols)
            {
                MessageBox.Show(familySymbol.FamilyName);

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    _doc.Regenerate();

                }
            }
            MessageBox.Show("Furniture count: "+ Furnitures.Count);
            for (int i = 0; i <= Furnitures.Count - 1; i++)
            {
                var currentFurniture = Furnitures[i];

                var familySymbol = familySymbols.Find(x => x.FamilyName == currentFurniture.Type);

                //MessageBox.Show(familySymbols[0].FamilyName);
                //MessageBox.Show(currentFurniture.Type);

                if (familySymbol == null)
                {
                    familySymbol = familySymbols[i];
                    //MessageBox.Show($"Didn't find the symbol that matches the name {familySymbol}");
                }


                int rowNumber = i / numberOfInstancesEARow;

                int positionNumber = i % numberOfInstancesEARow;

                location = new XYZ(spacing * positionNumber, spacing * rowNumber, 0);

                FamilyInstance instance = _application.ActiveUIDocument.Document.Create.NewFamilyInstance(location, familySymbol, null, StructuralType.NonStructural);

                SetFamilyInstanceParameters(instance, currentFurniture.W / 12.0, currentFurniture.L / 12.0, currentFurniture.H / 12.0);

            }




            tran.Commit();

        }
        // Create an instance of the Random class
        Random random = new Random();

        static List<string> GetAllFamilies()
        {
            List<string> allFamilyFiles = new List<string>();

            try
            {
                // Get all .rfa files in the specified folder
                string[] rfaFiles = Directory.GetFiles(relativeFolder, "*.rfa");

                // Output the file names
                Console.WriteLine("Found .rfa files:");
                foreach (var file in rfaFiles)
                {
                    allFamilyFiles.Add(file);
                    Console.WriteLine(Path.GetFileName(file)); // Get only the file name
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Console.WriteLine("The specified directory does not exist.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return allFamilyFiles;
        }


        private void SetFamilyInstanceParameters(FamilyInstance familyInstance, double width, double length, double height)
        {
            // Get the parameters of the family instance
            Parameter widthParam = familyInstance.LookupParameter("Width");
            Parameter lengthParam = familyInstance.LookupParameter("Length");
            Parameter heightParam = familyInstance.LookupParameter("Height");

            // Set the width, length, and height
            if (widthParam != null && widthParam.UserModifiable)
            {
                widthParam.Set(width);
            }
            if (lengthParam != null && lengthParam.UserModifiable)
            {
                lengthParam.Set(length);
            }
            if (heightParam != null && heightParam.UserModifiable)
            {
                heightParam.Set(height);
            }
        }


        public static List<FamilySymbol> GetFrameTypes(Document _doc)
        {
            IEnumerable<FamilySymbol> _kickerTypes;
            _kickerTypes = from elem in
                              new FilteredElementCollector(_doc).OfClass(
                              typeof(FamilySymbol)).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel)
                           let type = elem as FamilySymbol
                           select type;

            // can't obtain beam types from the active document
            if (null == _kickerTypes ||
                0 == _kickerTypes.Count())
            {
                //throw new Exception("No Structural Framing Family loaded. Please load one of the Structure Framing Family.");
                return new List<FamilySymbol>();
            }

            return _kickerTypes.ToList().OrderBy(x => x.Name).ToList();

        }
    }
}
