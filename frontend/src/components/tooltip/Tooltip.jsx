import React from "react";
import {
    Tooltip as TooltipMUI,
    Typography,
    withStyles,
    Fade
} from "@material-ui/core";
import PropTypes from 'prop-types';
import tooltipStyle from '../../variables/styles/tooltipStyle';
import cx from 'classnames';

function Tooltip({ ...props }) {
    const {
        classes,
        children,
        title,
        placement,
        hide,
        ...rest
    } = props;


    const tooltipClasses = cx({
        [classes.tooltip]: !hide,
        [classes.tooltipHidden]: hide,
    });


    return (
        <TooltipMUI
            TransitionComponent={Fade}
            enterDelay={100}
            placement={placement}
            title={
                <Typography className={classes.tooltipText}>
                    {title}
                </Typography>
            }
            classes={{
                tooltip: tooltipClasses
            }}
            {...rest}
        >
            {children}
        </TooltipMUI>
    );
}

Tooltip.propTypes = {
    classes: PropTypes.object.isRequired,
    children: PropTypes.node,
    title: PropTypes.string,
    placement: PropTypes.string,

};

export default withStyles(tooltipStyle)(Tooltip);
