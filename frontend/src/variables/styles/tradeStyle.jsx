import {
    backgroundDark,
    // root,
    icon,
    coinImg,
    menuRoot,
    menuIconBtn,
    // defaultFont,
    // backgroundPrimary,
    backgroundLight,
    // backgroundLight,
} from "../styles";


const tradeStyle = {
    icon,
    coinImg,
    menuIconBtn,
    menuRoot,
    appbarIcon: {
        display: 'flex',
        marginRight: 10
    },
    appBarTicker: {
        color: '#ffffff88',
        marginLeft: 15
    },
    iconBtn: {
        maxHeight: "1.75vh",
    },
    console: {
        background: backgroundDark,
        color: "#fff",
        padding: "1vh 1vh",
        // height: "15.5vh",
        height: "25vh",

        borderBottom: "0.5vh solid " + backgroundLight,
    },
    orderBtns: {
        padding: "0.5vh 0vh",
        background: backgroundLight,
        height: "9.5vh",
    },
    padding: {
        // padding: "0.5vh 0.5vh 0vh 0.5vh", 
        background: backgroundDark,
        padding: "1vh"
        // height: "3vh",
    },
    portfolio: {
        background: backgroundDark,
        height: "52vh",
        overflow: "scroll",
        // borderBottom: "0.5vh solid " + backgroundLight,
        // padding: "1vh 1vh 0vh 1vh",
    },
    errorMarker: {
        color: "red"
    },

    buttons: {
        padding: "1vh",
        background: backgroundDark,
      },
      orderForm: {
        height: "96.5vh",
        background: backgroundDark,
        overflow: "scroll",
      },
};

export default tradeStyle;
