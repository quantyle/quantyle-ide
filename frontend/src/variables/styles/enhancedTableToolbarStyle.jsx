

const enhancedTableToolbarStyle = theme => ({
    root: {
        paddingLeft:"0px",
    },
    highlight:
        theme.palette.type === 'light'
            ? {
                color: theme.palette.secondary.main,
                //backgroundColor: lighten(theme.palette.secondary.light, 0.85),
            }
            : {
                color: theme.palette.text.primary,
                backgroundColor: theme.palette.secondary.dark,
            },
    actions: {
        color: theme.palette.text.secondary,
    },
    title: {
        //flex: '0 0 auto',
        width: '100%',
        padding: "20px"
    },
});

export default enhancedTableToolbarStyle;
/**
 * 
 *                 <FormControl className={classes.formControl}>
                    <InputLabel htmlFor="age-simple">Age</InputLabel>
                    <Select
                        value={this.state.age}
                        onChange={event =>this.handleChange()}
                        inputProps={{
                            name: 'age',
                            id: 'age-simple',
                        }}
                    >
                        <MenuItem value={10}>Ten</MenuItem>
                        <MenuItem value={20}>Twenty</MenuItem>
                        <MenuItem value={30}>Thirty</MenuItem>
                    </Select>
                </FormControl>
 * 
 */