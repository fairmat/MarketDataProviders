/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Stefano Angeleri (stefano.angeleri@fairmat.com)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;
using System.IO;

namespace MEEFIntegration
{
    /// <summary>
    /// Represents a single historical quote from MEEF.
    /// </summary>
    internal class MEEFHistoricalQuote
    {
        #region Constructors

        /// <summary>
        /// Default constructor. It just sets default values.
        /// </summary>
        public MEEFHistoricalQuote()
        {
            this.SessionDate = new DateTime();
            this.ContractGroup = string.Empty;
            this.ContractCode = string.Empty;
            this.ContractSubgroupCode = string.Empty;
            this.CFICode = string.Empty;
            this.StrikePrice = 0.0f;
            this.MaturityDate = new DateTime();
            this.BidPrice = 0.0f;
            this.AskPrice = 0.0f;
            this.HighPrice = 0.0f;
            this.LowPrice = 0.0f;
            this.LastPrice = 0.0f;
            this.SettlPrice = 0.0f;
            this.SettlVolatility = 0.0f;
            this.SettlDelta = 0.0f;
            this.TotalRegVolume = 0;
            this.NumberOfTrades = 0;
            this.OpenInterest = 0;
        }

        /// <summary>
        /// Constructs a new instance starting from the provided line from a MEEF csv.
        /// </summary>
        /// <param name="csvLine">A line from a MEEF csv.</param>
        /// <param name="oldFormat">If the csvLine is in the MEEF old format.</param>
        public MEEFHistoricalQuote(string csvLine, bool oldFormat = false)
        {
            if (oldFormat)
            {
                ParseOldFormatCSVLine(csvLine);
            }
            else
            {
                ParseCSVLine(csvLine);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the Session Date of this data, usually
        /// the historical date revelation for the data.
        /// </summary>
        public DateTime SessionDate { get; private set; }

        public string ContractGroup { get; private set; }

        /// <summary>
        /// The code of this contact, also regarded as its name.
        /// </summary>
        public string ContractCode { get; private set; }


        public string ContractSubgroupCode { get; private set; }

        public string CFICode { get; private set; }

        /// <summary>
        /// Gets the Strike price.
        /// </summary>
        public double StrikePrice { get; private set; }

        /// <summary>
        /// Gets the maturity date.
        /// </summary>
        public DateTime MaturityDate { get; private set; }

        /// <summary>
        /// Gets The bid price.
        /// </summary>
        public double BidPrice { get; private set; }

        /// <summary>
        /// Gets the Ask price.
        /// </summary>
        public double AskPrice { get; private set; }

        /// <summary>
        /// Gets the highest price in the day.
        /// </summary>
        public double HighPrice { get; private set; }

        /// <summary>
        /// Gets the lowest price in the day.
        /// </summary>
        public double LowPrice { get; private set; }

        /// <summary>
        /// Gets the last price in the day.
        /// </summary>
        public double LastPrice { get; private set; }

        /// <summary>
        /// Gets the settled price in the day.
        /// </summary>
        public double SettlPrice { get; private set; }

        public double SettlVolatility { get; private set; }

        public double SettlDelta { get; private set; }

        public int TotalRegVolume { get; private set; }

        /// <summary>
        /// Gets the number of trades which happened in the day.
        /// </summary>
        public int NumberOfTrades { get; private set; }

        public int OpenInterest { get; private set; }

        #endregion Properties

        /// <summary>
        /// Populates the data starting from a provided line from a MEEF Historical csv
        /// using the pre-2006 format.
        /// </summary>
        /// <param name="csvLine">The line from a MEEF Historical csv to parse.</param>
        /// <remarks>
        /// This format lacks some data, so that will remain to its default value.
        /// </remarks>
        public void ParseOldFormatCSVLine(string csvLine)
        {
            // We take for granted the MEEF format doesn't change.
            // This means rows must be of 16 elements.
            string[] rows = csvLine.Split(',');

            // Check if the number of elements is right.
            if (rows.Length != 16)
            {
                throw new InvalidDataException("The csv line has a wrong number of items");
            }

            // Format used by MEEF for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ".";
            doubleFormat.NumberGroupSeparator = ",";

            // Format has a succession of dates, strings, doubles (prices) and integers values.
            this.SessionDate = DateTime.ParseExact(rows[0], "yyyyMMdd", CultureInfo.InvariantCulture);
            this.ContractGroup = rows[1].Trim();
            // row 2 maps to? instrument call put futuro
            this.MaturityDate = DateTime.ParseExact(rows[3], "yyMMdd", CultureInfo.InvariantCulture);
            this.StrikePrice = Convert.ToDouble(rows[4], doubleFormat);
            this.ContractCode = rows[5].Trim();
            this.BidPrice = Convert.ToDouble(rows[6], doubleFormat);
            this.AskPrice = Convert.ToDouble(rows[7], doubleFormat);
            this.HighPrice = Convert.ToDouble(rows[8], doubleFormat);
            this.LowPrice = Convert.ToDouble(rows[9], doubleFormat);
            this.LastPrice = Convert.ToDouble(rows[10], doubleFormat);
            this.TotalRegVolume = Convert.ToInt32(rows[11]);
            this.SettlPrice = Convert.ToDouble(rows[12], doubleFormat);
            this.OpenInterest = Convert.ToInt32(rows[13]);
            this.SettlVolatility = Convert.ToDouble(rows[14], doubleFormat);
        }

        /// <summary>
        /// Populates the data starting from a provided line from a MEEF Historical csv.
        /// </summary>
        /// <param name="csvLine">The line from a MEEF Historical csv to parse.</param>
        public void ParseCSVLine(string csvLine)
        {
            Func<string, string> cleanString = (str) => { return str.Substring(1, str.Length - 2); };

            // We take for granted the MEEF format doesn't change.
            // This means rows must be of 18 elements.
            string[] rows = csvLine.Split(';');

            // Check if the number of elements is right.
            if (rows.Length != 18)
            {
                throw new InvalidDataException("The csv line has a wrong number of items");
            }

            // Format used by MEEF for dates
            string dateFormat = "yyyyMMdd";

            // Format used by MEEF for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ",";
            doubleFormat.NumberGroupSeparator = ".";

            // Format has a succession of dates, strings, doubles (prices) and integers values.
            this.SessionDate = DateTime.ParseExact(cleanString(rows[0]), dateFormat, CultureInfo.InvariantCulture);
            this.ContractGroup = cleanString(rows[1]);
            this.ContractCode = cleanString(rows[2]);
            this.ContractSubgroupCode = cleanString(rows[3]);
            this.CFICode = cleanString(rows[4]);
            this.StrikePrice = Convert.ToDouble(rows[5], doubleFormat);
            this.MaturityDate = DateTime.ParseExact(cleanString(rows[6]), dateFormat, CultureInfo.InvariantCulture);
            this.BidPrice = Convert.ToDouble(rows[7], doubleFormat);
            this.AskPrice = Convert.ToDouble(rows[8], doubleFormat);
            this.HighPrice = Convert.ToDouble(rows[9], doubleFormat);
            this.LowPrice = Convert.ToDouble(rows[10], doubleFormat);
            this.LastPrice = Convert.ToDouble(rows[11], doubleFormat);
            this.SettlPrice = Convert.ToDouble(rows[12], doubleFormat);

            // In some cases these fields are just missing data, when this happens consider them as zero.
            this.SettlVolatility = rows[13].Length > 0 ? Convert.ToDouble(rows[13], doubleFormat) : 0.0f;
            this.SettlDelta = rows[14].Length > 0 ? Convert.ToDouble(rows[14], doubleFormat) : 0.0f;

            this.TotalRegVolume = Convert.ToInt32(rows[15]);
            this.NumberOfTrades = Convert.ToInt32(rows[16]);
            this.OpenInterest = Convert.ToInt32(rows[17]);
        }
    }
}
