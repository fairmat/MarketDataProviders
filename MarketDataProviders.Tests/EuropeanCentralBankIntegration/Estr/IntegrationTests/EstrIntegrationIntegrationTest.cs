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

using System;
using DVPLI;
using DVPLI.MarketDataTypes;
using EuropeanCentralBankIntegration.Estr;
using EuropeanCentralBankIntegration.Estr.Api;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration.Estr.IntegrationTests;

/// <summary>
/// Integration tests for <see cref="EuropeanCentralBankEstrIntegration"/>, which
/// is the integration / data ingestion layer between Fairmat's <c>DVPLI</c> library and
/// the external world. It consumes <see cref="EuropeanCentralBankEstrApiClient"/>
/// <para>
/// Proper logic testing should be done via unit testing. The main purpose of this class is to make
/// it easy to launch methods and debug them
/// </para>
/// </summary>
[TestFixture]
public class EstrIntegrationIntegrationTest
{
    // SUT
    private readonly EuropeanCentralBankEstrIntegration _integration = new();

    [Test]
    public void TestTestConnectivity_ShouldSuccessfullyConnect()
    {
        // Act
        Status status = _integration.TestConnectivity();

        // Assert
        Assert.That(status, Is.Not.Null,
            "Status should not be null");
        Assert.That(status.HasErrors, Is.EqualTo(false),
            "Status should not have errors");
    }

    [Test]
    public void TestGetMarketData_ShouldReturnPositiveRefreshStatusAndPopulateMarketData()
    {
        // Arrange
        MarketDataQuery mdq = new()
        {
            Field = "close",
            MarketDataType = "Scalar",
            // Might be flaky, currently relies on this date having a valid value in the ESTR API return payload
            // FIXME: Create a proper unit test with mocked API response eventually
            //  Needs a mocking library like NSubstitute in the solution
            Date = new DateTime(2019, 10, 01),
        };

        IMarketData marketData = new Scalar();

        // Act
        RefreshStatus status = _integration.GetMarketData(
            mdq: mdq,
            marketData: out marketData);

        // Assert
        Assert.That(status, Is.Not.Null,
            "Status should not be null");
        Assert.That(status.HasErrors, Is.EqualTo(false),
            "Status should not have errors");
        Assert.That(marketData, Is.Not.Null,
            "marketData should no longer be null, because it should have been written to at this point");
    }

    [Test]
    public void TestGetTimeSeries_ShouldCorrectlyGetQuoteInTimeRange()
    {
        // Arrange
        MarketDataQuery mdq = new()
        {
            Field = "close",
            MarketDataType = typeof(Scalar).ToString(),
            // Might be flaky, currently relies on this date having a valid value in the ESTR API return payload
            // FIXME: Create a proper unit test with mocked API response eventually
            Date = new DateTime(2019, 10, 01),
        };

        // Act
        RefreshStatus status = _integration.GetTimeSeries(
            mdq: mdq,
            end: new DateTime(2019, 10, 01),
            dates: out DateTime[] dates,
            marketData: out IMarketData[] marketData);

        // Assert
        Assert.That(status, Is.Not.Null,
            "Status should not be null");
        Assert.That(status.HasErrors, Is.EqualTo(false),
            "Status should not have errors");
        Assert.That(marketData, Is.Not.Null,
            "marketData should no longer be null, because it should have been written to at this point");
    }
}