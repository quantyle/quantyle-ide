import React from 'react';
import { Component } from "react";
import PropTypes from "prop-types";
import tradeStyle from "../variables/styles/tradeStyle";
import {
  withStyles,
  Grid,
} from "@material-ui/core";
import {
  GridItem,
  BacktestChart,
  OrderBook,
  ListItem,
  Loading,
  TradeHistory,
  PortfolioModal,
  TickChart,
  Select,
  FileModal,
  CustomizedTabs,
  CodeOutput,
  IconButton,
  MoneyField,
  Button,
} from "../components";
import {
  intervals,
  iconsUrl,
  usdFormat,
  sizeFormat,
  priceFormat,
  standardFormat,
  autoComplete,
  exchangeNames,

} from "../variables/global";
import api from "../providers/api";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-whale-dojo";
import "ace-builds/src-noconflict/theme-monokai";
import "ace-builds/src-noconflict/ext-language_tools";
import "ace-builds/src-noconflict/ext-searchbox";
import {
  Close,
  Code, FolderOpen, PlayArrow, Save,
} from "@material-ui/icons";
import { backgroundDark } from '../variables/styles';


class TradeView extends Component {
  static defaultProps = {
    isResizable: false,
  };

  constructor(props) {
    super(props);
    this.exchange = "GDAX"
    this.product = "SOL-USD";
    this.state = {
      exchange_id: this.exchange,
      product_id: this.product,
      base: this.product.split("-")[0],
      quote: this.product.split("-")[1],
      resolution: "",
      selectedInterval: "1m",
      selectAllOrders: false,
      selectAllFills: false,
      selectedOrders: [],
      selectedFills: [],
      fills: [],
      openOrders: [],
      change24hr: "",
      portfolio: {},
      allTicks: {},
      lineColors: [],
      tabValue: 1,
      openFiles: ["momentum_stategy", "crossover_strategy"],
      loadingStatus: "loading...",
      openFileModal: false,
      openPortfolioModal: false,
      percent: "",
      stopPrice: "",
      orderPrice: "",
      orderSide: "buy",
      orderType: "limit",
      size: 0,
      fee: 0,
      total: 0,
      quoteValue: 0,
      baseValue: 0,

    };
    this.code = "";
    document.title = this.exchange + " | " + this.product;
    this.pythonOutput = "";
    this.codeResponse = [];
    this.chart = [];
    this.ticks = [];
    this.activeAccounts = {};
    this.tradeHistory = [];
    this.printOutput = "";
    this.allTicks = [];
    this.count = 0;
    this.book = null;
    this.ticker = {};
    this.fileDictionary = {};

    // this needs to be set in settings.jsx and then passed as prop via App.jsx
    this.codeDirectory = "/home/satoshi/Projects/ctm-apollo/code/";
    // this.setRef = this.setRef.bind(this);
  }

  async componentDidMount() {
    // load the source code
    api.getCode(
      {
        fname: this.codeDirectory + "sample_code.py"
      }
    ).then(resp => {
      this.code = resp.data;
      console.log(this.code);
      this.setState({ loaded: true });
    });

    try {
      this.start(this.state.exchange_id, this.state.product_id);
    } catch (error) {
      console.log("mounting error");
      this.setState({
        loadingStatus: "system inactive"
      });
      return;
    }
  }

  componentWillUnmount() {
    this.stop()
  }

  stop() {
    if (this.feedSocket) {
      this.feedSocket.close();
      this.feedSocket.removeEventListener(
        "message",
        this.handleFeedSocketMessage
      );
      this.feedSocket.removeEventListener(
        "close",
        this.handleFeedSocketClose
      );
      this.chart = [];
      this.printOutput = [];
      this.count = 0;
      this.tradeHistory = [];
    }

    if (this.tickSocket) {
      this.tickSocket.close();
      this.tickSocket.removeEventListener(
        "message",
        this.handleTickSocketMessage
      );
      this.tickSocket.removeEventListener(
        "close",
        this.handleTickSocketClose
      );
    }
  }

