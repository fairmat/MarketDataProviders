/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Stefano Angeleri (stefano.angeleri@fairmat.com)
 *            Matteo Tesser (matteo.tesser@fairmat.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionQuotes;

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Represents a single option specific data.
    /// </summary>
    /// <remarks>
    /// This is used to deserialize messages from Yahoo! QML.
    /// </remarks>
    [Serializable]
    public class YahooOption : IOptionQuote
    {
        /// <summary>
        /// Gets or sets the ticker symbol this options is referred to.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the type of option (CALL or PUT).
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the strike price of the option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("strikePrice")]
        public double StrikePrice { get; set; }

        /// <summary>
        /// Gets or sets the last price of the option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("lastPrice")]
        public double LastPrice { get; set; }

        /// <summary>
        /// Gets or sets the amount of change of last Price.
        /// </summary>
        [System.Xml.Serialization.XmlElement("change")]
        public double Change { get; set; }

        /// <summary>
        /// Gets or sets the direction of change of last price (down, up).
        /// </summary>
        [System.Xml.Serialization.XmlElement("changeDir")]
        public string ChangeDir { get; set; }

        /// <summary>
        /// Gets or sets the bid price for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("bid")]
        public double Bid { get; set; }

        /// <summary>
        /// Gets or sets the ask price for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("ask")]
        public double Ask { get; set; }

        /// <summary>
        /// Gets or sets the volume for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("vol")]
        public double Vol { get; set; }

        /// <summary>
        /// Gets or sets the open interest for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("openInt")]
        public int OpenInt { get; set; }

        #region IOptionQuote implementation

        /// <summary>
        /// Gets the price of the option.
        /// </summary>
        public double Price
        {
            get
            {
                return this.LastPrice;
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
        /// Gets or sets the option Expiration.
        /// This field is loaded from option chain.
        /// </summary>
        public DateTime Maturity { get; set; }

        /// <summary>
        /// Gets the type of this option quote.
        /// </summary>
        OptionQuoteType IOptionQuote.Type
        {
            get
            {
                switch (this.Type)
                {
                    case "C":
                        {
                            return OptionQuoteType.Call;
                        }

                    case "P":
                        {
                            return OptionQuoteType.Put;
                        }
                }

                throw new NotImplementedException();
            }
        }

        #endregion IOptionQuote implementation
    }
}
