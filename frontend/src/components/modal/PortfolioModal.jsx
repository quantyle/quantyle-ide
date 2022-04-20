import React from 'react';
import PropTypes from 'prop-types';
import clsx from 'clsx';
import {
    Modal,
    withStyles,
} from '@material-ui/core';
import Button from '../button/Button';
import modalStyle from "../../variables/styles/modalStyle";
import {
    ListItem
} from '../../components';
import {
    standardFormat,
    exchangeNames,
} from "../../variables/global";

const PortfolioModal = ({ ...props }) => {
    const {
        classes,
        open,
        onClose,
        // onFileClick,
        exchange_id,
        product_id,
        iconsUrl,
        portfolio,
        allTicks,
        handleProductClick
    } = props;

    return (
        <Modal
            open={open}
            onClose={onClose}
            aria-labelledby="simple-modal-title"
            aria-describedby="simple-modal-description"
        >
            <div className={clsx(classes.paper)}>
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
        </Modal>

    );
}

PortfolioModal.propTypes = {
    className: PropTypes.string,
    message: PropTypes.string,
    onClose: PropTypes.func,
};

export default withStyles(modalStyle)(PortfolioModal);
