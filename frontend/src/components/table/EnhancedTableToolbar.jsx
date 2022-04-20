import React from 'react';
import clsx from 'clsx';
import {
    Toolbar,
    Typography,
    Tooltip,
    IconButton,
    withStyles,
    Icon,
} from '@material-ui/core';
import PropTypes from 'prop-types';
import enhancedTableToolbarStyle from '../../variables/styles/enhancedTableToolbarStyle';
import classNames from 'classnames';

const EnhancedTableToolbar = ({ ...props }) => {

    const { classes, numSelected } = props;

    return (
        <Toolbar
            className={clsx(classes.root, {
                [classes.highlight]: numSelected > 0,
            })}
        >
            <div className={classes.title}>
                <Typography variant="h6" id="tableTitle">
                    Assets
                </Typography>
            </div>
            <div className={classes.actions}>
                {numSelected > 0 ? (
                    <Tooltip title="Delete">
                        <IconButton aria-label="Delete">
                        <Icon className={classNames('fas fa-trash-alt')} />
                        </IconButton>
                    </Tooltip>
                ) : (
                        <Tooltip title="Filter list">
                            <IconButton aria-label="Filter list">
                            <Icon className={classNames('fas fa-filter')} />
                            </IconButton>
                        </Tooltip>
                    )}
            </div>
        </Toolbar>
    );
};

EnhancedTableToolbar.propTypes = {
    numSelected: PropTypes.number.isRequired,
};
export default withStyles(enhancedTableToolbarStyle)(EnhancedTableToolbar);