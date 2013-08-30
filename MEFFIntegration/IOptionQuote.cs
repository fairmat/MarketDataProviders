/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Matteo Tesser (matteo.tesser@fairmat.com)
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
using OptionQuotes;

namespace OptionQuotes
{
    /// <summary>
    /// Includes basic information about traded options.
    /// </summary>
    public interface IOptionQuote
    {
        /// <summary>
        /// Gets the price of the option.
        /// </summary>
        double Price { get; }

        /// <summary>
        /// Gets the strike value of the option.
        /// </summary>
        double Strike { get; }

        /// <summary>
        /// Gets the expiration date of this option.
        /// </summary>
        DateTime Maturity { get; }

        /// <summary>
        /// Gets the type of this option quote.
        /// </summary>
        OptionQuoteType Type { get; }
    }
}
