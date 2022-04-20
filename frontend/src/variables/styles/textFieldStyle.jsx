import {
  backgroundLight,
  primaryColor,
  backgroundPrimary,
  backgroundDark,
  defaultFont,
} from "../styles";

const textFieldStyle = {
  formControl: {
    display: "flex",
    // width: '100%',
    // height: '3vh',
    padding: "1vh",
    // backgroundColor: backgroundDark,
    
  },
  label: {
    // color: "white",
    // margin: '0.5vh 1vh',
    // //marginLeft: '0.5vh',
    // fontSize: '11px',
    // // 
    color: "#fff",
    // paddingLeft: "1vh",
    paddingBottom: "0.5vh",
    // height: "1vh",
    ...defaultFont
    
  },
  input: {
    // width: '100%',
    color: "#fff",
    overflow: 'hidden',
    whiteSpace: 'nowrap',
    textOverflow: 'ellipses',
    backgroundColor: backgroundLight,
    padding: "0.5vh",
    //fontSize: '12px',
    // borderColor: backgroundLight + ' !important',
    borderWidth: '1px !important',
    borderRadius: '0px !important',
    ...defaultFont,
    '&:hover': {
      background: backgroundLight,
      //border: '1px solid #fff'
    },
  },
  notchedOutline: {
    borderColor: backgroundLight + '00 !important',
    borderWidth: '1px !important',
    borderRadius: '0px !important',
  },
  focusedInput: {
    backgroundColor: backgroundLight,
    "& $notchedOutline": {
      borderColor: primaryColor + ' !important',
      borderWidth: '2px !important',
      borderRadius: '0px !important',
    }
  },
  menu: {
    border: '2px solid ' + backgroundLight,
    background: backgroundPrimary,
    color: "#fff",
  },
  menuList: {
    fontSize: '11px',
  },
  multiline: {
    padding: '0px !important'
  },

  inputSelect: {
    // width: '10vw',
    width: "100%",
    // height: "2vh",
    color: "#fff",
    overflow: 'hidden',
    textAlign: "left",
    // whiteSpace: 'nowrap',
    textOverflow: 'ellipses',
    backgroundColor: backgroundPrimary,
    padding: "0.2vh 0.5vh",
    // verticalAlign: "middle",
    // marginLeft: "auto",
    //fontSize: '12px',
    // borderColor: backgroundLight + ' !important',
    // borderWidth: '1px !important',
    borderRadius: '0px !important',
    '&:hover': {
      background: backgroundLight,
      //border: '1px solid #fff'
    },
    ...defaultFont,
  },
  selectIcon: {
    color: '#fff !important',
    height: "1.5vh",
    width: "1.5vh",
    margin: "0vh",


    // verticalAlign: "middle",
    // marginLeft: "auto",
    top: "0%",
    right: "0%",
    // position: 'relative'
  },

};

export default textFieldStyle;