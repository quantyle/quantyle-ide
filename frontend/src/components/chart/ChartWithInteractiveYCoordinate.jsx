import React from "react";
import PropTypes from "prop-types";

import { format } from "d3-format";
import { ChartCanvas, Chart } from "react-stockcharts";
import { CandlestickSeries } from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
	MouseCoordinateY,
} from "react-stockcharts/lib/coordinates";

import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { macd } from "react-stockcharts/lib/indicator";

import { fitWidth } from "react-stockcharts/lib/helper";
import { InteractiveYCoordinate, DrawingObjectSelector } from "react-stockcharts/lib/interactive";
import { getMorePropsForChart } from "react-stockcharts/lib/interactive/utils";
import { head, last, toObject } from "react-stockcharts/lib/utils";
import {
	saveInteractiveNodes,
	getInteractiveNodes,
} from "./Interactiveutils";

function round(number, precision = 0) {
	const d = Math.pow(10, precision);
	return Math.round(number * d) / d;
}


const sell = {
	...InteractiveYCoordinate.defaultProps.defaultPriceCoordinate,
	stroke: "#E3342F",
	textFill: "#E3342F",
    bgFill: '#000000',
	text: "Sell 320",
	edge: {
		...InteractiveYCoordinate.defaultProps.defaultPriceCoordinate.edge,
		stroke: "#E3342F",
        fill: '#000000',
	}
};
const buy = {
	...InteractiveYCoordinate.defaultProps.defaultPriceCoordinate,
	stroke: "#1F9D55",
	textFill: "#1F9D55",
    bgFill: '#000000',
	text: "Buy 120",
	edge: {
		...InteractiveYCoordinate.defaultProps.defaultPriceCoordinate.edge,
		stroke: "#1F9D55",
        fill: '#000000',
	}
};

class CandleStickChartWithInteractiveYCoordinate extends React.Component {
	constructor(props) {
		super(props);
		this.onDragComplete = this.onDragComplete.bind(this);
		this.onDelete = this.onDelete.bind(this);
		this.handleChoosePosition = this.handleChoosePosition.bind(this);
		this.handleSelection = this.handleSelection.bind(this);

		this.saveCanvasNode = this.saveCanvasNode.bind(this);


		this.handleDoubleClickAlert = this.handleDoubleClickAlert.bind(this);

		this.saveInteractiveNodes = saveInteractiveNodes.bind(this);
		this.getInteractiveNodes = getInteractiveNodes.bind(this);


		this.state = {
			enableInteractiveObject: false,
			yCoordinateList_1: [
				{
					...sell,
					yValue: 3849.90,
					id: 'PPfdREA',
					draggable: true,
				},
				// {
				// 	...buy,
				// 	yValue: 50.90,
				// 	id: 'PBTRREA',
				// 	draggable: false,
				// },
				// {
				// 	...sell,
				// 	yValue: 58.90,
				// 	id: 'TTYHFEA',
				// 	draggable: false,
				// },
                {
					...buy,
					yValue: 3859.90,
					id: 'PPBdsREA',
					draggable: true,
				},
			],
			yCoordinateList_3: [],
			showModal: false,
			alertToEdit: {}
		};
	}
	saveCanvasNode(node) {
		this.canvasNode = node;
	}
	handleSelection(interactives, moreProps, e) {
		if (this.state.enableInteractiveObject) {
			const independentCharts = moreProps.currentCharts.filter(d => d !== 2);
			if (independentCharts.length > 0) {
				const first = head(independentCharts);

				const morePropsForChart = getMorePropsForChart(moreProps, first);
				const {
					mouseXY: [, mouseY],
					chartConfig: { yScale },
				} = morePropsForChart;

				const yValue = round(yScale.invert(mouseY), 2);
				const newAlert = {
					...InteractiveYCoordinate.defaultProps.defaultPriceCoordinate,
					yValue,
					id: 'ffgreabdf',
				};
				this.handleChoosePosition(newAlert, morePropsForChart, e);
			}
		} else {
			const state = toObject(interactives, each => {
				return [
					`yCoordinateList_${each.chartId}`,
					each.objects,
				];
			});
			this.setState(state);
		}
	}

	handleChoosePosition(alert, moreProps) {
		const { id: chartId } = moreProps.chartConfig;
		this.setState({
			[`yCoordinateList_${chartId}`]: [
				...this.state[`yCoordinateList_${chartId}`],
				alert
			],
			enableInteractiveObject: false,
		});
	}
	
