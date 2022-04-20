import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import match from 'autosuggest-highlight/match';
import parse from 'autosuggest-highlight/parse';
import { withStyles } from '@material-ui/core/styles';
import searchFieldStyle from '../../variables/styles/searchFieldStyle';
import {
    TextField
} from '../../components';
import { backgroundPrimary } from '../../variables/styles';

class SearchField extends Component {

    constructor(props) {
        super(props);
        console.log('suggestions: ', props.suggestions);
        this.state = {
            anchorEl: null,
            single: '',
            popper: '',
            stateSuggestions: props.suggestions.slice(0, 3),
        };
    }

    renderInputComponent(inputProps) {
        const { classes, ref, ...other } = inputProps;

        return (
            <TextField
            fullWidth
            inputRef={ref}
            style={{background: backgroundPrimary}}
            {...other}
          />
        );
    }

    renderSuggestion(suggestion, { query, isHighlighted }) {
        const matches = match(suggestion.id, query);
        //const parts = parse(suggestion.id, matches);
    }


    handleSuggestionsFetchRequested = ({ value }) => {
        const newValue = this.props.getSuggestions(value);
        this.setState({
            stateSuggestions: newValue
        });
    };

    handleSuggestionsClearRequested = () => {
        this.setState({
            stateSuggestions: []
        });
    };

    handleChange = (event, { newValue }) => {
        console.log('******', this.state.stateSuggestions);
        if(newValue === ''){
            //this.props.handleEmpty();
        }
        this.setState({
            single: newValue.toUpperCase(),
        });
    }

    render() {


        const { classes, getSuggestionValue } = this.props;

        return (
            <Autosuggest
                renderInputComponent={this.renderInputComponent}
                suggestions={this.state.stateSuggestions}
                onSuggestionsFetchRequested={this.handleSuggestionsFetchRequested}
                onSuggestionsClearRequested={this.handleSuggestionsClearRequested}
                getSuggestionValue={getSuggestionValue}
                renderSuggestion={this.renderSuggestion}
                inputProps={{
                    classes,
                    id: 'react-autosuggest-simple',
                    //label: 'Country',
                    variant: 'outlined',
                    //margin: 'normal',
                    placeholder: 'Search Symbol',
                    value: this.state.single,
                    onChange: this.handleChange,
                }}
                theme={{
                    //container: classes.container,
                    //suggestionsContainerOpen: classes.suggestionsContainerOpen,
                    suggestionsList: classes.suggestionsList,
                    suggestion: classes.suggestion,
                }}
            />
        );
    }
}

export default withStyles(searchFieldStyle)(SearchField);






