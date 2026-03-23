/* Copyright (C) 2026 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 *
 * Author(s): Luca Bramè (luca.brame@fairmat.com)
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

using System.Collections.Generic;
using System.Linq;
using DVPLI;
using DVPLI.Enums;
using EuropeanCentralBankIntegration.Estr;
using EuropeanCentralBankIntegration.Estr.Enums;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration.Estr.UnitTests;

/// <summary>
/// Unit test for <see cref="EuropeanCentralBankEstrIntegration"/>,
/// with mocked API responses to test code logic independently
/// of the availability and format of ECB APIs
/// </summary>
[TestFixture]
public class EstrIntegrationUnitTest
{
    // SUT
    private readonly EuropeanCentralBankEstrIntegration _integration = new();

    [Test]
    public void TestGetSupportedTickers_ShouldReturnNotNull()
    {
        // Act
        ISymbolDefinition[] result = _integration.SupportedTickers();

        // Assert
        Assert.That(result, Is.Not.Null,
            "Result should not be null");
        Assert.That(result, Is.Not.Empty,
            "Result content is not empty");
    }

    [Test]
    public void TestGetDataAvailabilityInfo_ShouldReturnNotNull()
    {
        // Act
        MarketDataAccessType result = _integration.GetDataAvailabilityInfo(MarketDataCategory.EquityPrice);

        // Assert
        Assert.That(result, Is.EqualTo(MarketDataAccessType.Local),
            "In this specific case, the method should return MarketDataAccessType.Local");
    }

    [Test]
    public void TestGetMarketDataIdentifierInfo_ShouldReturnExpectedResult()
    {
        // Act
        IList<MarketDataIdentifierInfo> result = _integration.GetMarketDataIdentifierInfo();

        // Assert
        MarketDataIdentifierInfo expectedResult = new()
        {
            Category = IdentifierCategory.EquityAndIndex,
            Code = "ESTR",
            Name = "ESTR",
            Description = "Euro short-term rate, Daily - businessweek",
            Exported = true,
            Identifier = null,
            Currency = nameof(SupportedCurrencies.EUR),
            Visibility = false
        };

        Assert.That(result, Is.Not.Null,
            "Result should not be null");
        Assert.That(result.First().Code, Is.EqualTo(expectedResult.Code),
            "Returned result's code should be equal to expected result");
    }
}