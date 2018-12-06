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
        /// Output path for the results.
        /// </summary>
        private string _outputPath;

        /// <summary>
        /// List of criteria sets.
        /// </summary>
        private List<CriteriaSet> _criteriaSets = new List<CriteriaSet>();

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
        public bool Handle(List<string> directories, bool isNew)
        {
            _criteriaSetPath = directories[0];
            _dataSetPath = directories[1];
            _outputPath = directories[2];
            bool success = ObtainCriteriaSets() && ProcessData(isNew);
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
        private bool ProcessData(bool isNew)
        {
            if (isNew)
            {
                ProcessDataNew();
                return true;
            }
            else
            {
                ProcessDataHistory();
                return true;
            }
        }

        private void ProcessDataHistory()
        {
            /* Get list of files */
            List<string> files = new List<string>();
            string[] directories = Directory.GetFiles(_dataSetPath);
            foreach (string directory in directories)
            {
                files.Add(directory);
            }
            files = files.OrderBy(x => Regex.Replace(x, @"\d+", match => match.Value.PadLeft(10, '0'))).ToList();

            foreach (string file in files)
            {
                InitDatabase(); // Clear raw data
                if (files.IndexOf(file) == files.Count - 1)
                {
                    ProcessFile(file); // Process file
                    ProcessPreFilters(true); // Process prefilters with output
                    ProcessAggregations(false, true, false); // Process aggregations with output
                    ProcessPostFilters(); // Process postfilters with output
                }
                else if (files.IndexOf(file) == files.Count - 2)
                {
                    ProcessFile(file); // Process file
                    ProcessPreFilters(false); // Process prefilters
                    ProcessAggregations(true, false, false); // Process aggregations
                }
                else
                {
                    ProcessFile(file); // Process file
                    ProcessPreFilters(false); // Process prefilters
                    ProcessAggregations(false, false, false); // Process aggregations
                }
            }
        }

        private void ProcessDataNew()
        {
            InitDatabase(); // Clear raw data
            ProcessFile(_dataSetPath); // Process new file
            ProcessPreFilters(true); // Process prefilter
            ProcessAggregations(false, true, true); // Process aggregations
            ProcessPostFilters();
        }

        private void InitDatabase()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConnectionString;
                SqlCommand cmd = new SqlCommand("StockData.ClearRawData", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void ProcessFile(string file)
        {
            Parallel.ForEach(
                File.ReadLines(file),
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

                        for (int j = 0; j < values.Length; j++)
                        {
                            if (j == 0)
                                local.Cmd.Parameters.AddWithValue("StockCode", values[j]);
                            else if (j == 1)
                                local.Cmd.Parameters.AddWithValue("StockType", values[j]);
                            else if (j == 2)
                                local.Cmd.Parameters.AddWithValue("HolderId", values[j]);
                            else if (j == 3)
                                local.Cmd.Parameters.AddWithValue("HolderCountry", values[j]);
                            else if (j == 4)
                                local.Cmd.Parameters.AddWithValue("SharesHeld", Convert.ToDecimal(values[j]));
                            else if (j == 5)
                                local.Cmd.Parameters.AddWithValue("PercentageSharesHeld", Convert.ToDecimal(values[j]));
                            else if (j == 6)
                                local.Cmd.Parameters.AddWithValue("Direction", values[j]);
                            else
                                local.Cmd.Parameters.AddWithValue("Value", Convert.ToDecimal(values[j]));
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
        }

        private void ProcessPreFilters(bool isOutput)
        {
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

                    local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                    local.Cmd.Parameters.AddWithValue("HolderCountries", countries);
                    local.Cmd.Parameters.AddWithValue("StockType", stockType);
                    local.Cmd.Parameters.AddWithValue("Direction", direction);
                    local.Cmd.ExecuteNonQuery();
                    local.Cmd.Parameters.Clear();

                    if (isOutput)
                    {
                        string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\PreFilteredData.txt");
                        (new FileInfo(directory)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(directory))
                        {
                            writer.WriteLine("stockcode,stocktype,holderid,holdercountry,sharesheld,percentagesharesheld,direction,value");

                            using (SqlDataReader reader = local.Cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    StringBuilder newLine = new StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i != (reader.FieldCount - 1))
                                        {
                                            newLine.AppendFormat("{0},", Convert.ToString(reader.GetValue(i)));
                                        }
                                        else
                                        {
                                            newLine.AppendFormat("{0}", Convert.ToString(reader.GetValue(i)));
                                        }
                                    }
                                    writer.WriteLine(newLine.ToString());
                                }
                            }
                        }
                    }

                    return local;
                },
                (conn) =>
                {
                    conn.Cmd.Dispose();
                    conn.Conn.Dispose();
                }
            );
        }

        private void ProcessAggregations(bool isPrevious, bool isCurrent, bool isNew)
        {
            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    SqlCommand cmd = new SqlCommand("StockData.GetMaxAggregateData", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    return new { Conn = conn, Cmd = cmd };
                },
                (criteria, unused, local) =>
                {
                    if (isNew)
                    {
                        // Move current table info to previous table info
                        local.Cmd.CommandText = "StockData.MoveAggregateData";
                        local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                        local.Cmd.ExecuteNonQuery();
                        local.Cmd.Parameters.Clear();
                    }

                    if (isPrevious || isCurrent) // Previous Day
                    {
                        local.Cmd.CommandText = isPrevious ? "StockData.GetPreviousAggregateData" : "StockData.GetCurrentAggregateData"; // Current or Previous Procedure
                    }
                    local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                    local.Cmd.Parameters.AddWithValue("AggregateKeys", String.Join("~", criteria.AggregationColumns));
                    local.Cmd.ExecuteNonQuery();
                    local.Cmd.Parameters.Clear();

                    if (isPrevious)
                    {
                        local.Cmd.CommandText = "StockData.GetMaxAggregateData"; // Max Procedure
                        local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                        local.Cmd.Parameters.AddWithValue("AggregateKeys", String.Join("~", criteria.AggregationColumns));
                        local.Cmd.ExecuteNonQuery();
                        local.Cmd.Parameters.Clear();
                    }
                    else if (isCurrent)
                    {
                        string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\AggregatedData.txt");
                        (new FileInfo(directory)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(directory))
                        {
                            writer.WriteLine("aggregatekey,aggregatesharesheld,aggregatepercentagesharesheld,aggregatevalue");

                            using (SqlDataReader reader = local.Cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    StringBuilder newLine = new StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i != (reader.FieldCount - 1))
                                        {
                                            newLine.AppendFormat("{0},", Convert.ToString(reader.GetValue(i)));
                                        }
                                        else
                                        {
                                            newLine.AppendFormat("{0}", Convert.ToString(reader.GetValue(i)));
                                        }
                                    }
                                    writer.WriteLine(newLine.ToString());
                                }
                            }
                        }
                    }

                    return local;
                },
                (conn) =>
                {
                    conn.Cmd.Dispose();
                    conn.Conn.Dispose();
                }
            );
        }

        private void ProcessPostFilters()
        {
            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    SqlCommand cmd = new SqlCommand("StockData.GetPostfilterData", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    return new { Conn = conn, Cmd = cmd };
                },
                (criteria, unused, local) =>
                {
                    // https://stackoverflow.com/questions/22176471/pass-liststring-into-sql-parameter
                    foreach (string value in criteria.PostFilter.Item3)
                    {
                        local.Cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                        local.Cmd.Parameters.AddWithValue("ColumnName", criteria.PostFilter.Item1);
                        local.Cmd.Parameters.AddWithValue("Comparison", criteria.PostFilter.Item2);
                        local.Cmd.Parameters.AddWithValue("Value", value);
                        local.Cmd.ExecuteNonQuery();
                        local.Cmd.Parameters.Clear();
                    }

                    string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\PostFilteredData.txt");
                    (new FileInfo(directory)).Directory.Create();
                    using (StreamWriter writer = new StreamWriter(directory))
                    {
                        using (SqlDataReader reader = local.Cmd.ExecuteReader())
                        {
                            bool needsHeaders = true;
                            while (reader.Read())
                            {
                                if (needsHeaders)
                                {
                                    StringBuilder newLine = new StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i != (reader.FieldCount - 1))
                                        {
                                            newLine.AppendFormat("{0},", reader.GetName(i));
                                        }
                                        else
                                        {
                                            newLine.AppendFormat("{0}", reader.GetName(i));
                                        }
                                    }
                                    writer.WriteLine(newLine.ToString());
                                    needsHeaders = false;
                                }
                                else
                                {
                                    StringBuilder newLine = new StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i != (reader.FieldCount - 1))
                                        {
                                            newLine.AppendFormat("{0},", Convert.ToString(reader.GetValue(i)));
                                        }
                                        else
                                        {
                                            newLine.AppendFormat("{0}", Convert.ToString(reader.GetValue(i)));
                                        }
                                    }
                                    writer.WriteLine(newLine.ToString());
                                }
                            }
                        }
                    }

                    return local;
                },
                (conn) =>
                {
                    conn.Cmd.Dispose();
                    conn.Conn.Dispose();
                }
            );
        }
    }
}