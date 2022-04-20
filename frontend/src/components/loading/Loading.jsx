import React from "react";
import PropTypes from "prop-types";
import { withStyles } from "@material-ui/core";
import loadingStyle from '../../variables/styles/loadingStyle';
import MyImageSvg from '../../logo.svg';


function Loading({ ...props }) {
  const {
    classes,
    status,
  } = props;

  return (
    <div className={classes.root}>
      <img className={classes.img} src={MyImageSvg} alt='' />
      <div className={classes.console}>
        {status}
      </div>
    </div>
  );
}
Loading.propTypes = {
  classes: PropTypes.object,
};

export default withStyles(loadingStyle)(Loading);
