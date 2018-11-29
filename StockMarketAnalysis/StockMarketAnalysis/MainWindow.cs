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
        /// <summary>
        /// Handle to the MainWindowController send directories handle.
        /// </summary>
        private DataProcessing _process;

        public MainWindow(DataProcessing process)
        {
            InitializeComponent();
            _process = process;
        }

        /// <summary>
        /// Handles the event where the user presses a button on the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxButtonHandler(object sender, EventArgs e)
        {
            string button = ((Button)sender).Name; // Get button

            if (button.Equals("uxButtonCriteriaSet"))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                DialogResult result = openFileDialog.ShowDialog();

                if (result != DialogResult.OK) return;

                uxTextBoxCriteriaSet.Text = openFileDialog.FileName;
                openFileDialog.FileName = string.Empty;
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
            else
            {
                List<string> directories = new List<string>();
                directories.Add(uxTextBoxCriteriaSet.Text);
                directories.Add(uxTextBoxData.Text);
                if(_process(directories))
                {
                    MessageBox.Show("Program complete.");
                }
            }

            uxButtonGo.Enabled = (uxTextBoxCriteriaSet.TextLength > 0 && uxTextBoxData.TextLength > 0); // Check for enable go button
        }
    }
}