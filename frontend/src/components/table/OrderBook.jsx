import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import { format } from "d3-format";
import orderBookStyle from '../../variables/styles/orderBookStyle';
import {
    ListItem,
    // Button,
} from '../../components';
import { ArrowDownward, ArrowUpward } from '@material-ui/icons';

const priceFormat = format(".5f");
function OrderBook({ ...props }) {
    const {
        book,
        onClick,
        classes
    } = props;

    const askVolumeWidth = key => String(((book.asks[key][1] / book.volume) * 300)) + '%';
    const bidVolumeWidth = key => String(((book.bids[key][1] / book.volume) * 300)) + '%'
    const spread = book.asks[book.asks.length - 1][0] - book.bids[0][0];
    // const mid = (book.asks[book.asks.length - 1][0] + book.bids[0][0]) / 2;

    return (
        <div>
            <ListItem header label="Order Book" />
            <div className={classes.header}>
                <div className={classes.size}>
                    Size
                </div>
                <div className={classes.price}>
                    Price
                </div>
            </div>
            <div className={classes.book}>
                <div className={classes.rows} >
                    {book.asks.map((order, key) =>
                        <div
                            key={key}
                            className={classes.rowAsk}
                            onClick={() => onClick(order)}>
                            <div
                                className={classes.askVolume}
                                style={{ width: askVolumeWidth(key) }}
                            />
                            <div className={classes.size}>
                                {priceFormat(order[1])}
                            </div>
                            <div className={classes.price}>
                                {priceFormat(order[0])}
                            </div>
                        </div>
                    )}
                    <div className={classes.spread}>
                        <div className={classes.size}>
                            {book.bids[0][1] > book.asks[book.asks.length - 1][1] ?
                                <ArrowUpward className={classes.arrowUp} /> :
                                <ArrowDownward className={classes.arrowDown} />}
                        </div>
                        <div className={classes.spreadText}>
                            {priceFormat(spread)}
                        </div>
                    </div>
                    {book.bids.map((order, key) =>
                        <div
                            key={key}
                            className={classes.rowBid}
                            onClick={() => onClick(order)}>
                            <div
                                className={classes.bidVolume}
                                style={{ width: bidVolumeWidth(key) }}
                            />
                            <div className={classes.size}>
                                {priceFormat(order[1])}
                            </div>
                            <div className={classes.price}>
                                {priceFormat(order[0])}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>

    );
}


OrderBook.propTypes = {
    classes: PropTypes.object.isRequired,
    book: PropTypes.object.isRequired,
    scrollRef: PropTypes.any,
};

export default withStyles(orderBookStyle)(OrderBook);
