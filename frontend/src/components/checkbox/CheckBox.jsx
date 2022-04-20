

import React from "react";
import {
    Checkbox as MUICheckbox,
    withStyles
} from "@material-ui/core";
import PropTypes from 'prop-types';
import checkboxStyle from '../../variables/styles/checkboxStyle.jsx';

function Checkbox({ ...props }) {

    const {
        classes,
        ...rest
    } = props;

    return (
        <MUICheckbox
            classes={{
                root: classes.checkboxRoot,
                checked: classes.checkboxChecked
            }}
            {...rest}
        />
    );
}

Checkbox.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(checkboxStyle)(Checkbox);


