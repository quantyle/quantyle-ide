import React from "react";
import {
    TextField as TextFieldMUI,
    withStyles,
    // Grid,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import textFieldStyle from '../../variables/styles/textFieldStyle.jsx';
// import { GridItem } from "../index.js";
// import {
//     alpha,
// } from '@material-ui/core/styles';
// import InputBase from '@material-ui/core/InputBase';
// const BootstrapInput = withStyles((theme) => ({
//     root: {
//         'label + &': {
//             marginTop: theme.spacing(3),
//         },
//     },
//     input: {
//         borderRadius: 4,
//         position: 'relative',
//         backgroundColor: theme.palette.common.white,
//         border: '1px solid #ced4da',
//         fontSize: 16,
//         width: 'auto',
//         padding: '10px 12px',
//         transition: theme.transitions.create(['border-color', 'box-shadow']),
//         // Use the system font instead of the default Roboto font.
//         fontFamily: [
//             '-apple-system',
//             'BlinkMacSystemFont',
//             '"Segoe UI"',
//             'Roboto',
//             '"Helvetica Neue"',
//             'Arial',
//             'sans-serif',
//             '"Apple Color Emoji"',
//             '"Segoe UI Emoji"',
//             '"Segoe UI Symbol"',
//         ].join(','),
//         '&:focus': {
//             boxShadow: `${alpha(theme.palette.primary.main, 0.25)} 0 0 0 0.2rem`,
//             borderColor: theme.palette.primary.main,
//         },
//     },
// }))(InputBase);

function TextField({ ...props }) {
    const {
        children,
        classes,
        label,
        value,
        active,
        onToggle,
        select,
        ...rest
    } = props;


    return (
        <div className={classes.formControl}>
            <div className={classes.label}>
                {label}
            </div>
            <TextFieldMUI
                variant="outlined"
                InputProps={{
                    classes: {
                        input: classes.input,
                        notchedOutline: classes.notchedOutline,
                        focused: classes.focusedInput,
                        multiline: classes.multiline
                    },
                }}
                value={value}
                select={select}
                SelectProps={{
                    classes: {
                        icon: classes.selectIcon,
                        root: classes.select,
                        // outlined: classes.select
                    },
                    MenuProps: {
                        classes: {
                            paper: classes.menu,
                            list: classes.menuList,
                        }
                    }
                }}
                {...rest}
            >
                {children}
            </TextFieldMUI>

        </div>
    );

}

TextField.propTypes = {
    classes: PropTypes.object.isRequired,
    label: PropTypes.any,
    value: PropTypes.any,
};

export default withStyles(textFieldStyle)(TextField);
