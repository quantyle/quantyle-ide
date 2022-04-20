
import { format } from "d3-format";

const DEBUG = true;

const apiHost = DEBUG ? 'http://localhost:8001' : 'http://localhost:8000';
const apiHost2 = 'http://localhost:5000';
//const exchangesSocketUrl = "ws://localhost:8000/ws/exchanges";
const feedUrl = DEBUG ? "ws://localhost:8001/ws/feed/" : "ws://localhost:9000/ws/feed/";
//const tickerSocketUrl = "ws://localhost:8000/ws/tickers"; 

const iconsUrl = apiHost2 + '/image/';  //const iconsUrl = apiHost + '/media/icons/';

const logosUrl = apiHost + '/media/exchanges/logos/';
const logoIcon = apiHost + '/media/logo-v3.png';
const logoText = apiHost + '/media/q-logo-white.png';
const logoTextGrey = apiHost2 + '/image/ctm-logo-blue-3.png';
const logoSvg = apiHost + '/media/quantyle-logo-v3.svg';
const bgImg = apiHost + '/media/bg.png';
//const fomoImg = apiHost + '/media/portfolio-screenshot.png';
const fomoImg = apiHost + '/media/1085.png';
const loadingIcon = apiHost + '/media/q-coin-v3.png';
const indicators = apiHost + '/media/indicators.json';
const reCAPTCHAKey = "6LdfaL4UAAAAAJGReFGjMaa55tfVaNUVBYZcRQOe";

const coinbaseProLogo = apiHost + '/media/exchanges/coinbasepro.png';
const binanceLogo = apiHost + '/media/exchanges/binance.png';
const geminiLogo = apiHost + '/media/exchanges/gemini.png';
const krakenLogo = apiHost + '/media/exchanges/kraken.png';

const exchangesUrl = apiHost + '/media/exchanges.json';

const products = ['BTC-USD', 'ETH-USD', 'BCH-USD', 'LINK-USD'];

const intervals = ['1m', '5m', '15m', '30m', '1h', '3h', '6h', '12h', '1d', '7d'];

const websites = [
  { n: 'binance', w: 'https://www.binance.com/' },
  { n: 'bittrex', w: 'https://bittrex.com' },
  { n: 'bittrexinternational', w: 'https://bittrex.com/' },
  { n: 'coinbasepro', w: 'https://pro.coinbase.com/' },
  { n: 'coinbase', w: 'https://www.coinbase.com/' },
  { n: 'kucoin', w: 'https://www.kucoin.com/' },
  { n: 'bibox', w: 'https://www.bibox.com' },
];

const portfolioDef = [
  { name: 'Binance', tickers: [] },
  { name: 'Bittrex', tickers: [] },
  { name: 'Kucoin', tickers: [] },
  { name: 'Coinbasepro', tickers: [] },
  { name: 'Poloniex', tickers: [] },
  { name: 'Kraken', tickers: [] },
  { name: 'Bibox', tickers: [] },
  { name: 'Gemini', tickers: [] },
  { name: 'Huobi', tickers: [] },
  { name: 'Huobiglobal', tickers: [] },
  { name: 'Hitbtc', tickers: [] },
  { name: 'Bitmart', tickers: [] },
  { name: 'Bitstamp', tickers: [] },
  { name: 'Okex', tickers: [] },
  { name: 'Bitfinex', tickers: [] },
];

const exchangeNames = {
  "GDAX": "Coinbase Pro",
  "KRKN": "Kraken",
  "GMNI": "Gemini",
  "BINA": "Binance US",

};

const bookColumns = [
  {
    width: 500,
    label: 'Price',
    dataKey: 0,
  },
  {
    width: 500,
    label: 'Volume',
    dataKey: 1,
    numeric: true,
  },
  {
    width: 500,
    label: 'Cost',
    dataKey: 'cost',
    numeric: true,
  },
];

const exchangeColumns = [
  {
    width: 200,
    label: 'Name',
    dataKey: 'icon',
    img: true,
  },
  {
    width: 700,
    label: 'Exchange',
    dataKey: 'exchange',
  },
];

const toggleValue = (name, ref) => {
  ref.setState({
    [name]: !ref.state[name],
  });
};

const errorImgUrl = (e) => { e.target.src = apiHost2 + '/image/404-coin.png'; };

const changeFormat = format(".2f");
const usdFormat = format(".2f");
const btcFormat = format(".6f");
const standardFormat = format(",.5f");

/*
d3.format(".0%")(0.123);  // rounded percentage, "12%"
d3.format("($.2f")(-3.5); // localized fixed-point currency, "(£3.50)"
d3.format("+20")(42);     // space-filled and signed, "                 +42"
d3.format(".^20")(42);    // dot-filled and centered, ".........42........."
d3.format(".2s")(42e6);   // SI-prefix with two significant digits, "42M"
d3.format("#x")(48879);   // prefixed lowercase hexadecimal, "0xbeef"
d3.format(",.2r")(4223);  // grouped thousands with two significant digits, "4,200"
 */

const currencyFormat = {
  USD: format(".3f"),
  BTC: format(".2f"),
  ETH: format(".2f"),
  BCH: format(".2f"),
  LTC: format(".2f"),
  DOGE: format(".4f"),
  LINK: format(".5f"),
  UNI: format(".6f"),
  MATIC: format(".6f"),
  FIL: format(".6f"),
  SUSHI: format(".6f"),
  MKR: format(".6f"),
  ZEC: format(".6"),
}

