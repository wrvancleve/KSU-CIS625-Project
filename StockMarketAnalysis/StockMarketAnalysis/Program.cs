using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMarketAnalysis
{
    
    /// <summary>
    /// Delegate for the method that applys criteria sets to the data to be processed.
    /// </summary>
    /// <param name="directories">List containing the paths to the criteria set and data</param>
    public delegate bool DataProcessing(List<string> directories, bool isNew);

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainWindowController mwc = new MainWindowController();
            MainWindow mw = new MainWindow(mwc.Handle);
            mwc.AttachView(mw);

            Application.Run(mw);
        }
    }
}