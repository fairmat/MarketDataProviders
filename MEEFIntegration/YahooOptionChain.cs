using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEEFIntegration
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