  start = (exchange_id, product_id) => {
    console.log("componentDidMount");
    // let c = await api.getCache();
    api.getPortfolio().then(resp => {
      console.log("-----------------", resp.data);

      // feed websocket
      let feedUrl = "ws://127.0.0.1:8004/" + exchange_id + "/" + product_id;
      this.feedSocket = new WebSocket(feedUrl);
      this.handleFeedSocketMessage = this.handleFeedSocketMessage.bind(this);
      this.handleFeedSocketClose = this.handleFeedSocketClose.bind(this);
      this.feedSocket.addEventListener("message", this.handleFeedSocketMessage);
      this.feedSocket.addEventListener("close", this.handleFeedSocketClose);

      // code websocket
      let tickUrl = "ws://127.0.0.1:8004/allTicks";
      this.tickSocket = new WebSocket(tickUrl);
      // this.tickSocket.binaryType = "ArrayBuffer";
      this.handleTickSocketMessage = this.handleTickSocketMessage.bind(this);
      this.handleTickSocketClose = this.handleTickSocketClose.bind(this);
      this.tickSocket.addEventListener("message", this.handleTickSocketMessage);
      this.tickSocket.addEventListener("close", this.handleTickSocketClose);

      this.setState({
        exchange_id: exchange_id,
        product_id: product_id,
        base: this.product.split("-")[0],
        quote: this.product.split("-")[1],
        portfolio: resp.data,
      });
    })
  }

  handleKeyDown = (event) => {
    event.preventDefault();
    let charCode = String.fromCharCode(event.which).toLowerCase();
    if ((event.ctrlKey || event.metaKey) && charCode === 's') {
      alert("CTRL+S Pressed");
    } else if ((event.ctrlKey || event.metaKey) && charCode === 'c') {
      alert("CTRL+C Pressed");
    } else if ((event.ctrlKey || event.metaKey) && charCode === 'v') {
      alert("CTRL+V Pressed");
    }
  }

  SendCode = () => {
    // send python script to backend
    this.feedSocket.send(this.code);
  }

  SaveCode = () => {
    // send python script to backend
    api.postCode({
      fname: this.codeDirectory + "sample_code2",
      source: this.code,
    });
  }

  handleTickSocketMessage(msg) {
    this.allTicks = JSON.parse(msg.data);
  }

  handleTickSocketClose() {
    console.log(" handleTickSocketClose");
  }

  processCodeMessage(data) {
    // first check for errors
    if (data.error !== "") {
      console.log("ERROR: ", data.error);
      this.printOutput = data.error;

      // add error annotation
      this.refs.aceEditor.editor.getSession().setAnnotations([{
        row: data.errorLineNumber - 1,
        column: 0,
        text: data.error,
        type: "error" // also warning and information
      }]);

    } else {
      this.printOutput = data.printOutput;
      this.codeResponse = data.chart;
      this.chart = this.codeResponse;
      this.setState({
        lineColors: data.lineColors
      });
      console.log("CODE RESPONSE: ", data);

      // remove any previous annotations error
      this.refs.aceEditor.editor.getSession().setAnnotations([]);
    }
  }

  processFeedMessage(data) {
    // update ticks and matches
    for (var tick of data.tickers) {
      this.tradeHistory.unshift(tick);
      this.tradeHistory.pop();
      this.ticks.push(tick);
      this.ticks.shift();
    }

    if (data.count > 0) {
      // set USD portfolioValue
      // update the chart
      if (data.ohlcv[0] === this.chart[this.chart.length - 1][0]) {
        // update current candle
        this.chart.pop();
        this.chart.push(data.ohlcv);
      } else {
        // add the next candle
        this.chart.shift();
        this.chart.push(data.ohlcv);
      }

      // reverse asks
      // data.book.asks.reverse();
      this.ticker = data.ticker;
      this.book = data.book;
      this.forceUpdate();

    } else {
      this.chart = [...data.candles];
      this.tradeHistory = [...data.snapshot].reverse();
      this.ticks = [...data.snapshot];
      this.count = 1;
    }
  }

  handleFeedSocketMessage(msg) {
    //var decodedString = new TextDecoder("UTF8").decode(new DataView(msg.data));
    const data = JSON.parse(msg.data);
    if (data.type === "code") {
      this.processCodeMessage(data);
    } else {
      this.processFeedMessage(data);
    }
  }

  handleFeedSocketClose(msg) {
    console.log(" [ OK ] handleFeedSocketClose");
    //clearTimeout(this.chartTimeout);
    // try to reconnect websocket every 1000 ms
    // this.reconnectFeedInterval = this.reconnectFeedSocket.bind(this);
    // this.reconnectFeedInterval = setInterval(this.reconnectFeedInterval, 1000);
  }

