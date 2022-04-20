import React, { Component } from 'react';
import PropTypes from 'prop-types';
import tradeStyle from '../variables/styles/tradeStyle';
import {
  withStyles,
  Grid,
  // MenuItem,
  // List, 
  // ListSubheader,
  // ListItem,
  // ListItemIcon,
  // ListItemText,
  // Collapse,
} from '@material-ui/core';
import {
  GridItem,
  // Button,
  Snackbar,
  // BacktestChart,
  //TickChart,
  // OrderBook,
  // MoneyField,
  // ReactVirtualizedTable,
  // TextField,
  ListItem,
  // Loading,
  // TradeHistory,
  //BookChart,
} from '../components';
// import 'react-perfect-scrollbar/dist/css/styles.css';
import {
  // iconsUrl,
  usdFormat,
  btcFormat,
  currencyFormat,
  feedUrl,
  sizeFormat,
  // priceFormat,
  // intervals,
} from '../variables/global';
import api from '../providers/api';
import { Redirect } from "react-router-dom";
// import { format } from "d3-format";
// import {
//   successColor,
//   dangerColor,
// } from "../../variables/styles";
import {
  // Send,
  //  Drafts,
  //  Inbox,
  //  ExpandLess,
  //  ExpandMore,
} from '@material-ui/icons';


const resolutions = {
  '1m': 1,
  '5m': 5,
  '15m': 10,
  '30m': 30,
  '1h': 60,
  '6h': 360,
  '1d': 1440,
}

// const orderColumns = [
//   {
//     width: 100,
//     label: 'Side',
//     dataKey: 'side',
//     side: true,
//   },
//   {
//     width: 400,
//     label: 'Time',
//     dataKey: 'created_at',
//     date: true,
//   },

//   {
//     width: 350,
//     label: 'Size',
//     dataKey: 'remaining_size',
//   },
//   {
//     width: 350,
//     label: 'Price',
//     dataKey: 'price',
//     price: true,
//   },
// ];

// const fillsColumns = [
//   {
//     width: 100,
//     label: 'Side',
//     dataKey: 'side',
//     side: true,
//   },
//   {
//     width: 4230,
//     height: 240,

//     width: 400,
//     label: 'Time',
//     dataKey: 'created_at',
//     date: true,
//   },

//   {
//     width: 350,
//     label: 'Size',
//     dataKey: 'size',
//   },
//   {
//     width: 350,
//     label: 'Price',
//     dataKey: 'price',
//     price: true,
//   },
// ];

class SettingsView extends Component {

  static defaultProps = {
    isResizable: false,
  };

  constructor(props) {
    super(props);

    this.exchange = "GDAX"
    this.product = "BTC-USD";
    this.state = {
      exchange_id: this.exchange,
      product_id: this.product,
      base: this.product.split("-")[0],
      quote: this.product.split("-")[1],
      openSnackbar: false,
      snackbarType: "error",
      snackMessage: "",
      // redirectPath: null,
      percent: "",
      stopPrice: "",
      orderPrice: "",
      size: "",
      fee: "",
      total: "",
      resolution: "",
      orderSide: "buy",
      orderType: "limit",
      selectedInterval: "1m",
      selectAllOrders: false,
      selectAllFills: false,
      selectedOrders: [],
      selectedFills: [],
      fills: [],
      openOrders: [],
      accounts: {},
      portfolioValue: 0,
      quoteValue: 0,
      baseValue: 0,
      change24hr: "",
      // bookChart: [],
      portfolio: {},
      allTicks: {},
      // code: code,
      lineColors: [],
      newCodeResponse: false,
      tabValue: 1,
      // backtestResult: [],
      // indicatorNames: [],
      //ticker: {},
      //chart: [],
      //matches: [],
      //codeResponse: [],
      // book: {
      //   asks: [],
      //   bids: [],
      //   volume: 1,
      // },
      open: false,
    };


    this.chartTimeout = null;
    // this.myRef = React.createRef();
    // this.myRefScroll = false;

    this.chart = [];
    this.ticks = [];
    this.activeAccounts = {};
    this.ticker = {};
    this.tickers = [];
    this.book = {};
    this.data = {};

    //document.title = this.cache.exchange_id + ' | ' + this.cache.product_id;
  }

