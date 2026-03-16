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
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.Parsing;
using NUnit.Framework;

namespace MarketDataProviders.Tests.EuropeanCentralBankIntegration.Estr.IntegrationTests;

/// <summary>
/// Tests EstrParser.FilterByDateRange() method to verify it correctly
/// filters quotes between start and end dates
/// </summary>
[TestFixture]
public class EstrParserFilterIntegrationTest
{
    // SUT
    private EstrParser _parser;
    private List<EstrQuoteDto> _testQuotes;

    [SetUp]
    public void Setup()
    {
        _parser = new EstrParser();
            
        _testQuotes =
        [
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 1), ObsValue = 0.01 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 2), ObsValue = 0.02 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 3), ObsValue = 0.03 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 4), ObsValue = 0.04 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 5), ObsValue = 0.05 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 6), ObsValue = 0.06 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 7), ObsValue = 0.07 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 8), ObsValue = 0.08 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 9), ObsValue = 0.09 },
            new EstrQuoteDto { TimePeriod = new DateTime(2024, 1, 10), ObsValue = 0.10 }
        ];
    }

    [Test]
    public void FilterByDateRange_WithBothDates_ReturnsOnlyQuotesBetweenDates()
    {
        // Arrange
        DateTime startDate = new(2024, 1, 3);
        DateTime endDate = new(2024, 1, 7);

        // Act
        List<EstrQuoteDto> result = _parser.FilterByDateRange(_testQuotes, startDate, endDate).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(5), 
            "Should return exactly 5 quotes (Jan 3, 4, 5, 6, 7)");
        Assert.Multiple(() =>
        {
            Assert.That(result.All(q => q.TimePeriod >= startDate), Is.True,
                "All quotes should be >= start date");

            Assert.That(result.All(q => q.TimePeriod <= endDate), Is.True,
                "All quotes should be <= end date");

            Assert.That(result[0].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 3)));
            Assert.That(result[4].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 7)));
        });
    }

    [Test]
    public void FilterByDateRange_WithOnlyStartDate_ReturnsQuotesFromStartDateOnwards()
    {
        // Arrange
        DateTime startDate = new DateTime(2024, 1, 8);

        // Act
        List<EstrQuoteDto> result = _parser.FilterByDateRange(_testQuotes, startDate: startDate).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3), 
            "Should return 3 quotes (Jan 8, 9, 10)");
        Assert.Multiple(() =>
        {
            Assert.That(result.All(q => q.TimePeriod >= startDate), Is.True,
                "All quotes should be >= start date");

            Assert.That(result[0].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 8)));
            Assert.That(result[2].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 10)));
        });
    }

    [Test]
    public void FilterByDateRange_WithOnlyEndDate_ReturnsQuotesUpToEndDate()
    {
        // Arrange
        DateTime endDate = new(2024, 1, 3);

        // Act
        List<EstrQuoteDto> result = _parser.FilterByDateRange(_testQuotes, endDate: endDate).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3), 
            "Should return 3 quotes (Jan 1, 2, 3)");
        Assert.Multiple(() =>
        {
            Assert.That(result.All(q => q.TimePeriod <= endDate), Is.True,
                "All quotes should be <= end date");

            Assert.That(result[0].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 1)));
            Assert.That(result[2].TimePeriod, Is.EqualTo(new DateTime(2024, 1, 3)));
        });
    }

    [Test]
    public void FilterByDateRange_WithNullDates_ReturnsAllQuotes()
    {
        // Act
        List<EstrQuoteDto> result = _parser.FilterByDateRange(_testQuotes).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(10), 
            "Should return all 10 quotes when no filters are applied");
    }
}