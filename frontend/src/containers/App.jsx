import React from "react";
import PropTypes from "prop-types";
import {
    Route,
    HashRouter,
} from "react-router-dom";
import appRoutes from "../routes/app.jsx";
import appStyle from "../variables/styles/appStyle";
import {
    withStyles,
} from '@material-ui/core';
import {
    toggleValue,
} from '../variables/global';
import {
    Navbar,
} from '../components';
import { Redirect } from "react-router-dom";

class App extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            drawerOpen: false,
            currentPage: "/trade"
        };
        console.log(props.location.pathname);
        this.activeRouteName = props.location.pathname.replace('/', '').toUpperCase();
    }

    render() {
        const { classes, ...rest } = this.props;
        const { currentPage, } = this.state;
        return (
            <main className={classes.root}>
                <Navbar
                    open={false}
                    name={this.activeRouteName}
                    onClose={() => toggleValue('drawerOpen', this)}
                    menuClick={() => toggleValue('drawerOpen', this)}
                    routes={appRoutes}
                    {...rest}
                />
                <HashRouter>
                    {appRoutes.map((prop, key) =>
                        <Route
                            key={key}
                            path={prop.path}
                            render={(props) =>
                                <prop.component
                                    {...props}
                                />
                            }
                        />
                    )}
                    <Route render={() => <Redirect to={currentPage} />} />
                </HashRouter>
            </main>
        );
    }
}

App.propTypes = {
    classes: PropTypes.object.isRequired,
    location: PropTypes.any,
};

export default withStyles(appStyle)(App);