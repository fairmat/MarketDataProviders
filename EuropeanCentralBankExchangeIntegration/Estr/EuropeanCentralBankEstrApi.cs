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
using System.IO;
using System.Net.Http;
using System.Text;
using DVPLI.MarketDataTypes;
using EuropeanCentralBankIntegration.Estr.Dto;

namespace EuropeanCentralBankIntegration.Estr
{
    /// <summary>
    /// <para>
    /// This component provides the integration with the ECB API for the purpose of importing
    /// data related to the ESTR (Euro Short Term Rate).
    /// </para>
    /// Refer to SDMX REST API documentation for further info: <see href="https://github.com/sdmx-twg/sdmx-rest"></see>
    /// </summary>
    public class EuropeanCentralBankEstrApi
    {
        private const string BaseUrl = "https://data-api.ecb.europa.eu/service/data/EST/";
        private const string RequestQuery = "?format=csvdata&detail=dataonly";

        private static readonly HttpClient SharedHttpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl)
        };

        /// <summary>
        /// Get the Scalar representation of an ECB ESTR reading for a given DataPortal
        /// (eg: Daily, Total Volume, 75th percentile...)
        /// </summary>
        /// <param name="dataPortal"></param>
        /// <returns>Enumerable of scalar values representing the ECB ESTR reading requested</returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<Scalar> GetDailyBusinessWeekEstr(DataPortal dataPortal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Construct the GET URL to request to the ECB REST API to get an ESTR reading
        /// for a given DataPortal (eg: Daily, Total Volume, 75th percentile...)
        /// </summary>
        /// <param name="dataPortal"></param>
        /// <returns>URL to request to ECB REST API endpoint</returns>
        private static string ConstructGetRequestUrl(DataPortal dataPortal)
        {
            return $"{BaseUrl}{dataPortal.Value}{RequestQuery}";
        }

        private IEnumerable<EstrQuoteDto> ParseEstrCsv(string[] csv)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// WIP - this will probably get removed
        /// Get the CSV representation as a string enumerable for the "Euro short-term rate, Daily - businessweek"
        /// data point
        /// </summary>
        /// <returns>
        /// Enumerable representing the resulting CSV response containing the ESTR Daily-businessweek
        /// data-point split by lines
        /// </returns>
        public IEnumerable<string> GetEstrQuoteCsvDailyBusinessWeek()
        {
            return GetEstrQuoteCsv(DataPortal.DailyBusinessWeek);
        }

        /// <summary>
        /// Gets the CSV representation for an ESTR quote. This is the pre-serialization representation
        /// of the response result
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <returns>string containing the raw CSV returned by the service</returns>
        private IEnumerable<string> GetEstrQuoteCsv(DataPortal dataPortal)
        {
            string requestUrl = ConstructGetRequestUrl(dataPortal);

            try
            {
                HttpResponseMessage response = SharedHttpClient.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();

                string csvContent = response.Content.ReadAsStringAsync().Result;
                byte[] responseFileBytes = Encoding.UTF8.GetBytes(csvContent);
                IEnumerable<string> result = ReadCSvContent(responseFileBytes);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error calling ESTR API: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Reads the raw CSV from the REST API and separates it line by line, so that individual
        /// lines can be parsed later in the execution
        /// </summary>
        /// <param name="fileContent">Byte array containing the raw CSV file content from the API GET request</param>
        /// <returns>Enumerable containing the line-by-line representation of the CSV</returns>
        private static IEnumerable<string> ReadCSvContent(byte[] fileContent)
        {
            List<string> lines = new List<string>();

            using (StreamReader reader = new StreamReader(new MemoryStream(fileContent), Encoding.UTF8))
            {
                string line;
                do
                {
                    line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        lines.AddRange(line.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                } while (!string.IsNullOrEmpty(line));
            }

            return lines;
        }
    }
}