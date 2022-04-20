// import React from "react";
import { withStyles, Grid } from "@material-ui/core";
import PropTypes from 'prop-types';
import cx from 'classnames';
import gridItemStyle from '../../variables/styles/gridItemStyle';
// import { backgroundLight } from "../../variables/styles";

function GridItem({ ...props }) {
  const {
    classes,
    children,
    border,
    borderTop,
    borderBottom,
    borderLeft,
    borderRight,
    padding,
    paddingTop,
    paddingBottom,
    paddingLeft,
    paddingRight,
    ...rest
  } = props;

  const gridItemClasses = cx({
    // padding
    // [classes.root]: true,
    [classes.padding]: padding,
    [classes.paddingTop]: paddingTop,
    [classes.paddingBottom]: paddingBottom,
    [classes.paddingLeft]: paddingLeft,
    [classes.paddingRight]: paddingRight,
    [classes.border]: border,
    [classes.borderTop]: borderTop,
    [classes.borderBottom]: borderBottom,
    [classes.borderLeft]: borderLeft,
    [classes.borderRight]: borderRight,
    
  });

  return (
    <Grid
      item
      className={gridItemClasses}
      {...rest}
    >
      {children}
    </Grid>
  );
}

GridItem.propTypes = {
  classes: PropTypes.object.isRequired,
  border: PropTypes.bool,
};

export default withStyles(gridItemStyle)(GridItem);
