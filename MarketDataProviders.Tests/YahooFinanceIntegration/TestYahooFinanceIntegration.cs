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
using DVPLI;
using DVPLI.MarketDataTypes;
using NUnit.Framework;

namespace MarketDataProviders.Tests.YahooFinanceIntegration
{
    /// <summary>
    /// Tests the Fairmat interface to Yahoo! Finance API provided by
    /// <see cref="YahooFinanceIntegration"/>.
    /// </summary>
    [TestFixture]
    [Ignore("Yahoo! doesn't have these things anymore")]
    public class TestYahooFinanceIntegration
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
        /// Tests the TestConnectivity() method of the
        /// interface and checks if it works without errors.
        /// </summary>
        [Test]
        public void TestConnectivity()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            Status status = wrapper.TestConnectivity();
            Assert.That(!status.HasErrors, status.ErrorMessage);
        }

        /// <summary>
        /// Tests requesting a single entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// </summary>
        [Test]
        public void TestRequestOneEntry()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            IMarketData data;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "open";

            Status status = wrapper.GetMarketData(query, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(data.TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(data is Scalar);
            Assert.AreEqual((data as Scalar).Value, 603, 1);

            query.Field = "close";

            status = wrapper.GetMarketData(query, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(data.TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(data is Scalar);
            Assert.AreEqual((data as Scalar).Value, 600, 1);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            IMarketData[] data;
            DateTime[] dates;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "open";

            Status status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[0].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[1].TimeStamp);
            Assert.That(data[0] is Scalar);
            Assert.That(data[1] is Scalar);

            Assert.AreEqual(604, (data[0] as Scalar).Value, 1);
            Assert.AreEqual(603, (data[1] as Scalar).Value, 1);

            query.Field = "close";

            status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[0].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[1].TimeStamp);
            Assert.That(data[0] is Scalar);
            Assert.That(data[1] is Scalar);

            Assert.AreEqual(611, (data[0] as Scalar).Value, 1);
            Assert.AreEqual(600, (data[1] as Scalar).Value, 1);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// This function checks also conversions from USD to EUR.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntryCurrencyConversion()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            IMarketData[] data;
            DateTime[] dates;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "open";
            query.Market = "EU";

            Status status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[1].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[0].TimeStamp);
            Assert.That(data[0] is Scalar);
            Assert.That(data[1] is Scalar);

            Assert.AreEqual(441, (data[1] as Scalar).Value, 1);
            Assert.AreEqual(446, (data[0] as Scalar).Value, 1);

            query.Field = "close";

            status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[1].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[0].TimeStamp);
            Assert.That(data[0] is Scalar);
            Assert.That(data[1] is Scalar);

            Assert.AreEqual(446, (data[1] as Scalar).Value, 1);
            Assert.AreEqual(444, (data[0] as Scalar).Value, 1);
        }

        /// <summary>
        /// Tests the ticker list request. This test might change
        /// if symbols starting with G are added or removed.
        /// </summary>
        /// <remarks>
        /// It seems the output of Yahoo! Finance is quite variable and might give different
        /// results from time to time. So in case this fails check with
        /// http://d.yimg.com/autoc.finance.yahoo.com/autoc?query=G&amp;callback=YAHOO.Finance.SymbolSuggest.ssCallback
        /// if it has the same elements.
        /// </remarks>
        [Test]
        public void TestTickerList()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            List<ISymbolDefinition> data = new List<ISymbolDefinition>(wrapper.SupportedTickers("G"));

            // Check the actual elements.
            Assert.IsTrue(data.Exists(x => (x.Name == "G" && x.Description == "Yahoo! Finance Equity")));
            Assert.IsTrue(data.Exists(x => (x.Name == "GOOG" && x.Description == "Yahoo! Finance Equity")));
            Assert.IsTrue(data.Exists(x => (x.Name == "GE" && x.Description == "Yahoo! Finance Equity")));
            Assert.IsTrue(data.Exists(x => (x.Name == "GLD" && x.Description == "Yahoo! Finance ETF")));
        }

        /// <summary>
        /// Gets the Call price market data and checks if the request was succesful.
        /// </summary>
        [Test]
        public void TestGetCallPriceMarketData()
        {
            var wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();

            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Fairmat.MarketData.CallPriceMarketData).ToString();
            query.Field = "close";

            IMarketData data;
            var status = wrapper.GetMarketData(query, out data);
            Assert.IsFalse(status.HasErrors);
        }

        /// <summary>
        /// This test tests the reliability of the system
        /// by trying to extract data for an entire week.
        /// </summary>
        [Test]
        public void TestGetCallPriceMarketDataSeries()
        {
            var wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            var date0 = new DateTime(2011, 1, 31);
            for (int d = 0; d < 5; d++)
            {
                MarketDataQuery query = new MarketDataQuery();
                query.Ticker = "GOOG";
                query.Date = date0.AddDays(d);
                query.MarketDataType = typeof(Fairmat.MarketData.CallPriceMarketData).ToString();
                query.Field = "close";

                IMarketData data;
                var status = wrapper.GetMarketData(query, out data);
                Assert.IsFalse(status.HasErrors);
            }
        }
    }
}
