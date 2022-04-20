import {
    TextField,
    withStyles,
    FormControl,
    InputAdornment,
    Typography
} from "@material-ui/core";
import PropTypes from 'prop-types';
import textFieldStyle from '../../variables/styles/textFieldStyle.jsx';
import NumberFormat from 'react-number-format';
import { format } from "d3-format";

function NumberFormatCustom(props) {
    const { inputRef, onChange, ...other } = props;

    return (
        <NumberFormat
            {...other}
            getInputRef={inputRef}
            onValueChange={values => {
                onChange({
                    target: {
                        value: format(".4f")(values.value),
                    },
                });
            }}
            //thousandSeparator
            allowNegative={false}

        //prefix="$ "
        />
    );
}

NumberFormatCustom.propTypes = {
    inputRef: PropTypes.func.isRequired,
    onChange: PropTypes.func.isRequired,
};


function USDFormatCustom(props) {
    const { inputRef, onChange, ...other } = props;

    return (
        <NumberFormat
            {...other}
            getInputRef={inputRef}
            onValueChange={values => {
                onChange({
                    target: {
                        value: format(".2f")(values.value),
                    },
                });
            }}
            //thousandSeparator
            allowNegative={false}
        />
    );
}

USDFormatCustom.propTypes = {
    inputRef: PropTypes.func.isRequired,
    onChange: PropTypes.func.isRequired,
};



function MoneyField({ ...props }) {
    const { classes, label, id, value, onChange, usd, units, ...rest } = props;
    return (
        <FormControl className={classes.formControl}>
            <Typography className={classes.label}>
            {label}
            </Typography>
            <TextField
                fullWidth
                variant="outlined"
                placeholder={'0.00'}
                value={value}
                onChange={onChange}
                autoComplete='off'
                //label
                InputProps={{
                    endAdornment: units && <InputAdornment position="end" variant='filled'><Typography>{units}</Typography></InputAdornment>,
                    classes: {
                        input: classes.input,
                        notchedOutline: classes.notchedOutline,
                        focused: classes.focusedInput,
                        multiline: classes.multiline,

                    },
                    inputComponent: usd ? USDFormatCustom : NumberFormatCustom,
                }}
                {...rest}
            />
        </FormControl>
    );
}

MoneyField.propTypes = {
    classes: PropTypes.object.isRequired,
    title: PropTypes.string,
    label: PropTypes.string,
    value: PropTypes.any,
    id: PropTypes.any,
    onChange: PropTypes.func,
    percent: PropTypes.any,
    unit: PropTypes.any,
};

export default withStyles(textFieldStyle)(MoneyField);
