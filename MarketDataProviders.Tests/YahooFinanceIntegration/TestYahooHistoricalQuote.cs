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
using YahooFinanceIntegration;

namespace MarketDataProviders.Tests.YahooFinanceIntegration
{
    /// <summary>
    /// Tests the functionalities provided by <see cref="YahooHistoricalQuote"/>.
    /// Primarily it's construction and parsing are tested.
    /// </summary>
    [TestFixture]
    public class TestYahooHistoricalQuote
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
            // This is the format: Date,Open,High,Low,Close,Volume,Adj Close.
            return "2010-01-25,123.45,321.45,121.45,320.21,999,321.22";
        }

        /// <summary>
        /// Tries to construct an empty <see cref="YahooHistoricalQuote"/>
        /// and parse a string.
        /// </summary>
        [Test]
        public void TestParsing()
        {
            YahooHistoricalQuote quote = new YahooHistoricalQuote();
            quote.ParseCSVLine(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct a <see cref="YahooHistoricalQuote"/>
        /// with the data to be parsed.
        /// </summary>
        [Test]
        public void TestConstruction()
        {
            new YahooHistoricalQuote(GetSampleCSVLine());
        }

        /// <summary>
        /// Tries to construct a <see cref="YahooHistoricalQuote"/>,
        /// parse a string and check that values correspond.
        /// </summary>
        [Test]
        public void TestParsingResult()
        {
            YahooHistoricalQuote quote = new YahooHistoricalQuote(GetSampleCSVLine());
            Assert.AreEqual(new DateTime(2010, 1, 25), quote.Date);
            Assert.AreEqual(123.45f, quote.Open, 0.0001);
            Assert.AreEqual(321.45f, quote.High, 0.0001);
            Assert.AreEqual(121.45f, quote.Low, 0.0001);
            Assert.AreEqual(320.21f, quote.Close, 0.0001);
            Assert.AreEqual(999, quote.Volume);
            Assert.AreEqual(321.22f, quote.AdjClose, 0.0001);
        }
    }
}
