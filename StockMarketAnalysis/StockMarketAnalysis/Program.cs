using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMarketAnalysis
{
    
    /// <summary>
    /// Method for sending the file paths to the controller for different files
    /// </summary>
    /// <param name="files">A dynamic array of file directories/paths</param>
    public delegate void SendDirectories(List<string> directories);

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