import {
    backgroundLight,
    boxShadow,
} from "../styles";

const headerHeight = '6.5vh';

const cardStyle = {
    action: {
        margin: 0,
    },
    headerTitle: {
        color: "#fff",
        verticalAlign: 'middle',
        fontSize: '1.5rem',
    },
    subheader: {
        color: "#fff",
        verticalAlign: 'middle',
        fontSize: '1.2em',
    },
    headerRoot: {
        background: backgroundLight,
        ...boxShadow,
        height: headerHeight,
        margin: 0,
    },
    disabled: {
        opacity: 0.5
    },
    card: {
        ...boxShadow,
        background: ' linear-gradient(0deg, rgba(46,52,57,1) 65%, rgba(65,70,75,1) 100%)',
        margin: '5px',
        width: 'calc(100% - 10px)',
        height: '100%',        
    },
    content: {
        padding: 0,
        paddingBottom: '0px !important',
        overflow: 'scroll',
        height: '(100% - ' + headerHeight + ')',     
    }
};

export default cardStyle;