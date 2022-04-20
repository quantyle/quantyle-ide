import {
    backgroundDark,
    backgroundLight,
    defaultFont,
  } from "../styles";
  

  
  const modalStyle = theme => ({
    paper: {
        position: 'absolute',
        height: "40vh",
        overflow: 'scroll',
        width: "20%",
        backgroundColor: backgroundDark,
        border: '0.1vh solid ' + backgroundLight,
        boxShadow: theme.shadows[5],
        padding: theme.spacing(2, 4, 3),
        top: '10%',
        left: '45%',
        color: "#fff",
        ...defaultFont,
      },
      button: {
          padding: "0.5vh"
      }
  });
  
  export default modalStyle;
  