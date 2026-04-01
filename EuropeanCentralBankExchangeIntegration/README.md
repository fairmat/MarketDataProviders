EuropeanCentralBankIntegration
===================

This plugin provides access to the European Central Bank Historical Exchange Data in order to gather data for use by Fairmat.

The plugin currently supports only Scalar data requests of exchange rate between euro and other currencies.

The description of the API used can be found here: https://www.ecb.int/stats/exchange/eurofxref/html/index.en.html.

## €STR (Euro short-term rate)

This plugin now supports importing ESTR data from the ECB API.

The logic used to download and parse ESTR data from the European Central Bank's REST API service is available in the
`EuropeanCentralBankExchangeIntegration.Estr` namespace.

This part of the plugin actually consumes a different API ENDPOINT, the SDMX REST API service. Refer
to [its documentation](https://github.com/sdmx-twg/sdmx-rest) for more information.

The plugin entry point for Fairmat data ingestion is still `EuropeanCentralBankExchangeIntegration/EuropeanCentralBankIntegration.cs`
