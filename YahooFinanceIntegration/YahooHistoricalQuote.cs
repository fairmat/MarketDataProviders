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

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Represents a single historical quote from Yahoo! Finance.
    /// </summary>
    internal class YahooHistoricalQuote
    {
        #region Constructors

        /// <summary>
        /// Default constructor. It just sets default values.
        /// </summary>
        public YahooHistoricalQuote()
        {
            this.Date = new DateTime();
            this.Open = 0.0f;
            this.High = 0.0f;
            this.Low = 0.0f;
            this.Close = 0.0f;
            this.Volume = 0;
            this.AdjClose = 0.0f;
        }

        /// <summary>
        /// Constructs a new instance starting from the provided line from a Yahoo! Finance csv.
        /// </summary>
        /// <param name="csvLine">A line from a Yahoo! Finance csv.</param>
        public YahooHistoricalQuote(string csvLine)
        {
            ParseCSVLine(csvLine);
        }

        #endregion Constructos

        #region Properties

        /// <summary>
        /// Gets the date of this quote.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Gets the stock market opening price.
        /// </summary>
        public double Open { get; private set; }

        /// <summary>
        /// Gets the stock market highest price.
        /// </summary>
        public double High { get; private set; }

        /// <summary>
        /// Gets the stock market lowest price.
        /// </summary>
        public double Low { get; private set; }

        /// <summary>
        /// Gets the stock market close price.
        /// </summary>
        public double Close { get; private set; }

        /// <summary>
        /// Gets the volume of exchanges.
        /// </summary>
        public int Volume { get; private set; }

        /// <summary>
        /// Gets the stock market adjusted close price.
        /// </summary>
        public double AdjClose { get; private set; }

        #endregion Properties

        /// <summary>
        /// Populates the data starting from a provided line from a Yahoo! Finance csv.
        /// </summary>
        /// <param name="csvLine">The line from a Yahoo! Finance csv to parse.</param>
        public void ParseCSVLine(string csvLine)
        {
            // We take for granted the Yahoo! format doesn't change and so
            // the data is listed in this way: Date,Open,High,Low,Close,Volume,Adj Close.
            // This means rows must be of 7 elements.
            string[] rows = csvLine.Split(',');

            // Check if the number of elements is right.
            if (rows.Length != 7)
            {
                throw new InvalidDataException("The csv line has a wrong number of items");
            }

            // Format used by Yahoo! for dates
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy-MM-dd";
            dateFormat.DateSeparator = "-";

            // Format used by Yahoo! for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ".";
            doubleFormat.NumberGroupSeparator = ",";

            // First item is a date.
            this.Date = Convert.ToDateTime(rows[0], dateFormat);

            // The subsequent are all numbers. All doubles, except Volume which is an integer.
            this.Open = Convert.ToDouble(rows[1], doubleFormat);
            this.High = Convert.ToDouble(rows[2], doubleFormat);
            this.Low = Convert.ToDouble(rows[3], doubleFormat);
            this.Close = Convert.ToDouble(rows[4], doubleFormat);
            this.Volume = Convert.ToInt32(rows[5]);
            this.AdjClose = Convert.ToDouble(rows[6], doubleFormat);
        }
    }
}
