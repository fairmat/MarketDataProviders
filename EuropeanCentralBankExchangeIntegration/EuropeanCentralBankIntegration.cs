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
using DVPLI.Interfaces;
using DVPLI.MarketDataTypes;

namespace EuropeanCentralBankIntegration
{
    /// <summary>
    /// Implements the interface to provide Fairmat access to the
    /// Central European Bank Market Data Provider.
    /// </summary>
    /// <remarks>
    /// This Data Market Provider supports only Scalar requests.
    /// </remarks>
    [Mono.Addins.Extension("/Fairmat/MarketDataProvider")]
    public class EuropeanCentralBankIntegration : IMarketDataProvider, IDescription, ITickersInfo, IMarketDataProviderInfo
    {
        #region IDescription Implementation

        /// <summary>
        /// Gets an user friendly description of the service provided by this class.
        /// In this case "European Central Bank Exchange".
        /// </summary>
        public string Description
        {
            get
            {
                return "European Central Bank Exchange";
            }
        }

        #endregion IDescription Implementation

        #region IMarketDataProvider Implementation

        /// <summary>
        /// Sets the credentials to use to access this Market Data Provider.
        /// </summary>
        /// <remarks>
        /// This is part of the interface <see cref="IMarketDataProvider"/>
        /// but it's currently unused.
        /// </remarks>
        public string Credentials
        {
            set
            {
                // Ignore any set as the data is not needed.
            }
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
            // Reuse the Historical time series to get the single quote
            // (Equal start/end date = get the quote of the day).
            DateTime[] dates;
            IMarketData[] marketDataArray;
            RefreshStatus status = GetTimeSeries(mdq, mdq.Date, out dates, out marketDataArray);

            // If there were errors already just report them back.
            if (status.HasErrors)
            {
                marketData = null;
            }
            else
            {
                // If all was succesful try to prepare data for this type of query.
                status = new RefreshStatus();

                // Do some sanity check on the gathered data.
                if (marketDataArray.Length != 1 && dates.Length != 1 && dates[0] != mdq.Date)
                {
                    status.HasErrors = true;
                    status.ErrorMessage += "GetMarketData: Requested date " +
                                           "or Market Data not available.";
                    marketData = null;
                }
                else
                {
                    // If they pass just take the first element as resykt
                    // (which must be also the only one).
                    marketData = marketDataArray[0];
                }
            }

            return status;
        }

        /// <summary>
        /// Gets a series of Historical Market Data from the starting date
        /// to the end date.
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
        public RefreshStatus GetTimeSeries(MarketDataQuery mdq, DateTime end, out DateTime[] dates, out IMarketData[] marketData)
        {
            RefreshStatus status = new RefreshStatus();

            string currency;

            // Check if it's a close request.
            if (mdq.Field != "close")
            {
                // In case the request is not close return an error.
                marketData = null;
                dates = null;
                status.HasErrors = true;
                status.ErrorMessage += "GetTimeSeries: Market data not available (only " +
                                       "close values are available, " +
                                       mdq.Field + " was requested).";
                return status;
            }

            // Check that the requested value is available.
            if (mdq.Ticker.StartsWith("EUCF"))
            {
                // Extract the target currency name as that's used to request the data.
                currency = mdq.Ticker.Remove(0, 4);
            }
            else if(mdq.Ticker.StartsWith("EUR"))
            {
                // Extract the target currency name as that's used to request the data.
                currency = mdq.Ticker.Remove(0, 3);
            }
            else
            {
                // Only EUR TO TARGET CURRENCY is supported using the format EUCF<TARGET CURRENCY>
                marketData = null;
                dates = null;
                status.HasErrors = true;
                status.ErrorMessage += "GetTimeSeries: Market data not available (only " +
                                       "conversion rates from EUR to another currency are " +
                                       "available, " + mdq.Ticker + " was requested).";
                return status;
            }

            // For now only Scalar requests are handled.
            if (mdq.MarketDataType == typeof(Scalar).ToString())
            {
                List<EuropeanCentralBankQuote> quotes = null;

                try
                {
                    // Request the data to the Market Data Provider.
                    quotes = EuropeanCentralBankAPI.GetHistoricalQuotes(currency, mdq.Date, end);
                }
                catch (Exception e)
                {
                    // There can be conversion, server availability
                    // and other exceptions during this request.
                    marketData = null;
                    dates = null;
                    status.HasErrors = true;
                    status.ErrorMessage += "GetTimeSeries: Market data not available due " +
                                           "to problems with the European Central Bank service: " +
                                           e.Message;
                    return status;
                }

                // Check if there is at least one result.
                if (quotes.Count >= 1)
                {
                    // Allocate the structures for the output.
                    marketData = new Scalar[quotes.Count];
                    dates = new DateTime[quotes.Count];

                    // Scan the list of quotes to prepare the data for Fairmat.
                    for (int i = 0; i < quotes.Count; i++)
                    {
                        // Fill the dates array from the date field of each quote.
                        dates[i] = quotes[i].Date;

                        // Prepare the single scalar data.
                        Scalar val = new Scalar();
                        val.TimeStamp = quotes[i].Date;
                        val.Value = quotes[i].Value;

                        // Put it in the output structure.
                        marketData[i] = val;
                    }

                    return status;
                }
                else
                {
                    // If there isn't at least one result return an error.
                    marketData = null;
                    dates = null;
                    status.HasErrors = true;
                    status.ErrorMessage += "GetTimeSeries: Market data not available: " +
                                           "empty data set for the request.";
                    return status;
                }
            }
            else
            {
                // If control falls through here it means the request type was not supported.
                marketData = null;
                dates = null;
                status.HasErrors = true;
                status.ErrorMessage += "GetTimeSeries: Market data request type (" +
                                       mdq.MarketDataType +
                                       ") not supported by the Market Data Provider.";
                return status;
            }
        }