  componentDidMount() {
    console.log("started")
    //this.start(this.state.exchange_id, this.state.product_id, this.state.interval);
  }

  componentWillUnmount() {
    // cleanup
    //this.stop();
  }

  stop() {
    this.feedSocket.close();
    this.feedSocket.removeEventListener('message', this.handleFeedSocketMessage);
    this.feedSocket.removeEventListener('close', this.handleFeedSocketClose);

    this.accountSocket.close();
    this.accountSocket.removeEventListener('message', this.handleAccountSocketMessage);
    this.accountSocket.removeEventListener('close', this.handleAccountSocketClose);

    this.algoSocket.close()
    this.algoSocket.removeEventListener('message', this.handleAlgoSocketMessage);
    this.algoSocket.removeEventListener('close', this.handleAlgoSocketClose);
  }

  async getSnapshot(exchange_id, product_id) {
    // get products that need to be saved
    let response = await api.get('/api/ticker-snapshot/', {
      exchange_id: exchange_id,
      product_id: product_id
    });
    console.log("SNAPSHOT: ", response);
    response.data.forEach(t => t.date = new Date(t.time * 1000));
    this.ticks = response.data;
    this.matches = [...response.data].reverse().slice(0, 70);
  }

  async getChart(product_id, interval) {
    const response = await api.getChartData(
      'GDAX',
      product_id,
      interval,
      'start',
      'end',
    );
    console.log('getChart:', response)
    response.data.data.forEach(row => {
      row.t = new Date(row.date * 1000);
    });
    return response.data.data;
  }

  async getPortfolio() {
    const response = await api.getPortfolio();
    console.log(response);
    return response.data.data;
  }


  async start(exchange_id, product_id, interval) {
    // await this.getSnapshot(exchange_id, product_id);
    this.chart = await this.getChart(product_id, interval);
    //const portfolio = await api.get('/api/portfolio/');
    const portfolio = await this.getPortfolio();
    console.log("-----------------", portfolio);
    // get products that need to be saved
    // let algoSnapshotReponse = await api.get('/api/algo-snapshot/', {
    //   product_id: product_id
    // });
    // algoSnapshotReponse.data.forEach(tick => {
    //   tick.date = new Date(tick.time * 1000);
    // });

    // console.log('------', algoSnapshotReponse.data)
    this.chartInterval = setInterval(() => this.updateState(), 300);

    this.accountHost = feedUrl + product_id + '/BALANCES';
    this.algoHost = feedUrl + 'run/algos';
    this.feedHost = feedUrl + exchange_id + '/' + product_id;

    // this.feedSocket = new WebSocket(this.feedHost);
    this.feedSocket = new WebSocket('ws://127.0.0.1:8004/GDAX/BTC-USD');
    this.handleFeedSocketMessage = this.handleFeedSocketMessage.bind(this);
    this.handleFeedSocketClose = this.handleFeedSocketClose.bind(this);
    this.feedSocket.addEventListener('message', this.handleFeedSocketMessage);
    this.feedSocket.addEventListener('close', this.handleFeedSocketClose);

    this.accountSocket = new WebSocket(this.accountHost);
    this.handleAccountSocketMessage = this.handleAccountSocketMessage.bind(this);
    this.handleAccountSocketClose = this.handleAccountSocketClose.bind(this);
    this.accountSocket.addEventListener('message', this.handleAccountSocketMessage);
    this.accountSocket.addEventListener('close', this.handleAccountSocketClose);


    this.algoSocket = new WebSocket(this.algoHost);
    this.handleAlgoSocketMessage = this.handleAlgoSocketMessage.bind(this);
    this.handleAlgoSocketClose = this.handleAlgoSocketClose.bind(this);
    this.algoSocket.addEventListener('message', this.handleAlgoSocketMessage);
    this.algoSocket.addEventListener('close', this.handleAlgoSocketClose);

    this.setState({
      exchange_id,
      product_id,
      base: product_id.split("-")[0],
      quote: product_id.split("-")[1],
      //bookChart: algoSnapshotReponse.data,
      portfolio: portfolio
    });
  }


