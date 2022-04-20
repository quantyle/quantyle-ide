import { backgroundDark,  backgroundPrimary } from "../styles";


const loadingStyle = {
    root: {
        background: backgroundDark,
        width: '100%',
        height: '100vh',
        textAlign: "center"
    },
    img: {
        background: "transparent",
        height: "7vw",
        margin: "auto",
        paddingTop: "30vh",
        display: "block",
    },
    console: {
        color: "white",
        background: backgroundPrimary,
        width: '10vw',
        margin: '20px auto',
        padding: "20px",
        
    }
};

export default loadingStyle;
