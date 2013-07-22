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
    /// Provides a simple interface to the MEEF API.
    /// http://www.meff.es/aspx/Comun/Pagina.aspx?l1=Financiero&f=Ddescarga
    /// http://www.meff.es/aspx/Financiero/DescargaFicheros.aspx?id=esp.
    /// </summary>
    internal static class MEEFAPI
    {
        /// <summary>
        /// Gets a List of <see cref="MEEFHistoricalQuote"/> containing the requested data.
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
        /// A list of <see cref="MEEFHistoricalQuote"/> containing all
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
            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                // Calculate the start and end month for this year depending on the requested range.
                int startMonth = (year == startDate.Year) ? startDate.Month : 1;
                int endMonth = (year == endDate.Year) ? endDate.Month : 12;
                for (int month = startMonth; month <= endMonth; month++)
                {
                    // Try to do the request starting from the wanted data.
                    ZipInputStream reader = MakeRequest(new DateTime(year, month, 1));

                    // Start reading from the stream and unzipping the data.
                    byte[] data = new byte[4096];
                    int size = reader.Read(data, 0, data.Length);

                    // Keeps the current data in a format usable to parse the strings from the CSV.
                    string entryCSV = string.Empty;

                    // Gather all which is in the stream till there is nothing else to read.
                    while (size > 0)
                    {
                        // Convert the byte array in a string.
                        entryCSV += Encoding.ASCII.GetString(data, 0, size);

                        // If there is an "\n" it means we have at least one line ready to parse
                        // So continue scanning them till we have no more new lines.
                        while (entryCSV.Contains("\n"))
                        {
                            // Check that the new line isn't at the beginning of the string in that case cut it out.
                            if (entryCSV[0] != '\n')
                            {
                                // As we have a CSV line to parse go through  after cleaning it up.
                                MEEFHistoricalQuote quote = new MEEFHistoricalQuote(entryCSV.Substring(0, entryCSV.IndexOf("\n")).Replace("\r", ""));

                                // Check that the quote which was just parsed is what was asked.
                                if (quote.ContractCode == ticker && quote.SessionDate >= startDate && quote.SessionDate <= endDate)
                                {
                                    // In that case add to the results.
                                    quotes.Add(quote);
                                }
                            }

                            // Calculate and remove the line which was just parsed and prepare for the next line, if any.
                            int pos = entryCSV.IndexOf("\n") + 1;
                            entryCSV = pos < entryCSV.Length ? entryCSV.Substring(pos, entryCSV.Length - pos) : string.Empty;
                        }

                        // Read another chunk of data.
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
        /// <returns>A <see cref="ZipInputStream"/> ready for reading the request result.</returns>
        private static ZipInputStream MakeRequest(DateTime date)
        {
            // Generate the request to be sent to MEEF site.
            string year = date.Year.ToString();
            string request = string.Format("http://www.meff.es/docs/Ficheros/Descarga/dRV/HP{0}{1:00}ACO.zip", year.Substring(year.Length - 2), date.Month);

            return MakeRequest(request);
        }

        /// <summary>
        /// Actually makes a request with the provided requestUrl and generates
        /// a <see cref="ZipInputStream"/> ready for reading the result of the request.
        /// </summary>
        /// <param name="requestUrl">The url to use to request data.</param>
        /// <returns>A <see cref="ZipInputStream"/> ready for reading the request result.</returns>
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

                // Prepare a zip input stream as we are getting a zip file and we need the content of it.
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
