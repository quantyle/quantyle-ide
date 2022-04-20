import React from "react";
import PropTypes from "prop-types";
import clsx from "clsx";
import { withStyles } from "@material-ui/core/styles";
import { Typography } from "@material-ui/core";
import TableCell from "@material-ui/core/TableCell";
import { AutoSizer, Column, Table } from "react-virtualized";
import {
  backgroundPrimary,
  backgroundDark,
  backgroundLight,
  successColor,
  dangerColor,
  defaultFont,
} from "../../variables/styles";
import { format } from "d3-format";
import { timeFormat } from "d3-time-format";

const numberFormat = format(".6f");

const styles = {
  flexContainer: {
    display: "flex",
    alignItems: "center",
    boxSizing: "border-box",
    ...defaultFont
  },
  headerRow: {
    cursor: "pointer",
    paddingBottom: "0.25vh",
    paddingTop: "0.25vh",
    //background: backgroundD,
    //borderLeft:
  },

  tableRow: {
    cursor: "pointer",
    //borderRadius: '4px',
    //background: backgroundPrimary,
    //borderLeft: '5px solid ' + backgroundPrimary + ' !important',
  },
  tableRowHover: {
    //border: '1px solid ' + backgroundDark + ' !important',
    "&:hover": {
      //border: '1px solid ' + primaryColor + ' !important',
      background: backgroundPrimary,
    },
  },
  tableCell: {
    flex: 1,
    //borderBottom: '1px solid ' + backgroundDark,
    borderBottom: "none",
    color: "#fff",
    
  },
  cellText: {
    overflow: "hidden",
    whiteSpace: "nowrap",
    textOverflow: "ellipsis",
    
  },
  noClick: {
    cursor: "initial",
  },
  img: {
    width: "1vh",
    //maxWidth: 25,
    minWidth: 12,
  },
};

class MuiVirtualizedTable extends React.PureComponent {
  static defaultProps = {
    headerHeight: 28,
    rowHeight: 24,
  };

  getRowClassName = ({ index }) => {
    const { classes, onRowClick } = this.props;

    return clsx(classes.tableRow, classes.flexContainer, {
      [classes.tableRowHover]: index !== -1 && onRowClick != null,
    });
  };

  getHeaderClassName = () => {
    const { classes } = this.props;
    return clsx(classes.headerRow, classes.flexContainer);
  };

  // renders each cell within the Column component
  cellRenderer = ({ cellData, columnIndex }) => {
    const { columns, classes, rowHeight, onRowClick } = this.props;

    if (columns[columnIndex].float) {
      const numberFormat = format(".7f");
      cellData = numberFormat(cellData);
    }

    if (columns[columnIndex].price) {
      cellData = "$" + numberFormat(cellData);
    }

    // process dates
    if (columns[columnIndex].date) {
      const dateFormat = timeFormat("%b %d, %I:%M %p");
      cellData = dateFormat(new Date(cellData));
      //console.log(cellData)
    }

    return (
      <TableCell
        component="div"
        className={clsx(classes.tableCell, classes.flexContainer, {
          [classes.noClick]: onRowClick == null,
        })}
        variant="body"
        style={{ height: rowHeight }}
        align={
          (columnIndex != null && columns[columnIndex].numeric) || false
            ? "right"
            : "left"
        }
      >
        {columns[columnIndex].side ? (
          <Typography
            noWrap
            className={classes.cellText}
            style={
              columns[columnIndex].match
                ? { color: cellData === "buy" ? successColor : dangerColor }
                : { color: cellData === "buy" ? successColor : dangerColor }
            }
          >
            {cellData}
          </Typography>
        ) : (
          <Typography noWrap className={classes.cellText}>
            {cellData}
          </Typography>
        )}
      </TableCell>
    );
  };

