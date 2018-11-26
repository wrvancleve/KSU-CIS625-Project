using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMarketAnalysis
{
    public partial class MainWindow : Form, IWindow
    {

        private SendDirectories _sendDirectories;

        public MainWindow(SendDirectories sendDirectories)
        {
            InitializeComponent();
            _sendDirectories = sendDirectories;
        }

        private void uxButtonHandler(object sender, EventArgs e)
        {
            string button = ((Button)sender).Name; // Get button
           
           

            if (button.Equals("uxButtonCriteriaSet"))
            {

            }
            else if (button.Equals("uxButtonData"))
            {
                FolderBrowserDialog setFinder = new FolderBrowserDialog();
                DialogResult result = setFinder.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                uxTextBoxData.Text = setFinder.SelectedPath;
                setFinder.SelectedPath = string.Empty;
            
            }
        }
    }
}