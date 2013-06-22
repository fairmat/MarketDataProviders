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
using NUnit.Framework;
using YahooFinanceIntegration;

namespace MarketDataProviders.Tests.YahooFinanceIntegration
{
    /// <summary>
    /// Tests the Yahoo! Finance request APIs provided
    /// by <see cref="YahooFinanceAPI"/>.
    /// </summary>
    [TestFixture]
    public class TestYahooFinanceAPI
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
        /// Tests requesting a single entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// </summary>
        [Test]
        public void TestRequestOneEntry()
        {
            List<YahooHistoricalQuote> quotes = YahooFinanceAPI.GetHistoricalQuotes("GOOG",
                                                                                   new DateTime(2011, 1, 31),
                                                                                   new DateTime(2011, 1, 31));

            Assert.AreEqual(quotes.Count, 1);
            Assert.AreEqual(quotes[0].Date, new DateTime(2011, 1, 31));
            Assert.AreEqual(quotes[0].Open, 603, 1);
            Assert.AreEqual(quotes[0].Close, 600, 1);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {
            List<YahooHistoricalQuote> quotes = YahooFinanceAPI.GetHistoricalQuotes("GOOG",
                                                                                   new DateTime(2011, 1, 31),
                                                                                   new DateTime(2011, 2, 1));

            Assert.AreEqual(quotes.Count, 2);

            Assert.AreEqual(quotes[0].Date, new DateTime(2011, 2, 1));
            Assert.AreEqual(quotes[0].Open, 604, 1);
            Assert.AreEqual(quotes[0].Close, 611, 1);

            Assert.AreEqual(quotes[1].Date, new DateTime(2011, 1, 31));
            Assert.AreEqual(quotes[1].Open, 603, 1);
            Assert.AreEqual(quotes[1].Close, 600, 1);
        }
    }
}
