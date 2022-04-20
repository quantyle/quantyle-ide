import {
    withStyles
} from "@material-ui/core";
import PropTypes from 'prop-types';
import buttonStyle from '../../variables/styles/buttonStyle.jsx';
import cx from 'classnames';

function Button({ ...props }) {

    const {
        children,
        classes,
        full,
        primary,
        primaryActive,
        dark,
        secondary,
        secondaryActive,
        danger,
        dangerActive,
        plain,
        disabled,
        active,
        onClick,
    } = props;

    const btnClass = cx({
        [classes.base]: true,
        [classes.full]: full,
        [classes.primary]: primary && !primaryActive,
        [classes.secondary]: secondary && !secondaryActive,
        [classes.danger]: danger && !dangerActive,
        [classes.primaryActive]: primaryActive,
        [classes.secondaryActive]: secondaryActive,
        [classes.dangerActive]: dangerActive,
        [classes.dark]: dark,
        [classes.disabled]: disabled,
        [classes.plain]: plain,
        [classes.active]: active,
    });


    return (
        <div onClick={onClick} className={btnClass}>
            {children}
        </div>
    );
}

Button.propTypes = {
    children: PropTypes.node,
    title: PropTypes.string,
};

export default withStyles(buttonStyle)(Button);
