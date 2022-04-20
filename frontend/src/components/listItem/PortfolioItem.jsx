
import React from "react";
import {
    withStyles,
    ListItemText,
    ListItem as MUIListItem,
    ListItemIcon,
    Chip,
} from "@material-ui/core";
import PropTypes from 'prop-types';
import portfolioItemStyle from '../../variables/styles/portfolioItemStyle';
import cx from 'classnames';



function PortfolioItem({ ...props }) {

    const {
        classes,
        item,
        onClick,
        active,
    } = props;


    const listItemClass = cx({
        [classes.root]: true,
        [classes.active]: active,
    });

    return (
        <div>
            <MUIListItem
                button
                onClick={onClick}
                className={listItemClass}
            >
                <ListItemText primary={item.name} className={classes.text}/>
                <ListItemIcon>
                    <Chip label={item.timestamp} className={classes.chip}/>
                </ListItemIcon>
            </MUIListItem>
        </div>
    );
}

PortfolioItem.propTypes = {
    classes: PropTypes.object.isRequired,
    onClick: PropTypes.func,
    item: PropTypes.any,
    active: PropTypes.bool,
};

export default withStyles(portfolioItemStyle)(PortfolioItem);
