import { backgroundLight } from "../styles";

const iconButtonStyle = {
    root: {
        background: "transparent !important",
        padding: 0, 
        color: "#ccc",
        '&:hover': {
            color: "#fff",
            background: backgroundLight,
            borderRadius: '3px !important',
        }
    },
};

export default iconButtonStyle;