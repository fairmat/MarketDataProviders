/* Copyright (C) 2026 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
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
using System.Threading.Tasks;
using EuropeanCentralBankIntegration.Estr.Api;
using EuropeanCentralBankIntegration.Estr.Constants;
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.Parsing;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration.Estr.IntegrationTests;

/// <summary>
/// Tests the European Central Bank ESTR APIs
/// </summary>
[TestFixture]
public class EstrApiClientIntegrationTest
{
    [SetUp]
    public void Init()
    {
        TestCommon.TestInitialization.CommonInitialization();
    }

    [Test]
    public async Task TestGetCsvToEnumerable_ShouldCorrectlyCreateEnumerable()
    {
        // Act
        IEnumerable<string> result =
            await EuropeanCentralBankEstrApiClient.GetEstrMarketDataCsvBy(DataPortal.DailyBusinessWeek);
        IEnumerable<string> resultList = result.ToList();

        // Assert
        Assert.That(resultList, Is.Not.Null,
            "Result from the API client should not be null.");

        const string expectedHeaderLine = "KEY,FREQ,BENCHMARK_ITEM,DATA_TYPE_EST,TIME_PERIOD,OBS_VALUE";
        Assert.That(resultList, Is.Not.Empty,
            "Result from the API client should be empty.");
        Assert.That(!string.IsNullOrWhiteSpace(resultList.First()),
            "The row corresponding to the CSV header should not be empty. This likely means the CSV was not" +
            "correctly downloaded or parsed at all");
        Assert.That(resultList.First(), Is.EqualTo(expectedHeaderLine),
            "The CSV header should match the expected header value. Has the API changed?");
    }

    [Test]
    public async Task TestCsvGetAndParse_ShouldCorrectlyGetAndParseCsv()
    {
        // Act
        IEnumerable<string> csvLines =
            await EuropeanCentralBankEstrApiClient.GetEstrMarketDataCsvBy(DataPortal.DailyBusinessWeek);
        IEnumerable<EstrQuoteDto> result = EstrParser.DeserializeCsvToDto(csvLines);
        List<EstrQuoteDto> resultList = result.ToList();

        // Assert
        Assert.That(resultList, Is.Not.Null,
            "Result from the parser should not be null");
        Assert.That(resultList, Is.Not.Empty,
            "Result from the parser should not be empty");

        Assert.Multiple(() =>
        {
            Assert.That(resultList[1], Is.Not.Null,
                "Second quote should not be null");
            Assert.That(resultList[1].Key, Is.Not.Null.And.Not.Empty,
                "Second quote: Key should not be null or empty");
            Assert.That(resultList[1].Freq, Is.Not.EqualTo('\0'),
                "Second quote: Freq should not be null/default char");
            Assert.That(resultList[1].BenchmarkItem, Is.Not.Null.And.Not.Empty,
                "Second quote: BenchmarkItem should not be null or empty");
            Assert.That(resultList[1].DataTypeEst, Is.Not.Null.And.Not.Empty,
                "Second quote: DataTypeTest should not be null or empty");
        });
    }

    /// <summary>
    /// DVPLI integration currently does not support <c>async</c> methods, so we are going
    /// to have to call those APIs in a synchronous way with <c>.GetAwaiter().GetResult()</c>
    /// instead. This test checks if the process still completes successfully when it is called
    /// in this manner.
    /// </summary>
    [Test]
    public void TestCsvGetAndParseBlocking_ShouldCompleteSuccessfullyWithoutAsyncAwait()
    {
        // Act
        IEnumerable<string> csvLines =
            EuropeanCentralBankEstrApiClient.GetEstrMarketDataCsvBy(DataPortal.DailyBusinessWeek)
                .GetAwaiter().GetResult();
        IEnumerable<EstrQuoteDto> result = EstrParser.DeserializeCsvToDto(csvLines);
        List<EstrQuoteDto> resultList = result.ToList();

        // Assert
        Assert.That(resultList, Is.Not.Null,
            "Result from the parser should not be null");
        Assert.That(resultList, Is.Not.Empty,
            "Result from the parser should not be empty");

        Assert.Multiple(() =>
        {
            Assert.That(resultList[1], Is.Not.Null,
                "Second quote should not be null");
            Assert.That(resultList[1].Key, Is.Not.Null.And.Not.Empty,
                "Second quote: Key should not be null or empty");
            Assert.That(resultList[1].Freq, Is.Not.EqualTo('\0'),
                "Second quote: Freq should not be null/default char");
            Assert.That(resultList[1].BenchmarkItem, Is.Not.Null.And.Not.Empty,
                "Second quote: BenchmarkItem should not be null or empty");
            Assert.That(resultList[1].DataTypeEst, Is.Not.Null.And.Not.Empty,
                "Second quote: DataTypeTest should not be null or empty");
        });
    }

    [Test]
    public async Task TestGetMarketData_ShouldCorrectlyObtainMarketData()
    {
        // Act
        IEnumerable<EstrQuoteDto> result =
            await EuropeanCentralBankEstrApiClient.GetEstrMarketData(DataPortal.DailyBusinessWeek);

        // Assert
        IEnumerable<EstrQuoteDto> resultList = result.ToList();
        Assert.That(resultList, Is.Not.Null,
            "Result should not be null");
        Assert.That(resultList.Count(), Is.GreaterThan(0),
            "Result should have at least one item");
    }

    [Test]
    public async Task TestGetMarketDataByDateTime_ShouldGetQuotesWithinRange()
    {
        // Act
        DateTime startDate = new(2024, 1, 6);
        DateTime endDate = new(2024, 12, 31);
        IEnumerable<EstrQuoteDto> result = await EuropeanCentralBankEstrApiClient.GetEstrMarketDataInRange(
            dataPortal: DataPortal.DailyBusinessWeek,
            startDate: startDate,
            endDate: endDate);

        // Assert
        IEnumerable<EstrQuoteDto> resultList = result.ToList();
        Assert.That(resultList, Is.Not.Null,
            "Result should not be null");
        Assert.Multiple(() =>
        {
            Assert.That(resultList.Count(), Is.GreaterThan(0),
                "Result should have at least one item");
            Assert.That(resultList.First().TimePeriod, Is.GreaterThanOrEqualTo(startDate),
                "First item should not have a date that is before the queried start date");
            Assert.That(resultList.Last().TimePeriod, Is.LessThanOrEqualTo(endDate),
                "Last item should not have a date that is after the queried end date");
        });
    }
}