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
using System.Linq;
using System.Text;
using NUnit.Framework;
using YahooFinanceIntegration;
using DVPLI;
using DVPLI.MarketDataTypes;

namespace MarketDataProviders.Tests.YahooFinanceIntegration
{
    /// <summary>
    /// Tests the Fairmat interface to Yahoo! Finance API provided by
    /// <see cref="YahooFinanceIntegration"/>.
    /// </summary>
    [TestFixture]
    class TestYahooFinanceIntegration
    {
        /// <summary>
        /// Initializes the backend to run the tests.
        /// </summary>
        [SetUp]
        public void Init()
        {
            TestCommon.TestInitialization.CommonInitialization();
        }

        [Test]
        public void TestConnectivity()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            Status status = wrapper.TestConnectivity();
            Assert.That(!status.HasErrors, status.ErrorMessage);
        }

        /// <summary>
        /// Tests requesting a single entry and checks values correspond.
        /// The values of the ticker are aproximated.
        /// </summary>
        [Test]
        public void TestRequestOneEntry()
        {
            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            IMarketData myData;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "open";

            Status status = wrapper.GetMarketData(query, out myData);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(myData.TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(myData is Scalar);
            Assert.AreEqual((myData as Scalar).Value, 603, 1);

            query.Field = "close";

            status = wrapper.GetMarketData(query, out myData);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(myData.TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(myData is Scalar);
            Assert.AreEqual((myData as Scalar).Value, 600, 1);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// The values of the ticker are aproximated.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {

            global::YahooFinanceIntegration.YahooFinanceIntegration wrapper = new global::YahooFinanceIntegration.YahooFinanceIntegration();
            IMarketData[] myData;
            DateTime[] myDates;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GOOG";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "open";

            Status status = wrapper.GetTimeSeries(query,  new DateTime(2011, 2, 1), out myDates, out myData);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(myData.Length, 2);
            Assert.AreEqual(myDates.Length, 2);

            Assert.AreEqual(myData[0].TimeStamp, new DateTime(2011, 2, 1));
            Assert.AreEqual(myData[1].TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(myData[0] is Scalar);
            Assert.That(myData[1] is Scalar);

            Assert.AreEqual((myData[0] as Scalar).Value, 604, 1);
            Assert.AreEqual((myData[1] as Scalar).Value, 603, 1);

            query.Field = "close";

            status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out myDates, out myData);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(myData.Length, 2);
            Assert.AreEqual(myDates.Length, 2);

            Assert.AreEqual(myData[0].TimeStamp, new DateTime(2011, 2, 1));
            Assert.AreEqual(myData[1].TimeStamp, new DateTime(2011, 1, 31));
            Assert.That(myData[0] is Scalar);
            Assert.That(myData[1] is Scalar);

            Assert.AreEqual((myData[0] as Scalar).Value, 611, 1);
            Assert.AreEqual((myData[1] as Scalar).Value, 600, 1);
        }
    }
}
