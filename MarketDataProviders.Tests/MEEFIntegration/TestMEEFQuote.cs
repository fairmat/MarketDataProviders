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
using NUnit.Framework;
using MEEFIntegration;

namespace MarketDataProviders.Tests.MEEFIntegration
{
    /// <summary>
    /// Tests the functionalities provided by <see cref="MEEFHistoricalQuote"/>.
    /// Primarily it's construction and parsing are tested.
    /// </summary>
    [TestFixture]
    public class TestMEEFHistoricalQuote
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
        /// Tries to construct an empty <see cref="MEEFHistoricalQuote"/>
        /// and parse a string.
        /// </summary>
        [Test]
        public void TestParsing()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote();
            quote.ParseCSVLine(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>
        /// with the data to be parsed.
        /// </summary>
        [Test]
        public void TestConstruction()
        {
            new MEEFHistoricalQuote(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct an empty <see cref="MEEFHistoricalQuote"/>
        /// and parse a string. Uses the old format.
        /// </summary>
        [Test]
        public void TestParsingOldFormat()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote();
            quote.ParseOldFormatCSVLine(GetSampleCSVLineOldFormat());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>
        /// with the data to be parsed. Uses the old format.
        /// </summary>
        [Test]
        public void TestConstructionOldFormat()
        {
            new MEEFHistoricalQuote(GetSampleCSVLineOldFormat(), true);
        }

        /// <summary>
        /// Tries to construct an empty <see cref="MEEFHistoricalQuote"/>
        /// and parse a string. Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestParsingOldestFormat()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote();
            quote.ParseOldFormatCSVLine(GetSampleCSVLineOldestFormat());
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>
        /// with the data to be parsed. Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestConstructionOldestFormat()
        {
            new MEEFHistoricalQuote(GetSampleCSVLineOldestFormat(), true);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// </summary>
        [Test]
        public void TestParsingResult()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote(GetSampleCSVLine());
            Assert.AreEqual(quote.SessionDate, new DateTime(2013, 07, 01));
            Assert.AreEqual(quote.ContractGroup, "C2");
            Assert.AreEqual(quote.ContractCode, "AAABC");
            Assert.AreEqual(quote.ContractSubgroupCode, "12");
            Assert.AreEqual(quote.CFICode, "XIINAA");
            Assert.AreEqual(quote.StrikePrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.MaturityDate, new DateTime(2030, 12, 31));
            Assert.AreEqual(quote.BidPrice, 6.7895f, 0.0001);
            Assert.AreEqual(quote.AskPrice, 9.147f, 0.0001);
            Assert.AreEqual(quote.HighPrice, 12.2542f, 0.0001);
            Assert.AreEqual(quote.LowPrice, 1.8745f, 0.0001);
            Assert.AreEqual(quote.LastPrice, 9.1254f, 0.0001);
            Assert.AreEqual(quote.SettlPrice, 9.1254f, 0.0001);
            Assert.AreEqual(quote.SettlVolatility, 0.0f, 0.0001);
            Assert.AreEqual(quote.SettlDelta, 1.0f, 0.0001);
            Assert.AreEqual(quote.TotalRegVolume, 0);
            Assert.AreEqual(quote.NumberOfTrades, 0);
            Assert.AreEqual(quote.OpenInterest, 0);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// Uses the old format.
        /// </summary>
        [Test]
        public void TestParsingResultOldFormat()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote(GetSampleCSVLineOldFormat(), true);
            Assert.AreEqual(quote.SessionDate, new DateTime(2004, 07, 01));
            Assert.AreEqual(quote.ContractGroup, "AAB");
            Assert.AreEqual(quote.MaturityDate, new DateTime(2004, 07, 02));
            Assert.AreEqual(quote.StrikePrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.ContractCode, "AAAAAEXE");
            Assert.AreEqual(quote.BidPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.AskPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.HighPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.LowPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.LastPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.TotalRegVolume, 0);
            Assert.AreEqual(quote.SettlPrice, 34.44f, 0.0001);
            Assert.AreEqual(quote.OpenInterest, 0);
            Assert.AreEqual(quote.SettlVolatility, 15.75f, 0.0001);

            // Fields missing from this format.
            Assert.AreEqual(quote.ContractSubgroupCode, null);
            Assert.AreEqual(quote.CFICode, null);
            Assert.AreEqual(quote.SettlDelta, 0.0f, 0.0001);
            Assert.AreEqual(quote.NumberOfTrades, 0);
        }

        /// <summary>
        /// Tries to construct a <see cref="MEEFHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// Uses the old format with strings causing issues.
        /// </summary>
        [Test]
        public void TestParsingResultOldestFormat()
        {
            MEEFHistoricalQuote quote = new MEEFHistoricalQuote(GetSampleCSVLineOldestFormat(), true);
            Assert.AreEqual(quote.SessionDate, new DateTime(1999, 01, 04));
            Assert.AreEqual(quote.ContractGroup, "AAB");
            Assert.AreEqual(quote.MaturityDate, new DateTime(1999, 01, 05));
            Assert.AreEqual(quote.StrikePrice, 22.02f, 0.0001);
            Assert.AreEqual(quote.ContractCode, "AC 4578C");
            Assert.AreEqual(quote.BidPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.AskPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.HighPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.LowPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.LastPrice, 0.0f, 0.0001);
            Assert.AreEqual(quote.TotalRegVolume, 0);
            Assert.AreEqual(quote.SettlPrice, 22.32f, 0.0001);
            Assert.AreEqual(quote.OpenInterest, 0);
            Assert.AreEqual(quote.SettlVolatility, 19.00f, 0.0001);

            // Fields missing from this format.
            Assert.AreEqual(quote.ContractSubgroupCode, null);
            Assert.AreEqual(quote.CFICode, null);
            Assert.AreEqual(quote.SettlDelta, 0.0f, 0.0001);
            Assert.AreEqual(quote.NumberOfTrades, 0);
        }

    }
}
