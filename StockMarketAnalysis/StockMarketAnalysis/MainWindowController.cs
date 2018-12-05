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
using System.Data.SqlClient;
using System.Data;

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
        /// Connection string for sql server
        /// </summary>
        private const string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=U:\cis625\StockMarketAnalysis\StockMarketAnalysis\Database.mdf;Integrated Security=True";

        /// <summary>
        /// Regex for determining if a line is a comment line
        /// </summary>
        private static readonly Regex RegexCommentLine = new Regex(@"^\-\-");

        /// <summary>
        /// Regex for determining if a line is an operation line
        /// </summary>
        private static readonly Regex RegexOperationLine = new Regex(@"^[!@^#*+$&]");

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
            bool success = ObtainCriteriaSets() && ProcessData();
            return success;
        }

        /// <summary>
        /// Obtains the criteria sets from the criteria set file
        /// </summary>
        /// <returns></returns>
        private bool ObtainCriteriaSets()
        {
            try
            {
                /* Parsing variables */
                char previousOperation = '\0';
                char currentOperation = '\0';

                /* Criteria set creation variables */
                int count = 0;
                string currentPreFilterColumn = string.Empty;
                string currentPreFilterComparison = string.Empty;
                List<string> currentPreFilterValues = new List<string>();
                List<Tuple<string, string, List<string>>> preFilters = new List<Tuple<string, string, List<string>>>();
                List<string> currentAggregationColumns = new List<string>();
                List<string> currentAggregationSums = new List<string>();
                string currentPostFilterColumn = string.Empty;
                string currentPostFilterComparison = string.Empty;
                List<string> currentPostFilterValues = new List<string>();
                CriteriaSet criteria = new CriteriaSet();

                foreach (string line in File.ReadLines(_criteriaSetPath))
                {
                    if (!RegexCommentLine.IsMatch(line) && RegexOperationLine.IsMatch(line))
                    {
                        currentOperation = line[0]; // Get current operation

                        if (currentOperation == '!')
                        {
                            if (count > 0)
                            {
                                criteria.PostFilter = new Tuple<string, string, List<string>>(currentPostFilterColumn, currentPostFilterComparison, currentPostFilterValues); // Store Post Filter
                                _criteriaSets.Add(criteria); // Add previous criteria set
                            }

                            /* Start criteria set */
                            criteria = new CriteriaSet();
                            criteria.Name = (line.Contains(":")) ? (line.Substring((line.IndexOf(':') + 1)).Trim()) : (line.Substring(1));
                            criteria.Number = ++count;

                            /* Setup/Reset Variables */
                            currentPreFilterColumn = string.Empty;
                            currentPreFilterComparison = string.Empty;
                            currentPreFilterValues = new List<string>();
                            preFilters = new List<Tuple<string, string, List<string>>>();
                            currentAggregationColumns = new List<string>();
                            currentAggregationSums = new List<string>();
                            currentPostFilterColumn = string.Empty;
                            currentPostFilterComparison = string.Empty;
                            currentPostFilterValues = new List<string>();
                        }
                        else if (currentOperation == '@')
                        {
                            if (previousOperation != '!')
                            {
                                preFilters.Add(new Tuple<string, string, List<string>>(currentPreFilterColumn, currentPreFilterComparison, currentPreFilterValues)); // Add previous filter
                                currentPreFilterColumn = string.Empty;
                                currentPreFilterComparison = string.Empty;
                                currentPreFilterValues = new List<string>();
                            }
                            currentPreFilterColumn = line.Substring(1); // Save Pre Filter Column
                        }
                        else if (currentOperation == '^')
                        {
                            currentPreFilterComparison = line.Substring(1); // Save Pre Filter Comparison
                        }
                        else if (currentOperation == '*')
                        {
                            if (previousOperation != '*')
                            {
                                preFilters.Add(new Tuple<string, string, List<string>>(currentPreFilterColumn, currentPreFilterComparison, currentPreFilterValues)); // Add last filter
                                criteria.PreFilters = preFilters; // Store last Pre Filter
                            }
                            currentAggregationColumns.Add(line.Substring(1)); // Save Aggregated Column
                        }
                        else if (currentOperation == '+')
                        {
                            if (previousOperation != '+')
                            {
                                criteria.AggregationColumns = currentAggregationColumns; // Store Aggregated Columns
                            }
                            currentAggregationSums.Add(line.Substring(1)); // Save Aggregated Sum
                        }
                        else if (currentOperation == '$')
                        {
                            criteria.AggregationSums = currentAggregationSums; // Store Aggregated Sums
                            currentPostFilterColumn = line.Substring(1); // Save Post Filter Column
                        }
                        else if (currentOperation == '&')
                        {
                            currentPostFilterComparison = line.Substring(1); // Save Post Filter Comparison
                        }
                        else
                        {
                            if (previousOperation == '^')
                            {
                                currentPreFilterValues.Add(line.Substring(1)); // Save Pre Filter Value
                            }
                            else
                            {
                                currentPostFilterValues.Add(line.Substring(1)); // Save Post Filter Value
                            }
                        }

                        if (currentOperation != '#')
                        {
                            previousOperation = currentOperation;
                        }
                    }
                }

                criteria.PostFilter = new Tuple<string, string, List<string>>(currentPostFilterColumn, currentPostFilterComparison, currentPostFilterValues); // Store Post Filter
                _criteriaSets.Add(criteria);

                return true;
            }
            catch
            {
                MessageBox.Show("Error while reading in criteria sets. Please check your file and try again.", "Criteria Set Read Error");
                return false;
            }
        }

        /// <summary>
        /// Obtains the data from the data files.
        /// </summary>
        /// <returns>Whether the data was obtained successfully</returns>
        private bool ProcessData()
        {
            /* Get list of files */
            List<string> files = new List<string>();
            string [] directories = Directory.GetFiles(_dataSetPath);
            foreach (string directory in directories)
            {
                files.Add(directory);
            }
            files = files.OrderBy(x => Regex.Replace(x, @"\d+", match => match.Value.PadLeft(10, '0'))).ToList();

            /*
            foreach (string file in files)
            {
                
            }
            */



            Parallel.ForEach (
                File.ReadLines(files[0]),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    SqlCommand cmd = new SqlCommand("StockData.InsertRawData", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    return new { Conn = conn, Cmd = cmd };
                },
                (line, unused, local) =>
                {
                    if (!line.Contains("stockcode") && !String.IsNullOrWhiteSpace(line))
                    {
                        string[] values = line.Split(',');

                        for (int i = 0; i < values.Length; i++)
                        {
                            if (i == 0)
                                local.Cmd.Parameters.AddWithValue("StockCode", values[i]);
                            else if (i == 1)
                                local.Cmd.Parameters.AddWithValue("StockType", values[i]);
                            else if (i == 2)
                                local.Cmd.Parameters.AddWithValue("HolderId", values[i]);
                            else if (i == 3)
                                local.Cmd.Parameters.AddWithValue("HolderCountry", values[i]);
                            else if (i == 4)
                                local.Cmd.Parameters.AddWithValue("SharesHeld", Convert.ToDecimal(values[i]));
                            else if (i == 5)
                                local.Cmd.Parameters.AddWithValue("PercentageSharesHeld", Convert.ToDecimal(values[i]));
                            else if (i == 6)
                                local.Cmd.Parameters.AddWithValue("Direction", values[i]);
                            else
                                local.Cmd.Parameters.AddWithValue("Value", Convert.ToDecimal(values[i]));
                        }

                        local.Cmd.ExecuteNonQuery();
                        local.Cmd.Parameters.Clear();
                    }

                    return local;
                },
                (conn) =>
                {
                    conn.Cmd.Dispose();
                    conn.Conn.Dispose();
                }
            );

            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    SqlCommand cmd = new SqlCommand("StockData.GetPrefilterData", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    return new { Conn = conn, Cmd = cmd };
                },
                (criteria, unused, local) =>
                {
                    local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);

                    string countries = "null";
                    string stockType = "null";
                    string direction = "null";
                    foreach (Tuple<string, string, List<string>> preFilter in criteria.PreFilters)
                    {
                        if (preFilter.Item1.Equals("holdercountry"))
                        {
                            countries = String.Join(",", preFilter.Item3);
                        }
                        else if (preFilter.Item1.Equals("stocktype"))
                        {
                            if (preFilter.Item2.Equals("<>"))
                            {
                                stockType = preFilter.Item3[0].Equals("Preferred") ? "Common" : "Preferred";
                            }
                            else
                            {
                                stockType = preFilter.Item3[0];
                            }
                        }
                        else
                        {
                            if (preFilter.Item2.Equals("<>"))
                            {
                                direction = preFilter.Item3[0].Equals("Long") ? "Short" : "Long";
                            }
                            else
                            {
                                direction = preFilter.Item3[0];
                            }
                        }
                    }
                    local.Cmd.Parameters.AddWithValue("HolderCountries", countries);
                    local.Cmd.Parameters.AddWithValue("StockType", stockType);
                    local.Cmd.Parameters.AddWithValue("Direction", direction);

                    local.Cmd.ExecuteNonQuery();
                    local.Cmd.Parameters.Clear();

                    return local;
                },
                (conn) =>
                {
                    conn.Cmd.Dispose();
                    conn.Conn.Dispose();
                }
            );

            return true;
        }
    }
}