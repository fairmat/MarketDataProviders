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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVPLI;
using DVPLI.MarketDataTypes;

namespace MEEFIntegration
{
    /// <summary>
    /// Retrieves information from Option Quote lists
    /// </summary>
    public class OptionQuotesUtility
    {
        /// <summary>
        /// Populates CallPriceMarketData data structure from options quotes and additional calls to market data.
        /// </summary>
        /// <param name="provider">The underlying market data provider.</param>
        /// <param name="mdq">The underlying query.</param>
        /// <param name="quotes">The options quotes list.</param>
        /// <param name="data">The data structure to be populated.</param>
        /// <returns>The status of the operation.</returns>
        public static RefreshStatus GetCallPriceMarketData(IMarketDataProvider provider, MarketDataQuery mdq, List<IOptionQuote> quotes, Fairmat.MarketData.CallPriceMarketData data)
        {
            //Gets call options
            var calls = quotes.FindAll(x => x.Type == OptionQuoteType.Call);
            var puts = quotes.FindAll(x => x.Type == OptionQuoteType.Put);


            //get maturities and strikes
            var maturtiesDates = quotes.Select(item => item.Maturity).Distinct().OrderBy(x => x).ToList();
            var strikes = quotes.Select(item => item.Strike).Distinct().OrderBy(x => x).ToList();
            data.Strike = (Vector)strikes.ToArray();
            //var callMaturtiesDates = calls.Select(item => item.MaturityDate).Distinct().ToList();

            Console.WriteLine("Maturities");
            data.Maturity = new Vector(maturtiesDates.Count);
            for (int z = 0; z < maturtiesDates.Count; z++)
                data.Maturity[z] = RightValueDate.DatesDifferenceCalculator(mdq.Date, maturtiesDates[z]);


            data.CallPrice = new Matrix(data.Maturity.Length, data.Strike.Length);
            var PutPrices = new Matrix(data.Maturity.Length, data.Strike.Length);




            //Group maturities, calls, puts  with respect to strikes 
            Dictionary<double, Tuple<List<double>, List<double>, List<double>>> atm = new Dictionary<double, Tuple<List<double>, List<double>, List<double>>>();

            for (int si = 0; si < data.Strike.Length; si++)
                for (int mi = 0; mi < maturtiesDates.Count; mi++)
                {
                    IOptionQuote cQuote = calls.Find(x => x.Strike == data.Strike[si] && x.Maturity == maturtiesDates[mi]);
                    if (cQuote != null)
                        data.CallPrice[mi, si] = cQuote.Price;

                    IOptionQuote pQuote = puts.Find(x => x.Strike == data.Strike[si] && x.Maturity == maturtiesDates[mi]);
                    if (pQuote != null)
                        PutPrices[mi, si] = pQuote.Price;

                    if (cQuote != null && pQuote != null)
                    {
                        Tuple<List<double>, List<double>, List<double>> element = null;

                        if (atm.ContainsKey(data.Strike[si]))
                            element = atm[data.Strike[si]];
                        else
                        {
                            element = new Tuple<List<double>, List<double>, List<double>>(new List<double>(), new List<double>(), new List<double>());
                            atm.Add(data.Strike[si], element);
                        }

                        element.Item1.Add(data.Maturity[mi]);
                        element.Item2.Add(cQuote.Price);
                        element.Item3.Add(pQuote.Price);

                    }
                }
            Console.WriteLine("CallPrices");
            Console.WriteLine(data.CallPrice);
            Console.WriteLine("Putprices");
            Console.WriteLine(PutPrices);


            //Requests the spot price 
            var mdq2 = DVPLI.ObjectSerialization.CloneObject(mdq) as MarketDataQuery;
            mdq2.MarketDataType = typeof(Scalar).ToString();
            IMarketData s0;
            var s0Result = provider.GetMarketData(mdq2, out s0);
            if (s0Result.HasErrors)
                return s0Result;
            data.S0 = (s0 as Scalar).Value;


            //loads atm info (get the strike with the higher number of elements)
            int maxElements = -1;
            double argMax = -1;
            double spotDistance = double.MaxValue; // Strike-Spot distance 

            //Finds the options which minimize the distance from strike price and spot price
            foreach (double strike in atm.Keys)
            {
                double distance = Math.Abs(strike - data.S0);
                if (distance < spotDistance)
                {
                    spotDistance = distance;
                    maxElements = atm[strike].Item1.Count;
                    argMax = strike;
                }
            }

            if (spotDistance != double.MaxValue)
            {
                data.StrikeATM = argMax;
                data.MaturityATM = (Vector)atm[argMax].Item1.ToArray();
                data.CallPriceATM = (Vector)atm[argMax].Item2.ToArray();
                data.PutPriceATM = (Vector)atm[argMax].Item3.ToArray();
            }
            else
            {
                Console.WriteLine("Cannot find atm information");
            }

            data.Ticker = mdq.Ticker;
            data.Market = mdq.Market;
            data.Date = mdq.Date;

            return new RefreshStatus();
        }
    }

}