  selectAllOrders = () => {
    // make sure there are orders to be selected
    if (this.state.openOrders.length) {
      let orders = [];
      const selectAllOrders = !this.state.selectAllOrders;
      if (!this.state.selectAllOrders) {
        this.state.openOrders.forEach((item, index) => orders.push(index));
      }
      this.setState({
        selectedOrders: orders,
        selectAllOrders: selectAllOrders,
      });
    }
  };

  selectAllFills = () => {
    // make sure there are fills to be selected
    if (this.state.fills.length) {
      let fills = [];
      const selectAllFills = !this.state.selectAllFills;
      if (!this.state.selectAllFills) {
        this.state.fills.forEach((item, index) => fills.push(index));
      }
      this.setState({
        selectedFills: fills,
        selectAllFills: selectAllFills,
      });
    }
  };

  handleOrderClick = ({ index }) => {
    // toggle visibility of order
    // add new order index
    if (!this.state.selectedOrders.includes(index)) {
      const selectedOrders = [...this.state.selectedOrders, index];
      this.setState({
        selectedOrders,
      });
      this.cache.selectedOrders = selectedOrders;
    } else {
      // remove existing order index
      let array = [...this.state.selectedOrders];
      const i = array.indexOf(index);
      array.splice(i, 1);
      if (array.length === 0) {
        this.setState({
          selectedOrders: array,
          selectAllOrders: false,
        });
      } else {
        this.setState({
          selectedOrders: array,
        });
      }
    }
  };

  handleFillClick = ({ index }) => {
    // toggle visibility of fills
    // add new fill index
    if (!this.state.selectedFills.includes(index)) {
      console.log("!this.state.selectedFills.includes(index)");
      const selectedFills = [...this.state.selectedFills, index];
      this.setState({
        selectedFills,
      });
      this.cache.selectedFills = selectedFills;
    } else {
      // remove existing fill index
      let array = [...this.state.selectedFills];
      const i = array.indexOf(index);
      array.splice(i, 1);
      if (array.length === 0) {
        this.setState({
          selectedFills: array,
          selectAllFills: false,
        });
      } else {
        this.setState({
          selectedFills: array,
        });
      }
    }
  };

  handleCancelOrders = () => {
    this.state.selectedOrders.forEach((index) => {
      if (index < this.state.openOrders.length) {
        const order = this.state.openOrders[index];
        console.log(order);
        // send api request to cancel order
        api
          .delete("/api/orders/", {
            request_id: "cancel_order",
            order_id: order.order_id,
          })
          .then((response) => {
            console.log(response.data);
            this.setState({
              selectedOrders: [],
            });
          })
          .catch(function (error) {
            console.log("Error cancelling order", error);
          });
      }
    });
  };

  handleCancelAllOrders = () => {
    api
      .delete("/api/orders/", {
        request_id: "cancel_all",
        product_id: this.props.product,
      })
      .then((response) => {
        console.log(response.data);
      })
      .catch(function (error) {
        console.log("Error doing backtest", error);
      });
  };


  handleChange = (name) => (event) => {
    this.setState({
      [name]: event.target.value,
    });
  }



  setPriceFromChart = (price) => {
    //console.log("setPrice");
    let total = this.state.size * this.state.orderPrice;

    this.setState({
      orderPrice: standardFormat(price),
      total: this.state.size * this.state.orderPrice,
      fee: total * 0.0018,
    });
  };


  handleSelect = (trends, point) => {
    console.log(trends);
    if (point === "start") {
      this.setState({
        orderPrice: usdFormat(trends[0].start[1]),
      });
    } else {
      this.setState({
        orderPrice: usdFormat(trends[0].end[1]),
      });
    }
  };

  handleChangeInterval = () => (event) => {
    //this.reconnectSocket();
    this.stop();
    this.start(
      this.state.exchange_id,
      this.state.product_id,
      event.target.value
    ).then(() => {
      api.setCache(this.cache);
      this.setState({
        selectedInterval: event.target.value,
      });
    });
  };

  handleProductClick = (exchange_id, product_id) => {
    this.stop();
    console.log(exchange_id, product_id);
    this.start(exchange_id, product_id, this.state.selectedInterval);
    // this.cache.exchange_id = exchange_id;
    // this.cache.product_id = product_id;
    // api.setCache(this.cache);
  }

