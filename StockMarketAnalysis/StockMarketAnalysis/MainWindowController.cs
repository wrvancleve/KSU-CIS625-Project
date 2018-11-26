using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAnalysis
{
    public class MainWindowController
    {
        /// <summary>
        /// Handle to the MainWindow which is the view for this controller.
        /// </summary>
        private IWindow _view;

        /// <summary>
        /// Queue to store our raw_data taken from pre-filtering
        /// </summary>
        private Queue<List<string>> _data = new Queue<List<string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendDirectories"></param>
        public MainWindowController()
        {

        }

        public void AttachView(IWindow view)
        {
            _view = view;
        }

        /// <summary>
        /// Handles all function
        /// </summary>
        /// <param name="directories"></param>
        public void Handle(List<string> directories)
        {
            ReadFiles(directories);
        }

        /// <summary>
        /// Handles reading of the file content from the list of directories
        /// </summary>
        /// <param name="files"></param>
        private void ReadFiles(List<string> directories)
        {
                            
        }
    }
}