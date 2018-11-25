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

        public MainWindowController()
        {

        }

        public void AttachView(IWindow view)
        {
            _view = view;
        }
    }
}