using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StockMarketAnalysis
{
    public class MainWindowController
    {
        /// <summary>
        /// Handle to the MainWindow which is the view for this controller.
        /// </summary>
        private IWindow _view;

        private string _criteriaSetPath;

        private string _dataSetPath;

        private List<CriteriaSet> _criteriaSets = new List<CriteriaSet>();

        /// <summary>
        /// Queue to store our raw_data taken from pre-filtering;
        /// Each index of the queue represents a row in the database;
        /// Each index of the list represents a different column in each respective row.
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
        /// <param name="directory">2 strings: Criteria set path, Raw data path</param>
        public void Handle(List<string> directories)
        {
            _criteriaSetPath = directories[0];
            _dataSetPath = directories[1];
            StoreCriteriaSet();
        }

        /// <summary>
        /// stores the appropriate criteria set into the list of strings, _criteriaSet
        /// </summary>
        private void StoreCriteriaSet()
        {
            FileStream fs = new FileStream(_criteriaSetPath, FileMode.Open, FileAccess.Read);
            string line = "";
            using (StreamReader sr = new StreamReader(fs))
            {
                List<string> set;
                
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace(" ", string.Empty);

                    if (line == "[")
                    {
                        set = new List<string>();

                        for (int i = 0; i < 3; i++)
                        {
                            set.Add(sr.ReadLine());
                        }
                        CriteriaSet temp_set = new CriteriaSet(set);
                        _criteriaSets.Add(temp_set);
                    }
                }
            }
        }

    }
}