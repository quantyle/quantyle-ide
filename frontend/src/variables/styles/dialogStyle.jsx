import { 
    backgroundLight, backgroundDark
 } from "../styles";

const dialogStyle = {
    dialogTitle: {
        color: "#fff",
    },
    dialogRoot: {
        background: backgroundDark,
        border: '2px solid ' + backgroundLight,
        maxWidth: '380px'
    },
    dialogText: {
        color: '#fff !important'
    }
};

export default dialogStyle;