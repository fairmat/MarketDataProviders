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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EuropeanCentralBankIntegration.Estr.Constants;
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.Parsing;

namespace EuropeanCentralBankIntegration.Estr.Api
{
    /// <summary>
    /// <para>
    /// Abstracted client to interact with the ECB API for ESTR retrieval
    /// </para>
    /// Refer to SDMX REST API documentation for further info: <see href="https://github.com/sdmx-twg/sdmx-rest"></see>
    /// </summary>
    public class EuropeanCentralBankEstrApiClient
    {
        private const string BaseUrl = "https://data-api.ecb.europa.eu/service/data/EST/";
        private const string RequestQuery = "?format=csvdata&detail=dataonly";

        private readonly EstrParser _estrParser = new EstrParser();

        private static readonly HttpClient SharedHttpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl)
        };

        /// <summary>
        /// Gets the DTO representation for all the ESTR quotes available from the service
        /// Async API, recommended for external users
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Collection of DTOs representing ESTR quotes</returns>
        public async Task<IEnumerable<EstrQuoteDto>> GetEstrMarketData(DataPortal dataPortal,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<string> csvLines = await GetEstrMarketDataCsvBy(dataPortal, cancellationToken);
            return _estrParser.ParseEstrCsv(csvLines);
        }

        /// <summary>
        /// Gets the DTO representation for all the ESTR quotes available from the service
        /// Blocking, synchronous API for DVPLI
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <returns>Collection of DTOs representing ESTR quotes</returns>
        public IEnumerable<EstrQuoteDto> GetEstrMarketDataBlocking(DataPortal dataPortal)
        {
            IEnumerable<string> csvLines = GetEstrMarketDataCsvByBlocking(dataPortal);
            return _estrParser.ParseEstrCsv(csvLines);
        }

        /// <summary>
        /// Gets the DTO representation for all the ESTR quotes within the given range
        /// Async API, recommended for external users
        /// </summary>
        /// <param name="dataPortal">Data portal identifier to request to ECB API</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <param name="startDate">Start date to filter by (no lower bound if null)</param>
        /// <param name="endDate">End date to filter by (no upper bound if null)</param>
        /// <returns>Collection of DTOs representing ESTR quotes</returns>
        public async Task<IEnumerable<EstrQuoteDto>> GetEstrMarketDataInRange(DataPortal dataPortal,
            DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<string> csvLines = await GetEstrMarketDataCsvBy(dataPortal, cancellationToken);
            IEnumerable<EstrQuoteDto> quotes = _estrParser.ParseEstrCsv(csvLines);
            return _estrParser.FilterByDateRange(quotes, startDate, endDate);
        }

        /// <summary>
        /// Gets the DTO representation for all the ESTR quotes within the given range
        /// Blocking, synchronous API for DVPLI
        /// </summary>
        /// <param name="dataPortal">Data portal identifier to request to ECB API</param>
        /// <param name="startDate">Start date to filter by (no lower bound if null)</param>
        /// <param name="endDate">End date to filter by (no upper bound if null)</param>
        /// <returns>Collection of DTOs representing ESTR quotes</returns>
        public IEnumerable<EstrQuoteDto> GetEstrMarketDataInRangeBlocking(DataPortal dataPortal,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<string> csvLines = GetEstrMarketDataCsvByBlocking(dataPortal);
            IEnumerable<EstrQuoteDto> quotes = _estrParser.ParseEstrCsv(csvLines);
            return _estrParser.FilterByDateRange(quotes, startDate, endDate);
        }

        /// <summary>
        /// Gets the CSV representation for an ESTR quote.
        /// Blocking version, only meant for the TestConnection() method from <c>DVPLI</c>, which
        /// does not support async calls.
        /// of the response result
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <returns>string containing the raw CSV returned by the service</returns>
        protected internal static IEnumerable<string> GetEstrMarketDataCsvByBlocking(DataPortal dataPortal)
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
                throw new InvalidOperationException(
                    $"Failed to retrieve ESTR data from ECB API. URL: {requestUrl}. HTTP Error: {e.Message}",
                    e);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred while calling the ECB API. URL: {requestUrl}",
                    e);
            }
        }

        /// <summary>
        /// Gets the CSV representation for an ESTR quote
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Enumerable of CSV lines as strings</returns>
        private static async Task<IEnumerable<string>> GetEstrMarketDataCsvBy(DataPortal dataPortal,
            CancellationToken cancellationToken = default)
        {
            string requestUrl = ConstructGetRequestUrlBy(dataPortal);

            try
            {
                using (HttpResponseMessage response = await SharedHttpClient.GetAsync(requestUrl, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    byte[] csvBytes = await response.Content.ReadAsByteArrayAsync();
                    IEnumerable<string> result = EstrParser.ReadCsvContent(csvBytes);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve ESTR data from ECB API. URL: {requestUrl}. HTTP Error: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Request to ECB ESTR API timed out. URL: {requestUrl}",
                    ex);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("The operation was cancelled by user request.", cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred while calling the ECB API. URL: {requestUrl}",
                    ex);
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