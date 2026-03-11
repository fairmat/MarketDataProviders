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
using DVPLI.MarketDataTypes;
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
        /// Get the Scalar representation of an ESTR Daily - businessweek
        /// </summary>
        /// <returns>Enumerable of scalar values representing the ECB ESTR reading requested</returns>
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
        public IEnumerable<Scalar> GetEstrMarketDataBy(DataPortal dataPortal)
        {
            throw new NotImplementedException("WIP");
        }

        /// <summary>
        /// Get the Scalar representation of an ECB ESTR reading for a given DataPortal (async version)
        /// </summary>
        /// <param name="dataPortal">Data Portal to get on the ECB website</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Collection of Scalar values representing the extracted market data</returns>
        public async Task<IEnumerable<Scalar>> GetEstrMarketDataByAsync(DataPortal dataPortal,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<EstrQuoteDto> quotes = await GetEstrMarketDataCsvDtoAsync(dataPortal, cancellationToken);
            List<Scalar> scalars = new List<Scalar>();

            foreach (EstrQuoteDto dto in quotes)
            {
                scalars.Add(new Scalar(p_Value: dto.ObsValue, p_Date: dto.TimePeriod));
            }

            return scalars;
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
        /// Gets the CSV DTO representation for an ESTR quote (async version)
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Collection of parsed EstrQuoteDto objects</returns>
        private async Task<IEnumerable<EstrQuoteDto>> GetEstrMarketDataCsvDtoAsync(DataPortal dataPortal,
            CancellationToken cancellationToken)
        {
            IEnumerable<string> csvLines = await GetEstrMarketDataCsvByAsync(dataPortal, cancellationToken);
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
        /// Gets the CSV representation for an ESTR quote (async version with proper exception handling)
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Enumerable of CSV lines as strings</returns>
        private async Task<IEnumerable<string>> GetEstrMarketDataCsvByAsync(DataPortal dataPortal,
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
        /// Does the same as <see cref="GetEstrMarketDataCsvByAsync"/>, but synchronous and blocking.
        /// This is to comply to DVPLI interfaces which, unfortunately, do not allow us to check for
        /// connectivity asynchronously.
        /// This method is duplicated because it is an antipattern to rely on blocking API calls, and
        /// it exists only because of DVPLI constraints.
        /// </summary>
        /// <param name="dataPortal">Data Portal identifier to request to ECB API</param>
        /// <returns>Enumerable of CSV lines as strings</returns>
        /// <exception cref="HttpRequestException">Thrown on HTTP failure</exception>
        /// <exception cref="InvalidOperationException">Thrown on other failures</exception>
        public IEnumerable<string> TestConnectivity(DataPortal dataPortal)
        {
            string requestUrl = ConstructGetRequestUrlBy(dataPortal);

            HttpResponseMessage response = SharedHttpClient.GetAsync(requestUrl).Result;
            response.EnsureSuccessStatusCode();

            byte[] csvBytes = response.Content.ReadAsByteArrayAsync().Result;
            IEnumerable<string> result = EstrParser.ReadCsvContent(csvBytes);
            
            response.Dispose();
            
            return result;
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