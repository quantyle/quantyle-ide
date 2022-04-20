import React from "react";
import PropTypes from "prop-types";
import portfolioStyle from "../../variables/styles/portfolioStyle.jsx";
import {
    withStyles,
} from "@material-ui/core";
import {
    ListItem
} from '../../components';
import {
    standardFormat,
    exchangeNames,
} from "../../variables/global";
import {
    FolderOpen
} from '@material-ui/icons';

const Portfolio = ({ ...props }) => {

    const {
        classes,
        portfolio,
        exchange_id,
        product_id,
        iconsUrl,
        allTicks,
        handleProductClick
    } = props;


    return (
        <div>
            <ListItem
                header
                marginTop
                label="Portfolio"
                iconLeft={FolderOpen}
            />
            <div className={classes.root}>
                {Object.keys(portfolio).map((exchange =>
                    <div>
                        <ListItem
                            // header
                            label={exchangeNames[exchange]}
                            imgLeft={iconsUrl + exchange.toLowerCase() + '.png'}
                        />
                        {portfolio[exchange].map(prod =>
                            <ListItem
                                imgLeft={iconsUrl + prod.split("-")[0].toLowerCase() + ".png"}
                                button
                                active={exchange === exchange_id && prod === product_id}
                                label={prod}
                                iconRight={standardFormat(allTicks[exchange + "-" + prod].price)}
                                onClick={() => handleProductClick(exchange, prod)}
                            />
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
}

Portfolio.propTypes = {
    classes: PropTypes.object.isRequired,
    name: PropTypes.string,
    routes: PropTypes.any,
};

export default withStyles(portfolioStyle)(Portfolio);
