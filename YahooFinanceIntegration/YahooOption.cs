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
    /// This is used to deserialize messages from Yahoo! QML.
    /// </remarks>
    [Serializable]
    public class YahooOption : MEEFIntegration.IOptionQuote
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

        public double Price
        {
            get { return this.LastPrice; }
        }

        public double Strike
        {
            get { return this.StrikePrice; }
        }

        
        /// <summary>
        /// Option Expiration. This field is loaded from option chain.
        /// </summary>
        public DateTime Maturity { get; set; }
       
        MEEFIntegration.OptionQuoteType MEEFIntegration.IOptionQuote.Type
        {
            get {
                switch (this.Type)
                {
                    case "C":
                        return MEEFIntegration.OptionQuoteType.Call;
                    case "P":
                        return MEEFIntegration.OptionQuoteType.Put;
                }
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
