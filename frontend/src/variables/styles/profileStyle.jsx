import {
    gridItemLeft,
    gridItemRight,
    gridItemCenter,
    root,
    icon,
    coinImg,
    menuRoot,
    backgroundDark,
    backgroundLight,
    primaryColor,
    backgroundPrimary,
} from "../styles";


const profileStyle = {
    root,
    icon,
    gridItemLeft,
    gridItemRight,
    gridItemCenter,
    coinImg,

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
        height: 20,
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
        height: '40vh'
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
};

export default profileStyle;