  reconnectFeedSocket() {
    this.feedSocket = new WebSocket(this.feedHost);
    this.handleFeedSocketMessage = this.handleFeedSocketMessage.bind(this);
    this.handleFeedSocketClose = this.handleFeedSocketClose.bind(this);
    this.feedSocket.addEventListener('message', this.handleFeedSocketMessage);
    this.feedSocket.addEventListener('close', this.handleFeedSocketClose);
    clearInterval(this.reconnectFeedInterval);
  }

  reconnectAccountSocket() {
    this.accountSocket = new WebSocket(this.accountHost);
    this.handleAccountSocketMessage = this.handleAccountSocketMessage.bind(this);
    this.handleAccountSocketClose = this.handleAccountSocketClose.bind(this);
    this.accountSocket.addEventListener('message', this.handleAccountSocketMessage);
    this.accountSocket.addEventListener('close', this.handleAccountSocketClose);
    clearInterval(this.reconnectAccountInterval);
  }

  reconnectAlgoSocket() {
    this.algoSocket = new WebSocket(this.algoHost);
    this.handleAlgoSocketMessage = this.handleAlgoSocketMessage.bind(this);
    this.handleAlgoSocketClose = this.handleAlgoSocketClose.bind(this);
    this.algoSocket.addEventListener('message', this.handleAlgoSocketMessage);
    this.algoSocket.addEventListener('close', this.handleAlgoSocketClose);
    clearInterval(this.reconnectAlgoInterval);
  }


  async updateState() {
    const data = this.data;
    let ohlcv = data.ohlcv[data.ohlcv.length - 1];
    ohlcv.t = new Date(ohlcv.time * 1000);
    const lastCandle = this.chart[this.chart.length - 1];
    const t = 'time' in lastCandle ? 'time' : 'date'
    if (ohlcv.time === lastCandle[t]) {
      this.chart.pop();
      this.chart.push(ohlcv);
    }
    // add a new candle to the series
    else if (ohlcv.time === lastCandle[t] + (resolutions[this.state.interval] * 60)) {
      this.chart.shift();
      this.chart.push(ohlcv);
    }
    // missing the last two candles, if we have them, add them. otherwise do nothing and wait
    else if (ohlcv.time >= lastCandle[t] + (resolutions[this.state.interval] * 120)) {
      if (data.ohlcv.length === 2) {
        let ohlcv_prev = data.ohlcv[0];
        ohlcv_prev.t = new Date(ohlcv_prev.time * 1000);
        this.chart.shift();
        this.chart.shift();
        Array.prototype.push.apply(this.chart, [ohlcv_prev, ohlcv]);
      }
    }
  }


  handleFeedSocketMessage(msg) {
    let data = JSON.parse(msg.data);
    //console.log('msg', data)
    if (data) {
      if (data.count > 0) {
        this.data = data;
        this.ticker = data.ticker;
        // update data for tick chart and trade history

        data.tickers.forEach(tick => {
          tick.date = new Date(tick.time);
          this.ticks.push(tick);
          this.ticks.shift();
          this.matches.unshift(tick);
          this.matches.pop();
          // update text that appears on browser tab
        });

        this.setState({
          chart: this.chart,
          matches: this.matches,
          book: data.book,
          ticker: this.ticker,
          //change24hr
        });
        //document.title = currencyFormat[this.state.base](this.ticker.price) + ' | ' + this.ticker.product_id;
      } else {
        data.snapshot.forEach(t => t.date = new Date(t.time * 1000));
        this.ticks = data.snapshot;
        this.matches = [...data.snapshot].reverse().slice(0, 70);
      }
    } else {
      this.feedSocket.close();
    }
  }


