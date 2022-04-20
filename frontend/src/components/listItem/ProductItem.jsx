import {
    withStyles,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import listItemStyle from '../../variables/styles/listItemStyle';
import cx from 'classnames';
import { standardFormat } from "../../variables/global";

function ProductItem({ ...props }) {

    const {
        classes,
        label,
        onClick,
        active,
        item,
    } = props;


    const listItemClass = cx({
        [classes.productRoot]: true,
        [classes.active]: active,
    });

    return (
        <div
            // button={true}
            onClick={onClick}
            className={listItemClass}
        >
            <div className={classes.label} >
                {label}
            </div>
            <div className={classes.labelRight}>
                {standardFormat(item.price)}
            </div>
        </div>
    );
}

ProductItem.propTypes = {
    classes: PropTypes.object.isRequired,
    onClick: PropTypes.func,
    item: PropTypes.any,
};

export default withStyles(listItemStyle)(ProductItem);
