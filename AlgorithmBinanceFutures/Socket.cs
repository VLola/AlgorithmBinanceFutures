using Binance.Net;
using Binance.Net.Interfaces.SocketSubClient;
using Binance.Net.Interfaces.SubClients.Futures;
using System;

namespace AlgorithmBinanceFutures
{
    public static class Socket
    {
        public static string ApiKey { get; private set; }
        public static string SecretKey { get; private set; }
        public static BinanceClient client = new BinanceClient();
        public static BinanceSocketClient socketClient = new BinanceSocketClient();
        public static IBinanceClientFuturesUsdt futures { get; set; }
        public static IBinanceSocketClientFuturesUsdt futuresSocket { get; set; }
        public static void Connect(string api, string secret)
        {
            try
            {
                ApiKey = api;
                SecretKey = secret;
                client.SetApiCredentials(ApiKey, SecretKey);
                socketClient = new BinanceSocketClient();
                socketClient.SetApiCredentials(ApiKey, SecretKey);
                futures = client.FuturesUsdt;
                futuresSocket = socketClient.FuturesUsdt;
            }
            catch (Exception e)
            {
                ErrorText.Add($"Connect {e.Message}");
            }
        }
    }
}
