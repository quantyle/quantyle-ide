


import React from "react";
import {
    Dialog as DialogMUI,
    DialogTitle as MuiDialogTitle,
    Typography,
    DialogContent,
    DialogContentText,
    DialogActions,
    withStyles,
} from "@material-ui/core";
import {
} from "../../components";
import PropTypes from 'prop-types';
import dialogStyle from '../../variables/styles/dialogStyle';

const titleStyles = theme => ({
    root: {
        margin: 0,
        padding: theme.spacing(2),
    },
    closeButton: {
        position: 'absolute',
        right: theme.spacing(1),
        top: theme.spacing(1),
    },
});

function Dialog({ ...props }) {
    const {
        open,
        classes,
        onClose,
        title,
        actions,
        children,
        text,
        ...rest
    } = props;


    const DialogTitle = withStyles(titleStyles)(props => {
        const { classes, ...other } = props;
        return (
            <MuiDialogTitle disableTypography className={classes.root} {...other}>
                <Typography variant="h6">{title}</Typography>
            </MuiDialogTitle>
        );
    });


    return (
        <DialogMUI
            open={open}
            onClose={onClose}
            classes={{
                paper: classes.dialogRoot
            }}
            aria-labelledby="form-dialog-title"
            {...rest}
        >
            <DialogTitle
                id="form-dialog-title"
                className={classes.dialogTitle}
                onClose={onClose}>
                {title}
            </DialogTitle>
            <DialogContent>
                {text &&
                    <DialogContentText className={classes.dialogText}>
                        {text}
                    </DialogContentText>
                }
                {children}
            </DialogContent>
            <DialogActions>
                {actions}
            </DialogActions>
        </DialogMUI>

    );
}

Dialog.propTypes = {
    classes: PropTypes.object.isRequired,
    title: PropTypes.string,
    placement: PropTypes.string,

};

export default withStyles(dialogStyle)(Dialog);


