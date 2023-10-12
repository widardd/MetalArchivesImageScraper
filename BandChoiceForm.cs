using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetalArchivesImageScraper
{
    public partial class BandChoiceForm : Form
    {
        public string SelectedChoice { get; private set; }
        public string SelectedURL { get; private set; }

        public BandChoiceForm(string bandName, List<string> choices)
        {
            InitializeComponent();

            // Populate the list of band choices in your ListBox or other control.
            listBoxChoices.Items.AddRange(choices.ToArray());

            // Set the form's title or label based on the bandName.
            Text = "Choose Band for: " + bandName;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            // Retrieve the user's selection when they click the "Select" or "OK" button.
            if (listBoxChoices.SelectedItem != null)
            {
                SelectedChoice = listBoxChoices.SelectedItem.ToString();
                DialogResult = DialogResult.OK;               
                Close();
            }
        }
    }
}
