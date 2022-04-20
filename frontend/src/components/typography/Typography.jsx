import { makeStyles, withStyles } from '@material-ui/core/styles';
import Tabs from '@material-ui/core/Tabs';
import Tab from '@material-ui/core/Tab';
import {
    backgroundDark,
    backgroundPrimary,
    defaultFont,
    primaryColor
} from '../../variables/styles';


const StyledTabs = withStyles({

    scrollable: {
        height: "2vh",
        // borderTop: "2px solid black",        
    },
    indicator: {
        display: 'flex',
        justifyContent: 'center',
        // backgroundColor: primaryColor,
        backgroundColor: 'transparent',
        height: 0,
        margin: 0,
        // // margin: 0,
        // backgroundColor: primaryColor,
        // maxWidth: 40,
        '& > span': {
            maxWidth: 45,
            width: '100%',

            backgroundColor: primaryColor,
        },
    },
    scrollButtons: {
        height: "2vh",
        color: '#fff',
    }
})((props) =>
    <Tabs
        variant="scrollable"
        scrollButtons="auto"
        {...props} TabIndicatorProps={{ children: <span /> }} />);

const StyledTab = withStyles({
    // root: {
    //     textTransform: 'none',
    //     color: '#fff',
    //     //border: '2px solid red',
    //     // background: backgroundLight,
    //     //fontWeight: "regular",
    //     //fontSize: "13px",
    //     marginRight: "2px",
    //     //minHeight: "1.8vh",
    //     '&:focus': {
    //         opacity: 1,
    //     },
    // },
    root: {
        all: "none",
        minHeight: '2vh',
        textTransform: 'none',
        minWidth: 80,
        color: '#fff',
        height: '2vh',
        // border:"1px solid red",

        //height: "2.5vh",
        
        //fontWeight: 400,
        '&:hover': {
            //color: primaryColor,
            opacity: 1,
        },
        '&$selected': {
            //color: primaryColor,
            background: backgroundPrimary,
            //fontWeight: 400,
        },
        '&:focus': {
            //color: primaryColor,
        },
    },
    selected: {
        height: "2vh",
        minHeight: '2vh',
    },

})((props) => <Tab disableRipple {...props} />);

const useStyles = makeStyles(() => ({
    demo: {
        // backgroundColor: backgroundPrimary,
        height: "2vh",
        // minHeight: '2.5vh',
        // fontSize: "0.2vh"
    },
}));

export default function CustomizedTabs({ ...props }) {
    const {
        value,
        tabs,
        handleChange,
        fontSize,
        children
    } = props;

    const classes = useStyles();
    return (
        <div >
            {children}
        </div>
    );
}


// <div className={classes.demo1}>
// <AntTabs value={value} onChange={handleChange} aria-label="ant example">
//     <AntTab label="Tab 1" />
//     <AntTab label="Tab 2" />
//     <AntTab label="Tab 3" />
// </AntTabs>
// <Typography className={classes.padding} />
// </div>

