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
using System.Globalization;
using System.IO;
using System.Text;
using EuropeanCentralBankIntegration.Estr.Dto;
using EuropeanCentralBankIntegration.Estr.exceptions;

namespace EuropeanCentralBankIntegration.Estr
{
    public class EstrParser
    {
        /// <summary>
        /// Serialize an ESTR CSV from the REST API response into its intermediate DTO representation
        /// </summary>
        /// <param name="csvLines"></param>
        /// <returns>Enumerable containing intermediate representation for the returned CSV</returns>
        public IEnumerable<EstrQuoteDto> ParseEstrCsv(IEnumerable<string> csvLines)
        {
            NumberFormatInfo numberFormatInfo = new NumberFormatInfo()
            {
                NumberDecimalSeparator = ".",
            };
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
                    DataTypeTest = parts[3].Trim(),
                    TimePeriod = DateTime.Parse(parts[4].Trim()),
                    ObsValue = decimal.Parse(parts[5].Trim(), numberFormatInfo)
                };
                
                yield return quote;
            }
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