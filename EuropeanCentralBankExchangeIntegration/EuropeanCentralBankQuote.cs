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

namespace EuropeanCentralBankIntegration
{
    /// <summary>
    /// Represents a single historical quote from The European Central Bank exchange rates.
    /// </summary>
    internal class EuropeanCentralBankQuote
    {
        #region Constructors

        /// <summary>
        /// Default constructor. It just sets default values.
        /// </summary>
        public EuropeanCentralBankQuote()
        {
            this.Date = new DateTime();
            this.Value = 0.0f;
        }

        /// <summary>
        /// Constructs a new instance from a Date and a Value.
        /// </summary>
        /// <param name="date">The date corresponding to this quote, as a string.</param>
        /// <param name="value">The value corresponding to this quote, as a string.</param>
        public EuropeanCentralBankQuote(string date, string value)
        {
            ParseData(date, value);
        }

        /// <summary>
        /// Constructs a new instance from a Date and a Value.
        /// </summary>
        /// <param name="date">The date corresponding to this quote.</param>
        /// <param name="value">The value corresponding to this quote.</param>
        public EuropeanCentralBankQuote(DateTime date, double value)
        {
            this.Date = date;
            this.Value = value;
        }

        #endregion Constructos

        #region Properties

        /// <summary>
        /// Gets the date of this quote.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Gets the exchange rate.
        /// </summary>
        public double Value { get; private set; }

        #endregion Properties

        /// <summary>
        /// Populates the data starting from data from a line of the xml from the
        /// European Central Bank.
        /// </summary>
        /// <param name="date">The date from the xml to parse, as a string.</param>
        /// <param name="value">The value from the xml to parse, as a string.</param>
        public void ParseData(string date, string value)
        {
            // Format used by the European Central Bank for dates.
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy-MM-dd";
            dateFormat.DateSeparator = "-";

            // Format used by European Central Bank for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ".";
            doubleFormat.NumberGroupSeparator = ",";

            // Converts the date.
            this.Date = Convert.ToDateTime(date, dateFormat);

            // Then converts the value.
            this.Value = Convert.ToDouble(value, doubleFormat);
        }
    }
}
