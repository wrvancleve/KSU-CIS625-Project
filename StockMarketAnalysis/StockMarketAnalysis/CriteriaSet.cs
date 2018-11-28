using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMarketAnalysis
{
    /// <summary>
    /// Stock Type for the Pre-Filter
    /// </summary>
    public enum Type
    {
        Preferred,
        Common
    }

    /// <summary>
    /// Stock Position for the Pre-Filter
    /// </summary>
    public enum Position
    {
        Long,
        Short
    }

    public enum Criteria
    {
        Percent,
        Number,
        Value
    }

    public class CriteriaSet
    {
        #region Pre-Filter
        /// <summary>
        /// Country/Countries for the Pre-Filter
        /// </summary>
        private List<string> _countries = new List<string>();

        /// <summary>
        /// Type of stock
        /// </summary>
        private Type _type;
        
        /// <summary>
        /// Position of stock
        /// </summary>
        private Position _position;
        #endregion 

        #region Aggregation
        /// <summary>
        /// Could be a combination of StockCode, Stocktype, etc.
        /// Example: SGQNS~Preferred
        /// </summary>
        private List<string> _aggregationKey = new List<string>();

        /// <summary>
        /// Could be the number of shares held, or percentage of total shares held.
        /// </summary>
        private List<string> _aggregationSum = new List<string>();
        #endregion

        #region Post-Filter

        /// <summary>
        /// Holds the Criteria, threshold OR percentage
        /// </summary>
        private List<Tuple<Criteria, double>> _postFilters = new List<Tuple<Criteria, double>>();

        #endregion
            
        public CriteriaSet(List<string> set)
        {
            
            try
            {
                if (set.Count != 3)
                {
                    throw new Exception("Invalid Criteria Set Format");
                }
                Regex regex = new Regex(@"[\t]");
                GetPreFilters(regex.Replace(set[0].Trim(), ""));
                GetAggregates(regex.Replace(set[1].Trim(), ""));
                GetPostFilters(regex.Replace(set[2].Trim(), ""));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GetPreFilters(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length != 3) throw new Exception("Invalid Pre-Filtering Format");
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    string[] countries = parts[0].Split(',');
                    foreach (string country in countries)
                    {
                        _countries.Add(country.Trim());
                    }
                }
                else if (i == 1)
                {
                    if (parts[1].Trim().Equals("Short")) this._position = Position.Short;
                    else if (parts[1].Trim().Equals("Long")) this._position = Position.Long;
                    else throw new Exception("Invalid Position");
                }
                else
                {
                    if (parts[2].Trim().Equals("Preferred")) this._type = Type.Preferred;
                    else if (parts[2].Trim().Equals("Common")) this._type = Type.Common;
                    else throw new Exception("Invalid Type");
                }
            }
        }

        private void GetAggregates(string line)
        {
            string[] parts = line.Trim().Split('|');
            if (parts.Length != 2) throw new Exception("Invalid Aggregation Format");

            if (parts[0].Contains(','))
            {
                string[] keys = parts[0].Split(',');
                for (int i = 0; i < keys.Length; i++)
                {
                    _aggregationKey.Add(keys[i].Trim());
                }
            }
            else
            {
                _aggregationKey.Add(parts[0]);
            }

            if (parts[1].Contains(','))
            {
                string[] aggregates = parts[1].Split(',');
                for (int j = 0; j < aggregates.Length; j++)
                {
                    _aggregationSum.Add(aggregates[j].Trim());
                }
            }
            else
            {
                _aggregationSum.Add(parts[1]);
            }
        }

        private void GetPostFilters(string line)
        {
            if (line.Contains(','))
            {
                string[] parts = line.Split(',');

                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Replace(" ", "");
                    if (parts[i][0] == '#')
                    {
                        _postFilters.Add(new Tuple<Criteria, double>(Criteria.Number, Convert.ToDouble(parts[i].Substring(1))));
                    }
                    else if (parts[i][0] == '$')
                    {
                        _postFilters.Add(new Tuple<Criteria, double>(Criteria.Value, Convert.ToDouble(parts[i].Substring(1))));
                    }
                    else if (parts[i][0] == '%')
                    {
                        _postFilters.Add(new Tuple<Criteria, double>(Criteria.Percent, Convert.ToDouble(parts[i].Substring(1))));
                    }
                    else
                    {
                        throw new Exception("Invalid Post-Filter Format");
                    }
                }
            }
            else
            {
                if (line[0] == '#')
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Number, Convert.ToDouble(line.Substring(1))));
                }
                else if (line[0] == '$')
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Value, Convert.ToDouble(line.Substring(1))));
                }
                else if (line[0] == '%')
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Percent, Convert.ToDouble(line.Substring(1))));
                }
                else
                {
                    throw new Exception("Invalid Post-Filter Format");
                }
            }
        }
    }
}
