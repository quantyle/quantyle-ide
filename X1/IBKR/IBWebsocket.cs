using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace X1.IBKR
{
    public class IBWebsocket
    {
        private const string url = "127.0.0.1";

        private const Int32 port = 7497;

        private const string connectOptions = "";

        public const byte EOL = 0;

        public const int ClientVersion = 66; //API v. 9.71

        public const int MinVersion = 100;

        public const int MaxVersion = 151;

        public const int MaxMsgSize = 0x00FFFFFF;

        public const string BagSecType = "BAG";

        public const int REDIRECT_COUNT_MAX = 2;

        public const int FaGroups = 1;

        public const int FaProfiles = 2;

        public const int FaAliases = 3;

        private TcpClient client;

        private BinaryWriter tcpWriter;

        public Stream stream;

        private BinaryWriter paramsList;

        protected bool isConnected;

        protected int clientId;

        protected bool extraAuth;

        public int defaultInBufSize = ushort.MaxValue / 8;

        public IBWebsocket()
        {
            this.client = new TcpClient(url, port);
            this.stream = client.GetStream();
            this.tcpWriter = new BinaryWriter(stream);
            this.paramsList = new BinaryWriter(new MemoryStream());
        }

        public uint prepareBuffer(BinaryWriter paramsList)
        {
            var rval = (uint)paramsList.BaseStream.Position;
            paramsList.Write(0);
            return rval;
        }

        public void Send(uint lengthPos)
        {
            // go to the beginning of the file, right after API
            paramsList.Seek((int)lengthPos, SeekOrigin.Begin);
            paramsList
                .Write(IPAddress
                    .HostToNetworkOrder((
                    int
                    )(paramsList.BaseStream.Length - lengthPos - sizeof(int))));

            paramsList.Seek(0, SeekOrigin.Begin);

            var buf = new MemoryStream();
            paramsList.BaseStream.CopyTo(buf);

            // Send the message to the connected TcpServer.
            tcpWriter.Write(buf.ToArray());

            Console
                .WriteLine("Sent: {0}",
                Encoding.ASCII.GetString(buf.ToArray()));

            //paramsList.Flush();
            paramsList = new BinaryWriter(new MemoryStream());
        }

        public void Receive()
        {
            while (true)
            {
                if (IsDataAvailable())
                {
                    Byte[] data = new Byte[defaultInBufSize];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData =
                        System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                }
                else
                {
                    Console.WriteLine("done");
                    break;
                }
            }
        }

        public void StartApi()
        {
            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);

            // paramsList.Write(Encoding.UTF8.GetBytes("20"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes(code1.ToString()));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("EUR"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("CASH"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("0.0"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("IDEALPRO"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("USD"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("01"));
            // paramsList.Write (EOL);
            // paramsList.Write(" ");
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("min1"));
            // paramsList.Write (EOL);
            // paramsList.Write(" ");
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("D0BID20"));
            // paramsList.Write (EOL);
            const int VERSION = 2;
            const int clientId = 123;
            const int startApi = 71;

            paramsList.Write(Encoding.UTF8.GetBytes(startApi.ToString()));
            paramsList.Write(EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(VERSION.ToString()));
            paramsList.Write(EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(clientId.ToString()));
            paramsList.Write(EOL);
            paramsList.Write(EOL);
            Send(lengthPos);
            //Receive();
        }

        public bool IsDataAvailable()
        {
            // if (!isConnected) return false;
            var networkStream = stream as NetworkStream;

            return networkStream == null || networkStream.DataAvailable;
        }


        public void ReqHistoricalTicks(string symbol, string secType, string exchange, string currency = "USD", string lastTradeDateOrContractMonth = "", string whatToShow = "TRADES")
        {
            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);
            const int reqHistoricalTicks = 96;
            IBReqHistoricalTicks req =
                new IBReqHistoricalTicks
                {
                    reqType = reqHistoricalTicks,
                    reqId = 1,
                    conId = 0,
                    symbol = symbol,
                    secType = secType,
                    lastTradeDateOrContractMonth = lastTradeDateOrContractMonth,
                    strike = 0.0,
                    right = "",
                    multiplier = "",
                    exchange = exchange,
                    primaryExchange = "",
                    currency = currency,
                    localSymbol = "",
                    tradingClass = "",
                    includeExpired = 0,
                    startDateTime = "20210904 12:01:00",
                    endDateTime = "",
                    numberOfTicks = 100,
                    whatToShow = whatToShow,
                    useRTH = 0,
                    ignoreSize = "",
                    miscOptions = ""
                };

            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }

        public void ReqHistoricalData(string symbol, string secType, string exchange, string currency = "USD", string lastTradeDateOrContractMonth = "", string whatToShow = "TRADES")
        {
            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);
            const int reqHistoricalData = 20;
            IBReqHistoricalData req =
                new IBReqHistoricalData
                {
                    reqType = reqHistoricalData,
                    reqId = 1,
                    conId = 0,
                    symbol = symbol,
                    secType = secType,
                    lastTradeDateOrContractMonth = lastTradeDateOrContractMonth,
                    strike = 0.0,
                    right = "",
                    multiplier = "",
                    exchange = exchange,
                    primaryExchange = "",
                    currency = currency,
                    localSymbol = "",
                    tradingClass = "",
                    includeExpired = 1,
                    endDateTime = "",
                    barSizeSetting = "1 min",
                    durationStr = "1 D",
                    useRTH = 0,
                    whatToShow = whatToShow,
                    formatDate = 2,
                    keepUpToDate = 0,
                    chartOptions = ""
                };

            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }

        public void ReqTickByTick(string symbol, string secType, string exchange, string currency = "USD", string lastTradeDateOrContractMonth = "")
        {

            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);
            const int reqTickByTick = 97;
            // Last is not available for forex, only midpoint. It doesn't trade on an exchange
            string tickType = secType == "CASH" ? "MidPoint" : "Last";
            IBReqTickByTick req =
                new IBReqTickByTick
                {
                    reqType = reqTickByTick,
                    reqId = 1,
                    conId = 0,
                    symbol = symbol,
                    secType = secType,
                    lastTradeDateOrContractMonth = lastTradeDateOrContractMonth,
                    strike = 0.0,
                    right = "",
                    multiplier = "",
                    exchange = exchange,
                    primaryExchange = "",
                    currency = currency,
                    localSymbol = "",
                    tradingClass = "",
                    tickType = tickType,
                    numberOfTicks = 0,
                    ignoreSize = "",
                };

            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }

        public void ReqMarketDepth(string symbol, string secType, string exchange, string currency = "USD", string lastTradeDateOrContractMonth = "")
        {
            const int reqMktDepth = 10;
            uint lengthPos = prepareBuffer(paramsList);

            IBReqMktDepth req =
                new IBReqMktDepth
                {
                    reqType = reqMktDepth,
                    VERSION = 5,
                    reqId = 1,
                    conId = 0,
                    symbol = symbol,
                    secType = secType,
                    lastTradeDateOrContractMonth = lastTradeDateOrContractMonth,
                    strike = 0.0,
                    right = "",
                    multiplier = "",
                    exchange = exchange,
                    primaryExchange = "",
                    currency = currency,
                    localSymbol = "",
                    tradingClass = "",
                    numRows = 20,
                    isSmartDepth = 0,
                    mktDataOptions = "",
                };


            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
                //Console.WriteLine(prop.GetValue(req, null).ToString());
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }

        public void ReqAccountSummary()
        {
            const int reqAccountSummary = 62;
            uint lengthPos = prepareBuffer(paramsList);

            IBReqAccountSummary req =
                new IBReqAccountSummary
                {
                    reqType = reqAccountSummary,
                    VERSION = 1,
                    reqId = 1,
                    group = "All",
                    tags = "AccountType,NetLiquidation,TotalCashValue,SettledCash,AccruedCash,BuyingPower",
                };


            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
                //Console.WriteLine(prop.GetValue(req, null).ToString());
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }

        public void ReqMktData(string symbol, string secType, string exchange, string currency = "USD", string lastTradeDateOrContractMonth = "")
        {
            const int reqMktData = 1;
            uint lengthPos = prepareBuffer(paramsList);
            //     IBReqHistoricalData req =
            //         new IBReqHistoricalData
            //         {
            //             reqType = reqHistoricalData,
            //             reqId = 1,
            //             conId = 0,
            //             symbol = "EUR",
            //             secType = "CASH",
            //             lastTradeDateOrContractMonth = "",
            //             strike = 0.0,
            //             right = "",
            //             multiplier = "",
            //             exchange = "IDEALPRO",
            //             primaryExchange = "",
            //             currency = "USD",
            //             localSymbol = "",
            //             tradingClass = "",
            //             includeExpired = 0,
            //             endDateTime = "",
            //             barSizeSetting = "1 min",
            //             durationStr = "1 D",
            //             useRTH = 0,
            //             whatToShow = "BID",
            //             formatDate = 2,
            //             keepUpToDate = 0,
            //             chartOptions = ""
            //         };
            IBReqMktData req =
                new IBReqMktData
                {
                    reqType = reqMktData,
                    VERSION = 11,
                    reqId = 1,
                    conId = 0,
                    symbol = symbol,
                    secType = secType,
                    lastTradeDateOrContractMonth = lastTradeDateOrContractMonth,
                    strike = 0.0,
                    right = "",
                    multiplier = "",
                    exchange = exchange,
                    primaryExchange = "",
                    currency = currency,
                    localSymbol = "",
                    tradingClass = "",
                    internalUseOnly = "0",
                    genericTickList = "",
                    snapshot = 0,
                    regulatorySnapshot = 0,
                    mktDataOptions = "",
                };


            foreach (PropertyInfo prop in req.GetType().GetProperties())
            {
                paramsList.Write(Encoding.UTF8.GetBytes(prop.GetValue(req, null).ToString()));
                paramsList.Write(EOL);
                //Console.WriteLine(prop.GetValue(req, null).ToString());
            }

            paramsList.Write(EOL);
            Send(lengthPos);
        }




        public void Connect()
        {
            try
            {
                string encodedVersion =
                    MinVersion.ToString() +
                    (
                    MaxVersion != MinVersion ? ".." + MaxVersion : string.Empty
                    );

                paramsList.Write(Encoding.UTF8.GetBytes("API"));
                paramsList.Write(EOL);

                uint lengthPos = prepareBuffer(paramsList);

                // versions "v100..151"
                paramsList
                    .Write(Encoding
                        .ASCII
                        .GetBytes("v" +
                        encodedVersion +
                        (
                        string.IsNullOrEmpty(connectOptions)
                            ? string.Empty
                            : " " + connectOptions
                        )));

                // Send (paramsList, lengthPos);
                // Receive();
                // const int VERSION = 2;
                // const int clientId = 123;
                // const int startApi = 71;
                // // // paramsList = new BinaryWriter(new MemoryStream());
                // // lengthPos = prepareBuffer(paramsList);
                // // paramsList.Write (EOL);
                // paramsList.Write(startApi);
                // paramsList.Write (EOL);
                // paramsList.Write(VERSION);
                // paramsList.Write (EOL);
                // paramsList.Write(clientId);
                // paramsList.Write (EOL);
                Send(lengthPos);

                //Receive();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void Close()
        {
            // Close everything.
            stream.Close();
            client.Close();
        }
    }
}

