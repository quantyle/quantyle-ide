import React from "react";
import PropTypes from "prop-types";
import { timeFormat } from "d3-time-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
    BarSeries,
    LineSeries,
    CandlestickSeries,
    // CircleMarker,
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
    PriceCoordinate,
    MouseCoordinateX,
    MouseCoordinateY,
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { fitWidth } from "react-stockcharts/lib/helper";
import { getMorePropsForChart } from "react-stockcharts/lib/interactive/utils";
import {
    Label,
} from "react-stockcharts/lib/annotation";
import {
    successColor,
    dangerColor,
    backgroundDark,
    backgroundLight,
} from "../../variables/styles";
import { head, last } from "react-stockcharts/lib/utils";
import {
    saveInteractiveNodes,
    getInteractiveNodes,
} from "./Interactiveutils";
import { InteractiveYCoordinate, DrawingObjectSelector } from "react-stockcharts/lib/interactive";
// import ScatterSeries from "react-stockcharts/lib/series/ScatterSeries";


const color = d => d.close >= d.open ? successColor : dangerColor;
const colorDim = d => d[4] >= d[1] ? successColor + "20" : dangerColor + "20";

function round(number, precision = 0) {
    const d = Math.pow(10, precision);
    return Math.round(number * d) / d;
}


class CodeChart extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            width: window.innerWidth,
            height: window.innerHeight,
            enableInteractiveObject: true,
        };

        this.onDragComplete = this.onDragComplete.bind(this);
        this.onDelete = this.onDelete.bind(this);
        this.handleSelection = this.handleSelection.bind(this);
        this.handleDoubleClickAlert = this.handleDoubleClickAlert.bind(this);
        this.updateWindowDimensions = this.updateWindowDimensions.bind(this);
        this.saveInteractiveNodes = saveInteractiveNodes.bind(this);
        this.getInteractiveNodes = getInteractiveNodes.bind(this);
        this.saveCanvasNode = this.saveCanvasNode.bind(this);
        this.handleDownloadMore = this.handleDownloadMore.bind(this);
    }


    saveCanvasNode(node) {
        this.canvasNode = node;
    }

    guidGenerator() {
        var S4 = function () {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };
        return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
    }


    handleDownloadMore(start, end) {
        console.log('loading more...')
        // // this.state = {
        // // 	ema26,
        // // 	ema12,
        // // 	macdCalculator,
        // // 	smaVolume50,
        // // 	linearData,
        // // 	data: linearData,
        // // 	xScale,
        // // 	xAccessor, displayXAccessor
        // // };

        // if (Math.ceil(start) === end) return;
        // // console.log("rows to download", rowsToDownload, start, end)
        // const { data: prevData, ema26, ema12, macdCalculator, smaVolume50 } = this.state;
        // const { data: inputData } = this.props;


        // if (inputData.length === prevData.length) return;

        // const rowsToDownload = end - Math.ceil(start);

        // const maxWindowSize = getMaxUndefined([ema26,
        // 	ema12,
        // 	macdCalculator,
        // 	smaVolume50
        // ]);

        // /* SERVER - START */
        // const dataToCalculate = inputData
        // 	.slice(-rowsToDownload - maxWindowSize - prevData.length, - prevData.length);

        // const calculatedData = ema26(ema12(macdCalculator(smaVolume50(dataToCalculate))));
        // const indexCalculator = discontinuousTimeScaleProviderBuilder()
        // 	.initialIndex(Math.ceil(start))
        // 	.indexCalculator();
        // const { index } = indexCalculator(
        // 	calculatedData
        // 		.slice(-rowsToDownload)
        // 		.concat(prevData));
        // /* SERVER - END */

        // const xScaleProvider = discontinuousTimeScaleProviderBuilder()
        // 	.initialIndex(Math.ceil(start))
        // 	.withIndex(index);

        // const { data: linearData, xScale, xAccessor, displayXAccessor } = xScaleProvider(calculatedData.slice(-rowsToDownload).concat(prevData));

        // // console.log(linearData.length)
        // setTimeout(() => {
        // 	// simulate a lag for ajax
        // 	this.setState({
        // 		data: linearData,
        // 		xScale,
        // 		xAccessor,
        // 		displayXAccessor,
        // 	});
        // }, 300);
    }



    handleSelection(interactives, moreProps, e) {
        const independentCharts = moreProps.currentCharts.filter(d => d !== 2);
        const first = head(independentCharts);
        const morePropsForChart = getMorePropsForChart(moreProps, first);
        const {
            mouseXY: [, mouseY],
            chartConfig: { yScale },
        } = morePropsForChart;
        let yValue = round(yScale.invert(mouseY), 6);
        this.props.setPrice(yValue);
    }


    handleDoubleClickAlert(item) {
        const alertToEdit = {
            alert: item.object,
            chartId: item.chartId,
        }
        const yCoordinateList = this.state.yCoordinateList_1.filter(d => {
            return d.id !== alertToEdit.alert.id;
        });
        this.setState({
            showModal: false,
            alertToEdit: {},
            yCoordinateList_1: yCoordinateList
        });
    }

    onDelete() {
        console.log('onDelete')
        this.setState({
            yCoordinateList_1: [],
            enableInteractiveObject: true,
        });
    }


    onDragComplete(yCoordinateList, moreProps, draggedAlert) {
        //console.log('onDragComplete')
        // this gets called on drag complete of drawing object
        const { id: chartId } = moreProps.chartConfig;
        const alertDragged = draggedAlert != null;
        //console.log(draggedAlert.yValue);
        this.props.setPrice(draggedAlert.yValue);
        this.setState({
            enableInteractiveObject: false,
            yCoordinateList_1: yCoordinateList,
            showModal: alertDragged,
            alertToEdit: {
                alert: draggedAlert,
                chartId,
            },
            originalAlertList: this.state.yCoordinateList_1,
        });
    }

    componentDidMount() {
        window.addEventListener('resize', this.updateWindowDimensions());
    }

    componentWillUnmount() {
        window.removeEventListener('resize', this.updateWindowDimensions());
    }

    updateWindowDimensions() {
        console.log(window.innerWidth + ' ' + window.innerHeight);
        this.setState({ width: window.innerWidth, height: window.innerHeight });
        //document.removeEventListener("keyup", this.onKeyPress);
    }

    render() {
        const {
            type,
            data: initialData,
            ratio,
            width,
            priceFormat,
            product_id,
            // indColors,
            // openOrders,
            // selectedOrders,
            // fills,
            // selectedFills,
        } = this.props;

        // const defaultAnnotationProps = {
        //     onClick: console.log.bind(console),
        // };

        const margin = { left: 0, right: 70, top: 10, bottom: 0 };
        const canvasHeight = this.state.height * 0.30;
        const candlesChartHeight = canvasHeight * 0.86;
        const volumeChartHeight = canvasHeight * 0.50;
        const volumeChartOrigin = [0, candlesChartHeight - volumeChartHeight];

        // const sumChartHeight = canvasHeight * 0.19; // area chart height (vh)
        // const titleOffset = 150;
        const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(d => new Date(d[0] * 1000));

        const {
            data,
            xScale,
            xAccessor,
            displayXAccessor,
        } = xScaleProvider((initialData));

        const start = xAccessor(last(data));
        const end = xAccessor(data[0]);
        const xExtents = [start, end];

        // // var or const ?
        // const sellAnontationProps = {
        //     ...defaultAnnotationProps,
        //     //y: ({ yScale }) => yScale.range()[0],
        //     y: ({ yScale, datum }) => yScale(datum.high),
        //     path: sellPath,
        //     fill: dangerColor,
        //     tooltip: d => timeFormat("%B")(d.date),
        //     // onMouseOver: console.log.bind(console),
        // };

        // // var or const ?
        // const buyAnnotationProps = {
        //     ...defaultAnnotationProps,
        //     //y: ({ yScale }) => yScale.range()[0],
        //     y: ({ yScale, datum }) => yScale(datum.low),
        //     path: buyPath,
        //     fill: successColor,
        //     tooltip: d => timeFormat("%B")(d.date),
        //     // onMouseOver: console.log.bind(console),
        // };

        // // buy markers
        // const buyAnnotation = (
        //     <Annotate
        //         with={SvgPathAnnotation}
        //         when={d => {
        //             if (signals) {
        //                 var n = signals.filter(function (item) {
        //                     let newDate = new Date(item.t).toISOString();
        //                     return newDate === d.date.toISOString() && item.type === 'BUY EXECUTED';
        //                 });
        //                 if (n.length > 0) {
        //                     return true;
        //                 }
        //             } else {
        //                 return null;
        //             }
        //         }}
        //         usingProps={buyAnnotationProps}
        //     />
        // )

        // // sell markers
        // const sellAnnotation = (
        //     <Annotate
        //         with={SvgPathAnnotation}
        //         when={d => {
        //             if (signals) {
        //                 var n = signals.filter(function (item) {
        //                     let newDate = new Date(item.t).toISOString();
        //                     return newDate === d.date.toISOString() && item.type === 'SELL EXECUTED';
        //                 });
        //                 if (n.length > 0) {
        //                     return true;
        //                 }
        //             } else {
        //                 return null;
        //             }
        //         }}
        //         usingProps={sellAnontationProps}
        //     />
        // )





        const axisColor = '#ffffff99';
        //const priceCoordinateColor = ticker.change > 0 ? successColor : dangerColor;
        const lastTick = data[data.length - 1];
        const priceCoordinateColor = lastTick[4] >= lastTick[3] ? successColor : dangerColor;

        let orderPrices = [];

        const yExtents = [d => [d[2], d[1], lastTick[4], ...orderPrices]];


        // const ema20 = ema()
        //     .id(0)
        //     .options({ windowSize: 13 })
        //     .merge((d, c) => { d.ema20 = c; })
        //     .accessor(d => d.ema20);

        // const ema50 = ema()
        //     .id(2)
        //     .options({ windowSize: 50 })
        //     .merge((d, c) => { d.ema50 = c; })
        //     .accessor(d => d.ema50);


        return (
            <div style={{ borderBottom: '1px solid ' + backgroundLight }}>

                <ChartCanvas
                    height={canvasHeight}
                    width={width}
                    ratio={ratio}
                    margin={margin}
                    type={type}
                    seriesName="BTC-USD"
                    data={data}
                    xScale={xScale}
                    xAccessor={xAccessor}
                    displayXAccessor={displayXAccessor}
                    xExtents={xExtents}
                    clamp={true}
                    padding={10}
                    onLoadMore={this.handleDownloadMore}
                >
                    {/* VOLUME CHART */}
                    <Chart
                        id={2}
                        height={volumeChartHeight}
                        yExtents={[d => d[5]]}
                        origin={volumeChartOrigin}
                    >
                        <BarSeries
                            yAccessor={d => d[5]}
                            fill={colorDim}
                        />
                    </Chart>


                    {/* <Chart id={3}
                        height={macdHeight}
                        yExtents={macdCalculator.accessor()}
                        origin={[0, candlesChartHeight]}
                        padding={{ top: 8, bottom: 8 }}
                    >

                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={2}
                            stroke={axisColor}
                            tickStroke={axisColor}

                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            displayFormat={format(".2f")}
                            stroke={primaryColor}
                            dx={5}
                            fill={backgroundLight}
                        />
                        <MouseCoordinateX
                            at="bottom"
                            orient="bottom"
                            stroke={backgroundDark}
                            fill={backgroundLight}
                            displayFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                        />
                        <MACDSeries yAccessor={d => d.macd}
                            {...macdAppearance} />
                        <MACDTooltip
                            origin={[10, 15]}
                            yAccessor={d => d.macd}
                            options={macdCalculator.options()}
                            appearance={macdAppearance}
                        />
                    </Chart> */}
                    <Label
                        x={50}
                        y={15}
                        fontSize={18}
                        text={product_id}
                        fill="#ffffff50"
                    />
                    {/* CANDLESTICK CHART */}
                    <Chart
                        id={1}
                        yExtents={yExtents}
                        height={candlesChartHeight}
                    //origin={(w, h) => [0, h - 300]}
                    >
                        <CandlestickSeries
                            stroke={color}
                            wickStroke={color}
                            fill={color}
                            yAccessor={d => ({ open: d[1], high: d[2], low: d[3], close: d[4] })}
                        //classNames = {d => d[4] > d[3] ? "up" : "down"
                        // fill: d => d.close > d.open ? "#6BA583" : "#FF0000",




                        />
                        <LineSeries
                            yAccessor={d => d[6]}
                            stroke={priceCoordinateColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d[7]}
                            stroke={priceCoordinateColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d[8]}
                            stroke={priceCoordinateColor}
                            strokeWidth={1}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={4}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={priceFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={(width / (width / 10))}
                            //tickFormat={timeFormat("%I:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={4}
                            fill={backgroundDark}
                            displayFormat={priceFormat}

                        />
                        <MouseCoordinateX
                            at="bottom"
                            orient="bottom"
                            stroke={backgroundDark}
                            dx={4}
                            fill={backgroundDark}
                            displayFormat={timeFormat("%Y-%m-%d %I:%M %p")}

                        />
                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick[4]}
                            dx={4}
                            fill={backgroundDark}
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="Dash"
                            displayFormat={priceFormat}
                            lineOpacity={0.5}
                            lineStroke={priceCoordinateColor}
                        />



                        <InteractiveYCoordinate
                            ref={this.saveInteractiveNodes("InteractiveYCoordinate", 1)}
                            enabled={this.state.enableInteractiveObject}
                            onDragComplete={this.onDragComplete}
                            onDelete={this.onDelete}
                            yCoordinateList={this.state.yCoordinateList_1}

                        />
                        <CrossHairCursor stroke='#FFFFFF66' />

                    </Chart>

                    <DrawingObjectSelector
                        enabled
                        getInteractiveNodes={this.getInteractiveNodes}
                        drawingObjectMap={{
                            InteractiveYCoordinate: "yCoordinateList"
                        }}
                        onSelect={this.handleSelection}
                    //onDoubleClick={this.handleDoubleClickAlert}

                    />


                    {/* PRICE FEED CHART */}
                    {/* <Chart
                            id={3}
                            yExtents={[d => d.price]}
                            height={candlesChartHeight}
                            origin={[0, 0]}
                        >
                            <LineSeries
                                yAccessor={d => d.price}
                                //stroke={indColors[key]}
                                strokeWidth={2}
                            />
                            <YAxis
                                axisAt="right"
                                orient="right"
                                ticks={6}
                                stroke={axisColor}
                                tickStroke={axisColor}
                            />
                            <CurrentCoordinate
                                yAccessor={d => d.price}
                                fill="#fff"
                                r={5}
                            />
                        </Chart> */}



                </ChartCanvas>

            </div>
        );

    }
}

CodeChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
    // signals: PropTypes.any,
};

CodeChart.defaultProps = {
    type: "hybrid",
    ratio: 1,
};

// eslint-disable-next-line no-class-assign
CodeChart = fitWidth(CodeChart);

export default CodeChart;
