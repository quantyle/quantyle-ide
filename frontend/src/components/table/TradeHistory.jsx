import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import {
    successColor,
    dangerColor,
} from '../../variables/styles';
import orderBookStyle from '../../variables/styles/orderBookStyle';
import {
    ListItem,
} from '../../components';
import { format } from "d3-format";

const priceFormat = format(".5f");

function TradeHistory({ ...props }) {
    const {
        rows,
        classes
    } = props;

    const totalVolume = rows.reduce((a, v) => a = a + parseFloat(v.size), 0) / 20

    return (
        <div className={classes.historyWrapper}>
            <ListItem header marginTop label="Trade History"/>
            <div className={classes.header}>
                <div className={classes.size}>
                    Size
                </div>
                <div className={classes.price}>
                    Price
                </div>
            </div>
            <div className={classes.history}>
                <div className={classes.rows} >
                    {rows.map((row, key) =>
                        <div
                            key={key}
                            style={{ color: row.side === 'sell' ? dangerColor : successColor }}
                            className={classes.row}>
                                <div
                                    className={classes.volume}
                                    style={{
                                        background: row.side === 'sell' ? dangerColor + '20' : successColor + '30',
                                        width: String(((parseFloat(rows[key].size)) / totalVolume) * 100) + '%',
                                    }}
                                />
                            <div className={classes.size}>
                                {priceFormat(row.size)}
                            </div>
                            <div className={classes.price}>
                                {priceFormat(row.price)}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

TradeHistory.propTypes = {
    rows: PropTypes.any,
    classes: PropTypes.object.isRequired,
};

export default withStyles(orderBookStyle)(TradeHistory);
