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
using EuropeanCentralBankIntegration;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration
{
    /// <summary>
    /// Tests the functionalities provided by <see cref="EuropeanCentralBankQuote"/>.
    /// Primarily it's construction and parsing are tested.
    /// </summary>
    [TestFixture]
    public class TestEuropeanCentralBankQuote
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
        /// Gets a simple date string to be used for testing the parser.
        /// </summary>
        /// <returns>A simple string containing a date.</returns>
        private string GetSampleDate()
        {
            return "2010-01-25";
        }

        /// <summary>
        /// Gets a simple value string to be used for testing the parser.
        /// </summary>
        /// <returns>A simple string containing a value.</returns>
        private string GetSampleValue()
        {
            return "123.45";
        }

        /// <summary>
        /// Tries to construct an empty <see cref="EuropeanCentralBankQuote"/>
        /// and parse a string.
        /// </summary>
        [Test]
        public void TestParsing()
        {
            EuropeanCentralBankQuote quote = new EuropeanCentralBankQuote();
            quote.ParseData(GetSampleDate(), GetSampleValue());
        }

        /// <summary>
        /// Tries to construct a <see cref="EuropeanCentralBankQuote"/>
        /// with the data to be parsed.
        /// </summary>
        [Test]
        public void TestConstruction()
        {
            new EuropeanCentralBankQuote(GetSampleDate(), GetSampleValue());
        }

        /// <summary>
        /// Tries to construct a <see cref="EuropeanCentralBankQuote"/>,
        /// parse a string and check that values correspond.
        /// </summary>
        [Test]
        public void TestParsingResult()
        {
            EuropeanCentralBankQuote quote = new EuropeanCentralBankQuote(GetSampleDate(), GetSampleValue());
            Assert.AreEqual(new DateTime(2010, 1, 25), quote.Date);
            Assert.AreEqual(123.45, quote.Value);
        }
    }
}