  handleFeedSocketClose(msg) {
    console.log(' [ OK ] Closing websocket');
    // try to reconnect websocket every 1000 ms 
    this.reconnectFeedInterval = this.reconnectFeedSocket.bind(this);
    this.reconnectFeedInterval = setInterval(this.reconnectFeedInterval, 1000);
  }

  handleAlgoSocketMessage(msg) {
    let data = JSON.parse(msg.data)
    data.books.date = new Date(data.books.time * 1000);
    let bookChart = [...this.state.bookChart];
    //console.log(data.books)
    bookChart.push(data.books);
    bookChart.shift();
    this.setState({
      bookChart
    })
  }

  handleAlgoSocketClose(msg) {
    console.log(' [ OK ] Closing algo socket');
    // try to reconnect websocket every 1000 ms 
    this.reconnectAlgoInterval = this.reconnectAlgoSocket.bind(this);
    this.reconnectAlgoInterval = setInterval(this.reconnectAlgoInterval, 1000);
  }

  formatUSD(num) {
    // we round up for USD other than available (quote) USD
    return +(Math.ceil(num + "e+2") + "e-2");
  }

  formatUSDQuote(num) {
    // we round down for our available USD 
    return +(Math.floor(num + "e+2") + "e-2");
  }

  handleAccountSocketMessage(msg) {

    // ===========================================================================================================
    // console.log(Math.ceil(300.561 * 100) / 100)

    const accounts = JSON.parse(msg.data);
    const portfolioValue = currencyFormat.USD(accounts.portfolio_value);
    const quoteValue = currencyFormat.USD(accounts.quote.available);
    const baseValue = btcFormat(accounts.base.available);
    const openOrders = accounts.open_orders;
    const fills = accounts.fills;
    const allTicks = accounts.ticks;
    console.log('ACCOUNTS', accounts)
    this.setState({
      accounts,
      portfolioValue,
      quoteValue,
      baseValue,
      openOrders,
      fills,
      allTicks,
    });
  }


  handleAccountSocketClose(msg) {
    console.log(' [ OK ] Closing account socket');
    // try to reconnect websocket every 1000 ms 
    this.reconnectAccountInterval = this.reconnectAccountSocket.bind(this);
    this.reconnectAccountInterval = setInterval(this.reconnectAccountInterval, 1000);
  }


  // async updateState() {
  //   const data = this.data;
  //   let ohlcv = data.ohlcv[data.ohlcv.length - 1];
  //   ohlcv.t = new Date(ohlcv.time * 1000);
  //   const lastCandle = this.chart[this.chart.length - 1];
  //   const t = 'time' in lastCandle ? 'time' : 'date'
  //   if (ohlcv.time === lastCandle[t]) {
  //     this.chart.pop();
  //     this.chart.push(ohlcv);
  //   }
  //   // add a new candle to the series
  //   else if (ohlcv.time === lastCandle[t] + (resolutions[this.state.interval] * 60)) {
  //     this.chart.shift();
  //     this.chart.push(ohlcv);
  //   }
  //   // missing the last two candles, if we have them, add them. otherwise do nothing and wait
  //   else if (ohlcv.time === lastCandle[t] + (resolutions[this.state.interval] * 120)) {
  //     if (data.ohlcv.length === 2) {
  //       let ohlcv_prev = data.ohlcv[0];
  //       ohlcv_prev.t = new Date(ohlcv_prev.time * 1000);
  //       this.chart.shift();
  //       this.chart.shift();
  //       this.chart.push(ohlcv_prev);
  //       this.chart.push(ohlcv);
  //     }
  //   }
  //   const price = parseFloat(this.ticker.price)
  //   const open24 = parseFloat(this.ticker.open_24h)

  //   this.setState({
  //     chart: this.chart,
  //     ticks: this.ticks,
  //     matches: this.matches,
  //     book: data.book,
  //     ticker: this.ticker,
  //     change24hr: usdFormat((price - open24) / open24 * 100)
  //   });
  //   document.title = currencyFormat[this.state.base](this.ticker.price) + ' | ' + this.ticker.product_id;

