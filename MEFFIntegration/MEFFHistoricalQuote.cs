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
using OptionQuotes;
using DVPLI.Interfaces;
using DVPLI.Enums;

namespace MEFFIntegration
{
    /// <summary>
    /// Represents a single historical quote from MEFF.
    /// </summary>
    internal class MEFFHistoricalQuote : IOptionQuote
    {
        #region Constructors

        /// <summary>
        /// Default constructor. It just sets default values.
        /// </summary>
        public MEFFHistoricalQuote()
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
        /// Constructs a new instance starting from the provided line from a MEFF csv.
        /// </summary>
        /// <param name="csvLine">A line from a MEFF csv.</param>
        /// <param name="oldFormat">If the csvLine is in the MEFF old format.</param>
        public MEFFHistoricalQuote(string csvLine, bool oldFormat = false)
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

        /// <summary>
        /// Gets the group of this item.
        /// </summary>
        public string ContractGroup { get; private set; }

        /// <summary>
        /// Gets the code of this contact, also regarded as its name.
        /// </summary>
        public string ContractCode { get; private set; }

        /// <summary>
        /// Gets the code of the subgroup of this item.
        /// </summary>
        public string ContractSubgroupCode { get; private set; }

        /// <summary>
        /// Gets the Code of the financial instrument following ISO 10962.
        /// </summary>
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
        /// Gets the settle price in the day.
        /// </summary>
        public double SettlPrice { get; private set; }

        /// <summary>
        /// Gets the volatility of the settle price at the end of the day.
        /// This might be missing for big options.
        /// </summary>
        public double SettlVolatility { get; private set; }

        /// <summary>
        /// Gets the delta of the settle price at the end of the day.
        /// This might be missing for big options.
        /// </summary>
        public double SettlDelta { get; private set; }

        /// <summary>
        /// Gets the total registered volume of transactions.
        /// </summary>
        public int TotalRegVolume { get; private set; }

        /// <summary>
        /// Gets the number of trades which happened in the day.
        /// </summary>
        public int NumberOfTrades { get; private set; }

        /// <summary>
        /// Gets the open position for the owner.
        /// </summary>
        public int OpenInterest { get; private set; }

        #endregion Properties

        #region IOptionQuote implementation

        public OptionQuoteStyle Style
        {
            get
            {
                return OptionQuoteStyle.European;
            }
        }

        public double Volatility
        {
            get
            {
                return this.SettlVolatility;
            }
        }

        public double Volume
        {
            get
            {
                return this.TotalRegVolume;
            }
        }

        #endregion IOptionQuote implementation

        /// <summary>
        /// Populates the data starting from a provided line from a MEFF Historical csv
        /// using the pre-2006 format.
        /// </summary>
        /// <param name="csvLine">The line from a MEFF Historical csv to parse.</param>
        /// <remarks>
        /// This format lacks some data, so that will remain to its default value.
        /// </remarks>
        public void ParseOldFormatCSVLine(string csvLine)
        {
            // We take for granted the MEFF format doesn't change.
            // This means rows must be of 16 elements.
            string[] rows = csvLine.Split(',');

            // Check if the number of elements is right.
            if (rows.Length != 16)
            {
                throw new InvalidDataException("The csv line has a wrong number of items 16 expected, " + rows.Length + " found");
            }

            // Format used by MEFF for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ".";
            doubleFormat.NumberGroupSeparator = ",";

            // Format has a succession of dates, strings, doubles (prices) and integers values.
            this.SessionDate = DateTime.ParseExact(rows[0], "yyyyMMdd", CultureInfo.InvariantCulture);
            this.ContractGroup = rows[1].Trim();

            // row 2 maps to? instrument call put futuro

            // Row 2 is mapped to CFICODES OPASPS, OCASPS or F. To be checked if correct.
            this.CFICode = rows[2].Trim();
            if (this.CFICode != "F")
            {
                this.CFICode = "O" + this.CFICode;
            }

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
        /// Populates the data starting from a provided line from a MEFF Historical csv.
        /// </summary>
        /// <param name="csvLine">The line from a MEFF Historical csv to parse.</param>
        public void ParseCSVLine(string csvLine)
        {
            Func<string, string> cleanString = (str) => { return str.Substring(1, str.Length - 2); };

            // We take for granted the MEFF format doesn't change.
            // This means rows must be of 18 elements.
            string[] rows = csvLine.Split(';');

            // Check if the number of elements is right.
            if (rows.Length != 18)
            {
                throw new InvalidDataException("The csv line has a wrong number of items 18 expected, " + rows.Length + " found");
            }

            // Format used by MEFF for dates
            string dateFormat = "yyyyMMdd";

            // Format used by MEFF for doubles.
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

        #region IOptionQuote implementation

        /// <summary>
        /// Gets the price of the option.
        /// </summary>
        public double Price
        {
            get
            {
                return this.SettlPrice;
            }
        }

        /// <summary>
        /// Gets the strike value of the option.
        /// </summary>
        public double Strike
        {
            get
            {
                return this.StrikePrice;
            }
        }

        /// <summary>
        /// Gets the expiration date of this option.
        /// </summary>
        public DateTime Maturity
        {
            get
            {
                return this.MaturityDate;
            }
        }

        /// <summary>
        /// Gets the type of this option quote.
        /// </summary>
        public OptionQuoteType Type
        {
            get
            {
                if (this.ContractCode.StartsWith("C")) return OptionQuoteType.Call;
                if (this.ContractCode.StartsWith("P")) return OptionQuoteType.Put;
                throw new Exception("MEFF quote is not an option!");
            }
        }

        #endregion IOptionQuote
    }
}
