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
        private readonly EstrParser _estrParser = new EstrParser();

        private static readonly HttpClient SharedHttpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl)
        };

        /// <summary>
        /// Get the Scalar representation of an ESTR Daily - businessweek
        /// </summary>
        /// <returns>Enumerable of scalar values representing the ECB ESTR reading requested</returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<Scalar> GetDailyBusinessWeekEstr()
        {
            return GetEstrMarketDataBy(DataPortal.DailyBusinessWeek);
        }

        /// <summary>
        /// Get the Scalar representation of an ECB ESTR reading for a given DataPortal
        /// (eg: Daily, Total Volume, 75th percentile...)
        /// </summary>
        /// <param name="dataPortal">Data Portal to get on the ECB website</param>
        /// <returns>Collection of Scalar values representing the extracted market data</returns>
        private IEnumerable<Scalar> GetEstrMarketDataBy(DataPortal dataPortal)
        {
            IEnumerable<EstrQuoteDto> quotes = SerializeCsvToDto(GetEstrMarketDataCsvBy(dataPortal));
            throw new NotImplementedException("WIP");
        }
        

        /// <summary>
        /// Parse and serialize the CSV obtained from the API and split by lines into
        /// a collection of <see cref="EstrQuoteDto"/> objects
        /// </summary>
        /// <param name="csvLines">Line-by-line representation of the fetched CSV</param>
        /// <returns>Collection of Scalar values representing the extracted market data</returns>
        protected internal IEnumerable<EstrQuoteDto> SerializeCsvToDto(IEnumerable<string> csvLines)
        {
            return _estrParser.ParseEstrCsv(csvLines);
        }

        /// <summary>
        /// Gets the CSV representation for an ESTR quote. This is the pre-serialization representation
        /// of the response result
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <returns>string containing the raw CSV returned by the service</returns>
        protected internal IEnumerable<string> GetEstrMarketDataCsvBy(DataPortal dataPortal)
        {
            string requestUrl = ConstructGetRequestUrlBy(dataPortal);

            try
            {
                HttpResponseMessage response = SharedHttpClient.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();

                string csvContent = response.Content.ReadAsStringAsync().Result;
                byte[] responseFileBytes = Encoding.UTF8.GetBytes(csvContent);
                IEnumerable<string> result = EstrParser.ReadCsvContent(responseFileBytes);
                return result;
            }
            catch (HttpRequestException e)
            {
                throw new InvalidOperationException("Error while calling ECB API: response result was not 2XX OK", e);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("A generic error was encountered while calling the ECB API", e);
            }
        }
        
        /// <summary>
        /// Construct the GET URL to request to the ECB REST API to get an ESTR reading
        /// for a given DataPortal (eg: Daily, Total Volume, 75th percentile...)
        /// </summary>
        /// <param name="dataPortal"></param>
        /// <returns>URL to request to ECB REST API endpoint</returns>
        private static string ConstructGetRequestUrlBy(DataPortal dataPortal)
        {
            return $"{BaseUrl}{dataPortal.Value}{RequestQuery}";
        }
    }
}