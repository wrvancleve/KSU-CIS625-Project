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
    public class MainWindowController
    {
        /// <summary>
        /// Connection string for sql server
        /// </summary>
        private const string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=U:\cis625\StockMarketAnalysis\StockMarketAnalysis\Database.mdf;Integrated Security=True";

        private string MaxPostFilterQuery = String.Concat
        (
            @"SELECT CAD.AggregateKey, CAD.@ColumnName, ISNULL(MAD.@ColumnName, 0.0) ",
            @"FROM [StockData].[CurrentAggregateData] CAD ",
            @"LEFT JOIN [StockData].[MaxAggregateData] MAD ON MAD.AggregateKey = CAD.AggregateKey AND MAD.CriteriaSetId = CAD.CriteriaSetId ",
            @"WHERE CAD.CriteriaSetId = @CriteriaSetId"
        );

        private string CrossesPostFilterQuery = String.Concat
        (
            @"SELECT CAD.AggregateKey, CAD.@ColumnName, ISNULL(PAD.@ColumnName, 0.0) ",
            @"FROM [StockData].[CurrentAggregateData] CAD ",
            @"LEFT JOIN [StockData].[PreviousAggregateData] PAD ON PAD.AggregateKey = CAD.AggregateKey AND PAD.CriteriaSetId = CAD.CriteriaSetId ",
            @"WHERE CAD.CriteriaSetId = @CriteriaSetId"
        );

        private string ValuePostFilterQuery = String.Concat
        (
            @"SELECT CAD.AggregateKey, CAD.@ColumnName ",
            @"FROM [StockData].[CurrentAggregateData] CAD ",
            @"WHERE CAD.CriteriaSetId = @CriteriaSetId"
        );

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
        public void Handle(List<string> directories, bool isNew)
        {
            _criteriaSetPath = directories[0];
            _dataSetPath = directories[1];
            _outputPath = directories[2];
            _criteriaSets = new List<CriteriaSet>();
            ObtainCriteriaSets();
            ProcessData(isNew);
        }

        /// <summary>
        /// Obtains the criteria sets from the criteria set file
        /// </summary>
        /// <returns></returns>
        private void ObtainCriteriaSets()
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
                            if (currentPostFilterColumn.Equals("percentagesharesheld"))
                            {
                                currentPostFilterColumn = "AggregatePercentageSharesHeld";
                            }
                            else if (currentPostFilterColumn.Equals("value"))
                            {
                                currentPostFilterColumn = "AggregateValue";
                            }
                            else
                            {
                                currentPostFilterColumn = "AggregateSharesHeld";
                            }
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
                                currentPostFilterValues.Add(line.Substring(1).Replace(",", String.Empty)); // Save Post Filter Value
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
            }
            catch
            {
                MessageBox.Show("Error while reading in criteria sets. Please check your file and try again.", "Criteria Set Read Error");
            }
        }

        /// <summary>
        /// Obtains the data from the data files.
        /// </summary>
        /// <returns>Whether the data was obtained successfully</returns>
        private void ProcessData(bool isNew)
        {
            if (isNew)
            {
                ProcessDataNew();
            }
            else
            {
                ProcessDataHistory();
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
                if (files.IndexOf(file) == files.Count - 1) // Current Day
                {
                    ProcessFile(file); // Process file
                    ProcessCurrentPreFilters();
                    ProcessCurrentAggregations(); // Process aggregations with output
                    ProcessPostFilters(); // Process postfilters with output
                }
                else if (files.IndexOf(file) == files.Count - 2) // Previous Day
                {
                    ProcessFile(file); // Process file
                    ProcessPreviousAggregations(); // Process aggregations
                }
                else
                {
                    ProcessFile(file); // Process file
                    ProcessMaxAggregations(); // Process aggregations
                }
            }
        }

        private void ProcessDataNew()
        {
            InitDatabase(); // Clear raw data
            ProcessFile(_dataSetPath); // Process new file
            ProcessCurrentPreFilters(); // Process prefilters
            InitAggregations(); // Move current aggregations to previous
            ProcessCurrentAggregations(); // Process aggregations
            ProcessPostFilters();
        }

        private void InitDatabase()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConnectionString;
                SqlCommand cmd = new SqlCommand("StockData.ClearData", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void InitAggregations()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConnectionString;
                SqlCommand cmd = new SqlCommand("StockData.MoveAggregateData", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void ProcessFile(string file)
        {
            // .Take(100)
            Parallel.ForEach(
                File.ReadLines(file),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    SqlCommand cmd = new SqlCommand("StockData.InsertData", conn);
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

        private void ProcessMaxAggregations()
        {
            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    return new { Conn = conn };
                },
                (criteria, unused, local) =>
                {
                    if (criteria.PostFilter.Item2.Equals("MAX"))
                    {
                        using (SqlTransaction trans = local.Conn.BeginTransaction())
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = local.Conn;
                            cmd.Transaction = trans;
                            cmd.CommandText = "StockData.ObtainMaxAggregateData";
                            cmd.CommandType = CommandType.StoredProcedure;

                            string countries = null;
                            string stockType = null;
                            string direction = null;

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

                            if (countries != null) cmd.Parameters.AddWithValue("HolderCountries", countries);
                            if (stockType != null) cmd.Parameters.AddWithValue("StockType", stockType);
                            if (direction != null) cmd.Parameters.AddWithValue("Direction", direction);
                            cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                            cmd.Parameters.AddWithValue("AggregateKeys", String.Join("~", criteria.AggregationColumns));
                            cmd.ExecuteNonQuery();

                            trans.Commit();
                        }
                    }

                    return local;
                },
                (local) =>
                {
                    local.Conn.Dispose();
                }
            );
        }

        private void ProcessPreviousAggregations()
        {
            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    return new { Conn = conn };
                },
                (criteria, unused, local) =>
                {
                    if (criteria.PostFilter.Item2.Equals("CROSSES"))
                    {
                        using (SqlTransaction trans = local.Conn.BeginTransaction())
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = local.Conn;
                            cmd.Transaction = trans;
                            cmd.CommandText = "StockData.ObtainPreviousAggregateData";
                            cmd.CommandType = CommandType.StoredProcedure;

                            string countries = null;
                            string stockType = null;
                            string direction = null;

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

                            if (countries != null) cmd.Parameters.AddWithValue("HolderCountries", countries);
                            if (stockType != null) cmd.Parameters.AddWithValue("StockType", stockType);
                            if (direction != null) cmd.Parameters.AddWithValue("Direction", direction);
                            cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                            cmd.Parameters.AddWithValue("AggregateKeys", String.Join("~", criteria.AggregationColumns));
                            cmd.ExecuteNonQuery();

                            trans.Commit();
                        }
                    }

                    return local;
                },
                (local) =>
                {
                    local.Conn.Dispose();
                }
            );
        }

        private void ProcessCurrentPreFilters()
        {
            Parallel.ForEach(
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    return new { Conn = conn };
                },
                (criteria, unused, local) =>
                {
                    using (SqlTransaction trans = local.Conn.BeginTransaction())
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = local.Conn;
                        cmd.Transaction = trans;
                        cmd.CommandText = "StockData.ObtainCurrentPreFilteredData";
                        cmd.CommandType = CommandType.StoredProcedure;

                        string countries = null;
                        string stockType = null;
                        string direction = null;

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

                        if (countries != null) cmd.Parameters.AddWithValue("HolderCountries", countries);
                        if (stockType != null) cmd.Parameters.AddWithValue("StockType", stockType);
                        if (direction != null) cmd.Parameters.AddWithValue("Direction", direction);
                        cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);

                        string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\PreFilteredData.csv");
                        (new FileInfo(directory)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(directory))
                        {
                            writer.WriteLine("StockCode,StockType,HolderId,HolderCountry,SharesHeld,PercentageSharesHeld,Direction,Value");

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                foreach (DataRow row in table.Rows)
                                {
                                    writer.WriteLine(String.Join(",", row.ItemArray));
                                }
                            }
                        }

                        trans.Commit();
                    }

                    return local;
                },
                (local) =>
                {
                    local.Conn.Dispose();
                }
            );
        }

        private void ProcessCurrentAggregations()
        {
            Parallel.ForEach (
                _criteriaSets,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () =>
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    return new { Conn = conn };
                },
                (criteria, unused, local) =>
                {
                    using (SqlTransaction trans = local.Conn.BeginTransaction())
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = local.Conn;
                        cmd.Transaction = trans;
                        cmd.CommandText = "StockData.ObtainCurrentAggregateData";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("CriteriaSetId", criteria.Number);
                        cmd.Parameters.AddWithValue("AggregateKeys", String.Join("~", criteria.AggregationColumns));

                        string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\AggregatedData.csv");
                        (new FileInfo(directory)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(directory))
                        {
                            writer.WriteLine("AggregateKey,AggregateSharesheld,AggregatePercentageSharesheld,AggregateValue");

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                foreach (DataRow row in table.Rows)
                                {
                                    writer.WriteLine(String.Join(",", row.ItemArray));
                                }
                            }
                        }

                        trans.Commit();
                    }

                    return local;
                },
                (local) =>
                {
                    local.Conn.Dispose();
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
                    conn.Open();

                    return new { Conn = conn};
                },
                (criteria, unused, local) =>
                {
                    using (SqlTransaction trans = local.Conn.BeginTransaction())
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = local.Conn;
                        cmd.Transaction = trans;
                        if (criteria.PostFilter.Item2.Equals("MAX"))
                        {
                            cmd.CommandText = MaxPostFilterQuery.Replace(@"@ColumnName", criteria.PostFilter.Item1);
                        }
                        else if (criteria.PostFilter.Item2.Equals("CROSSES"))
                        {
                            cmd.CommandText = CrossesPostFilterQuery.Replace(@"@ColumnName", criteria.PostFilter.Item1);
                        }
                        else
                        {
                            cmd.CommandText = ValuePostFilterQuery.Replace(@"@ColumnName", criteria.PostFilter.Item1);
                        }
                        cmd.CommandText = cmd.CommandText.Replace(@"@CriteriaSetId", criteria.Number.ToString());

                        string directory = String.Concat(_outputPath, @"\CriteriaSet", criteria.Number, @"\PostFilteredData.csv");
                        using (StreamWriter writer = new StreamWriter(directory))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                /* Get column headers */
                                List<string> allColumns = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
                                List<string> newColumns = new List<string>();
                                foreach (string value in criteria.PostFilter.Item3)
                                {
                                    string column = String.Join(" ", new string[] { criteria.PostFilter.Item1, criteria.PostFilter.Item2, value });
                                    table.Columns.Add(column);
                                    allColumns.Add(column);
                                    newColumns.Add(column);
                                }
                                writer.WriteLine(String.Join(",", allColumns.ToArray())); // Write header

                                foreach (DataRow row in table.Rows)
                                {
                                    if (criteria.PostFilter.Item2.Equals("MAX") || criteria.PostFilter.Item2.Equals("CROSSES"))
                                    {
                                        for (int i = 0; i < newColumns.Count; i++)
                                        {
                                            // Row[1] = Current Aggregate Value
                                            // Row[2] = Max Aggregate Value or Previous Aggregate Value

                                            string column = newColumns[i];
                                            double value = Convert.ToDouble(criteria.PostFilter.Item3[i]);
                                            row[newColumns[i]] = Convert.ToDouble(row[2]) < value && Convert.ToDouble(row[1]) > value;
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < newColumns.Count; i++)
                                        {
                                            // Row[1] = Current Aggregate Value

                                            string column = newColumns[i];
                                            double value = Convert.ToDouble(criteria.PostFilter.Item3[i]);

                                            switch (criteria.PostFilter.Item2)
                                            {
                                                case "<":
                                                    row[newColumns[i]] = Convert.ToDouble(row[1]) < value;
                                                    break;
                                                case "<=":
                                                    row[newColumns[i]] = Convert.ToDouble(row[1]) <= value;
                                                    break;
                                                case ">":
                                                    row[newColumns[i]] = Convert.ToDouble(row[1]) > value;
                                                    break;
                                                case ">=":
                                                    row[newColumns[i]] = Convert.ToDouble(row[1]) >= value;
                                                    break;
                                            }
                                        }
                                    }

                                    /* Write line */
                                    writer.WriteLine(String.Join(",", row.ItemArray));
                                }
                            }
                        }

                        trans.Commit();
                    }

                    return local;
                },
                (local) =>
                {
                    local.Conn.Dispose();
                }
            );
        }
    }
}