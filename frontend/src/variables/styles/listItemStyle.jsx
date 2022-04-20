import {
    icon,
    backgroundPrimary,
    backgroundLight,
    backgroundDark,
    backgroundDarker,
} from "../styles";


const listItemStyle = {
    icon,
    root: {
        color: "#fff",
        minHeight: "2.5vh",
        padding: "0vh 1vh",
        display: "flex",
        flexDirection: "row",
        verticalAlign: "middle",
        background: backgroundDark,
        overflow: "hidden",
        
    },
    productRoot: {
        color: "#fff",
        // height: "2vh",
        paddingLeft: "1vh",
        display: "flex",
        flexDirection: "row",
        verticalAlign: "middle",
        
        '&:hover': {
            background: backgroundLight,
        },
    },
    active: {
        background: backgroundLight,
    },
    header: {
        background: backgroundPrimary
    },
    button: {
        '&:hover': {
            background: backgroundLight,
            cursor: "pointer"
        },
    },
    borderTop: {
        borderTop: '0.5vh solid ' + backgroundDarker,
    },
    borderBottom: {
        borderBottom: '0.5vh solid ' + backgroundDarker,
    },
    marginTop: {
        marginTop: '0.25vh',
    },
    marginBottom: {
        marginBottom: '0.5vh',
    },
    primary: {
        
    },
    secondary: {
        color: "#fff",
        
    },
    iconLeft: {
        height: "1vh",
        width: "1vh !important",
        minWidth: 0,
        minHeight: 0,
        paddingRight: '1vh',
        color: '#fff',
        
    },
    iconRight: {
        minHeight: 0,
        minWidth: 0,
        color: '#fff',
        // paddingLeft: "1vh",
        flex: 2,
        margin: 'auto',
        textAlign: 'right',
        
    },
    priceRight: {
        color: '#fff',
        marginLeft: 'auto',
        
    },
    label: {
        flex: 5,
        margin: "auto",
        paddingLeft: "0.5vw",
        
    },
    labelRight: {
        flex: 0.2,
        margin: "auto",
        textAlign: "right",
        
        
    },
    chart: {
        height: "2vh",
        color: '#fff',
        flex: 4,
        // background: backgroundDark,
        margin: "auto",
        backgroundDark,
        
    },
    buttonRight: {
        flex: 1,
        textAlign: "right",
        margin: "auto"
    },
};

export default listItemStyle;


