using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using BenchmarkLibrary;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BenchmarkClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            if (fileDialog.ShowDialog() == true)
                FileTextBlock.Text = fileDialog.FileName;
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var fileContents = File.ReadAllText(FileTextBlock.Text);
            var jsonContents = JsonConvert.DeserializeObject<List<BenchmarkSettings>>(fileContents);
            var url = HostUrlTextBlock.Text;
            //url = "http://0f3ba8f5f87d4dde91df72b249e36ca5.cloudapp.net/api/benchmark";
            

            RunButton.IsEnabled = false;
            ProgressBar.Maximum = jsonContents.Count;
            ProgressBar.Value = 0;
            foreach (var benchmarkRequest in jsonContents)
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(60);

                var content = new StringContent(JsonConvert.SerializeObject(benchmarkRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("text/json");
                try
                {
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    ResponseTextBox.Text += responseString;
                }
                catch(Exception ex)
                {
                    ResponseTextBox.Text += string.Format("Error: {0}", ex.Message);
                }

                ProgressBar.Value++;
            }

            RunButton.IsEnabled = true;
        }
    }
}