    handleDoubleClickAlert(item) {
        // console.log('double click')
		// this.setState({
		// 	showModal: true,
		// 	alertToEdit: {
		// 		alert: item.object,
		// 		chartId: item.chartId,
		// 	},
		// });

        const alertToEdit =  {
            alert: item.object,
            chartId: item.chartId,
        }
		const key = `yCoordinateList_${alertToEdit.chartId}`;
		const yCoordinateList = this.state[key].filter(d => {
			return d.id !== alertToEdit.alert.id;
		});
		this.setState({
			showModal: false,
			alertToEdit: {},
			[key]: yCoordinateList
		});


	}


	componentDidMount() {
		//document.addEventListener("keyup", this.onKeyPress);
	}
	componentWillUnmount() {
		//document.removeEventListener("keyup", this.onKeyPress);
	}
	onDelete(yCoordinate, moreProps) {
		this.setState(state => {
			const chartId = moreProps.chartConfig.id;
			const key = `yCoordinateList_${chartId}`;

			const list = state[key];
			return {
				[key]: list.filter(d => d.id !== yCoordinate.id)
			};
		});
	}
	onDragComplete(yCoordinateList, moreProps, draggedAlert) {
		// this gets called on drag complete of drawing object
		const { id: chartId } = moreProps.chartConfig;

		const key = `yCoordinateList_${chartId}`;
		const alertDragged = draggedAlert != null;

		this.setState({
			enableInteractiveObject: false,
			[key]: yCoordinateList,
			showModal: alertDragged,
			alertToEdit: {
				alert: draggedAlert,
				chartId,
			},
			originalAlertList: this.state[key],
		});
	}
	
	render() {
		const macdCalculator = macd()
			.options({
				fast: 12,
				slow: 26,
				signal: 9,
			})
			.merge((d, c) => {d.macd = c;})
			.accessor(d => d.macd);

		const { type, data: initialData, width, ratio } = this.props;

		const calculatedData = macdCalculator(initialData);
		const xScaleProvider = discontinuousTimeScaleProvider
			.inputDateAccessor(d => d.t);

		const {
			data,
			xScale,
			xAccessor,
			displayXAccessor,
		} = xScaleProvider(calculatedData);

		const start = xAccessor(last(data));
		const end = xAccessor(data[Math.max(0, data.length - 150)]);
		const xExtents = [start, end];

		// console.log(this.state)
		return (
			<div style={{ position: "relative" }}>
				<ChartCanvas ref={this.saveCanvasNode}
					height={600}
					width={width}
					ratio={ratio}
					margin={{ left: 70, right: 70, top: 20, bottom: 30 }}
					type={type}
					seriesName="MSFT"
					data={data}
					xScale={xScale}
					xAccessor={xAccessor}
					displayXAccessor={displayXAccessor}
					xExtents={xExtents}
				>
					<Chart id={1} height={400}
						yExtents={[d => [d.high, d.low]]}
						padding={{ top: 10, bottom: 20 }}
					>
						<XAxis axisAt="bottom" orient="bottom" showTicks={false} outerTickSize={0} />
						<YAxis axisAt="right" orient="right" ticks={5} />
						<MouseCoordinateY
							at="right"
							orient="right"
							displayFormat={format(".2f")} />

						<CandlestickSeries />
						<InteractiveYCoordinate
							ref={this.saveInteractiveNodes("InteractiveYCoordinate", 1)}
							enabled={this.state.enableInteractiveObject}
							onDragComplete={this.onDragComplete}
							onDelete={this.onDelete}
							yCoordinateList={this.state.yCoordinateList_1}
						/>

					</Chart>

					<DrawingObjectSelector
						enabled
						getInteractiveNodes={this.getInteractiveNodes}
						drawingObjectMap={{
							InteractiveYCoordinate: "yCoordinateList"
						}}
						onSelect={this.handleSelection}
						onDoubleClick={this.handleDoubleClickAlert}
					/>
				</ChartCanvas>
			</div>
		);
	}
}


CandleStickChartWithInteractiveYCoordinate.propTypes = {
	data: PropTypes.array.isRequired,
	width: PropTypes.number.isRequired,
	ratio: PropTypes.number.isRequired,
	type: PropTypes.oneOf(["svg", "hybrid"]).isRequired
};

CandleStickChartWithInteractiveYCoordinate.defaultProps = {
	type: "svg"
};

const CandleStickChart = fitWidth(
	CandleStickChartWithInteractiveYCoordinate
);

export default CandleStickChart;








