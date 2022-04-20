import React from "react";
import {
    IconButton as IconButtonMUI,
    withStyles
} from "@material-ui/core";
import PropTypes from 'prop-types';
import iconButtonStyle from '../../variables/styles/iconButtonStyle.jsx';

function Button({ ...props }) {

    const {
        classes,
        children,
        ...rest
    } = props;


    return (
        <IconButtonMUI
            classes={{
                root: classes.root
            }}
            {...rest}
        >
            {children}
        </IconButtonMUI>
    );
}

Button.propTypes = {
    classes: PropTypes.object.isRequired,
    children: PropTypes.node,
};

export default withStyles(iconButtonStyle)(Button);





