import React from 'react';
import {
    Table as MuiTable,
    TableBody,
    TableCell,
    TableHead,
    TableContainer,
    TableRow,
    Paper,
    makeStyles,
} from '@material-ui/core';
import { backgroundDark, backgroundPrimary } from '../../variables/styles';


const useStyles = makeStyles({
  container: {
    borderRadius: 0,
  },
  table: {
    minWidth: 650,
    background: backgroundDark,
  },
  tableHeader: {
    background: backgroundPrimary,
  },
  tableCell: {
    color: '#fff',
    border: 'none !important',
    padding: '5px 12px',
  },
  tableRow: {
    border: 'none !important'
  }
});

function createData(name, calories, fat, carbs, protein) {
  return { name, calories, fat, carbs, protein };
}

const rows = [
  { name: 'Frozen yoghurt', calories: 0, fat: 0, carbs: 0, protein: 0 },
];

export default function Table() {
  const classes = useStyles();

  return (
    <TableContainer component={Paper} classes={{root:classes.container}}>
      <MuiTable className={classes.table} aria-label="simple table">
        <TableHead className={classes.tableHeader}>
          <TableRow className={classes.tableRow}>
            {/* <TableCell className={classes.tableCell}>Dessert (100g serving)</TableCell> */}
            <TableCell align="right" className={classes.tableCell}>Calories</TableCell>
            <TableCell align="right" className={classes.tableCell}>Fat&nbsp;(g)</TableCell>
            <TableCell align="right" className={classes.tableCell}>Carbs&nbsp;(g)</TableCell>
            <TableCell align="right" className={classes.tableCell}>Protein&nbsp;(g)</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.name} className={classes.tableRow}>
              {/* <TableCell component="th" scope="row" className={classes.tableCell}>
                {row.name}
              </TableCell> */}
              <TableCell align="right" className={classes.tableCell}>{row.calories}</TableCell>
              <TableCell align="right" className={classes.tableCell}>{row.fat}</TableCell>
              <TableCell align="right" className={classes.tableCell}>{row.carbs}</TableCell>
              <TableCell align="right" className={classes.tableCell}>{row.protein}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </MuiTable>
    </TableContainer>
  );
}