import React from "react";
import PropTypes from "prop-types";
import { NavLink } from "react-router-dom";
import navbarStyle from "../../variables/styles/navbarStyle.jsx";
import {
    withStyles,
    AppBar,
    Toolbar,
    // Drawer,
    // Hidden,
    // IconButton,
} from "@material-ui/core";
import {
    // ListItem,
    Button,
} from '../../components';
// import {
//     // coinbaseProLogo,
//     logoText,
// } from "../../variables/global.jsx";
// import {
//     Menu,
// } from '@material-ui/icons';
import MyImageSvg from '../../logo.svg';

const Navbar = ({ ...props }) => {

    function activeRoute(routeName) {
        return props.location.pathname.indexOf(routeName) > -1 ? true : false;
    }

    const {
        classes,
        name,
        routes,
        // handleLogoutClick,
        // data,
        // productIndex,
        // recording,
        // onProductClick,
        // menuClick,
        // products,
        // ...rest
    } = props;


    return (
        <div>
            <AppBar position="static" className={classes.appbar}>
                <Toolbar className={classes.toolbar}>
                    <img src={MyImageSvg} className={classes.logo} alt='' />
                    <div className={classes.name}>
                        {name}
                    </div>

                    <div className={classes.iconButtons}>
                        {routes.map((prop, key) =>
                            <NavLink
                                key={key}
                                to={prop.path}
                                className={classes.navLink}
                            >
                                <Button
                                    primary={activeRoute(prop.path)}
                                    plain={!activeRoute(prop.path)}
                                >
                                    {prop.name}
                                </Button>
                            </NavLink>
                        )}
                    </div>
                    {/* desktop only */}
                    {/* <Hidden smDown>
                        <div className={classes.iconButtons}>
                        {links}
                        </div>
                    </Hidden> */}
                    {/* mobile only */}
                    {/* <Hidden smUp>
                        <IconButton
                            className={classes.menuButton}
                            onClick={menuClick}>
                            <Menu />
                        </IconButton>
                    </Hidden> */}
                </Toolbar>
            </AppBar>
            {/* <Drawer
                anchor='right'
                classes={{
                    paper: classes.drawer,
                }}
                {...rest}
            >
                <ListItem
                    className={classes.listItem}
                    onClick={handleLogoutClick}
                    label='Coinbase Pro'
                    iconLeft={
                        <img src={coinbaseProLogo} className={classes.icon} alt='' />
                    }
                />
                {links}
            </Drawer> */}
        </div>
    );
}

Navbar.propTypes = {
    classes: PropTypes.object.isRequired,
    name: PropTypes.string,
    routes: PropTypes.any,
};

export default withStyles(navbarStyle)(Navbar);
