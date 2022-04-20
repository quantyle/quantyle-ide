import React from "react";
import PropTypes from "prop-types";
// import { format } from "d3-format";
import { timeFormat } from "d3-time-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
    LineSeries,
    ScatterSeries,
    CircleMarker,
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
    MouseCoordinateX,
    MouseCoordinateY
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
// import {
//     HoverTooltip
// } from "react-stockcharts/lib/tooltip";
import { fitWidth } from "react-stockcharts/lib/helper";
import { last, } from "react-stockcharts/lib/utils";
import {
    successColor,
    dangerColor,
    backgroundDark,
    backgroundLight,
    primaryColor,
    warningColor,
    infoColor,
} from "../../variables/styles";
import {
    Label,
} from "react-stockcharts/lib/annotation";


const titleOffset = 20;
const titleFontSize = "17";

// const color = d => d.side === 'buy' ? successColor : dangerColor

class BookChart extends React.Component {

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
            currencyFormat,
        } = this.props;
        const margin = { left: 0, right: 70, top: 6, bottom: 0 };
        const canvasHeight = this.state.height * 0.6; // canvas height (vh)
        const sumChartHeight = canvasHeight * 0.19; // area chart height (vh)
        const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(d => d.date);
        const {
            data,
            xScale,
            xAccessor,
            displayXAccessor,
        } = xScaleProvider(initialData);
        const start = xAccessor(last(data));
        const end = xAccessor(data[0]);
        const xExtents = [start, end];
        const axisColor = '#ffffff99';

        return (
            <div>
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
                        id={48}
                        yExtents={[d => [d.gdax_price, d.bina_price, d.min, d.max]]}
                        height={sumChartHeight}
                        origin={[0, 0]}>

                        <LineSeries
                            yAccessor={d => d.gdax_price}
                            stroke={primaryColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.bina_price}
                            stroke={warningColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.gmni_price}
                            stroke={successColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.krkn_price}
                            stroke={infoColor}
                            strokeWidth={0.7}
                        />

                        <ScatterSeries
                            yAccessor={d => d.min}
                            marker={CircleMarker}
                            markerProps={{ r: 3, fill: successColor, stroke: successColor }}
                        />
                        <ScatterSeries
                            yAccessor={d => d.max}
                            marker={CircleMarker}
                            markerProps={{ r: 3, fill: dangerColor, stroke: dangerColor }}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={currencyFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={0}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill={backgroundLight}
                            displayFormat={currencyFormat}
                        />
                    </Chart>
                    <Label
                        // x={(width - margin.left - margin.right) / 2}
                        x={23}
                        y={20}
                        fontSize="11"
                        text="GDAX"
                        fill={primaryColor}
                    />
                    <Label
                        // x={(width - margin.left - margin.right) / 2}
                        x={22}
                        y={titleOffset + 20}
                        fontSize="11"
                        text="BINA"
                        fill={warningColor}
                    />
                    <Label
                        // x={(width - margin.left - margin.right) / 2}
                        x={22}
                        y={titleOffset + 40}
                        fontSize="10"
                        text="GMNI"
                        fill={successColor}
                    />
                    <Label
                        // x={(width - margin.left - margin.right) / 2}
                        x={23}
                        y={titleOffset + 60}
                        fontSize="11"
                        text="KRKN"
                        fill={infoColor}
                    />


                    <Label
                        x={58}
                        y={sumChartHeight + titleOffset}
                        fontSize={titleFontSize}
                        text="BBO Volumes"
                        fill="#ffffff70"
                    />
                    <Chart
                        id={54}
                        yExtents={[d => [d.ba_sizes, d.bb_sizes]]}
                        height={sumChartHeight}
                        origin={[0, sumChartHeight]}>
                        <LineSeries
                            yAccessor={d => d.bb_sizes}
                            stroke={successColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.ba_sizes}
                            stroke={dangerColor}
                            strokeWidth={0.7}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={currencyFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={0}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill={backgroundLight}
                            displayFormat={currencyFormat}
                        />

                    </Chart>




                    <Label
                        x={57}
                        y={(sumChartHeight * 2) + titleOffset}
                        fontSize={titleFontSize}
                        text="Book Volume"
                        fill="#ffffff70"
                    />
                    <Chart
                        id={45}
                        yExtents={[d => [d.a_sum, d.b_sum]]}
                        height={sumChartHeight}
                        origin={[0, (sumChartHeight * 2)]}>
                        <LineSeries
                            yAccessor={d => d.b_sum}
                            stroke={successColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.a_sum}
                            stroke={dangerColor}
                            strokeWidth={0.7}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={currencyFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={0}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill={backgroundLight}
                            displayFormat={currencyFormat}
                        />
                    </Chart>




                    <Label
                        x={100}
                        y={(sumChartHeight * 3) + titleOffset}
                        fontSize={titleFontSize}
                        text="Book Standard Deviation"
                        fill="#ffffff70"
                    />
                    <Chart
                        id={66}
                        yExtents={[d => [d.a_std, d.b_std]]}
                        height={sumChartHeight}
                        origin={[0, (sumChartHeight * 3)]}>
                        <LineSeries
                            yAccessor={d => d.b_std}
                            stroke={successColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.a_std}
                            stroke={dangerColor}
                            strokeWidth={0.7}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={currencyFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={0}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill={backgroundLight}
                            displayFormat={currencyFormat}
                        />
                    </Chart>





                    <Label
                        x={57}
                        y={(sumChartHeight * 4) + titleOffset}
                        fontSize={titleFontSize}
                        text="Trade Volume"
                        fill="#ffffff70"
                    />

                    <Chart
                        id={69}
                        yExtents={[d => [d.sell_vol, d.buy_vol]]}
                        height={sumChartHeight}
                        origin={[0, (sumChartHeight * 4)]}>
                        <LineSeries
                            yAccessor={d => d.buy_vol}
                            stroke={successColor}
                            strokeWidth={0.7}
                        />
                        <LineSeries
                            yAccessor={d => d.sell_vol}
                            stroke={dangerColor}
                            strokeWidth={0.7}
                        />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={3}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={currencyFormat}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={10}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                        <MouseCoordinateX
                            at="bottom"
                            orient="bottom"
                            stroke={backgroundDark}
                            fill={backgroundLight}
                            displayFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                        />
                        <MouseCoordinateY
                            at="right"
                            orient="right"
                            stroke={backgroundDark}
                            dx={5}
                            fill={backgroundLight}
                            displayFormat={currencyFormat}
                        />

                    </Chart>
                    <CrossHairCursor stroke={'#FFFFFF66'} />
                </ChartCanvas>
            </div>
        );
    }
}

BookChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
    signals: PropTypes.any,
};

BookChart.defaultProps = {
    type: "hybrid",
};

// eslint-disable-next-line no-class-assign
BookChart = fitWidth(BookChart);

export default BookChart;
