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
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace YahooFinanceIntegration
{
    /// <summary>
    /// Provides a simple interface to the Yahoo! Finance API.
    /// http://code.google.com/p/yahoo-finance-managed/wiki/YahooFinanceAPIs.
    /// </summary>
    /// <remarks>
    /// The class implements the CSV API using the historical quotes download functionality.
    /// This API is detailed here:
    /// http://code.google.com/p/yahoo-finance-managed/wiki/csvHistQuotesDownload.
    /// </remarks>
    internal static class YahooFinanceAPI
    {
        /// <summary>
        /// Gets a List of <see cref="YahooHistoricalQuote"/> containing the requested data.
        /// </summary>
        /// <param name="ticker">
        /// The symbol of the ticker to look for quotes.
        /// </param>
        /// <param name="startDate">
        /// The start date to look for quotes, the date is inclusive.
        /// </param>
        /// <param name="endDate">
        /// The ending date to look for quotes, the date is inclusive.
        /// </param>
        /// <returns>
        /// A list of <see cref="YahooHistoricalQuote"/> containing all
        /// the market open days from startDate to endDate.
        /// The list can be empty if the requested filters yield no results.
        /// </returns>
        /// <exception cref="Exception">
        /// A generic Exception can be thrown in case there are problems
        /// contacting Yahoo! servers, like timeout or HTTP errors.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// An InvalidDataException might be parsed if the CSV
        /// has different fields than expected.
        /// </exception>
        internal static List<YahooHistoricalQuote> GetHistoricalQuotes(string ticker, DateTime startDate, DateTime endDate)
        {
            List<YahooHistoricalQuote> quotes = new List<YahooHistoricalQuote>();

            // Try to do the request starting from the wanted data.
            StreamReader reader = MakeRequest(ticker, startDate, endDate);

            // Skip the first line of the output
            reader.ReadLine();

            // Start parsing the result line by line.
            while (!reader.EndOfStream)
            {
                YahooHistoricalQuote quote = new YahooHistoricalQuote(reader.ReadLine());
                quotes.Add(quote);
            }

            return quotes;
        }

        /// <summary>
        /// Prepares the request string starting from the provided
        /// variables and handles the request.
        /// </summary>
        /// <param name="ticker">
        /// The symbol of the ticker to look for quotes.
        /// </param>
        /// <param name="startDate">
        /// The start date to look for quotes, the date is inclusive.
        /// </param>
        /// <param name="endDate">
        /// The ending date to look for quotes, the date is inclusive.
        /// </param>
        /// <returns>A <see cref="StreamReader"/> ready for reading the request result.</returns>
        private static StreamReader MakeRequest(string ticker, DateTime startDate, DateTime endDate)
        {
            // Generate the request to be sent to Yahoo Finance.
            string request = string.Format("http://ichart.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&ignore=.csv",
                                           ticker, startDate.Month - 1, startDate.Day,
                                           startDate.Year, endDate.Month - 1,
                                           endDate.Day, endDate.Year);
            return MakeRequest(request);
        }

        /// <summary>
        /// Actually makes a request with the provided requestUrl and generates
        /// a <see cref="StreamReader"/> ready for reading the result of the request.
        /// </summary>
        /// <param name="requestUrl">The url to use to request data.</param>
        /// <returns>A <see cref="StreamReader"/> ready for reading the request result.</returns>
        private static StreamReader MakeRequest(string requestUrl)
        {
            try
            {
                // Prepare the object to handle the request to the Yahoo servers.
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                // Actually attempt the request to Yahoo.
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    // If this point is reached the response is instanced with something.
                    // Check if it was successful.
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                                                           response.StatusCode,
                                                           response.StatusDescription));
                    }


                    // Obtain the stream of the response and initialize a reader.
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        byte[] buffer = new byte[16 * 4096];
                        int read;
                        while ((read = receiveStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, read);
                        }

                        // Rollback the memory buffer to begin.
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        // Prepare the StreamReader with the downloaded data.
                        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        return new StreamReader(memoryStream, encode);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("There was an error while attempting " +
                                    "to contact Yahoo servers: " + e.Message);
            }
        }

        /// <summary>
        /// Tries to gather options data for the next 24 months, when available.
        /// </summary>
        /// <param name="ticker">The ticker to request option data for.</param>
        /// <returns>
        /// A list of <see cref="YahooOptionChain"/>
        /// containing all the gathered data.
        /// </returns>
        public static List<YahooOptionChain> RequestOptions(string ticker)
        {
            DateTime datePoint = DateTime.Now;
            List<YahooOptionChain> optionChains = new List<YahooOptionChain>();

            try
            {
                // Try only the next 24 months (2 years).
                for (int i = 0; i < 24; i++)
                {
                    // Prepare the request for th YQL API.
                    string requestUrl = string.Format("http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.options%20where%20symbol%3D%22{0}%22%20and%20expiration%3D%22{1:0000}-{2:00}%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys",
                                                        ticker, datePoint.Year, datePoint.Month);
                    Console.WriteLine(requestUrl);
                    Console.WriteLine(datePoint.ToString());

                    // Prepare some variables for use to handle Yahoo! Servers time outs.
                    bool failed = false;
                    int attempts = 10;

                    do
                    {
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                            // Actually attempt the request to the Yahoo! servers.
                            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                            {

                                // If this point is reached the response is instanced with something.
                                // Check if it was successful.
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                                                                       response.StatusCode,
                                                                       response.StatusDescription));
                                }

                                // Obtain the stream of the response and initialize a reader.
                                Stream receiveStream = response.GetResponseStream();
                                XmlReader reader = XmlReader.Create(receiveStream);

                                if (!reader.ReadToDescendant("optionsChain")) throw new Exception();

                                // Deserialize the message from Yahoo! servers.
                                XmlSerializer serializer = new XmlSerializer(typeof(YahooOptionChain));
                                YahooOptionChain optionChain = (YahooOptionChain)serializer.Deserialize(reader.ReadSubtree());

                                // The next month will be checked.
                                datePoint = datePoint.AddMonths(1);
                                optionChains.Add(optionChain);

                                // All was successful so reset the failure handling variables
                                failed = false;
                                attempts = 10;
                            }
                        }
                        catch (Exception e)
                        {
                            if (attempts == 0)
                            {
                                // just try for a while not always.
                                throw new Exception("Too many failed attempts to retrieve " +
                                                    "the data (" + e.Message + ")");
                            }

                            // There was a failure so set the failure handling variables accordly.
                            failed = true;
                            attempts--;

                            Console.WriteLine("Error during fetching attempt (" + e.Message +
                                              "). Retrying (left " + attempts + ")...");
                        }
                    }
                    while (failed);
                }
            }
            catch (Exception e)
            {
                throw new Exception("There was an error while attempting " +
                                    "to contact the Yahoo! Finance servers: " + e.Message);
            }

            return optionChains;
        }
    }
}
