import {
    iconButton,
    backgroundLight,
    backgroundDark,
    root,
    icon,
} from '../styles';

const backtestStyle = {
    root,
    icon,
    iconButton,
    dividerTop: {
        color: '#fff',
    },
    divider: {
        marginTop: '10px',
        marginBottom: '10px',
        color: '#fff',
    },
    listWrapper: {
        background: backgroundDark,
        padding: '10px',
        borderRadius: '5px',
        marginTop: '20px'
    },
    row: {
        height: '100%',
        display: 'flex',
        flexDirection: 'row',
        alignItems: 'center',
        padding: '0 10px',
        //borderBottom: '1px solid #333',
        '&:hover': {
            backgroundColor: backgroundLight
        }
    },
    ticker: {
        display: 'flex',
        //height: '40px',
        minWidth: '90px',
        lineHeight: '40px',
        //textAlign: 'center',
        padding: '0px 10px',
        borderRadius: '6px',
        color: 'white',
        marginRight: '15px',
        fontSize: '1em',
        '&:hover': {

        },
    },
    tickerSymbol: {
        fontWeight: 'bold',
    },
    tickerChange: {
        marginLeft: 'auto',
    },
    name: {
        fontWeight: 'bold',
        color: '#fff',
        whiteSpace: 'nowrap',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
    },
    index: {
        color: '#ddd',
        whiteSpace: 'nowrap',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
    },
    height: {
        flex: 1,
        textAlign: 'right',
        color: '#bdbdbd',
        fontSize: '.75em',
        fontWeight: '100',
    },
    results: {
        background: backgroundLight,
        //width: '100%',
        height: '10vh'
    },
};

export default backtestStyle;
