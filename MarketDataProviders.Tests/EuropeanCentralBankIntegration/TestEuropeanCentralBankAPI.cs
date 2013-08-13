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
using EuropeanCentralBankIntegration;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration
{
    /// <summary>
    /// Tests the European Central Bank APIs provided
    /// by <see cref="EuropeanCentralBankAPI"/>.
    /// </summary>
    [TestFixture]
    public class TestEuropeanCentralBankAPI
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
        /// </summary>
        [Test]
        public void TestRequestOneEntry()
        {
            List<EuropeanCentralBankQuote> quotes = EuropeanCentralBankAPI.GetHistoricalQuotes("ZAR",
                                                                                   new DateTime(2011, 1, 31),
                                                                                   new DateTime(2011, 1, 31));

            Assert.AreEqual(1, quotes.Count);
            Assert.AreEqual(new DateTime(2011, 1, 31), quotes[0].Date);
            Assert.AreEqual(9.8458, quotes[0].Value, 0.0001);
        }

        /// <summary>
        /// Tests requesting more than one entry and checks values correspond.
        /// </summary>
        [Test]
        public void TestRequestMultipleEntry()
        {
            List<EuropeanCentralBankQuote> quotes = EuropeanCentralBankAPI.GetHistoricalQuotes("ZAR",
                                                                                   new DateTime(2011, 1, 31),
                                                                                   new DateTime(2011, 2, 1));

            Assert.AreEqual(2, quotes.Count);

            Assert.AreEqual(new DateTime(2011, 2, 1), quotes[1].Date);
            Assert.AreEqual(9.8480, quotes[1].Value, 0.0001);

            Assert.AreEqual(new DateTime(2011, 1, 31), quotes[0].Date);
            Assert.AreEqual(9.8458, quotes[0].Value, 0.0001);
        }
    }
}
