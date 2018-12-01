using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMarketAnalysis
{
    public class CriteriaSet
    {
        /// <summary>
        /// Number of the criteria set
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Name of the criteria set
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of the Pre Filters of the criteria set.
        /// 
        /// Each Tuple Represents a Different Pre Filter.
        /// 
        /// Each Pre Filter Has:
        ///     (string) Item1 = Column Name
        ///     (string) Item2 = Comparison Type
        ///     (List<string>) Item3 = List of Values
        /// </summary>
        public List<Tuple<string, string, List<string>>> PreFilters { get; set; }

        /// <summary>
        /// Aggregation columns of the criteria set
        /// </summary>
        public List<string> AggregationColumns { get; set; }

        /// <summary>
        /// Aggregation sums of the criteria set
        /// </summary>
        public List<string> AggregationSums { get; set; }

        /// <summary>
        /// Post Filter of the criteria set.
        /// 
        /// The Post Filter Has:
        ///     (string) Item1 = Column Name
        ///     (string) Item2 = Comparison Type
        ///     (List<string>) Item3 = List of Values
        /// </summary>
        public Tuple<string, string, List<string>> PostFilter { get; set; }

        public CriteriaSet()
        {
            PreFilters = new List<Tuple<string, string, List<string>>>();
            AggregationColumns = new List<string>();
            AggregationSums = new List<string>();
        }
    }
}