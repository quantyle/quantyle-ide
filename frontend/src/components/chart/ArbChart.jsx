import React from "react";
import PropTypes from "prop-types";
import { format } from "d3-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import {
    LineSeries,
} from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { fitWidth } from "react-stockcharts/lib/helper";
import { last, } from "react-stockcharts/lib/utils";
import {
    successColor,
    dangerColor,
    backgroundDark,
    backgroundLight,
} from "../../variables/styles";

// const dateFormat = timeFormat("%H:%M");
// const numberFormat = format(".6f");


class ArbChart extends React.Component {

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


        const margin = { left: 0, right: 70, top: 6, bottom: 0 };
        const canvasHeight = this.state.height * 0.17; // canvas height (vh)
        const areaChartHeight = canvasHeight * 0.8; // area chart height (vh)

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
        //                     return newDate == d.date.toISOString() && item.type == 'BUY EXECUTED';
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
        // );

        // // sell markers
        // const sellAnnotation = (
        //     <Annotate
        //         with={SvgPathAnnotation}
        //         when={d => {
        //             if (signals) {
        //                 var n = signals.filter(function (item) {
        //                     let newDate = new Date(item.t).toISOString();
        //                     return newDate == d.date.toISOString() && item.type == 'SELL EXECUTED';
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
        // );

        const axisColor = '#ffffff99';
        const priceCoordinateColor = '#ffffff';

        return (
            <div style={{ background: backgroundDark, borderBottom: '2px solid ' + backgroundLight }}>
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
                        yExtents={[d => [d.price, d.GDAX, d.BINA, d.GMNI]]}
                        height={areaChartHeight}
                        origin={[0, 0]}
                    >
                        <LineSeries
                            yAccessor={d => d.price}
                            stroke={priceCoordinateColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d.BINA}
                            stroke={successColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d.GDAX}
                            stroke={dangerColor}
                            strokeWidth={1}
                        />
                        <LineSeries
                            yAccessor={d => d.GMNI}
                            stroke={'#1e96dd'}
                            strokeWidth={1}
                        />
                        <CrossHairCursor stroke={'#FFFFFFFF'} />
                        <YAxis
                            axisAt="right"
                            orient="right"
                            ticks={4}
                            stroke={axisColor}
                            tickStroke={axisColor}
                            tickFormat={format(".2f")}
                        />
                        <XAxis
                            axisAt="bottom"
                            orient="bottom"
                            ticks={10}
                            //tickFormat={timeFormat("%Y-%m-%d, %H:%M:%S %p")}
                            stroke={axisColor}
                            tickStroke={axisColor}
                        />
                    </Chart>
                </ChartCanvas>
            </div>
        );
    }
}

ArbChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
    type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
    signals: PropTypes.any,
};

ArbChart.defaultProps = {
    type: "hybrid",
};

// eslint-disable-next-line no-class-assign
ArbChart = fitWidth(ArbChart);

export default ArbChart;
