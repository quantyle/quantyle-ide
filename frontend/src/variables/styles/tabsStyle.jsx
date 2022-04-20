import { primaryColor, } from "../styles";

const tabsStyle = {
    // slider
    badge: {
        margin: 10
    },
    tabs: {
        color: '#fff',
        display: 'flex',
        width: '100%',
        '&:focus': {
            opacity: 1,
        },
    },
    tab: {
        textTransform: 'none !important',
        //borderLeft: '2px solid ' + backgroundLight,
        flex: 1
    },
    indicator: {
        display: 'flex',
        justifyContent: 'center',
        backgroundColor: primaryColor,
    },
};

export default tabsStyle;
