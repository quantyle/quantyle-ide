import { backgroundDark, backgroundDarker, backgroundLight, defaultFont, } from "../styles";


const appStyle = {
  root: {
    background: backgroundDarker,
    // background: "linear-gradient(180deg, #4040BB 10%, #DE3C4B 100%)",
    // background: "linear-gradient(180deg, #2B3236 10%, #16191B 100%)",
    // background: "#121C2E",
    height: "99.5vh",
    overflow: "hidden",
    borderBottom: "0.5vh solid " + backgroundLight,
    ...defaultFont,
  }
};

export default appStyle;