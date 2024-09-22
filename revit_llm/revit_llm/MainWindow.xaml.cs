using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Newtonsoft.Json;

using Ghostscript.NET.Rasterizer;
using ImageMagick;
using revit_llm.Properties;


namespace revit_llm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
        public bool IsClosed { get; private set; }

        string RootFolder
        {
            get
            {
                return txtRootFolder.Text;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        UIApplication _application;

        FamJamManager _famJamManager;

        Autodesk.Revit.DB.Document _doc
        {
            get
            {
                return _application.ActiveUIDocument.Document;
            }
        }

        public MainWindow()
        {
            var furniture = ToFurniture(jsonString);

            if (furniture != null)
            {
                var ii = furniture.W;
            }

            //string name = GetStringNames();

        }

        public MainWindow(UIApplication application, UIControlledApplication controlledApplication)
        {
            //MessageBox.Show("HERE 1");
            InitializeComponent();
            //MessageBox.Show("hERE");
            _application = application;
            //MessageBox.Show("hERE1");
            _famJamManager = new FamJamManager(application, _doc, txtRootFolder.Text.Trim());
        }

        static string EncodeImage(string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        string ReadKeyFromText()
        {
            // Define the relative path to the secret.txt file
            string relativePath = RootFolder + @"\secret.txt";

            try
            {
                // Read the contents of the file
                string secretContent = System.IO.File.ReadAllText(relativePath);

                // Output the content to the console
                Console.WriteLine("Content of secret.txt:");
                Console.WriteLine(secretContent);

                return secretContent;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found: Make sure the file exists at the specified path.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return "";

        }


        //private string OpenImageFileDialog()
        //{
        //    // Create an instance of OpenFileDialog
        //    OpenFileDialog openFileDialog = new OpenFileDialog();

        //    // Set filter for file types (all common image formats)
        //    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.ico|All Files|*.*";

        //    // Set a title for the dialog
        //    openFileDialog.Title = "Select an Image File";

        //    // Show the dialog and check if the user selected a file
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        // Get the selected file's path
        //        string filePath = openFileDialog.FileName;

        //        return filePath;

        //    }
        //    else
        //    {
        //        return "";

        //    }
        //}


        private async Task GetChatGPTResponseAsync(string rootFolder)
        {
            string prompt = "What is the capital of France?";

            // Assuming you have a method GetChatGPTResponse to fetch the result.
            string key = ReadKeyFromText();

            //string filePath = OpenImageFileDialog();

            var images = PDFToImage();

            if (images.Count > 0)
            {
                List<Furniture> furnitures = new List<Furniture>();
                foreach (var image in images)
                {
                    string result = await GetChatGPTResponse(prompt, ReadKeyFromText(), new List<string>() { image }, rootFolder);
                    var furniture = ToFurniture(result);
                    Console.WriteLine("Current JSON Furniture: " + result);
                    if (furniture == null)
                    {
                        Console.WriteLine($"Furniture is null!!!!!!!!!");
                    }
                    else
                    {
                        furnitures.Add(furniture);
                        Console.WriteLine($"Furniture okay");
                    }
                    //MessageBox.Show(result);
                }

                _famJamManager.Furnitures.Clear();

                _famJamManager.Furnitures.AddRange(furnitures);
                MessageBox.Show("HERE");
                _famJamManager._event.Raise();

            }



        }

        public static async Task<string> GetChatGPTResponse(string prompt, string apiKey, List<string> images, string rootFolder)
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4o",  // Or "gpt-3.5-turbo" depending on your model choice
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = CreateContent(images,rootFolder)
                        }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                Console.WriteLine("This is the content: " + content.ToString());
                string jsonString = await content.ReadAsStringAsync();
                Console.WriteLine(jsonString);
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                    return responseObject.choices[0].message.content.ToString();
                }
                else
                {
                    return $"Error: {response.StatusCode} - {responseString}";
                }
            }
        }


        List<string> PDFToImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();


            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            openFileDialog.Title = "Select a PDF file";

            if (openFileDialog.ShowDialog() == true)
            {

                List<string> imagines = new List<string>();

                string pdfPath = openFileDialog.FileName;
                string outputFolder = System.IO.Path.GetDirectoryName(pdfPath);

                using (var document = PdfiumViewer.PdfDocument.Load(pdfPath))
                {
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        using (var image = document.Render(i, 2000, 2000, true)) // 300 DPI
                        {
                            string outputPath = System.IO.Path.Combine(outputFolder, $"page_{i + 1}.png");
                            image.Save(outputPath, ImageFormat.Png);
                            imagines.Add(EncodeImage(outputPath));
                        }
                    }
                }

                //using (var rasterizer = new GhostscriptRasterizer())
                //{
                //    rasterizer.Open(pdfPath);

                //    for (int i = 1; i <= rasterizer.PageCount; i++)
                //    {
                //        // Render the page to an image
                //        using (var image = rasterizer.GetPage(300, i)) // 300 DPI
                //        {
                //            string outputPath = System.IO.Path.Combine(outputFolder, $"page_{i}.png");
                //            image.Save(outputPath, ImageFormat.Png); // Save as PNG or any desired format
                //            imagines.Add(EncodeImage(outputPath));
                //            Console.WriteLine($"Saved: {outputPath}");
                //        }
                //    }
                //}


                //MagickReadSettings settings = new MagickReadSettings()
                //{
                //    Density = new Density(300) // Set the resolution (DPI)
                //};

                //using (MagickImageCollection images = new MagickImageCollection())
                //{
                //    images.Read(pdfPath, settings);

                //    Loop through pages
                //    int page = 1;
                //    foreach (MagickImage image in images)
                //    {
                //        Save each page as a PNG image
                //        image.Write(outputFolder + $"_Page{page}.png");
                //        page++;
                //    }
                //}

                return imagines;
                //MessageBox.Show("PDF pages converted to images.");
            }

            return new List<string>();

        }


        static object[] CreateContent(List<string> images, string rootFolder)
        {
            var content = new object[images.Count + 1];

            content[0] = new
            {
                type = "text",
                text = GetTextCommand(rootFolder)
            };

            for (int i = 1; i <= images.Count; i++)
            {
                content[i] = new
                {
                    type = "image_url",
                    image_url = new
                    {
                        url = $"data:image/jpeg;base64,{images[i - 1]}"
                    }

                };
            }
            Console.WriteLine(content);
            return content;

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                await GetChatGPTResponseAsync(txtRootFolder.Text.Trim());

                //AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }

            //try
            //{
            //    GetStringNames(txtRootFolder.Text.Trim());
            //    var furniture = ToFurniture(jsonString);
            //    _famJamManager.Furnitures.Clear();
            //    _famJamManager.Furnitures.Add(furniture);
            //    _famJamManager._event.Raise();
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.Message + ex.StackTrace);
            //}

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            MessageBox.Show(args.Name);


            if (args.Name.Contains("pdfium"))
            {

                //Load my Assembly 
                Assembly assem = Assembly.LoadFrom(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + "\\pdfium.dll");
                MessageBox.Show(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + "\\pdfium.dll");
                
                if (assem != null)
                    return assem;
            }
            return Assembly.GetExecutingAssembly();

        }
        static string EncodeImage(Image image)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the image to the MemoryStream in PNG format (or any other format you prefer)
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                // Convert the MemoryStream to a byte array
                byte[] imageBytes = memoryStream.ToArray();

                // Convert byte array to Base64 string
                return Convert.ToBase64String(imageBytes);
            }
        }

        static string GetTextCommand(string rootFolder)
        {
            string textCommand = "You are a highly efficient assistant designed to extract structured data from master specification sheets.\r\nThe user will provide one or more pages of a specification sheet by uploading them as images. Your task is to generate a JSON object from this information with the following structure:\r\n\t• \"type\": This value should be selected from the provided list of types:" + GetStringNames(rootFolder) + "\r\n\t\t○ …\r\n\t• \"L\": The length of the object (in inches).\r\n\t• \"W\": The width of the object (in inches).\r\n\t• \"H\": The height of the object (in inches).\r\nImportant Notes:\r\n\t1. Unit Conversion: If the length (L), width (W), or height (H) are presented in units other than inches, convert them to inches before including them in the JSON.\r\n\t2. Ensure accurate and consistent extraction of data from the specification sheet, especially for the dimensional values.\r\nPlease only output a json string starting with { and ending with } for Example:\r\n{\r\n  \"type\": \"window\",\r\n  \"L\": 20,\r\n  \"W\": 15,\r\n  \"H\": 40\r\n}\r\n\r\n";
            return textCommand;
        }

        static string GetStringNames(string rootFolder)
        {
            string relativeFolder = Path.Combine(rootFolder + @"\database\revit_reference_geometry");
            string[] rfaFiles = Directory.GetFiles(relativeFolder, "*.rfa");
            string name = "";
            foreach (string s in rfaFiles)
            {
                name += "\r\n\t\t○ " + Path.GetFileName(s).Replace(".rfa", "");
            }

            return name;
        }

        static Furniture ToFurniture(string input)
        {

            // Regular expression to extract the JSON part
            string pattern = @"\{[\s\S]*?\}";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string jsonContent = match.Value;
                Console.WriteLine("Extracted JSON: " + jsonContent);

                // Deserialize the JSON into a C# object
                try
                {
                    return JsonConvert.DeserializeObject<Furniture>(jsonContent);

                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Error deserializing JSON: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No JSON found in the string.");
            }

            return null;
        }

        string jsonString = "```json\r\n{\r\n  \"type\": \"chair\",\r\n  \"L\": 31.5,\r\n  \"W\": 31.5,\r\n  \"H\": 28.25\r\n}\r\n```";

        public static void ConvertPdfToImages(string pdfFilePath, string outputFolder)
        {
            // Ensure the output folder exists
            System.IO.Directory.CreateDirectory(outputFolder);

            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(pdfFilePath);

                for (int i = 1; i <= rasterizer.PageCount; i++)
                {
                    // Render the page to an image
                    using (var image = rasterizer.GetPage(300, i)) // 300 DPI
                    {
                        string outputPath = System.IO.Path.Combine(outputFolder, $"page_{i}.png");
                        image.Save(outputPath, ImageFormat.Png); // Save as PNG or any desired format
                        Console.WriteLine($"Saved: {outputPath}");
                    }
                }
            }
        }

        public void ConvertPdfToImage(string inputPdfPath, string outputImagePath)
        {
            // Define read settings for PDF
            MagickReadSettings settings = new MagickReadSettings()
            {
                Density = new Density(300) // Set the resolution (DPI)
            };

            // Read PDF file
            using (MagickImageCollection images = new MagickImageCollection())
            {
                images.Read(inputPdfPath, settings);

                // Loop through pages
                int page = 1;
                foreach (MagickImage image in images)
                {
                    // Save each page as a PNG image
                    image.Write(outputImagePath + $"_Page{page}.png");
                    page++;
                }
            }
        }

    }


    public class Furniture
    {
        public string Type { get; set; }
        public double L { get; set; }
        public double W { get; set; }
        public double H { get; set; }
    }



}
