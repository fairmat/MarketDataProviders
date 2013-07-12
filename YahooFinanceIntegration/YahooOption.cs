using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Represents a single option specific data.
    /// </summary>
    /// <remarks>
    /// <remarks>
    /// This is used to deserialize messages from Yahoo! QML.
    /// </remarks>
    [Serializable]
    public class YahooOption
    {
        /// <summary>
        /// The ticker symbol this options is refered to.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// The type of option (CALL or PUT).
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// The strike price of the option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("strikePrice")]
        public double StrikePrice { get; set; }

        /// <summary>
        /// The last price of the option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("lastPrice")]
        public double LastPrice { get; set; }

        /// <summary>
        /// The amount of change of last Price.
        /// </summary>
        [System.Xml.Serialization.XmlElement("change")]
        public double Change { get; set; }

        /// <summary>
        /// The direction of change of last price (down, up).
        /// </summary>
        [System.Xml.Serialization.XmlElement("changeDir")]
        public string ChangeDir { get; set; }

        /// <summary>
        /// The bid price for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("bid")]
        public double Bid { get; set; }

        /// <summary>
        /// The ask price for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("ask")]
        public double Ask { get; set; }

        /// <summary>
        /// The volume for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("vol")]
        public int Vol { get; set; }

        /// <summary>
        /// The open Int for this option.
        /// </summary>
        [System.Xml.Serialization.XmlElement("openInt")]
        public int OpenInt { get; set; }
    }
}