  orderBookClick = (item) => {
    console.log(item[0]);
  }

  openCodeSnackbar = () => {
    api.getFiles({ directory: this.codeDirectory }).then(resp => {
      this.fileDictionary = resp.data;
    });
    this.setState({ openSnackbar: true });
  }


  openCodeModal = () => {
    api.getFiles({ directory: this.codeDirectory }).then(resp => {
      this.fileDictionary = resp.data;
    });
    this.setState({ openFileModal: true });
  }

  handleCloseModal = () => {
    this.setState({ openFileModal: false });
  }

  handleChangeCode = (newValue) => {
    // console.log("change", newValue);
    this.code = newValue;
    // this.setState({ code: newValue });
  }

  handleChangeTab = (event, newValue) => {
    this.setState({
      tabValue: newValue,
    });
  }



  handlePortfolioModalOpen = () => {
    this.setState({ openPortfolioModal: true });
  }
  handlePortfolioModalClose = () => {
    this.setState({ openPortfolioModal: false });
  }



  handleQuoteClick = () => {
    console.log("handleQuoteClick");
    // if we have selected a price already, only calculate size
    if (this.state.orderPrice !== "") {
      const quote = this.state.accounts.quote;
      let size = sizeFormat[this.state.base](
        quote.available / this.state.orderPrice
      );
      const total = size * this.state.orderPrice;
      const fee = total * 0.0018;

      this.setState({
        orderSide: "buy",
        size,
        total,
        fee,
      });
    }
    // we haven't selected a size yet,
    else {
      const quote = this.state.accounts.quote;
      let size = quote.available / this.state.orderPrice;
      this.setState({
        orderSide: "buy",
        size: sizeFormat[this.state.base](size),
        orderPrice: this.state.ticker.price,
      });
    }
  }



  handleBaseClick = () => {
    const base = this.state.accounts.base;
    // let s = base.available / this.state.orderPrice;
    //let fee = s * sizeFormat[this.state.base][1]
    const total = usdFormat(this.state.orderPrice * this.state.size);
    this.setState({
      orderSide: "sell",
      size: usdFormat(base.available),
      //amount: accounts.base,
      total,
    });
  }


  handleChangeSize = (event) => {
    // console.log("handleChangeSize");

    if (this.state.orderPrice.length) {
      let total = event.target.value * this.state.orderPrice;
      let fee = sizeFormat[this.state.base](total * 0.0018);

      this.setState({
        size: event.target.value,
        fee,
        total,
      });

    } else {
      this.setState({
        size: event.target.value,
      });
    }
  }


  handleChangePrice = (event) => {
    if (this.state.size.length) {
      let total = usdFormat(
        event.target.value * this.state.size -
        event.target.value * this.state.size * 0.0008
      );
      let fee = usdFormat(event.target.value * this.state.size * 0.0008);
      this.setState({
        orderPrice: event.target.value,
        fee,
        total,
      });
    } else {
      this.setState({
        orderPrice: event.target.value,
      });
    }
  }


  setOrderType = (orderType) => () => {
    this.setState({
      orderType,
    });
  };

  setOrderSide = (orderSide) => () => {
    this.setState({
      orderSide,
    });
  };

  handleChange = (name) => (event) => {
    this.setState({
      [name]: event.target.value,
    });
  }


  placeOrder = () => {
    // place order via REST
    if (this.state.orderType === "market") {
      api
        .placeOrder(
          this.state.product_id,
          this.state.orderSide, // buy or sell
          this.state.orderType, // limit, market, stop
          this.state.orderPrice, // usd limit
          this.state.size // btc
        )
        .then((response) => {
          console.log(response.data);
          this.setState({
            snackMessage: response.data.message,
            snackbarType: "error",
            openSnackbar: true,
          });
        })
        .catch(function (error) {
          console.log("Error in Explorer.placeOrder", error);
        });
    } else if (this.state.orderType === "limit") {
      api
        .placeOrder(
          this.state.product_id,
          this.state.orderSide, // buy or sell
          this.state.orderType, // limit, market, stop
          this.state.orderPrice, // usd limit
          this.state.size // btc
        )
        .then((response) => {
          console.log(response.data);
          this.setState({
            snackMessage: response.data.message,
            snackbarType: response.data.side === "buy" ? "success" : "error",
            openSnackbar: true,
          });
        })
        .catch(function (error) {
          console.log("Error in Explorer.placeOrder", error);
        });
    } else {
      // place order via Websocket
      const payload = JSON.stringify({
        order_type: this.state.orderType, // limit, market, stop
        exchange_id: this.state.exchange_id,
        product_id: this.state.product_id,
        side: this.state.orderSide, // buy or sell
        price: this.state.orderPrice, // usd limit
        size: this.state.size, // btc
      });
      console.log(payload);
      this.algoSocket.send(payload);
    }
  }



