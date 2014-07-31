/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Stefano Angeleri (stefano.angeleri@fairmat.com)
 *            Matteo Tesser (matteo.tesser@fairmat.com)
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
using System.IO;
using OptionQuotes;
using TickersUtils;

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Implements the interface to provide Fairmat access to the
    /// Yahoo! Finance Market Data Provider.
    /// </summary>
    /// <remarks>
    /// This Data Market Provider supports only Scalar requests.
    /// </remarks>
    [Mono.Addins.Extension("/Fairmat/MarketDataProvider")]
    public class YahooFinanceIntegration : IDescription, ITickersInfo, IMarketDataProviderInfo, IMarketDataProvider
    {
        #region IDescription Implementation

        /// <summary>
        /// Gets an user friendly description of the service provided by this class.
        /// In this case "Yahoo! Finance".
        /// </summary>
        public string Description
        {
            get
            {
                return "Yahoo! Finance";
            }
        }

        #endregion IDescription Implementation

        #region ITickersInfo Implementation

        /// <summary>
        /// Returns the list of the tickers currently supported by this market data provider.
        /// </summary>
        /// <param name="filter">The filter to use to choose which symbols to return.</param>
        /// <returns>The supported ticker array.</returns>
        /// <remarks>
        /// For Yahoo! Finance it's not possible to get the full list, so queries will return
        /// data only if filtered.
        /// </remarks>
        public ISymbolDefinition[] SupportedTickers(string filter = null)
        {
            // Normalization of =X to strings without them.
            Func<string, string> cleanCurrency = x => x.EndsWith("=X") ? x.Remove(x.Length-2) : x;

            try
            {
                if (filter != null && filter.Length > 0)
                {
                    List<ISymbolDefinition> symbols = new List<ISymbolDefinition>();
                    YahooFinanceAPI.GetTickersWithFilter(filter).ForEach(x => symbols.Add(new SymbolDefinition(cleanCurrency((string)x["symbol"]), "Yahoo! Finance " + (string)x["typeDisp"] + " (" + (string)x["name"] + ")")));
                    return symbols.ToArray();
                }
            }
            catch (Exception)
            {
            }

            return new ISymbolDefinition[0];
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
                        return MarketDataAccessType.Local;
                    }

                case MarketDataCategory.EquityVolatilitySurface:
                    {
                        return MarketDataAccessType.Local;
                    }

                default:
                    {
                        return MarketDataAccessType.NotAvailable;
                    }
            }
        }

        #endregion IMarketDataProviderInfo Implementation

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
            if (mdq.MarketDataType == typeof(Fairmat.MarketData.CallPriceMarketData).ToString())
            {
                return GetCallPriceMarketData(mdq, out marketData);
            }

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
                    // If they pass just take the first element as result
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

            // Holds whathever we should take market close or market open values.
            bool closeRequest;

            // Holds the currency conversion target in case a market different than
            // US is choosen. This handles conversion from USD values provided by yahoo.
            string targetMarket = null;

            string ticker = TickerUtility.PreparseSymbol(mdq.Ticker);
            bool divisionTransformation = false;
            bool inverseTransformation = false;

            // Check if open or close value was requested.
            switch (mdq.Field)
            {
                case "open":
                    {
                        closeRequest = false;
                        break;
                    }

                case "close":
                    {
                        closeRequest = true;
                        break;
                    }

                default:
                    {
                        // In case the request is neither open or close return an error.
                        marketData = null;
                        dates = null;
                        status.HasErrors = true;
                        status.ErrorMessage += "GetTimeSeries: Market data not available (only " +
                                               "open and close values are available, " +
                                               mdq.Field + " was requested).";
                        return status;
                    }
            }

            // Gather in which currency to get the data, this will require
            // an additional data fetching for the target currency for the same time period.
            if (mdq.Market.Length > 0 && mdq.Market != "US")
            {
                // Try to convert the entries in the Fairmat drop down box,
                // for the rest rely on the user.
                switch (mdq.Market)
                {
                    case "EU":
                        {
                            targetMarket = "EUR";
                            break;
                        }

                    case "GB":
                        {
                            targetMarket = "GBP";
                            break;
                        }

                    case "JP":
                        {
                            targetMarket = "JPY";
                            break;
                        }

                    case "CH":
                        {
                            targetMarket = "CHF";
                            break;
                        }

                    case "HK":
                        {
                            targetMarket = "HKD";
                            break;
                        }

                    default:
                        {
                            // In the fallback scenario just use directly the provided string.
                            targetMarket = mdq.Market;
                            break;
                        }
                }
            }

            // Check for currencies and handle them in a special way.
            // Check the single currency only (eg: EUR for USDEUR).
            if (GetCurrencyList().Contains(ticker))
            {
                ticker += "=X";

                // Disable the feature for this for now.
                targetMarket = null;
            }
            else
            {
                // Attempt a more throughout parsing. Check for <currency><currency> formats.
                foreach (string currency in GetCurrencyList())
                {
                    if (ticker.StartsWith(currency))
                    {
                        if(currency == "USD")
                        {
                            // If usd is in the ticker name it's a special case due to the way
                            // Yahoo! Finance keeps currencies.
                            ticker = ticker.Remove(0, 3) + "=X";
                            targetMarket = null;
                        }
                        else if (ticker.Remove(0, 3) == "USD")
                        {
                            // This is the inverse of the previous case.
                            // From other currency to USD. Similarly to above it's a special case.
                            ticker = currency + "=X";
                            targetMarket = null;
                            inverseTransformation = true;
                        }
                        else
                        {
                            // Normal not USD to not USD currency conversions.
                            targetMarket = currency;
                            ticker = ticker.Remove(0, 3) + "=X";
                            divisionTransformation = true;
                        }

                        break;
                    }
                }
            }

            // For now only Scalar requests are handled.
            if (mdq.MarketDataType == typeof(Scalar).ToString())
            {
                List<YahooHistoricalQuote> quotes = null;
                Dictionary<DateTime, YahooHistoricalQuote> currencyQuotes = new Dictionary<DateTime,YahooHistoricalQuote>();

                try
                {
                    // Request the data to the Market Data Provider.
                    quotes = YahooFinanceAPI.GetHistoricalQuotes(ticker, mdq.Date, end);

                    if (targetMarket != null)
                    {
                        // If we need currency quotes in order to handle currency conversions
                        // fetch them now.
                        List<YahooHistoricalQuote> fetchedCurrencyQuotes = YahooFinanceAPI.GetHistoricalQuotes(targetMarket + "=X", mdq.Date, end);

                        // Put all items in a dictionary for easy fetching.
                        fetchedCurrencyQuotes.ForEach(x => currencyQuotes.Add(x.Date, x));
                    }
                }
                catch (Exception e)
                {
                    // There can be conversion, server availability
                    // and other exceptions during this request.
                    marketData = null;
                    dates = null;
                    status.HasErrors = true;
                    status.ErrorMessage += "GetTimeSeries: Market data not available due " +
                                           "to problems with Yahoo! Finance: " + e.Message;
                    return status;
                }

                // Check if there is at least one result.
                if (quotes.Count >= 1)
                {
                    // Allocate the structures for the output.
                    List<Scalar> readyMarketData = new List<Scalar>();
                    List<DateTime> readyDates = new List<DateTime>();

                    // Scan the list of quotes to prepare the data for Fairmat.
                    for (int i = 0; i < quotes.Count; i++)
                    {
                        // Prepare the single scalar data.
                        Scalar val = new Scalar();
                        val.TimeStamp = quotes[i].Date;
                        val.Value = (closeRequest == true) ? quotes[i].Close : quotes[i].Open;

                        // Handle currency conversions if needed.
                        if (currencyQuotes != null)
                        {
                            // Check if the entry in a date exists also for the
                            // currency conversion, if not discard the data.
                            if (!currencyQuotes.ContainsKey(quotes[i].Date))
                            {
                                // We skip this entry.
                                continue;
                            }

                            YahooHistoricalQuote currencyQuote = currencyQuotes[quotes[i].Date];

                            //if (divisionTransformation)
                            //{
                            //    val.Value /= (closeRequest == true) ? currencyQuote.Close : currencyQuote.Open;
                            //}
                            //else
                            //{
                            //    val.Value *= (closeRequest == true) ? currencyQuote.Close : currencyQuote.Open;
                            //}
                        }

                        // Apply an inverse transformation, used for currency values when going
                        // from a not USD currency to USD.
                        //if (inverseTransformation)
                        //{
                        //    val.Value = 1 / val.Value;
                        //}

                        // Put it in the output structure.
                        readyMarketData.Add(val);

                        // Fill the dates array from the date field of each quote.
                        readyDates.Add(quotes[i].Date);
                    }

                    // Put in the output data.
                    marketData = readyMarketData.ToArray();
                    dates = readyDates.ToArray();

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
        /// Checks if the Yahoo! Finance service is reachable
        /// and answers to requests correctly.
        /// </summary>
        /// <remarks>
        /// This is done by requesting a well known quote and checking if data is returned,
        /// the sanity of the data is not checked.
        /// </remarks>
        /// <returns>
        /// A <see cref="Status"/> indicating if the
        /// Yahoo! Finance service is working.
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
                // and to produce 1 result (we use GOOG at 31 jan 2011).
                List<YahooHistoricalQuote> quotes = YahooFinanceAPI.GetHistoricalQuotes("GOOG",
                                                                                        new DateTime(2011, 1, 31),
                                                                                        new DateTime(2011, 1, 31));

                if (quotes.Count != 1)
                {
                    // If there is a number of results different than 1,
                    // it means the service is not answering as expected,
                    // so fail the test.
                    state.HasErrors = true;
                    state.ErrorMessage = "Data from Yahoo! Finance not available or unreliable.";
                }
            }
            catch (Exception e)
            {
                // If an exception was thrown during data fetching it means
                // there is a problem with the service (either timeout,
                // connection failure, or Yahoo! changed data format).
                state.HasErrors = true;
                state.ErrorMessage = "Unable to connect to Yahoo! Finance service: " + e.Message;
            }

            return state;
        }

        #endregion IMarketDataProvider Implementation

        #region Support methods

        /// <summary>
        /// Retrieves available call and put options for a given ticker.
        /// </summary>
        /// <param name="mdq">The market data query.</param>
        /// <param name="marketData">The requested market data.</param>
        /// <returns>A <see cref="RefreshStatus"/> with the status of the result.</returns>
        private RefreshStatus GetCallPriceMarketData(MarketDataQuery mdq, out IMarketData marketData)
        {
            marketData = null;

            //Check if the market data is in cache
            string cachedName = Path.Combine(Path.GetTempPath(), "CallPrices" + mdq.Ticker + mdq.Date.Year + mdq.Date.Month + mdq.Date.Day);
            List<YahooOptionChain> optionChains = null;
            if (System.IO.File.Exists(cachedName))
            {
                try
                {
                    optionChains= (List<YahooOptionChain>)DVPLI.ObjectSerialization.ReadFromFile(cachedName);
                }
                catch
                {
                    //Failed to read from cache
                }
            }

            //if not found in cache try to get from the Yahoo service
            if (optionChains == null)
            {
                //Yahoo returns only last traded options hence, we assume that the only
                //valid dates are Today and Yesterday

                DateTime tMax = DateTime.Today;
                DateTime tMin = tMax.AddDays(-1);
                if (tMax.DayOfWeek == DayOfWeek.Monday)
                    tMin = tMax.AddDays(-3);

                if (mdq.Date.Date < tMin || mdq.Date.Date > tMax)
                {
                    return new RefreshStatus("Options are not available for the requested period. Set the valuation date to Yesterday");
                }

                // Request options does not seems to give the option effectively
                // tradaded at a given date but the options that are still being traded.
                optionChains = YahooFinanceAPI.RequestOptions(mdq.Ticker);
                try
                {
                    DVPLI.ObjectSerialization.WriteToFile(cachedName, optionChains);
                }
                catch
                {
                }
            }
           
            Fairmat.MarketData.CallPriceMarketData data = new Fairmat.MarketData.CallPriceMarketData();
              
            
            // Extract a list of YahooOption from the YahooOptionChain List.
            List<YahooOption> options = new List<YahooOption>();
            foreach (YahooOptionChain q in optionChains)
            {
                Console.WriteLine(q.Symbol + " " + q.Expiration);
                foreach (YahooOption o in q.Options)
                {
                    // Loads into YahooOption the needed information.
                    o.Maturity = q.Expiration;
                    options.Add(o);
                }
            }

            // Populate the CallPriceMarketData data structure
            var status = OptionQuotesUtility.GetCallPriceMarketData(this, mdq, options.ConvertAll(x => (OptionQuotes.IOptionQuote)x), data);
            if (status.HasErrors)
            {
                return status;
            }

            marketData = data;
            Console.WriteLine(data);
            return status;
        }

        private List<string> GetCurrencyList()
        {
            List<string> currencies = new List<string>{ "USD",
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
                                                        "ZAR",
                                                        "EUR" };
            return currencies;
        }

        #endregion Support methods
    }
}