  // }



  selectAllOrders = () => {
    // make sure there are orders to be selected
    if (this.state.openOrders.length) {
      let orders = []
      const selectAllOrders = !this.state.selectAllOrders;
      if (!this.state.selectAllOrders) {
        this.state.openOrders.forEach((item, index) => orders.push(index))
      }
      this.setState({
        selectedOrders: orders,
        selectAllOrders: selectAllOrders,
      });
    }
  }


  selectAllFills = () => {
    // make sure there are fills to be selected
    if (this.state.fills.length) {
      let fills = []
      const selectAllFills = !this.state.selectAllFills;
      if (!this.state.selectAllFills) {
        this.state.fills.forEach((item, index) => fills.push(index))
      }
      this.setState({
        selectedFills: fills,
        selectAllFills: selectAllFills,
      });
    }
  }


  handleOrderClick = ({ index }) => {
    // toggle visibility of order
    // add new order index
    if (!this.state.selectedOrders.includes(index)) {

      const selectedOrders = [...this.state.selectedOrders, index];
      this.setState({
        selectedOrders,
      });
      this.cache.selectedOrders = selectedOrders;
    } else { // remove existing order index
      let array = [...this.state.selectedOrders];
      const i = array.indexOf(index);
      array.splice(i, 1);
      if (array.length === 0) {
        this.setState({
          selectedOrders: array,
          selectAllOrders: false
        });
      } else {
        this.setState({
          selectedOrders: array,
        });
      }
    }
  }

  handleFillClick = ({ index }) => {
    // toggle visibility of fills
    // add new fill index
    if (!this.state.selectedFills.includes(index)) {
      console.log('!this.state.selectedFills.includes(index)')
      const selectedFills = [...this.state.selectedFills, index];
      this.setState({
        selectedFills,
      });
      this.cache.selectedFills = selectedFills;
    } else { // remove existing fill index
      let array = [...this.state.selectedFills];
      const i = array.indexOf(index);
      array.splice(i, 1);
      if (array.length === 0) {
        this.setState({
          selectedFills: array,
          selectAllFills: false
        });
      } else {
        this.setState({
          selectedFills: array,
        });
      }
    }
  }

  handleCancelOrders = () => {
    this.state.selectedOrders.forEach((index) => {
      if (index < this.state.openOrders.length) {
        const order = this.state.openOrders[index];
        console.log(order)
        // send api request to cancel order
        api.delete('/api/orders/', {
          request_id: 'cancel_order',
          order_id: order.order_id,
        }).then((response) => {
          console.log(response.data);
          this.setState({
            selectedOrders: []
          })
        }).catch(function (error) {
          console.log('Error cancelling order', error);
        });
      }
    })
  }

  handleCancelAllOrders = () => {
    api.delete('/api/orders/', {
      request_id: 'cancel_all',
      product_id: this.props.product,
    }).then((response) => {
      console.log(response.data);
    }).catch(function (error) {
      console.log('Error doing backtest', error);
    });
  }


  handleCloseSnackbar = () => {
    this.setState({ openSnackbar: false });
  }

  handleChange = name => event => {
    this.setState({
      [name]: event.target.value,
    });
  }

  handleChangeSize = name => event => {
    console.log('handleChangeSize')
    if (this.state.orderPrice.length) {
      const total = event.target.value * this.state.orderPrice
      const fee = (sizeFormat[this.state.base])(total * 0.0018)
      this.setState({
        size: event.target.value,
        fee,
        total,
      });
    }
    else {
      this.setState({
        size: event.target.value,
      });
    }
  }

  handleChangePrice = event => {
    if (this.state.size.length) {
      let total = usdFormat((event.target.value * this.state.size) - (event.target.value * this.state.size * 0.0008))
      const fee = usdFormat(total * 0.0008)
      this.setState({
        orderPrice: event.target.value,
        fee,
        total,
      });
    }
    else {
      this.setState({
        orderPrice: event.target.value,
      });
    }
  }


