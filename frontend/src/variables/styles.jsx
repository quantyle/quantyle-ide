
// main colors
const listItemHeight = "2.5vh";
const primaryColor = "#2EAEE2";
const primaryColorShadow = "#1DA3D7"; // bluish green color, original
const darkColor = "#333333";
const warningColor = "#F79256";
const dangerColor = "#E9401F";
const dangerColorShadow = "#DF3716";
const successColor = "#A6E22E";
const successColorShadow = "#A6E22E";
const infoColor = "#851FDE";

const backgroundLight = "#4A5359";
const backgroundPrimary = "#383E43";
const backgroundDark = "#151719";
const backgroundDarker = "#5D676F";


const boxShadow = {
  boxShadow: "1px 1px 4px " + backgroundDark,
};

const defaultFontSize = "0.9vh";

const defaultFont = {
  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif !important',
  fontSize: defaultFontSize,
};


const appBarTitle = {
  color: "#666",
  height: "30px",
  fontSize: 20,
  flex: 1,
};

const iconButton = {
  borderRadius: "8px",
  color: "#fff",
};

const padding = {
  padding: "0.4h",
};
const paddingTop = {
  paddingTop: "0.4vh",
};

const paddingBottom = {
  paddingBottom: "0.4vh",
};

const paddingLeft = {
  paddingLeft: "0.4vh",
};

const paddingRight = {
  paddingRight: "0.4vh",
};

const icon = {
  color: "#fff",
  width: "1.25vh",
  height: "1.25vh",
  margin: "auto",
};

const coinImg = {
  maxWidth: "2vh",
  maxHeight: "2vh",
};

const menuIconBtn = {
  borderRadius: 0,
  width: 100,
  borderLeft: "2px solid " + backgroundLight,
  color: "#fff",
  "&:hover": {
    background: "transparent",
    color: primaryColor,
  },
};

const menuRoot = {
  background: backgroundDark,
  color: "#fff",
  border: "1px solid " + backgroundPrimary,
};

export {
  primaryColor,
  darkColor,
  warningColor,
  dangerColor,
  dangerColorShadow,
  successColor,
  successColorShadow,
  appBarTitle,
  iconButton,
  defaultFontSize,
  defaultFont,
  backgroundLight,
  backgroundPrimary,
  backgroundDark,
  primaryColorShadow,
  padding,
  paddingTop,
  paddingLeft,
  paddingRight,
  paddingBottom,
  icon,
  coinImg,
  menuIconBtn,
  menuRoot,
  boxShadow,
  infoColor,
  listItemHeight,
  backgroundDarker,
};
