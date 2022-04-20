
import {
    withStyles,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import listItemStyle from '../../variables/styles/listItemStyle.jsx';
import cx from 'classnames';
import {
    standardFormat,
} from "../../variables/global";

function ListItem({ ...props }) {

    const {
        classes,
        label,
        onClick,
        imgLeft,
        iconLeft,
        iconRight,
        button,
        header,
        active,
        borderBottom,
        borderTop,
        buttons,
        textRight,
        marginTop,
        marginBottom,
    } = props;

    const listItemClass = cx({
        [classes.root]: true,
        [classes.button]: button,
        [classes.header]: header,
        [classes.active]: active,
        [classes.borderTop]: borderTop,
        [classes.borderBottom]: borderBottom,
        [classes.marginTop]: marginTop,
        [classes.marginBottom]: marginBottom,
    });

    return (
        <div
            button={button}
            onClick={onClick}
            className={listItemClass}
        >

            {iconLeft && <props.iconLeft className={classes.icon}/>}
            {imgLeft &&
                <img src={imgLeft} alt='' className={classes.icon} />
            }
            <div className={classes.label}>{label}</div>
            {iconRight &&
                <div className={classes.iconRight}>
                    {iconRight}
                </div>
            }
            {
                textRight && <div className={classes.labelRight}>{standardFormat(textRight)}</div>
            }
            {buttons.map(item =>
                <div className={classes.buttonRight}>
                    {item}
                </div>)
            }
        </div>
    );
}

ListItem.propTypes = {
    label: PropTypes.any,
    classes: PropTypes.object.isRequired,
    onClick: PropTypes.func,
    iconLeft: PropTypes.any,
    iconRight: PropTypes.any,
    button: PropTypes.bool,
    active: PropTypes.bool,
    textRight: PropTypes.string,
};

ListItem.defaultProps = {
    buttons: [],
}

export default withStyles(listItemStyle)(ListItem);