  setPrice = (price) => {
    console.log('setPrice');
    const orderPrice = (sizeFormat[this.state.base])(price);

    const total = this.state.size * this.state.orderPrice;
    const fee = total * 0.0018;

    this.setState({
      orderPrice,
      total,
      fee
    })
  }


  handleQuoteClick = () => {
    console.log('handleQuoteClick');
    // if we have selected a price already, only calculate size
    if (this.state.orderPrice !== "") {
      const quote = this.state.accounts.quote;
      let size = (sizeFormat[this.state.base])(quote.available / this.state.orderPrice);
      const total = size * this.state.orderPrice;
      const fee = total * 0.0018;

      this.setState({
        orderSide: 'buy',
        size,
        total,
        fee
      });
    }
    // we haven't selected a size yet, 
    else {
      const quote = this.state.accounts.quote;
      let size = ((quote.available / this.state.orderPrice));
      this.setState({
        orderSide: 'buy',
        size: (sizeFormat[this.state.base])(size),
        orderPrice: this.state.ticker.price
      });
    }

  }

  handleBaseClick = () => {
    const base = this.state.accounts.base;
    // let s = ((base.available / this.state.orderPrice))
    //let fee = s * sizeFormat[this.state.base][1]
    const total = usdFormat(this.state.orderPrice * this.state.size)
    this.setState({
      orderSide: 'sell',
      size: usdFormat(base.available),
      //amount: accounts.base,
      total,
    })
  }

  handleSelect = (trends, point) => {
    console.log(trends)
    if (point === 'start') {
      this.setState({
        orderPrice: usdFormat(trends[0].start[1]),
      })
    } else {
      this.setState({
        orderPrice: usdFormat(trends[0].end[1]),
      })
    }
  }

  handleEmptySearch = () => {
    this.setState({
      searchActive: false,
    });
  }


  redirectToStrategies = () => {
    this.setState({ redirectPath: '/strategies' });
  }

  runBacktest = () => {
    console.log('Running backtest')
    api.post('/api/backtest/', {
      product: this.props.product,
      cash: this.state.cash,
      fee: this.state.fee,
      sizer: this.state.sizer,
      //slippage: 0.001,
    }).then((response) => {
      console.log(response.data);
      this.setState({ backtestResult: response.data })
    }).catch(function (error) {
      console.log('Error doing backtest', error);
    });
  }

  setOrderType = orderType => () => {
    this.setState({
      orderType,
    });
  }

  setOrderSide = orderSide => () => {
    this.setState({
      orderSide,
    });
  }
  submitOrder = e => {
    e.preventDefault();
    this.placeOrder();
  }

  placeOrder = () => {

    // place order via REST
    if (this.state.orderType === 'market') {
      api.placeOrder(
        this.state.product_id,
        this.state.orderSide, // buy or sell
        this.state.orderType, // limit, market, stop
        this.state.orderPrice, // usd limit
        this.state.size, // btc
      ).then((response) => {
        console.log(response.data);
        this.setState({
          snackMessage: response.data.message,
          snackbarType: 'error',
          openSnackbar: true,
        });
      }).catch(function (error) {
        console.log('Error in Explorer.placeOrder', error);
      });

    } else if (this.state.orderType === 'limit') {
      api.placeOrder(
        this.state.product_id,
        this.state.orderSide, // buy or sell
        this.state.orderType, // limit, market, stop
        this.state.orderPrice, // usd limit
        this.state.size, // btc
      ).then((response) => {
        console.log(response.data);
        this.setState({
          snackMessage: response.data.message,
          snackbarType: response.data.side === 'buy' ? 'success' : 'error',
          openSnackbar: true,
        });
      }).catch(function (error) {
        console.log('Error in Explorer.placeOrder', error);
      });
    }
    else {
      // place order via Websocket
      const payload = JSON.stringify({
        order_type: this.state.orderType, // limit, market, stop
        exchange_id: this.state.exchange_id,
        product_id: this.state.product_id,
        side: this.state.orderSide, // buy or sell
        price: this.state.orderPrice, // usd limit
        size: this.state.size, // btc
      })
      console.log(payload)
      this.algoSocket.send(payload)
    }
  }


