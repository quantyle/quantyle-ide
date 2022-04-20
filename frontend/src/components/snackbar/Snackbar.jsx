import React from 'react';
import PropTypes from 'prop-types';
import clsx from 'clsx';
import {
  Snackbar as MUISnackbar,
  SnackbarContent,
  IconButton,
} from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import classNames from 'classnames';
import {
  Close,
  Error,
  CheckCircle,
  Info,
  Warning,
} from '@material-ui/icons';
import {
  successColor,
  dangerColor,
  primaryColor,
  warningColor,
  successColorShadow,
} from '../../variables/styles';

const variantIcon = {
  success: CheckCircle,
  warning: Warning,
  error: Error,
  info: Info,
};

const useStyles1 = makeStyles(theme => ({
  success: {
    backgroundColor: successColorShadow + "20",
    border: "1px solid " + successColor,
  },
  error: {
    backgroundColor: dangerColor + "20",
    border: "1px solid " + dangerColor,
  },
  info: {
    backgroundColor: primaryColor + "20",
    border: "1px solid " + primaryColor,
  },
  warning: {
    backgroundColor: warningColor + "20",
    border: "1px solid " + warningColor,
  },
  icon: {
    fontSize: 20,
  },
  iconVariant: {
    opacity: 0.9,
    marginRight: 10,
  },
  message: {
    display: 'flex',
    alignItems: 'center',
  },
}));

function Snackbar(props) {
  const classes = useStyles1();
  const { className, open, message, onClose, variant, ...other } = props;
  const Icon = variantIcon[variant];
  return (
    <MUISnackbar
      open={open}
      autoHideDuration={4000}
      onClose={onClose}
      anchorOrigin={{
        vertical: 'top',
        horizontal: 'center',
      }}
      ContentProps={{
        'aria-describedby': 'message-id',
      }}
    >
      <SnackbarContent
        className={clsx(classes[variant], className)}
        aria-describedby="client-snackbar"
        message={
          <span id="client-snackbar" className={classes.message}>
            <Icon className={classNames(classes.icon, classes.iconVariant)} />
            {message}
          </span>
        }
        action={[
          <IconButton key="close" aria-label="close" color="inherit" onClick={onClose}>
            <Close />
          </IconButton>
        ]}
        {...other}
      />
    </MUISnackbar>
  );
}

Snackbar.propTypes = {
  className: PropTypes.string,
  message: PropTypes.string,
  onClose: PropTypes.func,
  variant: PropTypes.oneOf(['error', 'info', 'success', 'warning']).isRequired,
};

export default Snackbar;