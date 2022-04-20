import React from "react";
import PropTypes from "prop-types";
import { curveMonotoneX } from "d3-shape";
import { ChartCanvas, Chart } from "react-stockcharts";
import { AreaSeries, BarSeries, } from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import { fitWidth } from "react-stockcharts/lib/helper";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { last } from "react-stockcharts/lib/utils";
import { HoverTooltip } from "react-stockcharts/lib/tooltip";
import { format } from "d3-format";
import { timeFormat } from "d3-time-format";
import {
    primaryColorShadow,
    backgroundPrimary,
    primaryColor
} from "../../variables/styles";
import {
    CurrentCoordinate,
} from "react-stockcharts/lib/coordinates";
// import ZoomButtons from "react-stockcharts/lib/ZoomButtons";

const backgroundLight = "#65696D";
const dateFormat = timeFormat("%Y-%m-%d");
const numberFormat = format(".7f");
const volFormat = format(".0f");


function tooltipContent() {
    return ({ currentItem, xAccessor }) => {
        return {
            x: dateFormat(xAccessor(currentItem)),
            y: [
                {
                    label: "open",
                    value: currentItem.open && numberFormat(currentItem.open)
                },
                {
                    label: "high",
                    value: currentItem.high && numberFormat(currentItem.high)
                },
                {
                    label: "low",
                    value: currentItem.low && numberFormat(currentItem.low)
                },
                {
                    label: "close",
                    value: currentItem.close && numberFormat(currentItem.close)
                },
                {
                    label: "volume",
                    value: currentItem.volume && volFormat(currentItem.volume)
                }
            ]
        };
    };
}

class AreaChart extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            width: window.innerWidth,
            height: window.innerHeight,
            suffix: 1
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
        //document.removeEventListener("keyup", this.onKeyPress);
    }

    resetYDomain = () => {
        this.node.resetYDomain();
    }

    handleReset = (event) => {
        this.setState({
            suffix: this.state.suffix + 1
        });
    }

    render() {
        const { data: initialData, type, width, ratio, } = this.props;

        const canvasHeight = this.state.height * 0.45; // canvas height (vh)
        const areaChartHeight = canvasHeight * 0.6; // area chart height (vh)
        const volumeChartHeight = canvasHeight * 0.3; // volume chart height (vh)

        const margin = { left: 60, right: 5, top: 5, bottom: 0 };

        const xScaleProvider = discontinuousTimeScaleProvider
            .inputDateAccessor(d => d.date);
        const {
            data,
            xScale,
            xAccessor,
            displayXAccessor,
        } = xScaleProvider(initialData);

        const start = xAccessor(last(data));
        const end = xAccessor(data[Math.max(0, data.length - 150)]);
        const xExtents = [start, end];


        return (
            <ChartCanvas
                ratio={ratio}
                width={width}
                height={canvasHeight}
                margin={margin}
                seriesName="MSFT"
                data={data}
                type={type}
                xScale={xScale}
                xAccessor={xAccessor}
                displayXAccessor={displayXAccessor}
                xExtents={xExtents}
                //panEvent={false}
                clamp={true}


            >
                <Chart
                    height={areaChartHeight}
                    id={0}
                    yExtents={d => d.close}
                >
                    <defs>
                        <linearGradient id="MyGradient" x1="0" y1="100%" x2="0" y2="0%">
                            <stop offset="0%" stopColor={primaryColor} stopOpacity={0} />
                            <stop offset="80%" stopColor={primaryColor} stopOpacity={0.3} />
                            <stop offset="100%" stopColor={primaryColor} stopOpacity={0.6} />
                        </linearGradient>
                    </defs>
                    <YAxis
                        axisAt="left"
                        orient="left"
                        ticks={6}
                        stroke={backgroundLight}
                        tickStroke={backgroundLight}
                        tickFormat={format(".3f")}

                    />
                    <XAxis
                        axisAt="bottom"
                        orient="bottom"
                        //ticks={6}
                        stroke={backgroundLight}
                        tickStroke="#00000000"
                    />
                    <AreaSeries
                        yAccessor={d => d.close}
                        fill="url(#MyGradient)"
                        stroke={primaryColor}
                        strokeWidth={2}
                        interpolation={curveMonotoneX}
                    //canvasGradient={canvasGradient}
                    />
                    <CurrentCoordinate
                        yAccessor={d => d.close}
                        fill="#fff"
                        r={5}
                    />
                </Chart>
                <Chart
                    id={2}
                    height={volumeChartHeight}
                    yExtents={[d => d.volume]}
                    origin={[0, areaChartHeight]}
                >
                    <YAxis axisAt="left" orient="left"
                        stroke={backgroundLight}
                        tickStroke={backgroundLight}
                        ticks={5}
                        tickFormat={format(".2s")}
                    />
                    <XAxis
                        axisAt="bottom"
                        orient="bottom"
                        ticks={6}
                        stroke={backgroundLight}
                        tickStroke={backgroundLight}
                    />
                    <BarSeries
                        yAccessor={d => d.volume}
                        fill={primaryColorShadow + '88'}
                    />
                    <CurrentCoordinate
                        yAccessor={d => d.volume}
                        fill="#fff"
                        r={5}
                    />
                </Chart>
                <HoverTooltip
                    tooltipContent={tooltipContent()}
                    fontSize={15}
                    fill={backgroundPrimary}
                    bgOpacity={0}
                    opacity={1}
                    stroke={backgroundLight}
                    fontFill="#fff"
                />
            </ChartCanvas>
        );
    }
}


AreaChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
};

AreaChart.defaultProps = {
    type: "svg",
};

// eslint-disable-next-line no-class-assign
AreaChart = fitWidth(AreaChart);

export default AreaChart;