/* Copyright (C) 2026 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
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
using System.Collections.Generic;
using System.Linq;
using DVPLI;
using DVPLI.Enums;
using DVPLI.Interfaces;
using DVPLI.MarketDataTypes;
using EuropeanCentralBankIntegration.Estr.Api;
using EuropeanCentralBankIntegration.Estr.Constants;
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.Enums;

namespace EuropeanCentralBankIntegration.Estr
{
    public class EuropeanCentralBankEstrIntegration : IMarketDataProvider, IDescription, ITickersInfo,
        IMarketDataIdentifierInfoProvider
    {
        /// <summary>
        /// Test whether the connection to the ECB API is functional
        /// </summary>
        /// <returns>A Status type object (DVPLI type) representing whether the connection is usable</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Status TestConnectivity()
        {
            try
            {
                IEnumerable<string> responseLines =
                    EuropeanCentralBankEstrApiClient.GetEstrMarketDataCsvBy(DataPortal.DailyBusinessWeek)
                        .GetAwaiter().GetResult();

                if (responseLines == null || !responseLines.Any())
                {
                    return new Status()
                    {
                        HasErrors = true,
                        ErrorMessage = "ESTR API call returned 2XX OK, but payload is either missing or degraded"
                    };
                }
            }
            catch (Exception e)
            {
                return new Status()
                {
                    HasErrors = true,
                    ErrorMessage = "ESTR API call threw exception with message: " + e.Message
                };
            }

            return new Status()
            {
                HasErrors = false
            };
        }

        /// <summary>
        /// Gets the market data from a single day.
        /// </summary>
        /// <param name="mdq">
        /// A <see cref="MarketDataQuery"/> with the data request.
        /// </param>
        /// <param name="marketData">
        /// In case of success, the requested market data as <see cref="IMarketData"/>.
        /// </param>
        /// <returns>
        /// A <see cref="RefreshStatus"/> indicating if the query was successful.
        /// </returns>
        public RefreshStatus GetMarketData(MarketDataQuery mdq, out IMarketData marketData)
        {
            RefreshStatus status =
                GetTimeSeries(mdq, mdq.Date, out DateTime[] dates, out IMarketData[] marketDataArray);

            if (status.HasErrors)
            {
                marketData = null;
                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage = status.ErrorMessage
                };
            }

            if (marketDataArray.Length != 1 || dates.Length != 1 || dates[0] != mdq.Date)
            {
                marketData = null;
                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage = "GetMarketData: Requested date or Market Data not available."
                };
            }

            marketData = marketDataArray[0];
            return new RefreshStatus()
            {
                HasErrors = false,
                ErrorMessage = status.ErrorMessage
            };
        }

        /// <summary>
        /// Gets a series of Historical Market Data from the starting date
        /// to the end date.
        /// <para>
        /// Note about the return type: info about dates and market data gets passed through
        /// <c>out</c> parameters and side effects. Types are not nullable? because of DVPLI
        /// constraints.
        /// </para>
        /// </summary>
        /// <param name="mdq">
        /// A <see cref="MarketDataQuery"/> with the data request.
        /// </param>
        /// <param name="end">
        /// A <see cref="DateTime"/> with the ending date of the period to fetch data from.
        /// </param>
        /// <param name="dates">
        /// In case of success, a list of the dates data was fetched from in the requested period.
        /// </param>
        /// <param name="marketData">
        /// In case of success, a list of the fetched market data day
        /// by day corresponding to <see cref="dates"/>.
        /// </param>
        /// <returns>
        /// A <see cref="RefreshStatus"/> indicating if the query was successful.
        /// </returns>
        public RefreshStatus GetTimeSeries(MarketDataQuery mdq, DateTime end, out DateTime[] dates,
            out IMarketData[] marketData)
        {
            // Should never happen, but better safe than sorry
            if (mdq is null)
            {
                throw new ArgumentNullException(nameof(mdq));
            }

            if (mdq.Field != "close")
            {
                dates = null;
                marketData = null;

                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage =
                        $"GetTimeSeries: Market data not available (only close values are available, {mdq.Field} was requested)"
                };
            }

            if (mdq.MarketDataType != typeof(Scalar).ToString())
            {
                dates = null;
                marketData = null;

                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage =
                        $"Only Scalar requests are supported. Your request was of unsupported type {mdq.MarketDataType}"
                };
            }

            IEnumerable<EstrQuoteDto> quotes;
            try
            {
                quotes = EuropeanCentralBankEstrApiClient.GetEstrMarketDataInRange(
                        dataPortal: DataPortal.DailyBusinessWeek,
                        startDate: mdq.Date,
                        endDate: end)
                    .GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                marketData = null;
                dates = null;

                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage =
                        $"GetTimeSeries: Market data not available due to problems with the European Central Bank service. Exception: {e}"
                };
            }

            // Avoid multiple enumeration: converting to List<T> to speed up lookups
            // (assuming .NET 10 CoreCLR auto devirtualization does not work here)
            List<EstrQuoteDto> quotesList = quotes.ToList();

            if (!quotesList.Any())
            {
                marketData = null;
                dates = null;

                return new RefreshStatus()
                {
                    HasErrors = true,
                    ErrorMessage =
                        $"GetTimeSeries: Market data not available due to problems with the European Central Bank service. Returned data was null or empty"
                };
            }

            dates = quotesList.Select(q => q.TimePeriod).ToArray();
            marketData = quotesList.Select(q => (IMarketData)new Scalar { Value = q.ObsValue }).ToArray();

            return new RefreshStatus()
            {
                HasErrors = false
            };
        }

        /// <summary>
        /// Unused since the API endpoint we are calling is public, and it does not require
        /// any authentication. Field is compulsory to implement DVPLI interfaces.
        /// Any set will be ignored.
        /// user and password would be represented as "user;pwd"
        /// </summary>
        public string Credentials
        {
            set { }
        }

        /// <summary>
        /// The name that will appear in the menu item
        /// </summary>
        public string Description => "European Central Bank ESTR (Euro short-term rate)";

        /// <summary>
        /// Enumerate the tickers that are supported by this functionality.
        /// </summary>
        /// <param name="filter">Unused in this case, optional and defaulting to null</param>
        /// <returns>Array of supported ticker</returns>
        public ISymbolDefinition[] SupportedTickers(string filter = null)
        {
            List<ISymbolDefinition> tickers = new List<ISymbolDefinition>
            {
                new SymbolDefinition(
                    name: "European Central Bank Euro Short-Term Rate",
                    description: "Euro short-term rate, Daily - businessweek")
            };

            return tickers.ToArray();
        }

        /// <summary>
        /// Provides the information about the market data handled by the market data provider.
        /// </summary>
        /// <returns>A list containing the information about the market data handled.</returns>
        public IList<MarketDataIdentifierInfo> GetMarketDataIdentifierInfo()
        {
            IList<MarketDataIdentifierInfo> identifiers = new List<MarketDataIdentifierInfo>()
            {
                new MarketDataIdentifierInfo()
                {
                    Category = IdentifierCategory.EquityAndIndex,
                    Identifier = nameof(IdentifierCategory.EquityAndIndex),
                    Code = "ESTR",
                    Name = "ESTR",
                    Description = "Compounded ESTR",
                    Currency = nameof(SupportedCurrencies.EUR),
                    Visibility = false
                }
            };

            return identifiers;
        }
    }
}