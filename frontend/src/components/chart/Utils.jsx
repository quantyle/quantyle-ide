import { utcParse } from "d3-time-format";

//const parseDate = timeParse("%Y-%m-%d");
const parseDate = utcParse("%Y-%m-%dT%H:%M:%S.%LZ");

const parseData = parse => {
  return function (d) {
    d.date = parse(d.Date);
    d.open = +d.Open;
    d.high = +d.High;
    d.low = +d.Low;
    d.close = +d.Close;
    d.volume = +d.Volume;
    return d;
  };
};

const copyObj = obj => {
  // makes a copy of a JSON object, and passes it as a completely new variable 
  return JSON.parse(JSON.stringify(obj));
};

function calcOrderBooks(sells) {
  var res = [];
  // sells
  for (let i = 0; i < sells.length; i++) {
    if (i > 0) {
      sells[i][2] = sells[i - 1][2] + sells[i][1];
    }
    else {
      sells[i][2] = sells[i][1];
    }
    var dp = [sells[i][0], sells[i][2]];
    res.push(dp);
  }
  return res;
}



export {
  parseDate,
  parseData,
  copyObj,
  calcOrderBooks,
};

