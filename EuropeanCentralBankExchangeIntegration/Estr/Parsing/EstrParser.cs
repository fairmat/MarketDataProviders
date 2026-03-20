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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.Exceptions;

namespace EuropeanCentralBankIntegration.Estr.Parsing
{
    /// <summary>
    /// Parser that deserializes the CSV from the ESTR API into
    /// <see cref="EstrQuoteDto"/> objects
    /// </summary>
    public class EstrParser
    {
        /// <summary>
        /// Parse and serialize the CSV obtained from the API and split by lines into
        /// a collection of <see cref="EstrQuoteDto"/> objects
        /// </summary>
        /// <param name="csvLines">Line-by-line representation of the fetched CSV</param>
        /// <returns>Collection of Scalar values representing the extracted market data</returns>
        protected internal IEnumerable<EstrQuoteDto> SerializeCsvToDto(IEnumerable<string> csvLines)
        {
            return ParseEstrCsv(csvLines);
        }
        
        /// <summary>
        /// Serialize an ESTR CSV from the REST API response into its intermediate DTO representation.
        /// <para>
        /// Note: an iterator was used to minimize memory footprint in case of very large CSVs.
        /// Deferred execution. Behave accordingly if you are trying to debug this logic.
        /// </para>
        /// <para>
        /// Note: in the future, replacing this brittle custom logic with a 3P library that handles
        /// edge-cases better like SmallestCsvParser or CsvHelper might be a good idea.
        /// </para>
        /// </summary>
        /// <param name="csvLines">Line-by-line representation of the CSV from ECB API</param>
        /// <returns>Enumerable containing intermediate representation for the returned CSV</returns>
        public IEnumerable<EstrQuoteDto> ParseEstrCsv(IEnumerable<string> csvLines)
        {
            bool isFirstLine = true;

            foreach (string line in csvLines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(',');

                EstrQuoteDto quote = new EstrQuoteDto
                {
                    Key = parts[0].Trim(),
                    Freq = string.IsNullOrEmpty(parts[1])
                        ? throw new CsvParsingException("Value for FREQ column was missing")
                        : parts[1].Trim()[0],
                    BenchmarkItem = parts[2].Trim(),
                    DataTypeEst = parts[3].Trim(),
                    TimePeriod = DateTime.Parse(parts[4].Trim()),

                    // FIXME: Known lossy conversion from decimal to double. However, DVPLI's Scalar type
                    //  wants a double, so I have no choice.
                    //  If you are an external user and using a double is not a constraint, change to
                    //  decimal.Parse(...) and propagate this change to the DTO.
                    // NOTE: Value is divided by 100 because Fairmat needs percentages to be expressed in
                    //  0.0NNN format. Do not blindly trust this and adapt to how you handle percentages
                    //  if you are an external user.
                    ObsValue = double.Parse(parts[5].Trim(), CultureInfo.InvariantCulture) / 100
                };

                yield return quote;
            }
        }

        /// <summary>
        /// Filters parsed ESTR quotes by a date range (inclusive).
        /// <para>
        /// Note: Uses deferred execution via LINQ. The filter will be applied
        /// as quotes are enumerated, maintaining memory efficiency for large datasets.
        /// </para>
        /// </summary>
        /// <param name="quotes">Collection of ESTR quotes to filter</param>
        /// <param name="startDate">Start date (inclusive). If null, no lower bound is applied.</param>
        /// <param name="endDate">End date (inclusive). If null, no upper bound is applied.</param>
        /// <returns>Filtered enumerable of quotes within the specified date range</returns>
        public IEnumerable<EstrQuoteDto> FilterByDateRange(
            IEnumerable<EstrQuoteDto> quotes,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                throw new ArgumentException(
                    $"Start date ({startDate.Value:yyyy-MM-dd}) cannot be after end date ({endDate.Value:yyyy-MM-dd})",
                    nameof(startDate));

            return quotes.Where(quote =>
            {
                bool matchesStartDate = !startDate.HasValue || quote.TimePeriod >= startDate.Value;
                bool matchesEndDate = !endDate.HasValue || quote.TimePeriod <= endDate.Value;

                return matchesStartDate && matchesEndDate;
            });
        }

        /// <summary>
        /// Reads the raw CSV from the REST API and separates it line by line, so that individual
        /// lines can be parsed later in the execution
        /// </summary>
        /// <param name="fileContent">Byte array containing the raw CSV file content from the API GET request</param>
        /// <returns>Enumerable containing the line-by-line representation of the CSV</returns>
        public static IEnumerable<string> ReadCsvContent(byte[] fileContent)
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