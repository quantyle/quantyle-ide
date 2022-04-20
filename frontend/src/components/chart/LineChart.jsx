import React from "react";
import PropTypes from "prop-types";
import { curveMonotoneX } from "d3-shape";
import { ChartCanvas, Chart } from "react-stockcharts";
import { AreaSeries, } from "react-stockcharts/lib/series";
import { fitWidth } from "react-stockcharts/lib/helper";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { last } from "react-stockcharts/lib/utils";
import {
    primaryColor
} from "../../variables/styles";


function LineChart({ ...props }) {

    const { data: initialData, } = props;
    const width = window.innerWidth * 0.05;
    const canvasHeight = window.innerHeight * 0.025; // canvas height (vh)
    const lineChartHeight = canvasHeight * 0.60; // area chart height (vh)
    const margin = { left: 1, right: 0, top: 1, bottom: 0 };
    const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(
        (d) => new Date(d[0] * 1000)
    );
    const {
        data,
        xScale,
        xAccessor,
        displayXAccessor,
    } = xScaleProvider(initialData);
    const start = xAccessor(last(data));
    const end = xAccessor(data[Math.max(0, data.length - 150)]);
    const xExtents = [start, end];
    const yExtents = d => d[1];


    return (
        <ChartCanvas
            ratio={1}
            width={width}
            height={canvasHeight}
            margin={margin}
            seriesName="MSFT"
            data={data}
            type="canvas"
            xScale={xScale}
            xAccessor={xAccessor}
            displayXAccessor={displayXAccessor}
            xExtents={xExtents}
            useCrossHairStyleCursor={false}
            mouseMoveEvent={false}
            panEvent={false}
            zoomEvent={false}
            clamp={false}
        >
            <Chart
                height={lineChartHeight}
                id={0}
                yExtents={yExtents}
            >
                <AreaSeries
                    yAccessor={yExtents}
                    fill="#00000000"
                    stroke={primaryColor}
                    strokeWidth={2}
                    interpolation={curveMonotoneX}
                />
            </Chart>
        </ChartCanvas>
    );
}



LineChart.propTypes = {
    data: PropTypes.array.isRequired,
    width: PropTypes.number.isRequired,
    ratio: PropTypes.number.isRequired,
};

// eslint-disable-next-line no-class-assign

export default fitWidth(LineChart);