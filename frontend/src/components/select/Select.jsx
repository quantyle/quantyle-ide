import {
    TextField as TextFieldMUI,
    withStyles,
    MenuItem,
    Typography,
    FormControl,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import textFieldStyle from '../../variables/styles/textFieldStyle.jsx';


function Select({ ...props }) {
    const {
        children,
        classes,
        label,
        value,
        active,
        menuItems,
        ...rest
    } = props;


    return (
        <TextFieldMUI
            variant="outlined"
            InputProps={{
                classes: {
                    input: classes.inputSelect,
                    notchedOutline: classes.notchedOutline,
                    focused: classes.focusedInput,
                    // multiline: classes.multiline
                },
            }}
            value={value}
            select
            SelectProps={{
                classes: {
                    icon: classes.selectIcon,
                    root: classes.inputSelect,
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
            {menuItems.map((interval, key) => (
                <MenuItem key={key} value={interval}>
                    {interval}
                </MenuItem>
            ))}
        </TextFieldMUI>

    );
}

Select.propTypes = {
    classes: PropTypes.object.isRequired,
    label: PropTypes.any,
    value: PropTypes.any,
};

export default withStyles(textFieldStyle)(Select);
