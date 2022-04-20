import { backgroundLight, backgroundPrimary, backgroundDark } from "../styles";

const listStyle = {
    root: {
        padding: 0,
        background: backgroundDark,
    },
    inner: {
        overflowY: 'scroll',
        borderTop: "2px solid " + backgroundDark
    },
    subheader: {
        color: "#fff",
    }
};

export default listStyle;
