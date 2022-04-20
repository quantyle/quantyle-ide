import { backgroundDark, backgroundLight } from "../styles";

const footerStyle = {
    root: {
        //position: "fixed", 
        // bottom: 0, 
        background: backgroundDark,
        borderTop: "0.5vh solid " + backgroundLight,
        // background: successColorShadow,
        height: "3vh",
        
        width: "100%",
    },
    nav: {
        background: 'transparent', 
        color: "#fff"
    },
    item: {
        color: "#fff"
    },
};

export default footerStyle;