  handleChangeInterval = () => event => {
    //this.reconnectSocket();
    this.stop();
    this.start(this.state.exchange_id, this.state.product_id, event.target.value).then(() => {
      api.setCache(this.cache);
      this.setState({
        interval: event.target.value
      });
    });
  }

  async handleProductClick(exchange_id, product_id) {
    this.stop();
    await this.start(exchange_id, product_id, this.state.interval);
    this.cache.exchange_id = exchange_id;
    this.cache.product_id = product_id;
    api.setCache(this.cache);
  }


  handleClick = (item) => {
    // this.setState({
    //   open: !this.state.open,
    // });
    console.log(item);
  };


  render() {
    // const {
    //   classes,
    // } = this.props;

    const menu = [
      "API Keys",
      "Storage",
      "Color Theme",
      "Font Size"
    ];

    // const {
    //   // product_id,
    //   // exchange_id,      
    //   // quote,
    //   // orderSide,
    //   // orderType,
    //   // size,
    //   // stopPrice,
    //   // orderPrice,
    //   // selectedOrders,
    //   // selectedFills,
    //   // ticker,
    //   // book,
    //   // chart,
    //   // matches,
    //   // fills,
    //   // openOrders,
    //   // accounts,
    //   // portfolioValue,
    //   // quoteValue,
    //   // baseValue,
    //   // fee,
    //   // total,
    //   // change24hr,
    //   // bookChart,
    //   // portfolio,
    //   // allTicks,
    //   // accounts2,
    //   base,

    // } = this.state;

    // const exchangeImg = logosUrl + exchange_id + '.png'
    // const productImg = iconsUrl + base.toLowerCase() + '.png';


    return (!this.state.redirectPath ? (
      <div>
        <Grid container>
          <GridItem xs={2} sm={2} md={2} lg={2} borderRight>

            {menu.map((item, key) => 
              <ListItem
                button
                onClick={() => this.handleClick(item)}
                key={key}
                label={item}
              />
            )}

            {/* <List
              component="nav"
              aria-labelledby="nested-list-subheader"
              subheader={
                <ListSubheader component="div" id="nested-list-subheader">
                  Nested List Items
                </ListSubheader>
              }
              className={classes.root}
            >
              <ListItem button>
                <ListItemIcon>
                  <Send />
                </ListItemIcon>
                <ListItemText primary="Sent mail" />
              </ListItem>
              <ListItem button>
                <ListItemIcon>
                  <Drafts />
                </ListItemIcon>
                <ListItemText primary="Drafts" />
              </ListItem>
              <ListItem button onClick={this.handleClick}>
                <ListItemIcon>
                  <Inbox/>
                </ListItemIcon>
                <ListItemText primary="Inbox" />
                {this.state.open ? <ExpandLess /> : <ExpandMore />}
              </ListItem>
              <Collapse in={this.state.open} timeout="auto" unmountOnExit>
                <List component="div" disablePadding>
                  <ListItem button className={classes.nested}>
                    <ListItemIcon>
                    <Drafts />
                    </ListItemIcon>
                    <ListItemText primary="Starred" />
                  </ListItem>
                </List>
              </Collapse>
            </List> */}
          </GridItem>



        </Grid>


        <Snackbar
          open={this.state.openSnackbar}
          onClose={this.handleCloseSnackbar}
          variant={this.state.snackbarType}
          message={this.state.snackMessage}
        />

      </div >) : <Redirect to={this.state.redirectPath} />);


  }
}

SettingsView.propTypes = {
  classes: PropTypes.object.isRequired,
};


export default withStyles(tradeStyle)(SettingsView);