  render() {
    const { classes } = this.props;

    const {
      exchange_id,
      product_id,
      selectedOrders,
      selectedFills,
      fills,
      openOrders,
      portfolio,
      openFiles,
      selectedInterval,
      tabValue,
      openFileModal,
      openPortfolioModal,


      orderSide,
      orderType,
      size,
      stopPrice,
      orderPrice,
      baseValue,
      fee,
      total,
    } = this.state;

    const base = product_id.split("-")[0];
    const quote = product_id.split("-")[1];
    const exchangeImg = iconsUrl + exchange_id.toLowerCase() + '.png'
    const productImg = iconsUrl + base.toLowerCase() + ".png";

    if (this.count > 0) {
      const pFormat = priceFormat[quote];
      // const sFormat = sizeFormat[base];
      // const layout = [
      //   { i: "a", x: 0, y: 0, w: 1, h: 2, static: true },
      //   { i: "b", x: 1, y: 0, w: 3, h: 2, minW: 2, maxW: 4 },
      //   { i: "c", x: 4, y: 0, w: 1, h: 2 }
      // ];

      return (
        <div>
          <Grid container>
            <GridItem xs={2} xl={2} borderLeft borderTop>
            <div className={classes.orderForm}>

                <ListItem
                    label={quote + " Value"}
                    // textRight={this.feed.portfolioValue.toString()}
                    // textRight={this.feed.portfolioValue.toString()}
                    textRight={"15000"}

                />
                <ListItem
                    button
                    label={quote + " Available"}
                    onClick={this.handleQuoteClick}
                    // textRight={quoteValue.toString()}
                    textRight={"15000"}
                />
                <ListItem
                    button
                    label={base + " Available"}
                    onClick={this.handleBaseClick}
                    textRight={baseValue.toString()}
                />
                <div className={classes.buttons}>
                    <Grid container>
                        <GridItem xs={6}>
                            <Button
                                full
                                // fontSize={fontSize}
                                plain={orderSide !== "buy"}
                                secondary={orderSide === "buy"}
                                onClick={this.setOrderSide("buy")}
                            >
                                Buy
                            </Button>
                        </GridItem>
                        <GridItem xs={6}>
                            <Button
                                full
                                // fontSize={fontSize}
                                plain={orderSide !== "sell"}
                                danger={orderSide === "sell"}
                                onClick={this.setOrderSide("sell")}
                            >
                                Sell
                            </Button>
                        </GridItem>
                    </Grid>
                    <Button
                        full
                        // fontSize={fontSize}
                        plain={orderType !== "limit"}
                        secondary={orderSide === "buy" && orderType === "limit"}
                        danger={orderSide === "sell" && orderType === "limit"}
                        onClick={this.setOrderType("limit")}
                    >
                        Limit
                    </Button>

                    <Button
                        full
                        // fontSize={fontSize}
                        plain={orderType !== "market"}
                        secondary={
                            orderSide === "buy" && orderType === "market"
                        }
                        danger={orderSide === "sell" && orderType === "market"}
                        onClick={this.setOrderType("market")}
                    >
                        Market
                    </Button>
                    <Button
                        full
                        // fontSize={fontSize}
                        plain={orderType !== "stop"}
                        secondary={orderSide === "buy" && orderType === "stop"}
                        danger={orderSide === "sell" && orderType === "stop"}
                        onClick={this.setOrderType("stop")}
                    >
                        Stop
                    </Button>
                </div>

                <div className={classes.orderForm}>
                    <MoneyField
                        label={base + " Size"}
                        usd
                        value={size}
                        onChange={this.handleChangeSize}
                    />
                    {(orderType === "limit" || orderType === "stop") && (
                        <MoneyField
                            label="Limit Price"
                            value={orderPrice}
                            //units={product.split('-')[1]}
                            onChange={this.handleChangePrice}
                        />
                    )}
                    {orderType === "stop" && (
                        <MoneyField
                            label="Stop Price"
                            value={stopPrice}
                            //units={product.split('-')[1]}
                            onChange={this.handleChange("stopPrice")}
                        />
                    )}
                    <ListItem label="Total" textRight={total.toString()} />
                    <ListItem label="Fee" textRight={fee.toString()} />
                </div>
                <div className={classes.buttons}>
                    {orderSide === "buy" ? (
                        <Button
                            full
                            secondary
                            onClick={this.placeOrder}>
                            Buy
                        </Button>
                    ) : (
                        <Button
                            full
                            danger
                            onClick={this.placeOrder}>
                            Sell
                        </Button>
                    )}
                </div>
            </div>

            </GridItem>
            <GridItem xs={3} xl={3} borderTop borderRight borderLeft>
              {/* <Editor  handleSendCode={this.SendCode} code={this.code}/> */}

              <ListItem
                header
                label="Code Editor"
                iconLeft={Code}
                buttons={[
                  <IconButton onClick={this.openCodeModal}>
                    <FolderOpen />
                  </IconButton>,
                  <IconButton onClick={this.openCodeModal}>
                    <Save />
                  </IconButton>,
                  <IconButton onClick={this.openCodeModal}>
                    <Close />
                  </IconButton>,
                  <IconButton onClick={this.SendCode}>
                    <PlayArrow />
                  </IconButton>
                ]}
              />


              <CustomizedTabs
                tabs={openFiles}
                value={tabValue}
                handleChange={this.handleChangeTab}
              />
              {tabValue === 1 &&
                <AceEditor
                  ref="aceEditor"
                  width="100%"
                  height="73vh"
                  mode="python"
                  theme="monokai"
                  fontSize={"1.2vh"}
                  onChange={this.handleChangeCode}
                  setOptions={{
                    enableBasicAutocompletion: [autoComplete],
                    //enableBasicAutocompletion: true,
                    enableLiveAutocompletion: true,
                    enableSnippets: true,
                  }}
                  // markers={this.errorMarkers}
                  name="UNIQUE_ID_OF_DIV"
                  editorProps={{ $blockScrolling: true }}
                  defaultValue={this.code}
                />
              }

              <ListItem
                header
                marginTop
                label="Code Output"
                imgLeft={iconsUrl + 'output-icon.png'}
              />
              <CodeOutput output={this.printOutput} />
            </GridItem>

            <GridItem xs={6} xl={6} borderTop borderRight>
            <ListItem
                  header
                  button
                  imgLeft={exchangeImg}
                  label={exchangeNames[exchange_id]}
                  iconRight={product_id}
                  onClick={this.handlePortfolioModalOpen}
                />
              <ListItem
                label={product_id}
                imgLeft={productImg}
                iconRight={
                  <Select
                    value={selectedInterval}
                    onChange={this.handleChangeInterval()}
                    menuItems={intervals}
                  />
                }
              />
              <BacktestChart
                priceFormat={pFormat}
                data={this.chart}
                lineColors={this.state.lineColors}
                openOrders={openOrders}
                selectedOrders={selectedOrders}
                fills={fills}
                selectedFills={selectedFills}
                orderSide={this.state.orderSide}
                handleSelect={this.handleSelect}
                setPrice={this.setPriceFromChart}
                product_id={product_id}
              />
              <ListItem
                header
                marginTop
                label="Trade History"
              />
              <TickChart data={this.ticks} />
            </GridItem>

            <GridItem xs={1} xl={1} borderTop borderRight>
              <OrderBook
                book={this.book}
                onClick={this.orderBookClick}
              />
              <TradeHistory rows={this.tradeHistory} />
            </GridItem>
          </Grid>



          <FileModal
            open={openFileModal}
            onClose={this.handleCloseModal}
            files={this.fileDictionary}
            onFileClick={this.handleFileClick}
          />

          <PortfolioModal
            portfolio={portfolio}
            exchange_id={exchange_id}
            product_id={product_id}
            iconsUrl={iconsUrl}
            allTicks={this.allTicks}
            handleProductClick={this.handleProductClick}
            open={openPortfolioModal}
            onClose={this.handlePortfolioModalClose}
          />
        </div>
      );
    } else {
      return <Loading status={this.state.loadingStatus} />;
    }
  }
}

TradeView.propTypes = {
  classes: PropTypes.object.isRequired,
};

export default withStyles(tradeStyle)(TradeView);
