
import React from "react";
import {
    withStyles,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import colorPickerStyle from '../../variables/styles/colorPickerStyle.jsx';
import { HuePicker } from 'react-color';


function ColorPicker({ ...props }) {

    const {
        classes,
        label,
        color,
        onChangeComplete,
    } = props;


    return (
        <div className={classes.root}>
            <div className={classes.label}>
                {label}
            </div>
            <HuePicker
                className={classes.picker}
                color={color}
                onChangeComplete={onChangeComplete}
            />
        </div>
    );
}

ColorPicker.propTypes = {
    classes: PropTypes.object.isRequired,
    children: PropTypes.any,
    label: PropTypes.any,
};

export default withStyles(colorPickerStyle)(ColorPicker);