/*

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ibkr_net
{
    public class IBWebsocket
    {
        private const string url = "127.0.0.1";

        private const Int32 port = 7497;

        private const string connectOptions = "";

        public const byte EOL = 0;

        public const int ClientVersion = 66; //API v. 9.71

        public const int MinVersion = 100;

        public const int MaxVersion = 151;

        public const int MaxMsgSize = 0x00FFFFFF;

        public const string BagSecType = "BAG";

        public const int REDIRECT_COUNT_MAX = 2;

        public const int FaGroups = 1;

        public const int FaProfiles = 2;

        public const int FaAliases = 3;

        private TcpClient client;

        private BinaryWriter tcpWriter;

        private Stream stream;

        private BinaryWriter paramsList;

        protected bool isConnected;

        protected int clientId;

        protected bool extraAuth;

        private BinaryReader dataReader;

        private int nDecodedLen;

        const int defaultInBufSize = ushort.MaxValue / 8;

        public IBWebsocket()
        {
            this.client = new TcpClient(url, port);
            this.stream = client.GetStream();
            this.tcpWriter = new BinaryWriter(stream);
            this.paramsList = new BinaryWriter(new MemoryStream());
        }

        public bool IsDataAvailable()
        {
            // if (!isConnected) return false;
            var networkStream = stream as NetworkStream;

            return networkStream == null || networkStream.DataAvailable;
        }

        public uint prepareBuffer(BinaryWriter paramsList)
        {
            var rval = (uint) paramsList.BaseStream.Position;
            paramsList.Write(0);
            return rval;
        }

        public void Send(uint lengthPos)
        {
            // go to the beginning of the file, right after API
            paramsList.Seek((int) lengthPos, SeekOrigin.Begin);
            paramsList
                .Write(IPAddress
                    .HostToNetworkOrder((
                    int
                    )(paramsList.BaseStream.Length - lengthPos - sizeof(int))));

            paramsList.Seek(0, SeekOrigin.Begin);

            var buf = new MemoryStream();
            paramsList.BaseStream.CopyTo (buf);

            // Send the message to the connected TcpServer.
            tcpWriter.Write(buf.ToArray());

            Console
                .WriteLine("Sent: {0}",
                Encoding.ASCII.GetString(buf.ToArray()));

            //paramsList.Flush();
            paramsList = new BinaryWriter(new MemoryStream());
        }

        public void Receive()
        {
            // create input buffer
            List<byte> inBuf = new List<byte>(defaultInBufSize);

            inBuf.AddRange(ReadAtLeastNBytes(inBuf.Capacity - inBuf.Count));

            dataReader = new BinaryReader(new MemoryStream(inBuf.ToArray()));
            nDecodedLen = 0;

            // read string
            byte b = dataReader.ReadByte();

            nDecodedLen++;

            if (b == 0)
            {
                Console.WriteLine("> {0}", b);
                Console.WriteLine("EMPTY");
            }
            else
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append((char) b);
                while (true)
                {
                    b = dataReader.ReadByte();
                    if (b == 0)
                    {
                        break;
                    }
                    else
                    {
                        strBuilder.Append((char) b);
                    }
                }

                nDecodedLen += strBuilder.Length;
                Console.WriteLine("Received: |{0}|", strBuilder.ToString());
            }
        }

        public void StartApi()
        {
            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);

            // paramsList.Write(Encoding.UTF8.GetBytes("20"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes(code1.ToString()));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("EUR"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("CASH"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("0.0"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("IDEALPRO"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("USD"));
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("01"));
            // paramsList.Write (EOL);
            // paramsList.Write(" ");
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("min1"));
            // paramsList.Write (EOL);
            // paramsList.Write(" ");
            // paramsList.Write (EOL);
            // paramsList.Write(Encoding.UTF8.GetBytes("D0BID20"));
            // paramsList.Write (EOL);
            const int VERSION = 2;
            const int clientId = 123;
            const int startApi = 71;

            paramsList.Write(Encoding.UTF8.GetBytes(startApi.ToString()));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(VERSION.ToString()));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(clientId.ToString()));
            paramsList.Write (EOL);
            paramsList.Write (EOL);
            Send (lengthPos);
            Receive();
            // Receive();
            // Receive();
        }

        public byte[] ReadAtLeastNBytes(int msgSize)
        {
            var buf = new byte[msgSize];
            return buf.Take(stream.Read(buf, 0, msgSize)).ToArray();
        }

        public void ReqHistoricalData()
        {
            // 2010EURCASH0.0IDEALPROUSD01 min1 D0BID20
            uint lengthPos = prepareBuffer(paramsList);

            // // paramsList = new BinaryWriter(new MemoryStream());
            // lengthPos = prepareBuffer(paramsList);
            // paramsList.Write (EOL);
            // paramsList
            //     .Write(Encoding
            //         .UTF8
            //         .GetBytes("2010EURCASH0.0IDEALPROUSD01 min1 D0BID20 "));
            // paramsList.Write (EOL);
            //const int useRTH = 0;
            const int reqHistoricalData = 20;
            const int reqId = 1;

            const string symbol = "EUR";

            const string secType = "CASH";

            const string exchange = "IDEALPRO";

            const string currency = "USD";

            const string whatToShow = "BID";

            const string barSizeSetting = "1 min";

            const string durationStr = "1 D";

            paramsList
                .Write(Encoding.UTF8.GetBytes(reqHistoricalData.ToString()));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(reqId.ToString()));

            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("0"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(symbol));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(secType));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("0.0"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(exchange));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(currency));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("0"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(barSizeSetting));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(durationStr));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("0"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(whatToShow));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("2"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes("0"));
            paramsList.Write (EOL);
            paramsList.Write(Encoding.UTF8.GetBytes(""));
            paramsList.Write (EOL);
            paramsList.Write (EOL);

            Send (lengthPos);

            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // Receive();
            // create input buffer

            

            while (true)
            {
                List<byte> inBuf = new List<byte>(defaultInBufSize);
                inBuf.AddRange(ReadAtLeastNBytes(inBuf.Capacity - inBuf.Count));

                dataReader =
                    new BinaryReader(new MemoryStream(inBuf.ToArray()));
                nDecodedLen = 0;

                // read string
                byte b = dataReader.ReadByte();

                nDecodedLen++;

                if (b == 0)
                {
                    Console.WriteLine("> {0}", b);
                    Console.WriteLine("EMPTY");
                }
                else
                {
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.Append((char) b);
                    while (true)
                    {
                        b = dataReader.ReadByte();
                        if (b == 0)
                        {
                            break;
                        }
                        else
                        {
                            strBuilder.Append((char) b);
                        }
                    }

                    nDecodedLen += strBuilder.Length;
                    
                }
                Thread.Sleep(100);
            }
        }

        public void Connect()
        {
            try
            {
                string encodedVersion =
                    MinVersion.ToString() +
                    (
                    MaxVersion != MinVersion ? ".." + MaxVersion : string.Empty
                    );

                paramsList.Write(Encoding.UTF8.GetBytes("API"));
                paramsList.Write (EOL);

                uint lengthPos = prepareBuffer(paramsList);

                // versions "v100..151"
                paramsList
                    .Write(Encoding
                        .ASCII
                        .GetBytes("v" +
                        encodedVersion +
                        (
                        string.IsNullOrEmpty(connectOptions)
                            ? string.Empty
                            : " " + connectOptions
                        )));

                // Send (paramsList, lengthPos);
                // Receive();
                // const int VERSION = 2;
                // const int clientId = 123;
                // const int startApi = 71;
                // // // paramsList = new BinaryWriter(new MemoryStream());
                // // lengthPos = prepareBuffer(paramsList);
                // // paramsList.Write (EOL);
                // paramsList.Write(startApi);
                // paramsList.Write (EOL);
                // paramsList.Write(VERSION);
                // paramsList.Write (EOL);
                // paramsList.Write(clientId);
                // paramsList.Write (EOL);
                Send (lengthPos);

                Receive();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void Close()
        {
            // Close everything.
            stream.Close();
            client.Close();
        }
    }
}


*/
