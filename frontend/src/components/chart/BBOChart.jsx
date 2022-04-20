import React from "react";
import PropTypes from "prop-types";

// import { format } from "d3-format";
import { timeFormat } from "d3-time-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
    LineSeries,
    // RSISeries
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
    PriceCoordinate,
    MouseCoordinateX,
    MouseCoordinateY
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
// import {
//     HoverTooltip
// } from "react-stockcharts/lib/tooltip";
import { fitWidth } from "react-stockcharts/lib/helper";
// import {
//     Annotate,
//     SvgPathAnnotation,
//     sellPath,
//     buyPath,
// } from "react-stockcharts/lib/annotation";
import { last, } from "react-stockcharts/lib/utils";
import {
    successColor,
    dangerColor,
    backgroundDark,
    backgroundLight,
} from "../../variables/styles";


// import {
//     Label,
//     Annotate,
//     SvgPathAnnotation,
//     buyPath,
//     sellPath,
// } from "react-stockcharts/lib/annotation";


// const dateFormat = timeFormat("%H:%M");
// const numberFormat = format(".6f");



// export function sellPath({ x, y }) {
// 	return `M${x} ${y} `
// 		+ `L${x + halfWidth} ${y - halfWidth} `
// 		+ `L${x + bottomWidth} ${y - halfWidth} `
// 		+ `L${x + bottomWidth} ${y - height} `
// 		+ `L${x - bottomWidth} ${y - height} `
// 		+ `L${x - bottomWidth} ${y - halfWidth} `
// 		+ `L${x - halfWidth} ${y - halfWidth} `
// 		+ "Z";
// }


class BBOChart extends React.Component {

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

        // const defaultAnnotationProps = {
        //     onClick: console.log.bind(console),
        // };

        const margin = { left: 0, right: 70, top: 6, bottom: 0 };
        const canvasHeight = this.state.height * 0.26; // canvas height (vh)
        const areaChartHeight = canvasHeight * 0.87; // area chart height (vh)

        const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(d => d.date);

        const {
            data,
            xScale,
            xAccessor,
            displayXAccessor,
        } = xScaleProvider(initialData);

        const start = xAccessor(last(data));
        //const end = xAccessor(data[data.length - 400]);
        const end = xAccessor(data[0]);
        const xExtents = [start, end];

        const lastTick = data[data.length - 1];



        const axisColor = '#ffffff99';
        const priceCoordinateColor = '#ffffff';

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
                        id={3}
                        yExtents={[d => [d.best_bid_price, d.best_ask_price]]}
                        height={areaChartHeight}
                        origin={[0, 0]}
                    >

                        <LineSeries
                            yAccessor={d => d.best_ask_price}
                            stroke={dangerColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d.best_bid_price}
                            stroke={successColor}
                            strokeWidth={1}
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


                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick.price}
                            dx={4}
                            fill={backgroundDark}
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="Dash"
                            displayFormat={currencyFormat}
                            lineOpacity={1}
                            lineStroke={priceCoordinateColor}
                        />
                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick.GDAX}
                            dx={4}
                            fill={backgroundDark}
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="ShortDash"
                            displayFormat={currencyFormat}
                            lineOpacity={1}
                            lineStroke={priceCoordinateColor}
                        />
                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick.BINA}
                            dx={4}
                            fill={backgroundDark}
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="ShortDash"
                            displayFormat={currencyFormat}
                            lineOpacity={1}
                            lineStroke={priceCoordinateColor}
                        />
                        <PriceCoordinate
                            at="right"
                            orient="right"
                            price={lastTick.GMNI}
                            dx={4}
                            fill={backgroundDark}
                            textFill={priceCoordinateColor}
                            arrowWidth={7}
                            strokeDasharray="ShortDash"
                            displayFormat={currencyFormat}
                            lineOpacity={1}
                            lineStroke={priceCoordinateColor}
                        />
                        <CrossHairCursor stroke={'#FFFFFFFF'} />


                    </Chart>
                </ChartCanvas>
            </div>
        );
    }
}

BBOChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
    signals: PropTypes.any,
};

BBOChart.defaultProps = {
    type: "hybrid",
};

// eslint-disable-next-line no-class-assign
BBOChart = fitWidth(BBOChart);

export default BBOChart;
