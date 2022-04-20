import {
  backgroundPrimary, backgroundLight, primaryColor,
} from "../styles";

const searchFieldStyle = {
  container: {
    position: 'relative',
  },
  suggestionsContainerOpen: {
    position: 'absolute',
    left: 0,
    right: 0,
  },
  suggestion: {
    display: 'block',
  },
  suggestionsList: {
    margin: 0,
    padding: 0,
    listStyleType: 'none',
  },
  input: {
    marginLeft: 10,
    flex: 1,
    color: '#fff',
  },

    

};

export default searchFieldStyle;


