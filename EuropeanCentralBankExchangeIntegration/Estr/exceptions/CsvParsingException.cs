using System;

namespace EuropeanCentralBankIntegration.Estr.exceptions
{
    public class CsvParsingException : InvalidOperationException
    {
        public CsvParsingException(string message)
        {
            throw new InvalidOperationException(message);
        }
    }
}