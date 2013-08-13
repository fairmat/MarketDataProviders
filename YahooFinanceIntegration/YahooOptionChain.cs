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
using System.Collections.Generic;

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Represents an Option Chain, It's just a series of options
    /// maturities for a certain expiration date and ticker symbol.
    /// </summary>
    /// <remarks>
    /// This is used to deserialize messages from Yahoo! QML.
    /// </remarks>
    [Serializable]
    [System.Xml.Serialization.XmlRoot("optionsChain")]
    public class YahooOptionChain
    {
        /// <summary>
        /// Gets or sets the expiration date of this series of options.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("expiration")]
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Gets or sets Ticker symbol of this series of options.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the list of the options for this ticker symbol and expiration date.
        /// </summary>
        [System.Xml.Serialization.XmlElement("option")]
        public List<YahooOption> Options { get; set; }
    }
}