const sizeFormat = {
  USD: format(".3f"),
  BTC: format(".2f"),
  ETH: format(".6f"),
  BCH: format(".2f"),
  LTC: format(".2f"),
  DOGE: format(".4f"),
  LINK: format(".5f"),
  UNI: format(".6f"),
  MATIC: format(".6f"),
  FIL: format(".6f"),
  SUSHI: format(".6f"),
  MKR: format(".6f"),
  ZEC: format(".6f"),
  ADA: format(".3f"),
  SHIB: format(".3f"),
}


const priceFormat = {
  USD: format(".3f"),
  BTC: format(".2f"),
  ETH: format(".2f"),
  BCH: format(".2f"),
  LTC: format(".2f"),
  DOGE: format(".4f"),
  LINK: format(".5f"),
  UNI: format(".3f"),
  MATIC: format(".4f"),
  FIL: format(".3f"),
  SUSHI: format(".2f"),
  MKR: format(".4f"),
  ZEC: format(".2f"),
}
// const sizeFormat = {
//   USD: format(".3f"),
//   BTC: format(".2f"),
//   ETH: format(".4f"),
//   BCH: format(".2f"),
//   LTC: format(".2f"),
//   DOGE: format(".4f"),
//   LINK: format(".5f"),
//   UNI: format(".6f"),
//   MATIC: format(".6f"),
//   FIL: format(".6f"),
//   SUSHI: format(".6f"),
//   MKR: format(".6f"),
//   ZEC: format(".6"),
// }

const autoComplete = {
  getCompletions: function (
    editor,
    session,
    pos,
    prefix,
    callback) {
    var completions = [];

    var customSyntax = [
      {
        name: "set_cash",
        value: "set_cash(1000)",
        caption: "set_cash",
        meta: "sets starting cash amount",
        score: 1000,
      },
      {
        name: "get_cash",
        value: "get_cash()",
        caption: "get_cash",
        meta: "returns amount of cash in current portfolio",
        score: 1000,
      },
      {
        name: "get_crypto",
        value: "get_crypto()",
        caption: "get_crypto",
        meta: "returns amount of crypto in portfolio",
        score: 1000,
      },
      {
        name: "buy_market",
        value: "buy_market(size=1)",
        caption: "buy_market",
        meta: "executes market buy order",
        score: 1000,
      },
      {
        name: "sell_market",
        value: "sell_market(size=1)",
        caption: "sell_market",
        meta: "executes market sell order",
        score: 1000,
      },
      {
        name: "sma",
        value: "sma(period=5, color=\"red\")",
        caption: "sma",
        meta: "simple moving average",
        score: 1000,
      },
      {
        name: "ema",
        value: "ema(period=10, color=\"green\")",
        caption: "ema",
        meta: "exponential moving average",
        score: 1000,
      },
      {
        name: "vwap",
        value: "vwap(period=5, color=\"red\")",
        caption: "vwap",
        meta: "volume-weighted average price",
        score: 1000,
      },
      // {
      //   name: "macd",
      //   value: "macd(ema1=12, ema2=26, signal=9)",
      //   caption: "macd",
      //   meta: "moving average convergence divergence",
      //   score: 1000,
      // },
    ];
    customSyntax.forEach(function (w) {
      completions.push(w);
    });

    callback(null, completions);
  },
};

// let orderColumns = [
//   {
//     width: 500,
//     label: 'Time',
//     dataKey: 'created_at',
//     date: true,
//   },
//   {
//     width: 350,
//     label: "Side",
//     dataKey: "side",
//     side: true,
//   },
//   {
//     width: 350,
//     label: "Size",
//     dataKey: "remaining_size",
//   },
//   {
//     width: 350,
//     label: "Price",
//     dataKey: "price",
//     price: true,
//   },
// ];

// const consoleColumn = [
//   {
//     width: 800,
//     label: '',
//     dataKey: 'data',
//     date: true,
//   },
// ];

// const fillsColumns = [
//   {
//     width: 100,
//     label: "Side",
//     dataKey: "side",
//     side: true,
//   },
//   {
//     width: 400,
//     label: "Time",
//     dataKey: "created_at",
//     date: true,
//   },

//   {
//     width: 350,
//     label: "Size",
//     dataKey: "size",
//   },
//   {
//     width: 350,
//     label: "Price",
//     dataKey: "price",
//     price: true,
//   },
// ];


export {
  apiHost,
  DEBUG,
  //exchangesSocketUrl,
  feedUrl,
  //tickerSocketUrl,
  websites,
  errorImgUrl,
  bookColumns,
  iconsUrl,
  logosUrl,
  logoIcon,
  logoText,
  logoTextGrey,
  logoSvg,
  bgImg,
  fomoImg,
  loadingIcon,
  indicators,
  portfolioDef,
  reCAPTCHAKey,
  exchangeColumns,
  intervals,
  coinbaseProLogo,
  geminiLogo,
  binanceLogo,
  krakenLogo,
  exchangesUrl,
  toggleValue,
  products,
  changeFormat,
  usdFormat,
  btcFormat,
  currencyFormat,
  sizeFormat,
  priceFormat,
  standardFormat,
  autoComplete,
  exchangeNames,
};