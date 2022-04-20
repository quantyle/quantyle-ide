import { backgroundLight, backgroundPrimary, primaryColor } from "../styles";

const enhancedTableStyle = theme => ({
  root: {
    width: '100%',
    background: backgroundLight,
    height: '100%'
  },
  paper: {
    width: '100%',
    boxShadow: 'none',
    borderRadius: '0px !important',
    background: backgroundPrimary,
    height: '100%'
  },
  table: {
    minWidth: 750,
  },
  tableWrapper: {
    overflowX: 'auto',
  },
  numSelected: {
    float: 'left',
    padding: '15px',
    color: '#999'
  },
  title: {
    flex: '1 0 auto',
    color: '#fff',
  },
  actions: {
    color: theme.palette.text.secondary,
    display: 'flex'
  },
  toolbarRoot: {
    background: backgroundPrimary
  },
  checkbox: {
    color: '#999'
  },
  headerCell: {
    color: '#ddd',
    borderColor: backgroundLight
  },
  cell: {
    color: '#ddd',
    borderColor: backgroundLight,
  },


//start here:

  row: {
    '&:hover': {
      background: backgroundLight + ' !important',
    }
  },


  headRow: {
    background: backgroundPrimary + ' !important',
    '&:hover': {
      background: backgroundPrimary + ' !important',
    }
  },


  iconBtn: {
    borderRadius: '0px',
    background: 'transparent',
    '&:hover': {
      background: 'transparent'
    }
  },
  tablePagination: {
    color: "#fff"
  },

  tableSortLabelActive: {
    color: primaryColor + ' !important'
  },
});

export default enhancedTableStyle;