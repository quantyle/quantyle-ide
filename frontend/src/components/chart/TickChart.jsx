import React from "react";
import PropTypes from "prop-types";
import { timeFormat } from "d3-time-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
    LineSeries,
    ScatterSeries,
    CircleMarker,
    BarSeries,
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
    PriceCoordinate,
    MouseCoordinateY,
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { fitWidth } from "react-stockcharts/lib/helper";
import { last, } from "react-stockcharts/lib/utils";
import {
    successColor,
    dangerColor,
    backgroundDark,
    backgroundLight,
    backgroundPrimary,
} from "../../variables/styles";
import {
    Annotate,
    SvgPathAnnotation,
    buyPath,
    sellPath,
} from "react-stockcharts/lib/annotation";
import {
    standardFormat,
} from "../../variables/global";
import { HoverTooltip } from "react-stockcharts/lib/tooltip";

const color = d => d.side === 'buy' ? successColor : dangerColor
const colorVolume = (d) => (d.side === "buy" ? successColor + "80" : dangerColor + "80");
const dateFormat = timeFormat("%Y-%m-%d, %I:%M:%S %p");

function tooltipContent() {
    return ({ currentItem, xAccessor }) => {
        return {
            x: dateFormat(xAccessor(currentItem)),
            y: [
                {
                    label: "price",
                    value: standardFormat(currentItem.price),
                    stroke: currentItem.side === 'buy' ? successColor : dangerColor
                },
                {
                    label: "size",
                    value: standardFormat(currentItem.size),
                    stroke: currentItem.side === 'buy' ? successColor : dangerColor
                },
                {
                    label: "Total",
                    value: standardFormat(currentItem.price * currentItem.size),
                    stroke: currentItem.side === 'buy' ? successColor : dangerColor
                },
            ]
        };
    };
}

class TickChart extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            width: window.innerWidth,
            height: window.innerHeight
        };
        this.updateWindowDimensions = this.updateWindowDimensions.bind(this);
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
    }

    render() {
        const {
            type,
            data: initialData,
            ratio,
            width,
        } = this.props;

        const defaultAnnotationProps = {
            onClick: console.log.bind(console),
        };

        const margin = {
            left: 0,
            right: 0.075 * this.state.height,
            top: 15,
            bottom: 0
        };
        const canvasHeight = this.state.height * 0.16; // canvas height (vh)
        const areaChartHeight = canvasHeight * 0.80; // area chart height (vh)

        const volumeChartHeight = canvasHeight * 0.65;
        const volumeChartOrigin = [0, areaChartHeight - volumeChartHeight];
        const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(
            (d) => new Date(d.time * 1000)
        );

        const {
            data,
            xScale,
            xAccessor,
            displayXAccessor,
        } = xScaleProvider(initialData);

        const start = xAccessor(last(data));
        const end = xAccessor(data[0]);
        const xExtents = [start, end];
        const lastTick = data[data.length - 1];
        // var or const ?
        const sellAnontationProps = {
            ...defaultAnnotationProps,
            y: ({ yScale, datum }) => yScale(datum.price),
            path: sellPath,
            fill: dangerColor,
            tooltip: d => d.price,
        };

        // var or const ?
        const buyAnnotationProps = {
            ...defaultAnnotationProps,
            y: ({ yScale, datum }) => yScale(datum.price),
            path: buyPath,
            fill: successColor,
            tooltip: d => d.price,
        };

        const gridHeight = canvasHeight - margin.top - margin.bottom;
        const gridWidth = width - margin.left - margin.right;
        const yGrid = { innerTickSize: -1 * gridWidth };
        const xGrid = { innerTickSize: -1 * gridHeight };


        // buy markers
        const buyAnnotation = (
            <Annotate
                with={SvgPathAnnotation}
                when={d => d.min}
                usingProps={buyAnnotationProps}
            />
        );

        // sell markers
        const sellAnnotation = (
            <Annotate
                with={SvgPathAnnotation}
                when={d => d.max}
                usingProps={sellAnontationProps}
            />
        );

        const axisColor = '#ffffff99';
        const priceCoordinateColor = lastTick.price === data[data.length - 1].side === 'buy' ? successColor : dangerColor
        const defaultFontSize = 0.009 * this.state.height;
        const gridColor = "#ffffff15";

        return (
            <div style={{ background: backgroundDark }}>


                <ChartCanvas height={canvasHeight}
                    width={width}
                    ratio={ratio}
                    margin={margin}
                    type={type}
                    seriesName="TICKERS"
                    data={data}
                    xScale={xScale}
                    xAccessor={xAccessor}
                    displayXAccessor={displayXAccessor}
                    xExtents={xExtents}
                    clamp={true}
                >
                    <Chart
                        id={35}
                        yExtents={[d => [d.price]]}
                        height={areaChartHeight}
                        origin={[0, 0]}
                    >
                        <LineSeries
                            yAccessor={d => d.price}
                            stroke="#ffffff88"
                            strokeWidth={0.7}
                        />
                        <ScatterSeries
                            yAccessor={d => d.price}
                            marker={CircleMarker}
                            markerProps={{ r: 2, fill: color, stroke: color }}
                        />
                        {/* <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={standardFormat}
                            fontSize={defaultFontSize}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={20}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            fontSize={defaultFontSize}
                        /> */}
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



                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill="#00000000"
                            displayFormat={standardFormat}
                            fontSize={defaultFontSize}
                        />
                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick.price}
                            dx={4}
                            fill="#00000000"
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="Dash"
                            displayFormat={standardFormat}
                            lineOpacity={1}
                            lineStroke={priceCoordinateColor}
                            fontSize={defaultFontSize}
                        />
                        <CrossHairCursor stroke="#FFFFFF99" />
                        {sellAnnotation}
                        {buyAnnotation}
                        <HoverTooltip
                            tooltipContent={tooltipContent()}
                            fontSize={15}
                            fill={backgroundPrimary}
                            bgOpacity={0}
                            opacity={1}
                            stroke={backgroundLight}
                            fontFill="#fff"
                        />
                    </Chart>




                    {/* VOLUME CHART */}
                    <Chart
                        id={2}
                        height={volumeChartHeight}
                        yExtents={[(d) => d.size]}
                        origin={volumeChartOrigin}
                    >
                        <BarSeries
                            yAccessor={(d) => d.size}
                            fill={colorVolume} />


                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={6}
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


                </ChartCanvas>
            </div>
        );
    }
}

TickChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid", "canvas"]).isRequired,
    signals: PropTypes.any,
};

TickChart.defaultProps = {
    type: "canvas",
};

// eslint-disable-next-line no-class-assign
TickChart = fitWidth(TickChart);

export default TickChart;
