﻿using System;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void uxButtonHandler(object sender, EventArgs e)
        {
            string button = ((Button)sender).Name; // Get button

            if (button.Equals("uxButtonCriteriaSet"))
            {

            }
        }
    }
}