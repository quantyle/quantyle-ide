import React from "react";
import PropTypes from "prop-types";
import { timeFormat } from "d3-time-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
  BarSeries,
  LineSeries,
  CandlestickSeries,
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
  CrossHairCursor,
  PriceCoordinate,
  MouseCoordinateY,
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { fitWidth } from "react-stockcharts/lib/helper";
import { getMorePropsForChart } from "react-stockcharts/lib/interactive/utils";
import {
  Annotate,
  SvgPathAnnotation,
} from "react-stockcharts/lib/annotation";
import {
  successColor,
  dangerColor,
  backgroundDark,
  backgroundPrimary,
  backgroundLight,
  warningColor,
  primaryColor,
  successColorShadow,
  dangerColorShadow,
} from "../../variables/styles";
import { head, last } from "react-stockcharts/lib/utils";
import { saveInteractiveNodes, getInteractiveNodes } from "./Interactiveutils";
import {
  DrawingObjectSelector,
} from "react-stockcharts/lib/interactive";
import { format } from "d3-format";
import { HoverTooltip } from "react-stockcharts/lib/tooltip";
import {
  standardFormat,
} from "../../variables/global";

const color = (d) => (d.close >= d.open ? successColor : dangerColor);
function round(number, precision = 0) {
  const d = Math.pow(10, precision);
  return Math.round(number * d) / d;
}

const rad = 5;
export function buyPath({ x, y }) {
  return `
  M ${x} ${y}
  a ${rad}, ${rad} 0 1,0 ${rad * 2},0
  a ${rad}, ${rad} 0 1,0 -${rad * 2},0`;
}

export function sellPath({ x, y }) {
  return `
  M ${x} ${y}
  a ${rad}, ${rad} 0 1,0 ${rad * 2},0
  a ${rad}, ${rad} 0 1,0 -${rad * 2},0`;
}



const dateFormat = timeFormat("%Y-%m-%d, %I:%M %p");
const volFormat = format(".2f");

function tooltipContent() {
  return ({ currentItem, xAccessor }) => {
    const color = currentItem[1] > currentItem[4] ? successColor : dangerColor;
    return {
      x: dateFormat(xAccessor(currentItem)),
      y: [
        {
          label: "open",
          value: standardFormat(currentItem[1]),
          stroke: color
        },
        {
          label: "high",
          value: standardFormat(currentItem[2]),
          stroke: color
        },
        {
          label: "low",
          value: standardFormat(currentItem[3]),
          stroke: color
        },
        {
          label: "close",
          value: standardFormat(currentItem[4]),
          stroke: color
        },
        {
          label: "volume",
          value: volFormat(currentItem[5]),
          stroke: color
        },
        {
          label: "PnL",
          value: currentItem[10] ? volFormat(currentItem[10]) : "0",
          stroke: successColor
        },
        {
          label: "cash",
          value: currentItem[8] ? volFormat(currentItem[8]) : "0",
          stroke: warningColor
        },
        {
          label: "crypto",
          value: currentItem[9] ? volFormat(currentItem[9]) : "0",
          stroke: primaryColor
        }
      ]
    };
  };
}


