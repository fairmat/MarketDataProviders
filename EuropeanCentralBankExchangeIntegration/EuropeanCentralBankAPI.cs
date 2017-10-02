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
using System.Xml;

namespace EuropeanCentralBankIntegration
{
    /// <summary>
    /// Provides a simple interface to the Central European Bank data.
    /// http://www.ecb.europa.eu/stats/exchange/eurofxref/html/index.en.html
    /// http://www.ecb.int/stats/exchange/eurofxref/html/eurofxref-graph-zar.en.html.
    /// </summary>
    internal static class EuropeanCentralBankAPI
    {
        /// <summary>
        /// Gets a List of <see cref="EuropeanCentralBankQuote"/> containing the requested data.
        /// </summary>
        /// <param name="ticker">
        /// The symbol of the ticker to look for quotes (usually a currency).
        /// </param>
        /// <param name="startDate">
        /// The start date to look for quotes, the date is inclusive.
        /// </param>
        /// <param name="endDate">
        /// The ending date to look for quotes, the date is inclusive.
        /// </param>
        /// <returns>
        /// A list of <see cref="EuropeanCentralBankQuote"/> containing all
        /// the market open days from startDate to endDate.
        /// The list can be empty if the requested filters yield no results.
        /// </returns>
        /// <exception cref="Exception">
        /// A generic Exception can be thrown in case there are problems
        /// contacting the European Central Bank, like timeout or HTTP errors.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// An InvalidDataException might be parsed if the CSV
        /// has different fields than expected.
        /// </exception>
        internal static List<EuropeanCentralBankQuote> GetHistoricalQuotes(string ticker, DateTime startDate, DateTime endDate)
        {
            List<EuropeanCentralBankQuote> quotes = new List<EuropeanCentralBankQuote>();

            // Try to do the request starting from the wanted data.
            XmlReader reader = MakeRequest(ticker, startDate, endDate);

            // Start parsing the result line by line.
            try
            {
                while (reader.ReadToFollowing("Obs"))
                {
                    if (!reader.MoveToAttribute("TIME_PERIOD"))
                    {
                        throw new InvalidDataException("The data format is not valid");
                    }

                    string date = reader.Value;

                    if (!reader.MoveToAttribute("OBS_VALUE"))
                    {
                        throw new InvalidDataException("The data format is not valid");
                    }

                    string value = reader.Value;

                    EuropeanCentralBankQuote quote = new EuropeanCentralBankQuote(date, value);
                    if (quote.Date >= startDate && quote.Date <= endDate)
                    {
                        quotes.Add(quote);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("There was an error while attempting " +
                                    "to parse the data from the European Central Bank servers: " +
                                    e.Message);
            }
            //Quotes must be passed in decreasing date order
            quotes.Reverse();
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
        private static XmlReader MakeRequest(string ticker, DateTime startDate, DateTime endDate)
        {
            // Generate the request to be sent to the European Central Bank site.
            string request = string.Format("https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/{0}.xml",
                                            ticker.ToLower());

            return MakeRequest(request);
        }

        /// <summary>
        /// Actually makes a request with the provided requestUrl and generates
        /// a <see cref="StreamReader"/> ready for reading the result of the request.
        /// </summary>
        /// <param name="requestUrl">The url to use to request data.</param>
        /// <returns>A <see cref="StreamReader"/> ready for reading the request result.</returns>
        private static XmlReader MakeRequest(string requestUrl)
        {
            try
            {
                // Prepare the object to handle the request to the European Central Bank servers.
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                

                // Actually attempt the request to the European Central Bank.
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

                        // Prepare the XmlReader with the downloaded data.
                        return XmlReader.Create(memoryStream);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("There was an error while attempting " +
                                    "to contact the European Central Bank servers: " + e.Message);
            }
        }
    }
}
