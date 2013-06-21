using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

namespace YahooFinanceIntegration
{
    public class YahooHistoricalQuote
    {
        public DateTime Date { get; private set; }
        public double Open { get; private set; }
        public double High { get; private set; }
        public double Low { get; private set; }
        public double Close { get; private set; }
        public int Volume { get; private set; }
        public double AdjClose { get; private set; }

        public YahooHistoricalQuote()
        {
            Date = new DateTime();
            Open = 0.0f;
            High = 0.0f;
            Low = 0.0f;
            Close = 0.0f;
            Volume = 0;
            AdjClose = 0.0f;
        }

        public YahooHistoricalQuote(string csvLine)
        {
            ParseCSVLine(csvLine);
        }

        public void ParseCSVLine(string csvLine)
        {
            // We take for granted the yahoo format doesn't change and so
            // the data is listed in this way: Date,Open,High,Low,Close,Volume,Adj Close.
            // This means rows must be of 7 elements.
            string[] rows = csvLine.Split(',');

            // Check if the number of elements is right.
            if (rows.Length != 7)
            {
                throw new InvalidDataException("The csv line has a wrong number of items");
            }

            // Format used by Yahoo for dates
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy-MM-dd";
            dateFormat.DateSeparator = "-";

            // Format used by Yahoo for doubles.
            NumberFormatInfo doubleFormat = new NumberFormatInfo();
            doubleFormat.NumberDecimalSeparator = ".";
            doubleFormat.NumberGroupSeparator = ",";

            // First item is a date.
            Date = Convert.ToDateTime(rows[0], dateFormat);

            // The subsequent are all numbers.
            Open = Convert.ToDouble(rows[1], doubleFormat);
            High = Convert.ToDouble(rows[2], doubleFormat);
            Low = Convert.ToDouble(rows[3], doubleFormat);
            Close = Convert.ToDouble(rows[4], doubleFormat);
            Volume = Convert.ToInt32(rows[5]);
            AdjClose = Convert.ToDouble(rows[6], doubleFormat);
        }
    }
}