  headerRenderer = ({ label, columnIndex }) => {
    const { headerHeight, columns, classes } = this.props;

    return (
      <TableCell
        component="div"
        className={clsx(
          classes.tableCell,
          classes.flexContainer,
          classes.noClick
        )}
        variant="head"
        style={{
          height: headerHeight,
          //background: backgroundDark,
          // borderBottom: '3px solid ' + backgroundDark,
          // borderTop: '3px solid ' + backgroundDark,
        }}
        align={columns[columnIndex].numeric || false ? "right" : "left"}
      >
        <span>{label}</span>
      </TableCell>
    );
  };

  render() {
    const { classes, columns, onRowClick, ...tableProps } = this.props;

    return (
      <AutoSizer>
        {({ height, width }) => (
          <Table
            height={height}
            width={width}
            {...tableProps}
            onRowClick={onRowClick}
            rowClassName={this.getRowClassName}
            headerClassName={this.getHeaderClassName()}
          >
            {columns.map(({ dataKey, ...other }, index) => {
              return (
                <Column
                  key={dataKey}
                  headerRenderer={(headerProps) =>
                    this.headerRenderer({
                      ...headerProps,
                      columnIndex: index,
                    })
                  }
                  className={classes.flexContainer}
                  cellRenderer={this.cellRenderer}
                  dataKey={dataKey}
                  {...other}
                />
              );
            })}
          </Table>
        )}
      </AutoSizer>
    );
  }
}

MuiVirtualizedTable.propTypes = {
  classes: PropTypes.object.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  headerHeight: PropTypes.number,
  onRowClick: PropTypes.func,
  rowHeight: PropTypes.number,
};

const VirtualizedTable = withStyles(styles)(MuiVirtualizedTable);

function ReactVirtualizedTable({ ...props }) {
  const { onRowClick, rows, height, columns, selectedIndex } = props;

  return (
    <div style={{ height: height, width: "100%", background: backgroundDark, }}>
      <VirtualizedTable

        rowCount={rows.length}
        rowGetter={({ index }) => rows[index]}
        onRowClick={onRowClick}
        columns={columns}
        rowStyle={({ index }) => {
          return selectedIndex && selectedIndex.includes(index)
            ? { background: backgroundLight }
            : {};
        }}
      />
    </div>
  );
}

ReactVirtualizedTable.propTypes = {
  rows: PropTypes.any,
  onRowClick: PropTypes.func,
  height: PropTypes.string,
  columns: PropTypes.any,
  selectedIndex: PropTypes.any,
};

export default ReactVirtualizedTable;

// import React from 'react';
// import PropTypes from 'prop-types';
// import clsx from 'clsx';
// import { withStyles } from '@material-ui/core/styles';
// import { Typography } from '@material-ui/core';
// import TableCell from '@material-ui/core/TableCell';
// import {
//     AutoSizer,
//     Column,
//     Table
// } from 'react-virtualized';
// import {
//     backgroundDark,
//     successColor,
//     dangerColor,
//     backgroundLight
// } from '../../variables/styles';
// import { errorImgUrl, } from '../../variables/global';
// import { format } from "d3-format";
// import { timeFormat, } from "d3-time-format";
// import virtualizedTableStyle from '../../variables/styles/virtualizedTableStyle';

// const numberFormat = format(".6f");

// class MuiVirtualizedTable extends React.PureComponent {
//     static defaultProps = {
//         headerHeight: 30,
//         rowHeight: 27,
//     };

//     getRowClassName = ({ index }) => {
//         const { classes, onRowClick } = this.props;

//         if (this.props.rows[index])
//             return clsx(classes.flexContainer, {
//                 [classes.tableRowHover]: index !== -1 && onRowClick != null,
//             });
//     };

//     getHeaderClassName = () => {
//         const { classes } = this.props;
//         return clsx(classes.headerRow, classes.flexContainer);
//     };

//     // renders each cell within the Column component
//     cellRenderer = ({ cellData, columnIndex }) => {
//         const { columns, classes, rowHeight, onRowClick } = this.props;

//         if (columns[columnIndex].price) {
//             cellData = '$' + numberFormat(cellData);
//         }
//         // process dates
//         if (columns[columnIndex].date) {
//             const dateFormat = timeFormat("%Y-%m-%d %H:%M:%S.%L");
//             cellData = dateFormat(new Date(cellData));
//             //console.log(cellData)
//         }

