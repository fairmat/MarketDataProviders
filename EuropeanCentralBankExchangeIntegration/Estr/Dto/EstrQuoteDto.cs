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

namespace EuropeanCentralBankIntegration.Estr.Dto
{
    /// <summary>
    /// DTO that represents a line from the CSV that the ECB REST API returns
    /// when requesting ESTR data. Intermediate representation for parsing
    /// </summary>
    public class EstrQuoteDto
    {
        public string Key { get; set; }
        public char Freq { get; set; }
        public string BenchmarkItem { get; set; }
        public string DataTypeTest { get; set; }
        public DateTime TimePeriod { get; set; }
        public double ObsValue { get; set; }
    }
}
