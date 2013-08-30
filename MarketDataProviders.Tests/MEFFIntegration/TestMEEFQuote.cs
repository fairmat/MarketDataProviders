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
using MEFFIntegration;
using NUnit.Framework;

namespace MarketDataProviders.Tests.MEFFIntegration
{
    /// <summary>
    /// Tests the functionalities provided by <see cref="MEFFHistoricalQuote"/>.
    /// Primarily it's construction and parsing are tested.
    /// </summary>
    [TestFixture]
    public class TestMEFFHistoricalQuote
    {
        /// <summary>
        /// Initializes the backend to run the tests.
        /// </summary>
        [SetUp]
        public void Init()
        {
            TestCommon.TestInitialization.CommonInitialization();
        }

        /// <summary>
        /// Gets a simple CSV fake data line to be used for testing the parser.
        /// </summary>
        /// <returns>A simple CSV line with fake data.</returns>
        private string GetSampleCSVLine()
        {
            return "\"20130701\";\"C2\";\"AAABC\";\"12\";\"XIINAA\";0,000000;\"20301231\";6,789500;9,147000;12,254200;1,874500;9,125400;9,125400;0;1,00;0;0;0";
        }

        /// <summary>
        /// Gets a simple CSV fake data line in order format to be used for testing the parser.
        /// </summary>
        /// <returns>A simple CSV line with fake data.</returns>
        private string GetSampleCSVLineOldFormat()
        {
            return "20040701,AAB ,F,040702,    0.00,AAAAAEXE   ,    0.00,    0.00,    0.00,    0.00,    0.00,       0,   34.44,           0, 15.75,";
        }

        /// <summary>
        /// Gets a simple CSV fake data line in oldest format to be used for testing the parser.
        /// </summary>
        /// <returns>A simple CSV line with fake data.</returns>
        private string GetSampleCSVLineOldestFormat()
        {
            return "19990104,AAB ,C,990105,   22.02,AC 4578C   ,    0.00,    0.00,    0.00,    0.00,    0.00,       0,   22.32,           0, 19.00,";
        }

        /// <summary>
        /// Tries to construct an empty <see cref="MEFFHistoricalQuote"/>
        /// and parse a string.
        /// </summary>
        [Test]
        public void TestParsing()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote();
            quote.ParseCSVLine(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>
        /// with the data to be parsed.
        /// </summary>
        [Test]
        public void TestConstruction()
        {
            new MEFFHistoricalQuote(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct an empty <see cref="MEFFHistoricalQuote"/>
        /// and parse a string. Uses the old format.
        /// </summary>
        [Test]
        public void TestParsingOldFormat()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote();
            quote.ParseOldFormatCSVLine(GetSampleCSVLineOldFormat());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>
        /// with the data to be parsed. Uses the old format.
        /// </summary>
        [Test]
        public void TestConstructionOldFormat()
        {
            new MEFFHistoricalQuote(GetSampleCSVLineOldFormat(), true);
        }

        /// <summary>
        /// Tries to construct an empty <see cref="MEFFHistoricalQuote"/>
        /// and parse a string. Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestParsingOldestFormat()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote();
            quote.ParseOldFormatCSVLine(GetSampleCSVLineOldestFormat());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>
        /// with the data to be parsed. Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestConstructionOldestFormat()
        {
            new MEFFHistoricalQuote(GetSampleCSVLineOldestFormat(), true);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// </summary>
        [Test]
        public void TestParsingResult()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote(GetSampleCSVLine());
            Assert.AreEqual(new DateTime(2013, 07, 01), quote.SessionDate);
            Assert.AreEqual("C2", quote.ContractGroup);
            Assert.AreEqual("AAABC", quote.ContractCode);
            Assert.AreEqual("12", quote.ContractSubgroupCode);
            Assert.AreEqual("XIINAA", quote.CFICode);
            Assert.AreEqual(0.0f, quote.StrikePrice, 0.0001);
            Assert.AreEqual(new DateTime(2030, 12, 31), quote.MaturityDate);
            Assert.AreEqual(6.7895f, quote.BidPrice, 0.0001);
            Assert.AreEqual(9.147f, quote.AskPrice, 0.0001);
            Assert.AreEqual(12.2542f, quote.HighPrice, 0.0001);
            Assert.AreEqual(1.8745f, quote.LowPrice, 0.0001);
            Assert.AreEqual(9.1254f, quote.LastPrice, 0.0001);
            Assert.AreEqual(9.1254f, quote.SettlPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.SettlVolatility, 0.0001);
            Assert.AreEqual(1.0f, quote.SettlDelta, 0.0001);
            Assert.AreEqual(0, quote.TotalRegVolume);
            Assert.AreEqual(0, quote.NumberOfTrades);
            Assert.AreEqual(0, quote.OpenInterest);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// Uses the old format.
        /// </summary>
        [Test]
        public void TestParsingResultOldFormat()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote(GetSampleCSVLineOldFormat(), true);
            Assert.AreEqual(new DateTime(2004, 07, 01), quote.SessionDate);
            Assert.AreEqual("AAB", quote.ContractGroup);
            Assert.AreEqual(new DateTime(2004, 07, 02), quote.MaturityDate);
            Assert.AreEqual(0.0f, quote.StrikePrice, 0.0001);
            Assert.AreEqual("AAAAAEXE", quote.ContractCode);
            Assert.AreEqual(0.0f, quote.BidPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.AskPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.HighPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.LowPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.LastPrice, 0.0001);
            Assert.AreEqual(0, quote.TotalRegVolume);
            Assert.AreEqual(34.44f, quote.SettlPrice, 0.0001);
            Assert.AreEqual(0, quote.OpenInterest);
            Assert.AreEqual(15.75f, quote.SettlVolatility, 0.0001);

            // Fields missing from this format.
            Assert.AreEqual(null, quote.ContractSubgroupCode);
            Assert.AreEqual("F", quote.CFICode);
            Assert.AreEqual(0.0f, quote.SettlDelta, 0.0001);
            Assert.AreEqual(0, quote.NumberOfTrades);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEFFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestParsingResultOldestFormat()
        {
            MEFFHistoricalQuote quote = new MEFFHistoricalQuote(GetSampleCSVLineOldestFormat(), true);
            Assert.AreEqual(new DateTime(1999, 01, 04), quote.SessionDate);
            Assert.AreEqual("AAB", quote.ContractGroup);
            Assert.AreEqual(new DateTime(1999, 01, 05), quote.MaturityDate);
            Assert.AreEqual(22.02f, quote.StrikePrice, 0.0001);
            Assert.AreEqual("AC 4578C", quote.ContractCode);
            Assert.AreEqual(0.0f, quote.BidPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.AskPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.HighPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.LowPrice, 0.0001);
            Assert.AreEqual(0.0f, quote.LastPrice, 0.0001);
            Assert.AreEqual(0, quote.TotalRegVolume);
            Assert.AreEqual(22.32f, quote.SettlPrice, 0.0001);
            Assert.AreEqual(0, quote.OpenInterest);
            Assert.AreEqual(19.00f, quote.SettlVolatility, 0.0001);

            // Fields missing from this format.
            Assert.AreEqual(null, quote.ContractSubgroupCode);
            Assert.AreEqual("OC", quote.CFICode);
            Assert.AreEqual(0.0f, quote.SettlDelta, 0.0001);
            Assert.AreEqual(0, quote.NumberOfTrades);
        }
    }
}
