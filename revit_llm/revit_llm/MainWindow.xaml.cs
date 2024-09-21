using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Files;

namespace revit_llm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";


        public MainWindow()
        {
            InitializeComponent();
            GetChatGPTResponseAsync();
        }

        static string EncodeImage(string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        string ReadKeyFromText()
        {
            // Define the relative path to the secret.txt file
            string relativePath = Path.Combine(@"..\..\..\", "secret.txt");

            string absolutePath = Path.GetFullPath(relativePath);

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


        private string OpenImageFileDialog()
        {
            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter for file types (all common image formats)
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.ico|All Files|*.*";

            // Set a title for the dialog
            openFileDialog.Title = "Select an Image File";

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file's path
                string filePath = openFileDialog.FileName;

                return filePath;

            }
            else
            {
                return "";

            }
        }


        private async Task GetChatGPTResponseAsync()
        {
            string prompt = "What is the capital of France?";

            // Assuming you have a method GetChatGPTResponse to fetch the result.
            string key = ReadKeyFromText();

            string filePath = OpenImageFileDialog();

            if (!string.IsNullOrEmpty(filePath))
            {
                string result = await GetChatGPTResponse(prompt, ReadKeyFromText(), filePath);

                Console.WriteLine(result);
            }


        }

        public static async Task<string> GetChatGPTResponse(string prompt, string apiKey, string imagePath)
        {


            // Getting the base64 string of the image
            string base64Image = EncodeImage(imagePath);

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
                            content = new object[]
                            {
                                new
                                {
                                    type = "text",
                                    text = "Can you extract data such as Width, Height, Length, as Product Information in a tabular format that can be downloaded as excel"
                                },
                                new
                                {
                                    type = "image_url",
                                    image_url = new
                                    {
                                        url = $"data:image/jpeg;base64,{base64Image}"
                                    }
                                }
                            }
                        }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

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


    }



}
