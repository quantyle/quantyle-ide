import {
  primaryColor,
  primaryColorShadow,
  padding,
  paddingTop,
  paddingBottom,
  paddingLeft,
  paddingRight,
  backgroundPrimary,
  backgroundLight,
  backgroundDark,
  dangerColor,
  dangerColorShadow,
  successColor,
  successColorShadow,
} from "../styles";



const buttonStyle = {
  padding,
  paddingTop,
  paddingBottom,
  paddingLeft,
  paddingRight,
  base: {
    borderRadius: '0px !important',
    boxShadow: "none",
    // all: "unset",
    // height: listItemHeight,
    // width: "100%",
    padding: '0.5vh 0vh',
    minWidth: "3vw",
    textAlign: "center",
    // display: "inline-block",
    verticalAlign: "middle",
    // width: "100%",
    // height: "2vh",
    // width: "100%",
    border: "none",
    // margin: "0px !important",
    // textTransform: 'none !important',
    
  },
  full: {
    width: "100%",
  },
  primary: {
    color: "#fff",
    //background: 'linear-gradient(0deg, rgba(46,140,147,1) 0%, rgba(53,184,193,1) 100%)',
    backgroundColor: primaryColorShadow,
    "&:hover": {
      backgroundColor: primaryColor,
    }
  },
  secondary: {

    color: "#fff",
    backgroundColor: successColorShadow + 'aa',
    "&:hover": {
      backgroundColor: successColorShadow,
    }
  },
  danger: {

    color: "#fff",
    //background: 'linear-gradient(0deg, rgba(204,35,35,1) 0%, rgba(224,38,38,1) 100%)',
    backgroundColor: dangerColorShadow + 'aa',
    "&:hover": {
      backgroundColor: dangerColorShadow,
    }
  },
  primaryActive: {

    color: "#fff",
    //background: 'linear-gradient(0deg, rgba(46,140,147,1) 0%, rgba(53,184,193,1) 100%)',
    backgroundColor: primaryColor,
    "&:hover": {
      backgroundColor: primaryColor,
    }
  },
  secondaryActive: {

    color: "#fff",
    backgroundColor: successColor + 'dd',
    "&:hover": {
      backgroundColor: successColor,
    }
  },
  dangerActive: {
    color: "#fff",
    //background: 'linear-gradient(0deg, rgba(204,35,35,1) 0%, rgba(224,38,38,1) 100%)',
    backgroundColor: dangerColor + 'dd',
    "&:hover": {
      backgroundColor: dangerColor,
    }
  },
  dark: {
    color: "#fff",
    //background: 'linear-gradient(0deg, rgba(46,140,147,1) 0%, rgba(53,184,193,1) 100%)',
    // border: '2px solid ' + backgroundLight,
    backgroundColor: backgroundDark,
    "&:hover": {
      backgroundColor: backgroundPrimary,
    }
  },
  disabled: {
    backgroundColor: backgroundDark,
    color: backgroundLight,
    "&:hover": {

    }
  },
  plain: {
    backgroundColor: backgroundPrimary,
    //border: '2px solid ' + backgroundLight,
    color: "#bbb",
    "&:hover": {
      color: "#fff",
      backgroundColor: backgroundLight,
    }
  },
  active: {
    backgroundColor: backgroundLight,
    //border: '2px solid ' + backgroundLight,
    color: "#fff",
    "&:hover": {
      backgroundColor: backgroundLight,
    }
  },

};

export default buttonStyle;