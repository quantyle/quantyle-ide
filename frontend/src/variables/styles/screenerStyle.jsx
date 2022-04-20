import {
    // gridItemLeft,
    // gridItemRight,
    // gridItemCenter,
    root,
    icon,
    // coinImg,
    menuRoot,
    primaryColor,
    backgroundPrimary,
    backgroundDark,
    backgroundLight,
    // appBarHeight
} from "../styles";


const screenerStyle = {
    root,
    icon,
    // gridItemLeft,
    // gridItemRight: {
    //     overflowY: 'scroll',
    //     display: 'flex',
    //     flexDirection: 'column',
    //     height: 'calc(100vh - ' + appBarHeight + ')',
    // },
    // coinImg,
    menuRoot,
    coinImgSlash: {
        color: '#ffffff44',
        margin: '0 3px',
    },
    appbarIcon: {
        display: 'flex',
        marginRight: 10
    },
    appBarTicker: {
        color: '#ffffff88',
        marginLeft: 15
    },
    img: {
        width: 20,
        height: 'auto'
    },
    // icon
    floatLeft: {
        color: 'red',
        paddingRight: '10px'
    },
    chevron: {
        fontSize: "15px"
    },
    chartWrapper: {
        width: '100%',
        height: '45vh'
    },
    menuIconBtn: {
        //borderRadius: 0,
        width: 50,
        //borderLeft: '2px solid ' + backgroundLight,
        fontSize: '0.9rem',
        color: "#fff",
        '&:hover': {
            background: 'transparent',
            color: primaryColor,
        },
    },
    btnWrapper: {
        background: backgroundPrimary,
    },
    logo: {
        width: 20,
        height: 20,
    },
    searchWrapper: {
        backgroundDark,
        margin: 10,
    },
    assetList: {
        overflow: 'scroll',
        height: '100%',
        border: '2px solid ' + backgroundLight
    },
};

export default screenerStyle;
