// import React from "react";
import {
    withStyles,
    Card as MUICard,
    CardHeader,
    CardContent,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import cardStyle from '../../variables/styles/cardStyle.jsx';


function Card({ ...props }) {
    const {
        children,
        classes,
        ...rest
    } = props;

    return (
        <MUICard className={classes.card}>
            <CardHeader
                {...rest}
                classes={{
                    root: classes.headerRoot,
                    title: classes.headerTitle,
                    action: classes.action,
                    subheader: classes.subheader,
                }} />
            <CardContent className={classes.content}>
                {children}
            </CardContent>
        </MUICard>
    );
}

Card.propTypes = {
    children: PropTypes.any,
    classes: PropTypes.object.isRequired,
    disabled: PropTypes.bool,
    fullHeight: PropTypes.bool,

};

export default withStyles(cardStyle)(Card);
