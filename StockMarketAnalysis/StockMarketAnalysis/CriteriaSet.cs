using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAnalysis
{
    public class CriteriaSet
    {
        #region Pre-Filter
        /// <summary>
        /// Country/Countries for the Pre-Filter
        /// </summary>
        private List<string> _countries;

        /// <summary>
        /// Stock Type for the Pre-Filter
        /// </summary>
        private enum Type
        {
            Preferred,
            Common
        }

        /// <summary>
        /// Stock Position for the Pre-Filter
        /// </summary>
        private enum Position
        {
            Long,
            Short
        }
        #endregion 

        #region Aggregation
        /// <summary>
        /// Could be a combination of StockCode, Stocktype, etc.
        /// Example: SGQNS~Preferred
        /// </summary>
        private List<string> _aggregationKey;

        /// <summary>
        /// Could be the number of shares held, or percentage of total shares held.
        /// </summary>
        private List<string> _aggregationSum;
        #endregion

        #region Post-Filter
        private enum Criteria
        {
            Percent,
            Number,
            Value
        }

        /// <summary>
        /// Could be Percent (TotalPercentageSharesHeld), Number (TotalShares), Value (TotalValue)
        /// </summary>
        private long _number;
        #endregion
            
        public CriteriaSet(List<string> set)
        {

        }
    }
}
