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
    /// Singleton containing constants and configurations for ESTR data provider
    /// </summary>
    public static class EstrTickerConstants
    {
        /// <summary>
        /// Ticker postfixes that may be appended to ticker symbols for compatibility with different platforms
        /// </summary>
        public static readonly string[] CurrencyPostfixes = { " Curncy", " Index", " ECB Curncy" };

        /// <summary>
        /// List of currencies supported by the ECB for exchange rate data
        /// </summary>
        public static readonly string[] SupportedCurrencies = 
        { 
            "AUD",
            "BGN",
            "BRL",
            "CAD",
            "CHF",
            "CNY",
            "CZK",
            "DKK",
            "GBP",
            "HKD",
            "HRK",
            "HUF",
            "IDR",
            "ILS",
            "INR",
            "JPY",
            "KRW",
            "LTL",
            "LVL",
            "MXN",
            "MYR",
            "NOK",
            "NZD",
            "PHP",
            "PLN",
            "RON",
            "SEK",
            "SGD",
            "THB",
            "TRY",
            "USD",
            "ZAR",
        };

        /// <summary>
        /// ESTR ticker identifiers
        /// </summary>
        public static class Tickers
        {
            /// <summary>
            /// Base ESTR ticker symbol
            /// </summary>
            public const string EstrDaily = "ESTR";

            /// <summary>
            /// ESTR ticker with Bloomberg Index postfix
            /// </summary>
            public const string EstrDailyIndex = "ESTR Index";
        }
    }
}