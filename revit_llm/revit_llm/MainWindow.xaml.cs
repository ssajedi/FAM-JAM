using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace revit_llm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly string apiKey = "sk-sKxY02_wnlwpxEOsjbUVWAmTAoODmzVmoliM84KNGgT3BlbkFJtouSBzoVbhNLnVfFfFc1pB4yiwA9ygoaIuXQlSusUA";
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";


        public MainWindow()
        {
            InitializeComponent();
            GetChatGPTResponseAsync();
        }

        private async Task GetChatGPTResponseAsync()
        {
            string prompt = "What is the capital of France?";

            // Assuming you have a method GetChatGPTResponse to fetch the result.
            string result = await GetChatGPTResponse(prompt);

            // Output the result
            Console.WriteLine(result);
        }

        public static async Task<string> GetChatGPTResponse(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4o",  // Or "gpt-3.5-turbo" depending on your model choice
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
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
