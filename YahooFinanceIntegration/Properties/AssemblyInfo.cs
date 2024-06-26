﻿/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Stefano Angeleri (stefano.angeleri@fairmat.com)
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

using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Addins;

// The following lines tell that the assembly is an addin.
[assembly: Addin("Yahoo! Finance Integration", "1.0.7", Category = "Market Data Provider")]
[assembly: AddinDependency("Fairmat", "1.0")]
[assembly: AddinAuthor("Fairmat SRL")]
[assembly: AddinDescription("Provides access to Yahoo! Finance Market Data.")]
[assembly: AddinDescription("Provides access to Yahoo! Finance Market Data.")]

[assembly: AssemblyTrademark("Fairmat")]
[assembly: AssemblyCulture("")]

[assembly: InternalsVisibleTo("MarketDataProviders.Tests")]
