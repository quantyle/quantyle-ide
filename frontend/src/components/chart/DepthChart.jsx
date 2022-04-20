
import React from "react";
import PropTypes from "prop-types";
import {
    ChartCanvas,
    Chart,
} from "react-stockcharts";
import {
    AreaSeries,
} from "react-stockcharts/lib/series";
import {
    XAxis,
    YAxis
} from "react-stockcharts/lib/axes";
// import { format } from "d3-format";
// import { timeFormat } from "d3-time-format";
import {
    CurrentCoordinate,
} from "react-stockcharts/lib/coordinates";
import { fitWidth } from "react-stockcharts/lib/helper";
import {
    successColor,
    backgroundLight,
    dangerColor,
} from "../../variables/styles";
import { scaleLinear, } from "d3-scale";
import { curveStep } from 'd3-shape';
import { max, min } from "d3-array";
import { LabelAnnotation, Annotate } from "react-stockcharts/lib/annotation";
// import {
// 	OHLCTooltip,
// 	MovingAverageTooltip,
// 	MACDTooltip,
// 	HoverTooltip,
// } from "react-stockcharts/lib/tooltip";
// import {
// 	ema,

// } from "react-stockcharts/lib/indicator";

// const numberFormat = format(".2f");

// const dateFormat = timeFormat("%Y-%m-%d");

// function tooltipContent(ys) {
// 	return ({ currentItem, xAccessor }) => {
// 		return {
// 			x: dateFormat(xAccessor(currentItem)),
// 			y: [
// 				{
// 					label: "open",
// 					value: currentItem.open && numberFormat(currentItem.open)
// 				},
// 				{
// 					label: "high",
// 					value: currentItem.high && numberFormat(currentItem.high)
// 				},
// 				{
// 					label: "low",
// 					value: currentItem.low && numberFormat(currentItem.low)
// 				},
// 				{
// 					label: "close",
// 					value: currentItem.close && numberFormat(currentItem.close)
// 				}
// 			]
// 				.concat(
// 					ys.map(each => ({
// 						label: each.label,
// 						value: each[0](currentItem),
// 						stroke: each.stroke
// 					}))
// 				)
// 				.filter(line => line[0])
// 		};
// 	};
// }



class DepthChart extends React.Component {

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
        this.setState({ width: window.innerWidth, height: window.innerHeight });
    }

    render() {

        const {
            type,
            book,
            ratio,
            width
        } = this.props;


        const canvasHeight = this.state.height * 0.35; // 35vh (hopefully this works on mobile)

        const marginBook = {
            left: 0, right: 0, top: 0, bottom: 1
        };
        const marginAxis = {
            left: 0, right: 0, top: 0, bottom: 0
        };

        const yAxisColor = "#A9ABAE";
        const linesColor = "#65696D";

        const yAxisRange = [min(book, d => d.c), max(book, d => d.c)];

        const minBid = min(book, d => d.p);
        const maxBid = max(book, d => d.s === 'b' && d.p);
        const maxAsk = max(book, d => d.s === 'a' && d.p);

        // left side
        const xExtentsBid = [minBid, maxBid];

        // right side
        const xExtentsAsk = [maxBid, maxAsk];
        const xExtentsBook = [
            minBid,
            maxAsk,
        ];




        return (
            <div style={{ display: 'flex', position: 'relative' }}>
                <ChartCanvas
                    ratio={ratio}
                    width={width / 2}
                    height={canvasHeight}
                    margin={marginBook}
                    seriesName="bids"
                    data={book}
                    type={type}
                    panEvent={false}
                    xAccessor={d => d.p}
                    xScale={scaleLinear()}
                    xExtents={xExtentsBid}
                    zoomEvent={false}
                    displayXAccessor={d => d.s === 'b' && d.p}
                >
                    <Chart
                        id={10}
                        yExtents={yAxisRange}
                    >
                        <AreaSeries
                            yAccessor={d => d.s === 'b' && d.c}
                            fill={successColor + '33'}
                            strokeWidth={2}
                            stroke={successColor}
                            interpolation={curveStep}
                        />
                        <YAxis
                            axisAt="left"
                            orient="right"
                            tickStroke={yAxisColor}
                            ticks={4}
                            yZoomWidth={0}
                            stroke={backgroundLight}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            zoomEnabled={false}
                            stroke={linesColor}
                        />
                        <CurrentCoordinate
                            yAccessor={d => d.s === 'b' && d.c}
                            fill={successColor}
                            r={5}
                            onHover
                        />
                    </Chart>
                </ChartCanvas>

                <ChartCanvas
                    seriesName="asks"
                    ratio={ratio}
                    width={width / 2}
                    height={canvasHeight}
                    margin={marginBook}
                    data={book}
                    type={type}
                    panEvent={false}
                    xAccessor={d => d.p}
                    xScale={scaleLinear()}
                    xExtents={xExtentsAsk}
                    zoomEvent={false}
                    displayXAccessor={d => d.s === 'a' && d.p}
                >
                    <Chart
                        id={11}
                        yExtents={yAxisRange}
                    >
                        <YAxis
                            stroke={backgroundLight}
                            axisAt="left"
                            orient="right"
                            tickStroke={linesColor}
                            ticks={0}
                            yZoomWidth={0}
                        />
                        <YAxis
                            axisAt="right"
                            orient="left"
                            tickStroke={yAxisColor}
                            ticks={4}
                            yZoomWidth={0}
                            stroke={backgroundLight}
                        />

                        <CurrentCoordinate
                            yAccessor={d => d.s === 'a' && d.c}
                            fill={dangerColor}
                            r={5}
                            onHover
                        />
                        <AreaSeries
                            yAccessor={d => d.s === 'a' && d.c}
                            fill={dangerColor + '33'}
                            stroke={dangerColor}
                            strokeWidth={2}
                            interpolation={curveStep}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            zoomEnabled={false}
                            stroke={linesColor}
                        />
                    </Chart>

                </ChartCanvas>

                <div style={{ position: 'absolute', top: canvasHeight, pointerEvents: 'none' }}>
                    <ChartCanvas
                        ratio={ratio}
                        width={width}
                        height={canvasHeight}
                        margin={marginAxis}
                        data={book}
                        type={type}
                        panEvent={false}
                        xScale={scaleLinear()}
                        xExtents={xExtentsBook}
                        zoomEvent={false}
                        displayXAccessor={d => d.p}
                    >
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            tickStroke={yAxisColor}
                            ticks={6}
                            zoomEnabled={false}
                            stroke={linesColor}
                        />

                        <Annotate with={LabelAnnotation}
                            when={d => d.p > 6750 /* some condition */}
                            usingProps={{
                                fontFamily: "Glyphicons Halflings",
                                fontSize: 20,
                                fill: "#FFFFFF",
                                opacity: 0.8,
                                text: "\ue182",
                                y: d => d.c,
                                onClick: console.log.bind(console),
                                tooltip: d => d.p,
                                // onMouseOver: console.log.bind(console),
                            }} />
                    </ChartCanvas>
                </div>
            </div>
        );
    }
}

DepthChart.propTypes = {
    book: PropTypes.array,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
};

DepthChart.defaultProps = {
    type: "svg",
};

const DepthChartWithInteractiveIndicator = fitWidth(DepthChart);

export default DepthChartWithInteractiveIndicator;
