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

namespace EuropeanCentralBankIntegration.Estr.Constants
{
    /// <summary>
    /// Enumerates the possible Data Portals that can be requested by the API.
    /// Currently only shows the main Key data point, but modularizing the logic
    /// is useful for future developments
    /// </summary>
    public sealed class DataPortal
    {
        public string Value { get; }

        private DataPortal(string value)
        {
            Value = value;
        }

        // Euro short-term, Daily - businessweek, marked "Key Data"
        public static readonly DataPortal DailyBusinessWeek = new DataPortal("B.EU000A2X2A25.WT");
    }
}