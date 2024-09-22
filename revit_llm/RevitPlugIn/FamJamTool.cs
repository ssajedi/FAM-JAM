using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using revit_llm;
using RevitPlugIn;

namespace RevitPlugIn
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class FamJam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            if (null == commandData)
            {
                throw new ArgumentNullException("commandData");
            }


            if (MainEntry.thisApp.famJam == null || MainEntry.thisApp.famJam.IsClosed)
            {

                MainEntry.thisApp.famJam = new MainWindow(commandData.Application, MainEntry.thisApp._controlledApplication);

                MainEntry.thisApp.famJam.Show();

            }
            else
            {

                MainEntry.thisApp.famJam.Close();

                MainEntry.thisApp.famJam = new MainWindow(commandData.Application, MainEntry.thisApp._controlledApplication);

                MainEntry.thisApp.famJam.Show();

            }

            return Autodesk.Revit.UI.Result.Succeeded;


        }
    }
}