class BacktestChart extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      // width: window.innerWidth,
      height: window.innerHeight,
      enableInteractiveObject: true,
    };

    this.onDragComplete = this.onDragComplete.bind(this);
    // this.onDelete = this.onDelete.bind(this);
    this.handleSelection = this.handleSelection.bind(this);
    // this.handleDoubleClickAlert = this.handleDoubleClickAlert.bind(this);
    // this.updateWindowDimensions = this.updateWindowDimensions.bind(this);
    this.saveInteractiveNodes = saveInteractiveNodes.bind(this);
    this.getInteractiveNodes = getInteractiveNodes.bind(this);
    this.saveCanvasNode = this.saveCanvasNode.bind(this);
    this.handleDownloadMore = this.handleDownloadMore.bind(this);
    // this.changeScroll = this.changeScroll.bind(this);
  }

  saveCanvasNode(node) {
    this.canvasNode = node;
  }

  handleDownloadMore(start, end) {
    console.log("loading more...");

  }

  handleSelection(interactives, moreProps, e) {

    // only proceed if the main chart was clicked... for some reason clicking the volume chart causes error
    if (moreProps.currentCharts[0] === 1) {
      const independentCharts = moreProps.currentCharts.filter((d) => d !== 2);
      const first = head(independentCharts);
      const morePropsForChart = getMorePropsForChart(moreProps, first);
      const {
        mouseXY: [, mouseY],
        chartConfig: { yScale },
      } = morePropsForChart;
      let yValue = round(yScale.invert(mouseY), 6);
      this.props.setPrice(yValue);
    }

  }

  handleDoubleClickAlert(item) {
    const alertToEdit = {
      alert: item.object,
      chartId: item.chartId,
    };
    const yCoordinateList = this.state.yCoordinateList_1.filter((d) => {
      return d.id !== alertToEdit.alert.id;
    });
    this.setState({
      showModal: false,
      alertToEdit: {},
      yCoordinateList_1: yCoordinateList,
    });
  }

  onDelete() {
    console.log("onDelete");
    this.setState({
      yCoordinateList_1: [],
      enableInteractiveObject: true,
    });
  }

  changeScroll() {
    let style = document.body.style.overflow
    document.body.style.overflow = (style === 'hidden') ? 'auto' : 'hidden'
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

  render() {
    const {
      lineColors,
      data: initialData,
      width,
      // indColors,
      openOrders,
      selectedOrders,
      fills,
      selectedFills,
      // standardFormat,
      // product_id,
    } = this.props;

    const initialDataLength = initialData[0].length;

    const defaultAnnotationProps = {
      onClick: console.log.bind(console),
    };


    const canvasHeight = this.state.height * 0.725;
    const pnlChartHeight = canvasHeight * 0.12;

    let candlesChartHeight = initialData[0].length > 6 ? canvasHeight * 0.59 : canvasHeight * 0.90;
    const volumeChartHeight = canvasHeight * 0.35;
    const volumeChartOrigin = [0, candlesChartHeight - volumeChartHeight];


    const pnlChartOrigin = [0, candlesChartHeight];

    const cashChartOrigin = [0, candlesChartHeight + pnlChartHeight];

    const cryptoChartOrigin = [0, candlesChartHeight + (pnlChartHeight * 2)];


    const margin = {
      left: 0,
      right: 0.075 * this.state.height,
      top: 5,
      bottom: 0.0
    };


    const gridHeight = canvasHeight - margin.top - margin.bottom;
    const gridWidth = width - margin.left - margin.right;

    const yGrid = { innerTickSize: -1 * gridWidth };
    const xGrid = { innerTickSize: -1 * gridHeight };

    // const titleOffset = 150;
    const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(
      (d) => new Date(d[0] * 1000)
    );

    const { data, xScale, xAccessor, displayXAccessor } =
      xScaleProvider(initialData);

    const start = xAccessor(last(data));
    // const end = xAccessor(data[0]);
    const end = data.length - 200;
    const xExtents = [start, end];

    // var or const ?
    const sellAnontationProps = {
      ...defaultAnnotationProps,
      //y: ({ yScale }) => yScale.range()[0],
      y: ({ yScale, datum }) => yScale(datum[2]),
      path: sellPath,
      fill: dangerColorShadow,
      // tooltip: (d) => timeFormat("%B")(d.date),
      // onMouseOver: console.log.bind(console),
    };

    // var or const ?
    const buyAnnotationProps = {
      ...defaultAnnotationProps,
      //y: ({ yScale }) => yScale.range()[0],
      y: ({ yScale, datum }) => yScale(datum[3]),
      path: buyPath,
      fill: successColorShadow,
      // tooltip: (d) => timeFormat("%B")(d.date),
      // onMouseOver: console.log.bind(console),
    };

    // buy markers
    const buyAnnotation = (
      <Annotate
        with={SvgPathAnnotation}
        when={(d) => d[6] > 0}
        usingProps={buyAnnotationProps}
      />
    );

    // sell markers
    const sellAnnotation = (
      <Annotate
        with={SvgPathAnnotation}
        when={(d) => d[7] > 0}
        usingProps={sellAnontationProps}
      />
    );

    const axisColor = "#ffffff99";
    const gridColor = "#ffffff15";
    //const priceCoordinateColor = ticker.change > 0 ? successColor : dangerColor;
    const lastTick = data[data.length - 1];
    const priceCoordinateColor =
      lastTick[4] >= lastTick[1] ? successColor : dangerColor;

    let orderPrices = [];

    selectedOrders.forEach((index) => {
      // failsafe for when sockets clean orders in between renders
      if (openOrders[index]) {
        orderPrices.push(parseFloat(openOrders[index].price));
      }
    });

    selectedFills.forEach((index) => {
      // failsafe for when sockets clean orders in between renders
      if (fills[index]) {
        orderPrices.push(parseFloat(fills[index].price));
      }
    });

    const yExtents = [(d) => [d[2], d[3], lastTick[4], ...orderPrices]];
    const defaultFontSize = 0.009 * this.state.height;


    return (
      <div style={{ background: backgroundDark, }}
      // onMouseEnter={this.changeScroll}
      // onMouseLeave={this.changeScroll}
      >
        <ChartCanvas
          height={canvasHeight}
          width={width}
          ratio={1}
          margin={margin}
          type="canvas"
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
            yExtents={[(d) => d[5]]}
            origin={volumeChartOrigin}
          >
            <BarSeries
              yAccessor={(d) => d[5]}
              fill={backgroundLight} />
          </Chart>



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
              yAccessor={(d) => ({
                open: d[1],
                high: d[2],
                low: d[3],
                close: d[4],
              })}
            />
            {lineColors.map((lineColor, key) => (
              <LineSeries
                key={key}
                yAccessor={(d) => d[11 + key] > 0 ? d[11 + key] : undefined}
                stroke={lineColor}
                strokeWidth={1}
              />
            ))}

            <YAxis
              axisAt="right"
              orient="right"
              ticks={4}
              stroke={axisColor}
              tickStroke={gridColor}
              tickFormat={standardFormat}
              {...yGrid}
            />
            <YAxis // secondary YAxis is a hack 
              axisAt="right"
              orient="right"
              ticks={4}
              stroke={gridColor}
              tickStroke={axisColor}
              tickFormat={standardFormat}
              fontSize={defaultFontSize}
            />
            {initialDataLength < 6 ?
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={4}
                stroke={axisColor}
                tickStroke={gridColor}
                fontSize={0}
                {...xGrid}
              />
              :
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={6}
                stroke={axisColor}
                tickStroke={axisColor}
                fontSize={defaultFontSize}
              />
            }




            <MouseCoordinateY
              at="right"
              orient="right"
              stroke={backgroundPrimary}
              dx={4}
              fill={backgroundDark}
              displayFormat={standardFormat}
              fontSize={defaultFontSize}
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
              displayFormat={standardFormat}
              lineOpacity={0.5}
              lineStroke={priceCoordinateColor}
              fontSize={defaultFontSize}
            />

            {/* {openOrders.length > 0 &&
              selectedOrders.map(
                (index) =>
                  function () {
                    if (index < openOrders.length) {
                      const color =
                        openOrders[index].side === "buy"
                          ? successColor
                          : dangerColor;
                      return (
                        <PriceCoordinate
                          key={index}
                          at="right"
                          orient="right"
                          price={parseFloat(openOrders[index].price)}
                          stroke={color}
                          fill="#000000"
                          textFill={color}
                          arrowWidth={7}
                          strokeDasharray="Dot"
                          displayFormat={standardFormat}
                          lineOpacity={1}
                          lineStroke={color}
                        />
                      );
                    }
                  }
              )} */}

            {/* {fills.length > 0 &&
              selectedFills.map(
                (index) =>
                  function () {
                    if (index < fills.length) {
                      const color =
                        fills[index].side === "buy"
                          ? successColor
                          : dangerColor;            
                      return (
                        <PriceCoordinate
                          key={index}
                          at="right"
                          orient="right"
                          price={parseFloat(fills[index].price)}
                          stroke="#000000"
                          fill={color}
                          textFill="#000000"
                          arrowWidth={7}
                          strokeDasharray="Dash"
                          displayFormat={standardFormat}
                          lineOpacity={1}
                          lineStroke={color}
                        />
                      );
                    }
                  }
              )} */}


            <CrossHairCursor stroke="#FFFFFF99" />

            {sellAnnotation}
            {buyAnnotation}
            {/* <Annotate
              with={SvgPathAnnotation}
              when={(d) => d[6] > d[7]}
              usingProps={sellAnontationProps}
            /> */}
          </Chart>



          {/* PNL CHART */}
          {initialDataLength > 6 &&
            <Chart
              id={5}
              height={pnlChartHeight}
              yExtents={[(d) => d[10]]}
              origin={pnlChartOrigin}
            >
              <LineSeries
                yAccessor={(d) => d[10]}
                stroke={successColor} />

              <MouseCoordinateY
                at="right"
                orient="right"
                stroke={backgroundDark}
                dx={4}
                fill={backgroundDark}
                displayFormat={standardFormat}
                fontSize={defaultFontSize}
              />

              <YAxis
                axisAt="right"
                orient="right"
                ticks={4}
                stroke={axisColor}
                tickStroke={axisColor}
                tickFormat={standardFormat}
                fontSize={defaultFontSize}
              />
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={4}
                stroke={axisColor}
                tickStroke={gridColor}
                fontSize={0}
              />
            </Chart>
          }

          {/* CASH */}
          {initialDataLength > 6 &&
            <Chart
              id={3}
              height={pnlChartHeight}
              yExtents={[(d) => d[8]]}
              origin={cashChartOrigin}
            >
              <LineSeries
                yAccessor={(d) => d[8]}
                stroke={warningColor} />

              <MouseCoordinateY
                at="right"
                orient="right"
                stroke={backgroundDark}
                dx={4}
                fill={backgroundDark}
                displayFormat={standardFormat}
                fontSize={defaultFontSize}
              />
              <YAxis
                axisAt="right"
                orient="right"
                ticks={4}
                stroke={axisColor}
                tickStroke={axisColor}
                tickFormat={standardFormat}
                fontSize={defaultFontSize}
              />
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={4}
                stroke={axisColor}
                tickStroke={gridColor}
                fontSize={0}
              />
            </Chart>
          }
          {/* CRYPTO CHART */}
          {initialDataLength > 6 &&

            <Chart
              id={4}
              height={pnlChartHeight}
              yExtents={[(d) => d[9]]}
              origin={cryptoChartOrigin}
            >
              <LineSeries
                yAccessor={(d) => d[9]}
                stroke={primaryColor} />

              <MouseCoordinateY
                at="right"
                orient="right"
                stroke={backgroundDark}
                dx={4}
                fill={backgroundDark}
                displayFormat={standardFormat}
                fontSize={defaultFontSize}
              />

              <YAxis
                axisAt="right"
                orient="right"
                ticks={4}
                stroke={axisColor}
                tickStroke={axisColor}
                tickFormat={standardFormat}
                fontSize={defaultFontSize}
              />
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={4}
                stroke={axisColor}
                tickStroke={gridColor}
                fontSize={defaultFontSize}
                {...xGrid}
              />
              <XAxis
                axisAt="bottom"
                orient="bottom"
                ticks={6}
                stroke={axisColor}
                tickStroke={axisColor}
                fontSize={defaultFontSize}
              />
            </Chart>
          }



          {/* <DrawingObjectSelector
            enabled
            getInteractiveNodes={this.getInteractiveNodes}
            drawingObjectMap={{
              InteractiveYCoordinate: "yCoordinateList",
            }}
            onSelect={this.handleSelection}
          //onDoubleClick={this.handleDoubleClickAlert}
          /> */}

          <HoverTooltip
            tooltipContent={tooltipContent()}
            fontSize={15}
            fill={backgroundPrimary}
            bgOpacity={0}
            opacity={1}
            stroke={backgroundLight}
            fontFill="#fff"
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

BacktestChart.propTypes = {
  data: PropTypes.array.isRequired,
  width: PropTypes.number.isRequired,
  // signals: PropTypes.any,
};


// eslint-disable-next-line no-class-assign
BacktestChart = fitWidth(BacktestChart);

export default BacktestChart;
