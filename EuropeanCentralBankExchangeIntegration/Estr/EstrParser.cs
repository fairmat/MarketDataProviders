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
using System.Text;
using EuropeanCentralBankIntegration.Estr.Dto;

namespace EuropeanCentralBankIntegration.Estr
{
    public abstract class EstrParser
    {
        /// <summary>
        /// Serialize an ESTR CSV from the REST API response into its intermediate DTO representation
        /// </summary>
        /// <param name="csv"></param>
        /// <returns>Enumerable containing intermediate representation for the returned CSV</returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<EstrQuoteDto> ParseEstrCsv(string[] csv)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Reads the raw CSV from the REST API and separates it line by line, so that individual
        /// lines can be parsed later in the execution
        /// </summary>
        /// <param name="fileContent">Byte array containing the raw CSV file content from the API GET request</param>
        /// <returns>Enumerable containing the line-by-line representation of the CSV</returns>
        public static IEnumerable<string> ReadCSvContent(byte[] fileContent)
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