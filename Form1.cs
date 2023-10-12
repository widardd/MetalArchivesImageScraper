using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace MetalArchivesImageScraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }       


        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;                    

                    // Process the selected folders here.
                    ProcessFolders(selectedFolder);
                }
            }
        }

        private void ProcessFolders(string folderPath)
        {
            string[] bandFolders = Directory.GetDirectories(folderPath);

            foreach (string bandFolder in bandFolders)
            {
                string bandName = Path.GetFileName(bandFolder);
                string bandUrl = "https://www.metal-archives.com/bands/" + bandName.Replace(' ', '_');
                string logoImageUrl = "";
                string photoImageUrl = "";

                // Update feedback labels with the current folder being processed.
                folderStatusLabel.Text = $"Processing: {bandName}";
                folderStatusLabel.Refresh();

                // Check if logo.jpg and default.jpg already exist.
                if (File.Exists(Path.Combine(bandFolder, "logo.jpg")) && File.Exists(Path.Combine(bandFolder, "default.jpg")))
                {
                    // Update feedback textbox with a message.
                    feedbackTextBox.AppendText($"Skipped existing images for '{bandName}'.{Environment.NewLine}");
                    continue;
                }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string bandPageHtml = client.DownloadString(bandUrl);

                        // Check if the page contains "may refer to" indicating multiple bands.
                        if (bandPageHtml.Contains("may refer to"))
                        {
                            var choices = GetBandChoices(bandPageHtml);
                            if (choices.Count > 0)
                            {
                                // Prompt the user to select a band choice.
                                var selectedChoice = ShowBandChoiceDialog(bandName, choices);
                                if (!string.IsNullOrEmpty(selectedChoice))
                                {
                                    bandUrl = selectedChoice;
                                }
                                else
                                {
                                    feedbackTextBox.AppendText($"Skipped processing '{bandName}' due to user choice.{Environment.NewLine}");
                                    continue;
                                }
                            }
                        }

                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(bandPageHtml);

                        var logoNode = doc.DocumentNode.SelectSingleNode("//div[@id='band_sidebar']//div[@class='band_name_img']//img");
                        var photoNode = doc.DocumentNode.SelectSingleNode("//div[@id='band_sidebar']//div[@class='band_img']//img");

                        if (logoNode != null)
                        {
                            logoImageUrl = logoNode.GetAttributeValue("src", "");
                            SaveImage(logoImageUrl, Path.Combine(bandFolder, "logo.jpg"));
                            feedbackTextBox.AppendText($"Downloaded logo image for '{bandName}'.{Environment.NewLine}");
                        }

                        if (photoNode != null)
                        {
                            photoImageUrl = photoNode.GetAttributeValue("src", "");
                            SaveImage(photoImageUrl, Path.Combine(bandFolder, "default.jpg"));
                            feedbackTextBox.AppendText($"Downloaded photo image for '{bandName}'.{Environment.NewLine}");
                        }
                    }

                    // Introduce a delay to avoid making requests too quickly.
                    Thread.Sleep(2000); // Delay for 2 seconds (you can adjust the duration as needed).
                }
                catch (Exception ex)
                {
                    LogError(bandName, ex.Message);
                    feedbackTextBox.AppendText($"Error processing '{bandName}': {ex.Message}{Environment.NewLine}");
                }
            }

            // Clear the folder processing status label.
            folderStatusLabel.Text = string.Empty;
        }



        private List<string> GetBandChoices(string bandPageHtml)
        {
            List<string> choices = new List<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(bandPageHtml);

            var choiceNodes = doc.DocumentNode.SelectNodes("//ul/li/a[contains(@href, '/bands/')]");
            if (choiceNodes != null)
            {
                foreach (var choiceNode in choiceNodes)
                {
                    choices.Add(choiceNode.GetAttributeValue("href", ""));
                }
            }

            return choices;
        }

        private string ShowBandChoiceDialog(string bandName, List<string> choices)
        {
            string selectedChoice = null;
            using (var choiceForm = new BandChoiceForm(bandName, choices))
            {
                if (choiceForm.ShowDialog() == DialogResult.OK)
                {
                    selectedChoice = choiceForm.SelectedChoice;
                }
            }
            return selectedChoice;
        }

        private void SaveImage(string imageUrl, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(imageUrl, filePath);
            }
        }

        private void LogError(string bandName, string errorMessage)
        {
            string errorLog = $"Error for band '{bandName}': {errorMessage}";
            File.AppendAllText("error.txt", errorLog + Environment.NewLine);
        }
        // Rest of your code...
    }
}


