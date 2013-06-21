/* Copyright (C) 2013 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVPLI;

namespace YahooFinanceIntegration
{
    [Mono.Addins.Extension("/Fairmat/MarketDataProvider")]
    public class YahooFinanceIntegration : IMarketDataProvider
    {
        #region IMarketDataProvider Implementation

        private string credentials;

        public string Credentials
        {
            set
            {
                credentials = value;
            }
        }

        public RefreshStatus GetMarketData(MarketDataQuery mdq, out IMarketData marketData)
        {
            RefreshStatus status = new RefreshStatus();
            status.HasErrors = true;
            status.ErrorMessage += "GetMarketData: Market data not available. Query = " + mdq;
            marketData = null;
            return status;
        }

        public RefreshStatus GetTimeSeries(MarketDataQuery mdq, DateTime end, out DateTime[] dates, out IMarketData[] marketData)
        {
            marketData = null;
            dates = null;
            RefreshStatus stat = new RefreshStatus();
            stat.HasErrors = true;
            stat.ErrorMessage += "GetTimeSeries: Market data not available";
            return stat;
        }

        public Status TestConnectivity()
        {
            Status state = new Status();
            state.HasErrors = false;
            state.ErrorMessage = "";
            return state;
        }

        #endregion
    }
}