//         // process dates
//         if (columns[columnIndex].volume) {
//             cellData = numberFormat(cellData);
//         }

//         return (
//             <TableCell
//                 component="div"
//                 className={clsx(classes.tableCell, classes.flexContainer, {
//                     [classes.noClick]: onRowClick === null,
//                 })}
//                 variant="body"
//                 style={{ height: rowHeight }}
//                 align={(columnIndex != null && columns[columnIndex].numeric) || false ? 'right' : 'left'}
//             >

//                 {columns[columnIndex].side ?
//                     <Typography
//                         noWrap
//                         className={classes.cellText}
//                         style={columns[columnIndex].match ? { color: cellData === 'buy' ? successColor : dangerColor } : { color: cellData === 'buy' ? successColor : dangerColor }}>
//                         {cellData}
//                     </Typography> :
//                     <Typography
//                         noWrap
//                         className={classes.cellText}>
//                         {cellData}
//                     </Typography>
//                 }
//             </TableCell>
//         );
//     };

//     headerRenderer = ({ label, columnIndex }) => {
//         const { headerHeight, columns, classes } = this.props;
//         return (
//             <TableCell
//                 component="div"
//                 className={clsx(classes.tableCell, classes.flexContainer, classes.noClick)}
//                 variant="head"
//                 style={{
//                     height: headerHeight,
//                     background: backgroundDark,
//                     //fontSize: '11px',
//                     // borderTop: '3px solid ' + backgroundDark,
//                 }}
//                 align={columns[columnIndex].numeric || false ? 'right' : 'left'}
//             >
//                 <span>
//                     {label}
//                 </span>
//             </TableCell>
//         );
//     };

//     render() {

//         const {
//             classes,
//             columns,
//             onRowClick,
//             ...tableProps } = this.props;

//         return (
//             <AutoSizer>
//                 {({ height, width }) => (
//                     <Table
//                         height={height}
//                         width={width}
//                         {...tableProps}
//                         onRowClick={onRowClick}
//                         rowClassName={this.getRowClassName}
//                         headerClassName={this.getHeaderClassName}
//                     >
//                         {columns.map(({ dataKey, ...other }, index) => {
//                             return (
//                                 <Column
//                                     key={dataKey}
//                                     headerRenderer={
//                                         headerProps =>
//                                             this.headerRenderer({
//                                                 ...headerProps,
//                                                 columnIndex: index,
//                                             })
//                                     }
//                                     className={classes.flexContainer}
//                                     cellRenderer={this.cellRenderer}
//                                     dataKey={dataKey}
//                                     {...other}
//                                 />
//                             );
//                         })}
//                     </Table>
//                 )}
//             </AutoSizer>
//         );
//     }
// }

// MuiVirtualizedTable.propTypes = {
//     classes: PropTypes.object.isRequired,
//     columns: PropTypes.arrayOf(PropTypes.object).isRequired,
//     headerHeight: PropTypes.number,
//     onRowClick: PropTypes.func,
//     rowHeight: PropTypes.number,
// };

// const VirtualizedTable = withStyles(virtualizedTableStyle)(MuiVirtualizedTable);

// function ReactVirtualizedTable({ ...props }) {
//     const { onRowClick, rows, height, columns, selectedIndex, } = props;
//     return (
//         <div style={{ height: height, width: '100%', }}>
//             <VirtualizedTable
//                 rows={rows}
//                 columns={columns}
//                 rowCount={rows.length}
//                 rowGetter={({ index }) => rows[index]}
//                 onRowClick={onRowClick}
//                 rowStyle={({ index }) => {
//                     return selectedIndex && selectedIndex.includes(index) ?
//                         { background: backgroundLight }
//                         : {};
//                 }}
//             />
//         </div>
//     );
// }

// ReactVirtualizedTable.propTypes = {
//     rows: PropTypes.any,
//     onRowClick: PropTypes.func,
//     height: PropTypes.string,
//     columns: PropTypes.any,
//     selectedIndex: PropTypes.any,
// };

// export default ReactVirtualizedTable;
