using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            GetPreFilters(set[0]);
            GetAggregates(set[1]);
            GetPostFilters(set[2]);
        }

        private void GetPreFilters(string line)
        {
            string[] parts = line.Split('|');
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    string[] countries = parts[0].Split(',');
                    foreach (string country in countries)
                    {
                        _countries.Add(country);
                    }
                }
                else if (i == 1)
                {
                    if (parts[1].Equals("Short"))
                    {
                        this._position = Position.Short;
                    }
                    else
                    {
                        this._position = Position.Long;
                    }
                }
                else
                {
                    if (parts[2].Equals("Preferred"))
                    {
                        this._type = Type.Preferred;
                    }
                    else
                    {
                        this._type = Type.Common;
                    }
                }
            }
        }

        private void GetAggregates(string line)
        {
            string[] parts = line.Split('|');

            if (parts[0].Contains(','))
            {
                string[] keys = parts[0].Split(',');
                for (int i = 0; i < keys.Length; i++)
                {
                    _aggregationKey.Add(keys[i]);
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
                    _aggregationSum.Add(aggregates[j]);
                }
            }
            else
            {
                _aggregationSum.Add(parts[1]);
            }
        }

        private void GetPostFilters(string line)
        {
            string[] parts; // Create parts array

            if (line.Contains(','))
            {
                parts = line.Split(','); // Split into parts
            }
            else
            {
                parts = new string[1];
                parts[0] = line; // Make one part
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i][0] == '#')
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Number, Convert.ToDouble(parts[i].Substring(1))));
                }
                else if (parts[i][0] == '$')
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Value, Convert.ToDouble(parts[i].Substring(1))));
                }
                else
                {
                    _postFilters.Add(new Tuple<Criteria, double>(Criteria.Percent, Convert.ToDouble(parts[i].Substring(1))));
                }
            }
        }
    }
}