import {
    backgroundLight,
    root,
    backgroundPrimary,
    backgroundDark,
    gridItemLeft,
    icon,
    menuIconBtn,
    menuRoot,
    gridItemRight,
    gridItemCenter,
} from '../styles';

const strategiesStyle = {
    root,
    icon,
    gridItemLeft,
    gridItemRight,
    menuIconBtn,
    menuRoot,
    gridItemCenter,
    iconButton: {
        width: '100%',
        borderRadius: '0px !important',
        background: backgroundPrimary,
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
    indicatorId: {
        marginBottom: '10px'
    },
    indicatorHeader: {
        minHeight: "100px",
        maxHeight: 'auto',
        padding: '10px 0px',
        color: '#fff',
    },
    listItem: {
        borderBottom: "4px solid " + backgroundDark,
        background: backgroundPrimary
    },
    form: {
        padding: 15,
    },
    btnWrapper: {
        background: backgroundPrimary,
    },
    row: {
        display: 'flex',
        width: '100%',
    },
    flex: {
        flex: 1,
    },
    list: {
        padding: 0,
    },
    indSettings: {
        height: '100%',
        padding: '10px',
        overflow: 'scroll',

    },
    drawerPaper: {
        background: backgroundDark,
        height: '100vh',
        width: 'calc(100vw / 12 * 3)', 
    }
};

export default strategiesStyle;
