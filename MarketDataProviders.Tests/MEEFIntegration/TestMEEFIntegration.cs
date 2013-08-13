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

namespace MarketDataProviders.Tests.MEEFIntegration
{
    /// <summary>
    /// Tests the Fairmat interface to MEEF API provided by
    /// <see cref="MEEFIntegration"/>.
    /// </summary>
    [TestFixture]
    public class TestMEEFIntegration
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
            global::MEEFIntegration.MEEFIntegration wrapper = new global::MEEFIntegration.MEEFIntegration();
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
            global::MEEFIntegration.MEEFIntegration wrapper = new global::MEEFIntegration.MEEFIntegration();
            IMarketData data;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GRF";
            query.Date = new DateTime(2013, 6, 3);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "close";

            Status status = wrapper.GetMarketData(query, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(data.TimeStamp, new DateTime(2013, 6, 3));
            Assert.That(data is Scalar);
            Assert.AreEqual((data as Scalar).Value, 28, 1);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// The values of the ticker are approximate.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {
            global::MEEFIntegration.MEEFIntegration wrapper = new global::MEEFIntegration.MEEFIntegration();
            IMarketData[] data;
            DateTime[] dates;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "GRF";
            query.Date = new DateTime(2013, 6, 3);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "close";

            Status status = wrapper.GetTimeSeries(query, new DateTime(2013, 6, 4), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(data.Length, 2);
            Assert.AreEqual(dates.Length, 2);

            Assert.AreEqual(data[0].TimeStamp, new DateTime(2013, 6, 3));
            Assert.AreEqual(data[1].TimeStamp, new DateTime(2013, 6, 4));
            Assert.That(data[0] is Scalar);
            Assert.That(data[1] is Scalar);

            Assert.AreEqual((data[0] as Scalar).Value, 28, 1);
            Assert.AreEqual((data[1] as Scalar).Value, 29, 1);
        }

        /// <summary>
        /// Tests the ticker list request. This test might change
        /// if symbols starting with G are added or removed.
        /// </summary>
        [Test]
        public void TestTickerList()
        {
            global::MEEFIntegration.MEEFIntegration wrapper = new global::MEEFIntegration.MEEFIntegration();
            List<ISymbolDefinition> data = new List<ISymbolDefinition>(wrapper.SupportedTickers("G"));

            // Should contain GAS, GAM, GRF. So 3 elements.
            Assert.AreEqual(data.Count, 3);
            
            // Check the actual elements.
            Assert.IsTrue(data.Exists(x => (x.Name == "GAS" && x.Description == "MEEF Market Equity")));
            Assert.IsTrue(data.Exists(x => (x.Name == "GAM" && x.Description == "MEEF Market Equity")));
            Assert.IsTrue(data.Exists(x => (x.Name == "GRF" && x.Description == "MEEF Market Equity")));
        }


        [Test]
        public void TestGetCallPriceMarketData()
        {
            global::MEEFIntegration.MEEFIntegration wrapper = new global::MEEFIntegration.MEEFIntegration();
                
            MarketDataQuery mdq= new MarketDataQuery();
            mdq.Ticker="BBVA";
            mdq.Date= new DateTime(2013,07,01);
            mdq.Market="EU";
            mdq.Field = "close";
            mdq.MarketDataType=typeof(Fairmat.MarketData.CallPriceMarketData).ToString();
            IMarketData marketData;
            var status=wrapper.GetMarketData(mdq, out marketData);
            Assert.IsFalse(status.HasErrors);
        }


    }
}
