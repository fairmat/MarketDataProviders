using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVPLI.Interfaces;
using DVPLI;


namespace TickersUtils
{
    public class TickerUtility
    {
        /// <summary>
        /// Parses the provided symbol looking for parts added by
        /// the <see cref="AddSymbols"/> method.
        /// If any is found the symbol is cleaned up from them.
        /// </summary>
        /// <param name="symbol">The symbol to parse and cleanup.</param>
        /// <returns>The cleaned up symbol.</returns>
        public static string PreparseSymbol(string symbol)
        {
            if (symbol.EndsWith(" Index"))
            {
                return symbol.Remove(symbol.LastIndexOf(" Index"));
            }

            return symbol;
        }
    }
}
