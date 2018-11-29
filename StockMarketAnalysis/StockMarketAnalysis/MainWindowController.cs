using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace StockMarketAnalysis
{
    public enum Operation
    {
        PreFilter,
        Aggregation,
        PostFilter
    }

    public class MainWindowController
    {
        /// <summary>
        /// Handle to the MainWindow which is the view for this controller.
        /// </summary>
        private IWindow _view;

        /// <summary>
        /// File path for the criteria set.
        /// </summary>
        private string _criteriaSetPath;

        /// <summary>
        /// File path for the data.
        /// </summary>
        private string _dataSetPath;

        /// <summary>
        /// List of criteria sets.
        /// </summary>
        private List<CriteriaSet> _criteriaSets = new List<CriteriaSet>();

        /// <summary>
        /// Queue to store our raw_data taken from pre-filtering;
        /// Each index of the queue represents a row in the database;
        /// Each index of the list represents a different column in each respective row.
        /// </summary>
        private Queue<Tuple<List<string>, Operation>> _data = new Queue<Tuple<List<string>, Operation>>();

        /// <summary>
        /// Constructs the controller.
        /// </summary>
        public MainWindowController()
        {

        }

        /// <summary>
        /// Attaches the view to its controller.
        /// </summary>
        /// <param name="view">The view to attach</param>
        public void AttachView(IWindow view)
        {
            _view = view;
        }

        /// <summary>
        /// Delegate for the method that applys criteria sets to the data to be processed.
        /// </summary>
        /// <param name="directories">List containing the paths to the criteria set and data</param>
        /// <returns>Whether the data was processed correctly</returns>
        public bool Handle(List<string> directories)
        {
            _criteriaSetPath = directories[0];
            _dataSetPath = directories[1];
            bool success = ObtainCriteriaSets(); //&& ObtainData();
            return success;
        }

        /// <summary>
        /// Obtains the criteria sets from the criteria set file
        /// </summary>
        /// <returns></returns>
        private bool ObtainCriteriaSets()
        {
            FileStream fs = new FileStream(_criteriaSetPath, FileMode.Open, FileAccess.Read);
            string line = "";
            using (StreamReader sr = new StreamReader(fs))
            {
                List<string> set;
                int lineNumber = 0;
                int criteriaLineNumber;
                
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace(" ", string.Empty);

                    if (line == "[")
                    {
                        criteriaLineNumber = lineNumber;
                        set = new List<string>();

                        while (!(line = sr.ReadLine()).Contains("]"))
                        {
                            set.Add(CleanLine(line)); // Add clean line to set
                        }

                        if (set.Count != 3)
                        {
                            MessageBox.Show("The criteria set outlined on line " + (criteriaLineNumber + 1) + " contains too many or too few lines.", "Criteria Set Error");
                            return false;
                        }

                        if (VerifyCriteriaSetFormat(set, (criteriaLineNumber + 1)))
                        {
                            CriteriaSet criteria = new CriteriaSet(set);
                            _criteriaSets.Add(criteria);
                            lineNumber += 5;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        lineNumber++;
                    }
                }
            }

            if (_criteriaSets.Count > 0)
            {
                return true;
            }
            else
            {
                MessageBox.Show("No criteria sets were found. Check the file or try a new one.", "Criteria Set Error");
                return false;
            }
        }

        /// <summary>
        /// Obtains the data from the data files.
        /// </summary>
        /// <returns>Whether the data was obtained successfully</returns>
        private bool ObtainData()
        {
            int threadCount = Process.GetCurrentProcess().Threads.Count;
            ConcurrentBag<string> bag = new ConcurrentBag<string>();
            List<string> data = new List<string>();

            Parallel.ForEach(File.ReadLines(_dataSetPath), line =>
            {
                bag.Add(line);

                // Add line to database
            });

            foreach (string s in bag)
            {
                data.Add(s);
            }

            int size = data.Count / threadCount;
            int remainder = data.Count % threadCount;            

            for (int i = 0; i < threadCount; i++)
            {
                List<string> lines = new List<string>();
                int start = i * size;
                start = ((remainder == 0) ? (start) : ((i < remainder) ? (start + i) : (start + remainder))); // Adjust start postion based on remainders
                int end = start + size;
                end = ((remainder == 0 || i >= remainder) ? (start + size) : (start + size + 1)); // Gets end position from start postion and remainders

                for (int j = start; j < end; j++)
                {
                    lines.Add(data[j]);
                }

                _data.Enqueue(new Tuple<List<string>, Operation>(lines, Operation.PreFilter));
            }

            return true;
        }

        /// <summary>
        /// Takes a line and removes tab characters and unnecessary spaces.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>The cleaned line</returns>
        private string CleanLine(string line)
        {
            line = Regex.Replace(line, @"\s*\,\s*", ","); // Remove unnecessary spaces by ,
            line = Regex.Replace(line, @"\s*\|\s*", "|"); // Remove unnecessary spaces by |
            line = Regex.Replace(line, @"[\t]", string.Empty); // Remove tab characters
            line = line.Trim();

            return line;
        }

        /// <summary>
        /// Check each line of the set to verify formats.
        /// </summary>
        /// <param name="set">List of lines of the set</param>
        /// <param name="criteriaLineNumber">Line number where the criteria set was created</param>
        /// <returns>Whether the criteria set is valid for object creation</returns>
        private bool VerifyCriteriaSetFormat(List<string> set, int criteriaLineNumber)
        {            
            if (!Regex.Match(set[0], @"^[a-zA-Z0-9\s]+(\,[a-zA-Z0-9\s]+)*\|((Preffered)|(Common))\|((Long)|(Short))$").Success)
            {
                MessageBox.Show("Invalid Pre-Filtering Format on Line " + (criteriaLineNumber + 1) + ".", "Criteria Set Error");
                return false;
            }

            if (!Regex.Match(set[1], @"^[a-zA-Z0-9]+(\,[a-zA-Z0-9]+)*\|[a-zA-Z0-9]+(\,[a-zA-Z0-9]+)*$").Success)
            {
                MessageBox.Show("Invalid Aggregation Format on Line " + (criteriaLineNumber + 2) + ".", "Criteria Set Error");
                return false;
            }

            if (!Regex.Match(set[2], @"^[#$%][0-9]+(\,[#$%][0-9]+)*$").Success)
            {
                MessageBox.Show("Invalid Post-Filtering Format on Line " + (criteriaLineNumber + 3) + ".", "Criteria Set Error");
                return false;
            }

            return true;
        }
    }
}