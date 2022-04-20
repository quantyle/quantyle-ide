
import React from "react";
import PropTypes from "prop-types";
import { format } from "d3-format";
import { timeFormat } from "d3-time-format";
import {
	ChartCanvas,
	Chart
} from "react-stockcharts";
import {
	BarSeries,
	CandlestickSeries,
	// LineSeries,
	// MACDSeries,
} from "react-stockcharts/lib/series";
import {
	XAxis,
	YAxis
} from "react-stockcharts/lib/axes";
import {
	CrossHairCursor,
	CurrentCoordinate,
	// EdgeIndicator,
	// MouseCoordinateX,
	// MouseCoordinateY,
} from "react-stockcharts/lib/coordinates";
import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import {
	// OHLCTooltip,
	// MovingAverageTooltip,
	// MACDTooltip,
	HoverTooltip,
} from "react-stockcharts/lib/tooltip";
// import {
// 	ema,
// 	macd
// } from "react-stockcharts/lib/indicator";
import { fitWidth } from "react-stockcharts/lib/helper";
import {
	// TrendLine,
	DrawingObjectSelector
} from "react-stockcharts/lib/interactive";
import {
	last,
	toObject
} from "react-stockcharts/lib/utils";
import {
	saveInteractiveNodes,
	getInteractiveNodes,
} from "./Interactiveutils";
import {
	successColor,
	// primaryColor,
	dangerColor,
	backgroundPrimary
} from "../../variables/styles";


const backgroundLight = "#65696D";
const dateFormat = timeFormat("%Y-%m-%d %H:%M");
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


class CandlestickChart extends React.Component {

	constructor(props) {
		super(props);

		this.handleSelection = this.handleSelection.bind(this);

		this.saveInteractiveNodes = saveInteractiveNodes.bind(this);
		this.getInteractiveNodes = getInteractiveNodes.bind(this);

		this.saveCanvasNode = this.saveCanvasNode.bind(this);

		this.state = {
			enableTrendLine: false,

			width: window.innerWidth,
			height: window.innerHeight
		};
		this.updateWindowDimensions = this.updateWindowDimensions.bind(this);

	}

	saveCanvasNode(node) {
		this.canvasNode = node;
	}


	componentDidMount() {

		window.addEventListener('resize', this.updateWindowDimensions());
		//document.addEventListener("keyup", this.onKeyPress);
	}

	componentWillUnmount() {
		window.removeEventListener('resize', this.updateWindowDimensions());
	}

	updateWindowDimensions() {
		console.log(window.innerWidth + ' ' + window.innerHeight);
		this.setState({ width: window.innerWidth, height: window.innerHeight });
		//document.removeEventListener("keyup", this.onKeyPress);
	}


	handleSelection(interactives) {
		const state = toObject(interactives, each => {
			return [
				`trends_${each.chartId}`,
				each.objects,
			];
		});
		this.setState(state);
	}


	render() {

		const { type, data: initialData, ratio, width, } = this.props;

		const canvasHeight = this.state.height * 0.35; // canvas height (vh)
		const candleChartHeight = canvasHeight * 0.6; // area chart height (vh)
		const volumeChartHeight = canvasHeight * 0.3; // volume chart height (vh)

		// margin?
		const margin = { left: 15, right: 50, top: 5, bottom: 5 };

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
				ref={this.saveCanvasNode}
				height={canvasHeight}
				width={width}
				ratio={ratio}
				margin={margin}
				type={type}
				seriesName="MSFT"
				data={data}
				xScale={xScale}
				xAccessor={xAccessor}
				displayXAccessor={displayXAccessor}
				xExtents={xExtents}
			>
				<Chart
					id={1}
					height={candleChartHeight}
					yExtents={[d => [d.high, d.low]]}
				>
					<XAxis
						axisAt="bottom"
						orient="bottom"
						stroke={backgroundLight}
						tickStroke={backgroundLight}
						outerTickSize={0}
					/>
					<YAxis
						axisAt="right"
						orient="right"
						stroke={backgroundLight}
						tickStroke={backgroundLight}
					/>
					<CandlestickSeries
						stroke={d => d.close > d.open ? successColor : dangerColor}
						wickStroke={d => d.close > d.open ? successColor : dangerColor}
						fill={d => d.close > d.open ? successColor : dangerColor}
					/>
					<CurrentCoordinate
						yAccessor={d => d.close}
						fill="#fff"
						r={5}
					/>
				</Chart>

				{/* Volume Chart */}
				<Chart
					id={2}
					height={volumeChartHeight}
					yExtents={[d => d.volume]}
					origin={[0, candleChartHeight]}
				>
					<YAxis axisAt="right" orient="right"
						stroke={backgroundLight}
						tickStroke={backgroundLight}
						ticks={5}
						tickFormat={format(".2s")}
					/>
					<BarSeries
						yAccessor={d => d.volume}
						fill={d => d.close > d.open ? successColor + '55' : dangerColor + '55'}
					/>
					<CurrentCoordinate
						yAccessor={d => d.volume}
						fill="#fff"
						r={5}
					/>
				</Chart>
				<CrossHairCursor stroke={'#fff'} />
				<DrawingObjectSelector
					enabled={!this.state.enableTrendLine}
					getInteractiveNodes={this.getInteractiveNodes}
					drawingObjectMap={{
						Trendline: "trends"
					}}
					onSelect={this.handleSelection}
				/>
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

CandlestickChart.propTypes = {
	data: PropTypes.array.isRequired,
	width: PropTypes.number.isRequired,
	ratio: PropTypes.number.isRequired,
	type: PropTypes.oneOf(["svg", "hybrid"]).isRequired,
};

CandlestickChart.defaultProps = {
	type: "svg",
};

const CandleStickChartWithInteractiveIndicator = fitWidth(CandlestickChart);

export default CandleStickChartWithInteractiveIndicator;
