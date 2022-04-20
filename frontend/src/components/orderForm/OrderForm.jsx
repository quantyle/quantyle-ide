import React from "react";
import PropTypes from "prop-types";
import orderFormStyle from "../../variables/styles/orderFormStyle.jsx";
import { Component } from "react";
import {
    Grid,
    withStyles,
} from "@material-ui/core";
import {
    GridItem,
    ListItem,
    Button,
    MoneyField,
} from '../../components';
import {
    AttachMoney,
} from "@material-ui/icons";
import {
    usdFormat,
    sizeFormat,
} from "../../variables/global";
import api from "../../providers/api";


class OrderForm extends Component {

    constructor(props) {
        super(props);
        this.exchange = "GDAX"
        this.product = "DOGE-USD";
        this.state = {
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

        const {
            classes,
            base,
            quote,
            // quoteValue,
        } = this.props;

        const {
            // code,
            orderSide,
            orderType,
            size,
            stopPrice,
            orderPrice,
            baseValue,
            fee,
            total,
        } = this.state;



        return (
            <div>

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
                            // fontSize={fontSize}
                            onClick={this.placeOrder}>
                            Buy
                        </Button>
                    ) : (
                        <Button
                            full
                            // fontSize={fontSize}
                            danger
                            onClick={this.placeOrder}>
                            Sell
                        </Button>
                    )}
                </div>
            </div>
        );
    }
}

OrderForm.propTypes = {
    classes: PropTypes.object.isRequired,
    name: PropTypes.string,
    routes: PropTypes.any,
};

export default withStyles(orderFormStyle)(OrderForm);
