import React from "react";
// import Chart from "react-google-charts";
// import { backgroundDark, primaryColor, backgroundLight, backgroundPrimary } from "../../variables/styles";
import { Doughnut } from 'react-chartjs-2';

// const height = "30vh";

// const pieOptions = {
//     title: "",
//     pieHole: 0.5,
//     slices: [
//         {
//             color: primaryColor,
//         },
//         {
//             color: "#d91e48",
//         },
//         {
//             color: "#007fad"
//         },
//         {
//             color: "#e9a227"
//         }
//     ],
//     legend: {
//         position: "bottom",
//         alignment: "center",
//         textStyle: {
//             color: "ffffff",
//             fontSize: 12
//         },
//         scrollArrows: {
//             color: "#fff",
//             inactiveColor: backgroundPrimary,
//             activeColor: "#fff"
//         },
//         pagingTextStyle: {
//             color: "#fff"
//         },
//     },
//     chartArea: {
//         left: 0,
//         top: 15,
//         width: "100%",
//         height: height,
//         backgroundColor: '#000000',
//         stroke: backgroundLight,
//     },
//     fontName: "Roboto",
//     backgroundColor: backgroundDark,
//     //enableInteractivity: false,
//     pieSliceBorderColor: backgroundDark,

//     //legend: { position: 'bottom', maxLines: 3,  },
// };
function DonutChart({ ...props }) {

    //const { data } = props;
    const data = {
        labels: ['BTC-USD', 'ETH-USD', 'BCH-USD', 'LTC-USD'],
        datasets: [
          {
            label: '# of Votes',
            data: [12, 19, 3, 5,],
            backgroundColor: [
              'rgba(255, 99, 132, 0.2)',
              'rgba(54, 162, 235, 0.2)',
              'rgba(255, 206, 86, 0.2)',
              'rgba(75, 192, 192, 0.2)',
            //   'rgba(153, 102, 255, 0.2)',
            //   'rgba(255, 159, 64, 0.2)',
            ],
            borderColor: [
              'rgba(255, 99, 132, 1)',
              'rgba(54, 162, 235, 1)',
              'rgba(255, 206, 86, 1)',
              'rgba(75, 192, 192, 1)',
            //   'rgba(153, 102, 255, 1)',
            //   'rgba(255, 159, 64, 1)',
            ],
            borderWidth: 1,
          },
        ],
      };

    return (
        <Doughnut data={data} />
    );

}

export default DonutChart;


// import React from "react";
// import Chart from "react-google-charts";
// import { backgroundDark, primaryColor, backgroundLight, backgroundPrimary } from "../../variables/styles";
// import { Doughnut } from 'react-chartjs-2';

// const height = "30vh";

// const pieOptions = {
//     title: "",
//     pieHole: 0.5,
//     slices: [
//         {
//             color: primaryColor,
//         },
//         {
//             color: "#d91e48",
//         },
//         {
//             color: "#007fad"
//         },
//         {
//             color: "#e9a227"
//         }
//     ],
//     legend: {
//         position: "bottom",
//         alignment: "center",
//         textStyle: {
//             color: "ffffff",
//             fontSize: 12
//         },
//         scrollArrows: {
//             color: "#fff",
//             inactiveColor: backgroundPrimary,
//             activeColor: "#fff"
//         },
//         pagingTextStyle: {
//             color: "#fff"
//         },
//     },
//     chartArea: {
//         left: 0,
//         top: 15,
//         width: "100%",
//         height: height,
//         backgroundColor: '#000000',
//         stroke: backgroundLight,
//     },
//     fontName: "Roboto",
//     backgroundColor: backgroundDark,
//     //enableInteractivity: false,
//     pieSliceBorderColor: backgroundDark,

//     //legend: { position: 'bottom', maxLines: 3,  },
// };
// function DonutChart({ ...props }) {

//     //const { data } = props;
//     const data = {
//         labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
//         datasets: [
//           {
//             label: '# of Votes',
//             data: [12, 19, 3, 5, 2, 3],
//             backgroundColor: [
//               'rgba(255, 99, 132, 0.2)',
//               'rgba(54, 162, 235, 0.2)',
//               'rgba(255, 206, 86, 0.2)',
//               'rgba(75, 192, 192, 0.2)',
//               'rgba(153, 102, 255, 0.2)',
//               'rgba(255, 159, 64, 0.2)',
//             ],
//             borderColor: [
//               'rgba(255, 99, 132, 1)',
//               'rgba(54, 162, 235, 1)',
//               'rgba(255, 206, 86, 1)',
//               'rgba(75, 192, 192, 1)',
//               'rgba(153, 102, 255, 1)',
//               'rgba(255, 159, 64, 1)',
//             ],
//             borderWidth: 1,
//           },
//         ],
//       };

//     return (
//         <Doughnut data={data} />
//     );

// }

// export default DonutChart;
