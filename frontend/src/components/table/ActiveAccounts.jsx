
import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import {
    successColor,
    dangerColor,
} from '../../variables/styles';
import orderBookStyle from '../../variables/styles/tradeHistoryStyle';
import { format } from "d3-format";

const priceFormat = format(".2f");
const sizeFormat = format(".4f");

function ActiveAccounts({ ...props }) {
    const {
        rows,
        classes
    } = props;

    //@TODO calculate total sizes to fix size/volume bar size
    
    return (
        <div className={classes.book}>

            {/*  HEADER */}
            <div className={classes.header}>
                <div className={classes.column} />
                <div className={classes.column}>
                    Account
                </div>
                <div className={classes.mysize}>
                    Available
                </div>
                <div className={classes.mysize}>
                    Balance
                </div>
                <div className={classes.columnSmall} />
            </div>

            {/*  ASKS */}
            <div className={classes.wrapper} >
                <div className={classes.rows} >
                    {rows.map((row, key) =>
                        <div
                            key={key}
                            style={{color: row.side == 'buy' ? dangerColor : successColor}}
                            className={classes.row}>
                            <div className={classes.column}>
                                <div
                                    className={classes.volume}
                                    style={{
                                        background: row.side == 'buy' ? dangerColor + '33' : successColor + '33',
                                        width: String((rows[key].size) * 100) + '%',
                                    }}
                                />
                            </div>
                            <div className={classes.column}>
                                {'$' + priceFormat(row.price)}
                            </div>
                            <div className={classes.mysize}>
                                {sizeFormat(row.size)}
                            </div>
                            <div className={classes.mysize}>
                                {'-'}
                            </div>
                            <div className={classes.columnSmall} />
                        </div>
                    )}

                </div>
            </div>
        </div>

    );
}

ActiveAccounts.propTypes = {
    classes: PropTypes.object.isRequired,
    scrollRef: PropTypes.any,
};

export default withStyles(orderBookStyle)(ActiveAccounts);
