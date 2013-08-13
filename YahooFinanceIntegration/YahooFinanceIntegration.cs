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
using System.Linq;
using DVPLI;
using DVPLI.MarketDataTypes;
using DVPLI.Interfaces;
using OptionQuotes;

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
        /// <returns>The supported ticker array.</returns>
        /// <remarks>
        /// For Yahoo! Finance it's not possible to get the full list, so queries will return
        /// data only if filtered.
        /// </remarks>
        public ISymbolDefinition[] SupportedTickers(string filter = null)
        {
            try
            {
                if (filter != null && filter.Length > 0)
                {
                    List<ISymbolDefinition> symbols = new List<ISymbolDefinition>();
                    YahooFinanceAPI.GetTickersWithFilter(filter).ForEach(x => symbols.Add(new SymbolDefinition((string)x["symbol"], "Yahoo! Finance " + (string)x["typeDisp"])));
                    return symbols.ToArray();
                }
            }
            catch (Exception)
            { }

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
            IMarketData[] marketDatas;
            RefreshStatus status = GetTimeSeries(mdq, mdq.Date, out dates, out marketDatas);

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
                if (marketDatas.Length != 1 && dates.Length != 1 && dates[0] != mdq.Date)
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
                    marketData = marketDatas[0];
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

            // For now only Scalar requests are handled.
            if (mdq.MarketDataType == typeof(Scalar).ToString())
            {
                List<YahooHistoricalQuote> quotes = null;

                try
                {
                    // Request the data to the Market Data Provider.
                    quotes = YahooFinanceAPI.GetHistoricalQuotes(mdq.Ticker, mdq.Date, end);
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
                        val.Value = (closeRequest == true) ? quotes[i].Close : quotes[i].Open;

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
        /// <param name="mdq">The market data query</param>
        /// <param name="marketData"></param>
        /// <returns></returns>
        RefreshStatus GetCallPriceMarketData(MarketDataQuery mdq, out IMarketData marketData)
        {
            Console.WriteLine(mdq);

            marketData = null;
            string fname=@"C:\tmp\GOOG.bin";
            List<YahooOptionChain> opt=null;
            Fairmat.MarketData.CallPriceMarketData data = new Fairmat.MarketData.CallPriceMarketData();

            
            //if (!System.IO.File.Exists(fname))
            {
                /*
                 *  Request options does not seems to give the option effectively tradaded at a given
                 *  date but the options that are still being traded.
                 */
                opt = YahooFinanceAPI.RequestOptions(mdq.Ticker);
                DVPLI.ObjectSerialization.WriteToFile(fname, opt);
            }
            //else
            //    opt = (List<YahooOptionChain>)DVPLI.ObjectSerialization.ReadFromFile(fname);
            

            //From the YahooOptionChain List, extracts a List of YahooOption
            List<YahooOption> options = new List<YahooOption>();
            foreach (YahooOptionChain q in opt)
            {
                Console.WriteLine(q.Symbol + " " + q.Expiration);
                foreach (YahooOption o in q.Options)
                {
                    //Loads into YahooOption the needed information
                    o.Maturity = q.Expiration;
                    options.Add(o);
                }
            }
            // Populate CallPriceMarketData data structure
            var status = OptionQuotesUtility.GetCallPriceMarketData(this, mdq, options.ConvertAll(x => (IOptionQuote)x), data);
            if (status.HasErrors)
                return status;

            marketData = data;
            Console.WriteLine(data);
            return status;
             
        }

        #endregion
    }
}