        /// <summary>
        /// Checks if the Central European Bank service is reachable
        /// and answers to requests correctly.
        /// </summary>
        /// <remarks>
        /// This is done by requesting a well known quote and checking if data is returned,
        /// the sanity of the data is not checked.
        /// </remarks>
        /// <returns>
        /// A <see cref="Status"/> indicating if the
        /// European Central Bank exchange service is working.
        /// </returns>
        public Status TestConnectivity()
        {
            // Prepare the default result, in case everything will go well.
            Status state = new Status();
            state.HasErrors = false;
            state.ErrorMessage = string.Empty;

            try
            {
                // Try simply requesting a single data series known to exist
                // and to produce 1 result (we use ZAR at 31 jan 2011).
                List<EuropeanCentralBankQuote> quotes = EuropeanCentralBankAPI.GetHistoricalQuotes("ZAR",
                                                                                                   new DateTime(2011, 1, 31),
                                                                                                   new DateTime(2011, 1, 31));

                if (quotes.Count != 1)
                {
                    // If there is a number of results different than 1,
                    // it means the service is not answering as expected,
                    // so fail the test.
                    state.HasErrors = true;
                    state.ErrorMessage = "Data from the European Central Bank " +
                                         "not available or unreliable.";
                }
            }
            catch (Exception e)
            {
                // If an exception was thrown during data fetching it means
                // there is a problem with the service (either timeout,
                // connection failure, or the European Central Bank changed data format).
                state.HasErrors = true;
                state.ErrorMessage = "Unable to connect to the European CentralBank service: " +
                                     e.Message;
            }

            return state;
        }
        #endregion IMarketDataProvider Implementation

        #region ITickersInfo Implementation
        /// <summary>
        /// Gets the list of the tickers currently supported by this market data provider.
        /// </summary>
        public ISymbolDefinition[] SupportedTickers(string filter = null)
        {
            string[] currencies = new string[]{"USD",
                                               "JPY",
                                               "BGN",
                                               "CZK",
                                               "DKK",
                                               "GBP",
                                               "HUF",
                                               "LTL",
                                               "LVL",
                                               "PLN",
                                               "RON",
                                               "SEK",
                                               "CHF",
                                               "NOK",
                                               "HRK",
                                               "RUB",
                                               "TRY",
                                               "AUD",
                                               "BRL",
                                               "CAD",
                                               "CNY",
                                               "HKD",
                                               "IDR",
                                               "ILS",
                                               "INR",
                                               "KRW",
                                               "MXN",
                                               "MYR",
                                               "NZD",
                                               "PHP",
                                               "SGD",
                                               "THB",
                                               "ZAR"};

            List<ISymbolDefinition> tickers = new List<ISymbolDefinition>();
            foreach(string currency in currencies)
            {
                // Generate the string for output.
                string fullName = "EUCF" + currency;

                // Check if the string is ok with the current filter if any.
                if (filter != null && !fullName.StartsWith(filter))
                {
                    // If it's not skip this currency.
                    continue;
                }

                tickers.Add(new SymbolDefinition(fullName, "EBC exchange rate"));
            }

            return tickers.ToArray();
        }

        #endregion ITickersInfo Implementation

        #region IMarketDataProviderInfo Implementation

        /// <summary>
        /// Returns information about the data providers capabilities.
        /// </summary>
        /// <param name="category">The required data category.</param>
        /// <returns>The data category access type.</returns>
        public MarketDataAccessType GetDataAvailabilityInfo(MarketDataCategory category)
        {
            switch (category)
            {
                case MarketDataCategory.EquityPrice:
                    {
                        // Exchange rate series are considered equities becuase they share the same undelying type 
                        return MarketDataAccessType.Local;
                    }
                default:
                    {
                        return MarketDataAccessType.NotAvailable;
                    }
            }
        }

        #endregion IMarketDataProviderInfo Implementation
    }
}
