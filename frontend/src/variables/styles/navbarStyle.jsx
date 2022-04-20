import {
  backgroundPrimary,
} from "../styles";

const appBarHeight = "2.5vh"


const sidebarStyle = theme => ({
  appbar: {
    background: backgroundPrimary,
    color: "#fff",
    height: appBarHeight,
    width: '100%',
    display: 'flex',
    padding: 0,
    boxShadow: 'none',
  },
  logo: {
    width: 'auto',
    height: "2vh",
    padding: '0px 1vh'
  },
  toolbar: {
    display: 'flex',
    minHeight: appBarHeight,
    padding: 0,
  },
  list: {
    paddingTop: '10px',
    height: '100vh !important'
  },
  navLink: {
    textDecoration: "none",
    marginRight: "0.5vw"
  },
  active: {
    display: "block",
    textDecoration: "none",
    color: "#fff",
    "&:hover,&:focus,&:visited,&": {
      fontWeight: "500 !important",
    },
  },
  recordingLabel: {
    fontSize: '0.9rem',
  },
  iconButtons: {
    marginLeft: 'auto',
    display: 'flex',
    width: "auto"
  },

  logoText: {
    margin: '0',
    lineHeight: '30px',
    marginLeft: '50px',
    marginTop: '14px',
    
  },
  name: {
    paddingTop: '0.2vh',
    paddingLeft: '10px',
    paddingRight: '10px',
    flex: 1,
  },

  menuButton: {
    marginLeft: 'auto',
    color: '#fff'
  },
});

export default sidebarStyle;
