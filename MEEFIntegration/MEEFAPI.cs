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
using ICSharpCode.SharpZipLib.Zip;

namespace MEEFIntegration
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
    internal static class MEEFAPI
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
        internal static List<MEEFHistoricalQuote> GetHistoricalQuotes(string ticker, DateTime startDate, DateTime endDate)
        {
            List<MEEFHistoricalQuote> quotes = new List<MEEFHistoricalQuote>();

            // Scan through months and years in order to gather all needed data.
            DateTime date = startDate;
            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                int startMonth = (year == startDate.Year) ? startDate.Month : 1;
                int endMonth = (year == endDate.Year) ? endDate.Month : 12;
                for (int month = startMonth; month <= endMonth; month++)
                {
                    // Try to do the request starting from the wanted data.
                    ZipInputStream reader = MakeRequest(date);
                    byte[] data = new byte[4096];

                    int size = reader.Read(data, 0, data.Length);
                    string entryCSV = string.Empty;
                    while (size > 0)
                    {
                        entryCSV += Encoding.ASCII.GetString(data, 0, size);

                        while (entryCSV.Contains("\n"))
                        {
                            if (entryCSV[0] != '\n')
                            {
                                MEEFHistoricalQuote quote = new MEEFHistoricalQuote(entryCSV.Substring(0, entryCSV.IndexOf("\n")).Replace("\r", ""));
                                if (quote.ContractCode == ticker)
                                {
                                    if (quote.SessionDate >= startDate && quote.SessionDate <= endDate)
                                        quotes.Add(quote);
                                }
                            }

                            int pos = entryCSV.IndexOf("\n") + 1;
                            entryCSV = pos < entryCSV.Length ? entryCSV.Substring(pos, entryCSV.Length - pos) : string.Empty;
                        }

                        size = reader.Read(data, 0, data.Length);
                    }
                }
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
        private static ZipInputStream MakeRequest(DateTime date)
        {
            // Generate the request to be sent to MEEF site.
            string year = date.Year.ToString();
            string request = string.Format("http://www.meff.es/docs/Ficheros/Descarga/dRV/HP{0}{1:00}ACO.zip", year.Substring(year.Length - 2), date.Month);

            return MakeRequest(request);
        }

        /// <summary>
        /// Actually makes a request with the provided requestUrl and generates
        /// a <see cref="StreamReader"/> ready for reading the result of the request.
        /// </summary>
        /// <param name="requestUrl">The url to use to request data.</param>
        /// <returns>A <see cref="StreamReader"/> ready for reading the request result.</returns>
        private static ZipInputStream MakeRequest(string requestUrl)
        {
            try
            {
                // Prepare the object to handle the request to the Yahoo servers.
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                // Actually attempt the request to Yahoo.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

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

                ZipInputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(receiveStream);

                zipStream.GetNextEntry();

                return zipStream;
            }
            catch (Exception e)
            {
                throw new Exception("There was an error while attempting " +
                                    "to contact Yahoo servers: " + e.Message);
            }
        }
    }
}
