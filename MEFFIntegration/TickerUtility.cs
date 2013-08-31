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
        /// Adds the provided symbol to a list of <see cref="ISymbolDefinition"/>
        /// and adds additional similar symbol used for compatibility with some data providers.
        /// For now it adds "ticker" and "ticker Index".
        /// </summary>
        /// <param name="list">The list where to add the elements.</param>
        /// <param name="ticker">The ticker symbol to add.</param>
        /// <param name="description">The description to use for this addition.</param>
        public static void AddSymbols(List<ISymbolDefinition> list, string ticker, string description)
        {
            list.Add(new SymbolDefinition(ticker, description));
            list.Add(new SymbolDefinition(ticker + " Index", description));
        }

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
