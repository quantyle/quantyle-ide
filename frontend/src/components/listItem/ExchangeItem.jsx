
import React from "react";
import {
    withStyles,
    // ListItem as MUIListItem,
    Typography
} from "@material-ui/core";
import {
    iconsUrl
} from '../../variables/global';
import PropTypes from 'prop-types';
import listItemStyle from '../../variables/styles/listItemStyle';
import {
    ListItem,
  } from "../../components";

function ExchangeItem({ ...props }) {

    const {
        classes,
        item,
        onClick,
    } = props;



    // const changeClass = cx({
    //     [classes.changeNeg]: item.change < 0,
    //     [classes.changePos]: item.change >= 0,
    //     [classes.change]: true
    // });

    const imageUrl = iconsUrl + item.split('-')[0].toLowerCase() + '.png'

    return (
        <ListItem
            button
            onClick={onClick}
        >
            <img src={imageUrl} className={classes.logo} alt='' />
            <Typography classes={{ root: classes.text}}>
                {item}
            </Typography>
            {/* <Typography classes={{ root: classes.text}}>
                {'$' + numberFormat(item.price)}
            </Typography>
            <Typography classes={{
                root: changeClass
            }}>
                {(item.change >= 0 ? '+' : '') + numberFormat(item.change * 100) + "%"
            </Typography> */}
            {/* <ListItemText
                primary=
                classes={{
                    root: classes.text,
                    primary: classes.primary,
                }}
            /> */}


        </ListItem>

    );
}

ExchangeItem.propTypes = {
    classes: PropTypes.object.isRequired,
    onClick: PropTypes.func,
    item: PropTypes.any,
};

export default withStyles(listItemStyle)(ExchangeItem);
