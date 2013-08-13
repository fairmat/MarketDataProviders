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
using DVPLI;
using DVPLI.MarketDataTypes;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration
{
    /// <summary>
    /// Tests the Fairmat interface to the European Central Bank API provided by
    /// <see cref="EuropeanCentralBankIntegration"/>.
    /// </summary>
    [TestFixture]
    public class TestEuropeanCentralBankIntegration
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
            global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration wrapper = new global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration();
            Status status = wrapper.TestConnectivity();
            Assert.That(!status.HasErrors, status.ErrorMessage);
        }

        /// <summary>
        /// Tests requesting a single entry and checks values correspond.
        /// </summary>
        [Test]
        public void TestRequestOneEntry()
        {
            global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration wrapper = new global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration();
            IMarketData data;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "EUCFZAR";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "close";

            Status status = wrapper.GetMarketData(query, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(new DateTime(2011, 1, 31), data.TimeStamp);
            Assert.That(data is Scalar);
            Assert.AreEqual(9.8458, (data as Scalar).Value, 0.0001);

            query.Ticker = "EURZAR";

            status = wrapper.GetMarketData(query, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(new DateTime(2011, 1, 31), data.TimeStamp);
            Assert.That(data is Scalar);
            Assert.AreEqual(9.8458, (data as Scalar).Value, 0.0001);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {
            global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration wrapper = new global::EuropeanCentralBankIntegration.EuropeanCentralBankIntegration();
            IMarketData[] data;
            DateTime[] dates;
            MarketDataQuery query = new MarketDataQuery();
            query.Ticker = "EUCFZAR";
            query.Date = new DateTime(2011, 1, 31);
            query.MarketDataType = typeof(Scalar).ToString();
            query.Field = "close";

            Status status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[1].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[0].TimeStamp);
            Assert.That(data[1] is Scalar);
            Assert.That(data[0] is Scalar);

            Assert.AreEqual(9.8480, (data[1] as Scalar).Value, 0.0001);
            Assert.AreEqual(9.8458, (data[0] as Scalar).Value, 0.0001);

            query.Ticker = "EURZAR";

            status = wrapper.GetTimeSeries(query, new DateTime(2011, 2, 1), out dates, out data);

            Assert.That(!status.HasErrors, status.ErrorMessage);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(2, dates.Length);

            Assert.AreEqual(new DateTime(2011, 2, 1), data[1].TimeStamp);
            Assert.AreEqual(new DateTime(2011, 1, 31), data[0].TimeStamp);
            Assert.That(data[1] is Scalar);
            Assert.That(data[0] is Scalar);

            Assert.AreEqual(9.8480, (data[1] as Scalar).Value, 0.0001);
            Assert.AreEqual(9.8458, (data[0] as Scalar).Value, 0.0001);
        }
    }
}
