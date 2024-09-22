using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Reflection;

using System.Windows.Media.Imaging;
using revit_llm;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


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

        internal revit_llm.MainWindow famJam;

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

            PushButtonData pbtndtFamJamTool = new PushButtonData("FamJam", "FamJam Tool", AssemblyDirctionry + "RevitPlugIn.dll", "RevitPlugIn.FamJam");

            PushButton pbtnFamJam = docPanel.AddItem(pbtndtFamJamTool) as PushButton;

            pbtnFamJam.ToolTip = "FamJam Tool Developed in AECTech LA";

            BitmapImage imageTool = BitmapToBitmapImage(Resource1.Icon);

            pbtnFamJam.LargeImage = ScaledIcon(imageTool, 32, 32);
            pbtnFamJam.Image = ScaledIcon(imageTool, 16, 16);


        }
        public static BitmapImage BitmapToBitmapImage(Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Convert a Bitmap to a BitmapSource
        /// </summary>
        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                  hBitmap, IntPtr.Zero, Int32Rect.Empty,
                  BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return retval;
        }

        /// <summary>
        /// Convert a BitmapImage to Bitmap
        /// </summary>
        public static Bitmap BitmapImageToBitmap(
             BitmapImage bitmapImage)
        {
            //BitmapImage bitmapImage = new BitmapImage(
            // new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// Scale down large icon to desired size for Revit 
        /// ribbon button, e.g., 32 x 32 or 16 x 16
        /// </summary>
        public static BitmapSource ScaledIcon(
          BitmapImage large_icon,
          int w,
          int h)
        {
            return BitmapToBitmapSource(ResizeImage(
              BitmapImageToBitmap(large_icon), w, h));
        }


        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(
          Image image,
          int width,
          int height)
        {
            var destRect = new System.Drawing.Rectangle(
              0, 0, width, height);

            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution,
              image.VerticalResolution);

            using (var g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, destRect, 0, 0, image.Width,
                      image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

    }


}
