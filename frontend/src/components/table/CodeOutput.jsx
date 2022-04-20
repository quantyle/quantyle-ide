import PropTypes from "prop-types";
import { withStyles } from "@material-ui/core/styles";
import { Typography } from "@material-ui/core";
import {
  backgroundDark,
  defaultFont,
} from "../../variables/styles";

const styles = {
  root: {
    color: "#fff",
    // width: "100%",
    height: "15vh",
    overflow: "scroll",
    background: backgroundDark,
    whiteSpace: "pre-line",
    padding: "0.5vh",
    ...defaultFont
  }
};

function CodeOutput({ ...props }) {
  const { classes, output, } = props;
  return (
    <Typography className={classes.root}>
      {output}
    </Typography>
  );
}

CodeOutput.propTypes = {
  rows: PropTypes.any,
  onRowClick: PropTypes.func,
  height: PropTypes.string,
  columns: PropTypes.any,
  selectedIndex: PropTypes.any,
};

export default withStyles(styles)(CodeOutput);
