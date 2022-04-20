import { backgroundLight, backgroundDark } from "../styles";


const tooltipStyle = {
    tooltip: {
        backgroundColor: backgroundDark,
        border: '1px solid '  + backgroundLight,
    },
    tooltipHidden: {
        visibility: 'hidden'
    },
    tooltipText: {
        color: "#fff",
    },

};

export default tooltipStyle;
