
import React from "react";
import {
    withStyles,
    ListItemText,
    ListItem as MUIListItem,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import listItemStyle from '../../variables/styles/listItemStyle';
import cx from 'classnames';
import { format } from "d3-format";




function CoinItem({ ...props }) {

    const {
        classes,
        account,
        onClick,
        active,
    } = props;

    const listItemClass = cx({
        [classes.active]: active,
        [classes.root]: true,
    });

    if (account) {
        // const imageUrl = iconsUrl + account.currency.toLowerCase() + '.png'
        const numberFormat = format(account.currency === 'USD' ? ".2f" : ".10f");

        return (
            <MUIListItem
                button
                onClick={onClick}
                className={listItemClass}
            >
                {/* <img src={imageUrl} className={classes.logo} alt='' /> */}
                <ListItemText
                    primary={account.currency}
                    classes={{
                        root: classes.text,
                        primary: classes.primary,
                    }}
                />
                <ListItemText
                    classes={{
                        primary: classes.available,
                    }}
                    primary={numberFormat(account.available)}
                />
            </MUIListItem>
        );
    } else {
        return null;
    }

}

CoinItem.propTypes = {
    classes: PropTypes.object.isRequired,
    onClick: PropTypes.func,
    item: PropTypes.any,
    active: PropTypes.bool,
};

export default withStyles(listItemStyle)(CoinItem);
