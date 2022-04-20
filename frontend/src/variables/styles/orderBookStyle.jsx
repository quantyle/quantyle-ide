import {
  backgroundLight,
  dangerColor,
  successColor,
  backgroundDark,
} from "../styles";

const rowHeight = "1.95vh";

const orderBookStyle = {
  book: {
    background: backgroundDark,
    height: '41.5vh',
    overflowY: "scroll",
  },
  history: {
    background: backgroundDark,
    // borderBottom: '0.5vh solid ' + backgroundLight,
    height: '46vh',
    overflowY: "scroll",
  },
  historyWrapper: {
    // background: backgroundDark,
    // borderBottom: '0.5vh solid ' + backgroundLight
  },
  header: {
    height: '1.5vh',
    padding: '0vh',
    paddingTop: '0.25vh',
    color: '#fff',
    display: 'flex',
    overflow: 'hidden',
    background: backgroundDark,
    
  },
  rows: {
    display: 'table-cell', 
    width: '11vw',
  },
  row: {
    position: 'relative',
    display: 'flex',
    overflow: 'hidden',
    // padding: '0.4vh 0px',
    height: rowHeight,
    verticalAlign: "middle",
    cursor: 'pointer',
    '&:hover': {
        background: backgroundLight,
    },
    
  },
  rowAsk: {
    position: 'relative',
    display: 'flex',
    overflow: 'hidden',
    //paddingRight: '14px',
    // padding: '0.4vh 0px',
    height: rowHeight,
    verticalAlign: "middle",
    cursor: 'pointer',
    color: dangerColor,
    '&:hover': {
        //border: '1px solid ' + primaryColor + ' !important',
        background: backgroundLight,
    },
    
  },
  rowBid: {
    position: 'relative',
    display: 'flex',
    overflow: 'hidden',
    height: rowHeight,
    verticalAlign: "middle",
    //paddingRight: '14px',
    // padding: '0.3vh 0px',
    cursor: 'pointer',
    color: successColor,
    '&:hover': {
        //border: '1px solid ' + primaryColor + ' !important',
        background: backgroundLight,
    },
    
  },
  spread: {
    position: 'relative',
    display: 'flex',
    overflow: 'hidden',
    color: '#fff',
    padding: '0.25vh 0vh',
    
  },
  size: {
    flex: 1,
    overflow: 'hidden',
    textAlign: 'left',
    paddingLeft: "0.7vh"
  },
  price: {
    flex: 1,
    overflow: 'hidden',
    textAlign: 'right',
    color: "#ddd !important",
    paddingRight:  "0.7vh"
  },
  spreadText: {
    flex: 1,
    overflow: 'hidden',
    textAlign: 'right',
    color: "#ddd !important",
    paddingTop: "0.25vh",
    paddingRight:  "0.7vh"
  },
  askVolume: {
    background: dangerColor + '40',
    position: 'absolute',
    top: '10%',
    left: 0,
    padding: "0.35vh",
  },
  bidVolume: {
    background: successColor + '40',
    position: 'absolute',
    top: '10%',
    left: 0,
    padding: "0.35vh",
  },
  volume: {
    background: successColor + '40',
    position: 'absolute',
    top: '10%',
    left: 0,
    padding:  "0.35vh",
    
  },
  arrowUp: {
    color: successColor, height: "1.5vh" 
  },
  arrowDown: {
    color: dangerColor, height: "1.5vh" 
  },
};

export default orderBookStyle;