
// import React from "react";
import {
    withStyles,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import footerStyle from '../../variables/styles/footerStyle.jsx';
import { Component } from "react";
import { Redirect } from "react-router-dom";

class Footer extends Component {
    constructor(props) {
        super(props);
        this.state = {
            value: ''
        };

    }

    handleChange = (event, value) => {
        this.setState({
            value,
        });
    }

    handleClickStatus = () => {
        console.log("CLICK");
    }

    render() {
        const {
            classes,
        } = this.props;
        // const n = d.getFullYear();

        return (this.state.value !== '' ? (
            <Redirect to={this.state.value} />
        ) : (
            <div className={classes.root}>

            </div>
        )
        );
    }


}

Footer.propTypes = {
    classes: PropTypes.node,
};

export default withStyles(footerStyle)(Footer);

// // import React from "react";
// import {
//     BottomNavigation,
//     BottomNavigationAction,
//     withStyles,
//     Typography
// } from "@material-ui/core";
// import PropTypes from 'prop-types';
// import footerStyle from '../../variables/styles/footerStyle.jsx';
// import { Component } from "react";
// import { Redirect } from "react-router-dom";

// class Footer extends Component {
//     constructor(props) {
//         super(props);
//         this.state = {
//             value: ''
//         };

//     }

//     handleChange = (event, value) => {
//         this.setState({
//             value,
//         });
//     }

//     render() {
//         const {
//             classes,
//         } = this.props;
//         const d = new Date();
//         // const n = d.getFullYear();

//         return (this.state.value !== '' ? (
//             <Redirect to={this.state.value} />
//         ) : (
//             <footer className={classes.root}>
//                 <BottomNavigation
//                     value={this.state.value}
//                     onChange={this.handleChange}
//                     className={classes.nav}
//                     showLabels
//                 >
//                     <BottomNavigationAction
//                         disabled
//                         label={
//                             <Typography className={classes.item}>
//                                 Crypto Trading Machines
//                             </Typography>
//                         }
//                         value='' />
//                     <BottomNavigationAction
//                         label={
//                             <Typography className={classes.item}>
//                                 Terms of Use
//                             </Typography>
//                         }
//                         value="/terms" />
//                     <BottomNavigationAction
//                         label={
//                             <Typography className={classes.item}>
//                                 Privacy Policy
//                             </Typography>
//                         }
//                         value="/privacy" />
//                 </BottomNavigation>
//             </footer>
//         )
//         );
//     }


// }

// Footer.propTypes = {
//     classes: PropTypes.node,
// };

// export default withStyles(footerStyle)(Footer);
