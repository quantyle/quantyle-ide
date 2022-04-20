using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X1.Compiler
{
  class Tokenizer
  {
    string[] source; // lines of source code
    internal List<TokenList> token_lists = new List<TokenList>();
    
    /// <summary>
    /// Each string is a line in the source code. It's assumed that
    /// the ending white spaces have been trimmed.
    /// </summary>
    public Tokenizer(string[] source)
    {
      this.source = source;

      // tokenization starts here
      int line_number = 0; 
      int char_number = 0;

      while (line_number < source.Length)
      {
        var token_list = get_token_list(ref line_number, ref char_number);

        if (token_list != null)
        {
          // If found something on this line
          token_lists.Add(token_list);
        }
        // When token_list == null, the line_number is incremented inside
        // of get_token_list(...). So the code here doesn't need to do
        // anything. The strategy is to have all line increments happen
        // inside one function - the get_token_list(...).
      }
    }



    /// <summary>
    /// Starting at the given (line_number, char_number) location, 
    /// process enough source code to return a TokenList. Returns
    /// null if no token can be found on the given line.
    /// </summary>
    TokenList get_token_list(ref int line_number, ref int char_number)
    {
      char_number = find_non_whitespace(line_number, char_number);
      int indentation = find_non_whitespace(line_number, 0);

      // Usually, "char_number" is the same as "indentation".
      // The difference occurs if the line contains more than one statement,
      // as in "x = 1; y = 2".
      // When processing the second statement, the indentation = 0
      // while the char_number = 7.

      if (indentation == -1)
      {
        line_number++;
        char_number = 0;
        return null; // nothing on this line
      }

      // There is indentation, but make sure it's not a comment line.
      char current_char = source[line_number][char_number];
      if (current_char == '#')
      {
        line_number++;
        char_number = 0;
        return null;
      }

      // Code gets here if there is an indentation value, and 
      // it's not a comment line, that means there is at least one token.
      var token_list = new TokenList(line_number, indentation);

      // The end of (token) list is usually the end of the line. 
      // However, Python have several line continuation scenarios.      
      
      // Exit the while loop by returning the token_list
      while (true)
      {
        // get next token
        char_number = find_non_whitespace(line_number, char_number);
        var token = get_token(ref line_number, ref char_number);

        if (token != null)
        {
          // handle ';' line termination
          if (token.type == TokenType.Semicolon)
            return token_list;

          // The previous token is needed for the special cases
          Token prev_token = null;
          if (token_list.Count > 0)
            prev_token = token_list[-1];

          // special case: merge Is, Not tokens --> Is_Not token
          if (prev_token != null 
            && prev_token.type == TokenType.Is && token.type == TokenType.Not)
          {
            token_list.remove_last(1);
            token_list.add_token(Token.Constants.Is_Not);
          }

          // special case: merge Not, In tokens --> Not_In token
          else if(prev_token != null 
            && prev_token.type == TokenType.Not && token.type == TokenType.In)
          {
            token_list.remove_last(1);
            token_list.add_token(Token.Constants.Not_In);
          }

          // standard case: 
          else
          {
            token_list.add_token(token);
          }
        }
        else
        {
          // token == null case
          // End of the line, but look for line continuations.
          line_number++;
          char_number = 0;

          if (line_number >= source.Length) return token_list;

          if (token_list[-1].type == TokenType.Slash)
          {
            // handle '\' line continuation
            token_list.remove_last(1);
          }

          else if (token_list.count_parenthesis(excessive_left_token_error: false) > 0
            || token_list.count_braces(excessive_left_token_error: false) > 0 
            || token_list.count_brackets(excessive_left_token_error: false) > 0)
          {
            // This means '(', '[', or '{' line continuation.
            // Don't return, keep going.
          }
          else
            return token_list;
        }
      }
    }

    /// <summary>
    /// Starting at the given (line_number, char_number) location, 
    /// process enough source code to return a Token. Returns null if
    /// no token can be found on the given line.
    /// </summary>
    Token get_token(ref int line_number, ref int char_number)
    {
      // The char_number can be invalid since the get_token_list(...)
      // is relying on get_token(...) to detect end of the line.
      // That way, the get_token_list(...) handles all end of the line
      // special cases in one place.
      if (line_number >= source.Length)
        return null;
      if (char_number < 0 || char_number >= source[line_number].Length)
        return null;

      var current_line = source[line_number];
      char current_char = current_line[char_number];

      if (current_char == '#') return null;
      
      // Handle string tokens
      if (current_char == '"' || current_char == '\'')
      {
        return get_string_token(ref line_number, ref char_number);
      }

      // Second character, after the current one, is needed for
      // certain situations.
      char? second_char = null;
      if (char_number + 1 < current_line.Length)
        second_char = current_line[char_number + 1];

      // Handle number tokens
      Token token;
      int length;

      // number token case 1 - seeing a digit
      // number token case 2 - seeing a period followed by a number
      if (char.IsDigit(current_char)
        || (second_char != null && current_char == '.' && char.IsDigit(second_char.Value)))
      {
        (token, length) = Token.parse_number(
          current_line.Substring(char_number), line_number);

        char_number += length;
        return token;
      }

      // Previously, tokens that start with a period has already
      // been tested for being the start of a number.
      // If the period is not the start of a number, tokens that start 
      // with period is processed as an independent token.
      // However, periods inside identifier tokens is treated as part of the
      // identifier.
      if (current_char == '.')
      {
        char_number++;
        return Token.Constants.Period;
      }
      
      // Look for symbolic tokens, like '+' or "**"
      // Try to parse as symbol
      (token, length) = Token.parse_symbol(current_char, second_char, line_number);
      if (token != null)
      {
        char_number += length;
        return token;
      }

      // Generic token handling:
      // Find the end of the token, then extract token.
      int end_char_number = find_token_end(line_number, char_number);
      length = end_char_number - char_number;
      if (length < 1) length = 1;

      string token_str = current_line.Substring(char_number, length);

      // update char_number for future
      char_number = end_char_number;

      // Try to parse as keyword
      token = Token.parse_keyword(token_str);
      if (token != null) return token;

      // Parse as identifier - last resort
      return Token.parse_identifier(token_str, line_number);
    }


    /// <summary>
    /// Starting at the given (line_number, char_number) location, 
    /// process enough source code to return a string Token. Throws
    /// exception if the string token is not properly terminated.
    /// </summary>
    Token get_string_token(ref int line_number, ref int char_number)
    {
      var current_line = source[line_number];
      var current_char = current_line[char_number];

      char quote_char = current_char; // either ' or "

      bool triple_quote = false;

      // detect triple quote case
      if (char_number + 2 < current_line.Length)
      {
        if (current_line[char_number + 1] == quote_char
          && current_line[char_number + 2] == quote_char)
          triple_quote = true; 
      }

      // move to start of string
      if (triple_quote) char_number += 3;
      else char_number++;

      var sb = new StringBuilder();

      // code will exit the for loop when at end of the string
      while (true)
      {
        // process current character
        current_line = source[line_number];
        current_char = current_line[char_number];

        if(current_char == '\\')
        {
          // handle escape characters
          if (char_number + 1 >= current_line.Length)
          {
            // The '\\' is for line continuation
            char_number = 0;
            line_number++;
          }
          else
          {
            // There is a second character after this.
            var second_char = current_line[char_number + 1];

            // Add the escape character to "sb"
            if (second_char == '\\' || second_char == '\'' || second_char == '\"')
              sb.Append(second_char);
            else if (second_char == 'n')
              sb.Append('\n');
            else if (second_char == 'r')
              sb.Append('\r');
            else if (second_char == 't')
              sb.Append('\t');
            else
            {
              // unsupported escape sequence
              throw new CompilerException("Source code error on line " 
                + (line_number + 1) + ". Unsupported escape sequence \"\\"
                + second_char + "\".", (line_number + 1));
            }

            // In all above cases, the escape sequence is two characters long
            char_number += 2;
          }
        } // end of: if(current_char == '\\')

        else if (current_char == quote_char)
        {
          // Handle quotation mark found.
          if (triple_quote == false)
          {
            // Handle end of single quote string
            char_number++;
            return Token.create_string_token(sb.ToString());
          }
          else
          {
            // Handle the triple quote case of seeing a quote
            if (char_number + 2 < current_line.Length)
            {
              if (current_line[char_number + 1] == quote_char
                && current_line[char_number + 2] == quote_char)
              {
                // Handle the end of triple quote string.
                char_number += 3;
                return Token.create_string_token(sb.ToString());
              }
            }
            // Handle the continuation of triple quote string.
            sb.Append(current_char);
            char_number++;
          }
        } // end of: if (current_char == quote_char)

        else
        {
          // Standard case - current_char not a quote and not an escape
          sb.Append(current_char);
          char_number++;
        } 

        // end of the line handling
        if (char_number >= current_line.Length)
        {
          if (triple_quote)
          {
            // Triple quote string newline
            sb.Append('\n');
            char_number = 0;
            line_number++;

            // Detect end of source code error
            if (line_number >= source.Length)
            {
              throw new CompilerException("Source code error on line "
                + (line_number + 1) + ". A triple quote string "
                + "did not have a corresponding triple quote "
                + "termination.", (line_number + 1));
            }
          }
          else
          {
            // Only triple quote strings are allow to extend over multiple lines.
            throw new CompilerException("Source code error on line "
              + (line_number + 1) + ". A single quote string "
              + "ended the line without a quote nor a line "
              + "continuation character \\.", (line_number + 1));
          }
        }
      }
    }


    /// <summary>
    /// Starting at character number "start", find the next non-white-space
    /// character on the same line. Return the index of the character. Return 
    /// -1 if all characters encountered are white spaces.
    /// </summary>
    int find_non_whitespace(int line_number, int start)
    {
      var current_line = source[line_number];
      int index = start;
      
      while (index < current_line.Length)
      {
        if (char.IsWhiteSpace(current_line[index]) == false) return index;
        index++;
      }
      return -1;
    }


    /// <summary>
    /// Starting at character number "start", find a character that marks the
    /// end of the token. Return the index of the end character.
    /// </summary>
    int find_token_end(int line_number, int start)
    {
      var current_line = source[line_number];
      int index = start;

      while (index < current_line.Length)
      {
        char current_char = current_line[index];

        if (Token.is_token_end(current_char)) return index;

        index++;
      }

      return index;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();

      foreach (var token_list in token_lists)
        sb.AppendLine(token_list.ToString());

      return sb.ToString().Trim();
    }

  }

  class TokenList
  {
    public readonly int line_number;
    public readonly int indentation;

    List<Token> tokens;

    public TokenList(int line_number, int indentation)
    {
      this.line_number = line_number;
      this.indentation = indentation;
      tokens = new List<Token>();
    }

    public Token this[int index]
    {
      // -1 gets the last token, -2 gets the second last token, and so on...
      get
      {
        if (index >= 0)
          return tokens[index];
        else
        {
          index = tokens.Count + index;
          return tokens[index];
        }
      }

      set
      {
        if (index < 0)
          index = tokens.Count + index;

        tokens[index] = value;
      }
    }

    public int Count
    {
      get { return tokens.Count; }
    }

    public void add_token(Token token)
    {
      tokens.Add(token);
    }

    public void add_tokens(params Token[] tokens)
    {
      foreach (var token in tokens)
        this.tokens.Add(token);
    }


    /// <summary>
    /// Adds tokens from a given "list", from start_index, to its end.
    /// </summary>
    public void add_token_list(TokenList list, int start_index)
    {
      add_token_list(list, start_index, list.Count - 1);
    }


    /// <summary>
    /// Adds tokens from a given "list", from start_index, to end_index.
    /// </summary>
    public void add_token_list(TokenList list, int start_index, int end_index)
    {
      if (start_index > end_index) return;

      for (int i = start_index; i <= end_index; i++)
        tokens.Add(list[i]);
    }


    /// <summary>
    /// Remove the final "n" tokens.
    /// </summary>
    public void remove_last(int n)
    {
      if (n > tokens.Count) n = tokens.Count;
      var tokens_removed = new Token[n];

      int start_index = tokens.Count - n;
      tokens.RemoveRange(start_index, n);
    }


    /// <summary>
    /// Returns the number of left braces minus right braces. Having
    /// too many right braces always trigger an immediate error.
    /// </summary>
    /// <param name="excessive_left_tokens_error">More left braces 
    /// than the right braces optionally cause an exception.</param>
    public int count_braces(bool excessive_left_token_error)
    {
      return count_token_balance(Token.Constants.Left_Brace,
        Token.Constants.Right_Brace, excessive_left_token_error);
    }


    /// <summary>
    /// Returns the number of left brackets minus right brackets. Having
    /// too many right brackets always trigger an immediate error.
    /// </summary>
    /// <param name="excessive_left_tokens_error">More left brackets 
    /// than the right brackets optionally cause an exception.</param>
    public int count_brackets(bool excessive_left_token_error)
    {
      return count_token_balance(Token.Constants.Left_Bracket,
        Token.Constants.Right_Bracket, excessive_left_token_error);
    }


    /// <summary>
    /// Returns the number of left parenthesis minus right parenthesis. Having
    /// too many right parenthesis always trigger an immediate error.
    /// </summary>
    /// <param name="excessive_left_tokens_error">More left parenthesis 
    /// than the right parenthesis optionally cause an exception.</param>
    public int count_parenthesis(bool excessive_left_token_error)
    {
      return count_token_balance(Token.Constants.Left_Paren,
        Token.Constants.Right_Paren, excessive_left_token_error);
    }


    /// <summary>
    /// Return the number of 'left' token minus 'right' token. 
    /// More right token than the left immediately causes an
    /// exception.
    /// </summary>
    /// <param name="excessive_left_tokens_error">More left token 
    /// than the right optionally cause an exception.</param>
    int count_token_balance(Token left, Token right, bool excessive_left_token_error)
    {
      int left_count = 0;
      // Increase for every left token, decrease for every right token.

      foreach (var token in tokens)
      {
        if (token.type == left.type) left_count++;
        else if (token.type == right.type) left_count--;

        // Right token exceeding left token, at any time, is always an error.
        if (left_count < 0)
          throw new CompilerException("Source code error on line "
            + (line_number + 1) + ". More " + right.value.ToString()
            + " than " + left.value.ToString() + " found.", (line_number + 1));
      }

      // During the build up phase of a token list, the left 
      // token can exceed the right token. That's why an excessive 
      // number of left tokens doesn't always trigger an exception.
      if (left_count > 0 && excessive_left_token_error == true)
        throw new CompilerException("Source code error on line "
            + (line_number + 1) + ". More \"" + left.value.ToString()
            + "\" than \"" + right.value.ToString() + "\" found.", (line_number + 1));

      return left_count;
    }


    /// <summary>
    /// Check for a particular TokenType pattern at a particular index.
    /// Return true if pattern matches. Note: not any combination of
    /// patterns will work correctly.
    /// </summary>
    public bool match_pattern(TokenType[] pattern, int index)
    {
      int i = 0;

      // the strategy below is to go through "pattern", and "return false" on any mismatch
      while (i < pattern.Length)
      {
        // Compare pattern[i] with tokens[index].
        // Check for running out of "tokens"
        if (index >= tokens.Count) return false;

        // Handle variable length patterns: OneOrMore and ZeroOrMore
        if (pattern[i] == TokenType.TwoOrMore || pattern[i] == TokenType.OneOrMore
          || pattern[i] == TokenType.ZeroOrMore)
        {
          if (pattern[i] == TokenType.OneOrMore)
            index++;
          else if (pattern[i] == TokenType.TwoOrMore)
            index += 2;

          // If this is the last pattern, then pattern matching is done
          if (i >= pattern.Length - 1) return true;

          // go to next token
          i++;
                    
          // search for pattern[i] - this is the pattern after the "OneOrMore"
          // This search needs to take parenthesis characters "([{" into account.
          index = find_token_outside_of_paren(pattern[i], index, tokens.Count - 1);
          
          // code gets here either due to a match, or due to running out of tokens
          if (index < 0) return false;
          else
          {
            i++;
            index++;
          }
        } // end of: if (pattern[i] == TokenType.OneOrMore || pattern[i] == TokenType.ZeroOrMore)

        // Handle the bracket expression pattern (BracketExpr)
        else if (pattern[i] == TokenType.BracketExpr)
        {
          // the current token has to be '['
          if (tokens[index].type != TokenType.Left_Bracket) return false;

          // advance "index" to the after the matching ']'
          index = find_ending_token(index);
          if (index == -1) return false;

          index++; // move beyond the ']'

          // from this point on, further '[' are optional
          while(index < tokens.Count &&
            tokens[index].type == TokenType.Left_Bracket)
          {
            index = find_ending_token(index);
            if (index == -1) return false;
            index++;
          }

          i++;
        }

        // Handle fixed length patterns
        else
        {
          bool match = false; // this flag is only for patter[i] == tokens[index], not the whole thing

          if (pattern[i] == TokenType.AnyOne)
            // Case: any token will match
            match = true;
          else if (pattern[i] == TokenType.NotNextToken)
          {
            // Case: next token must not match
            i++;
            if (pattern[i] != tokens[index].type)
              match = true;
          }
          else
          {
            // Case: token must match
            if (pattern[i] == tokens[index].type)
              match = true;
          }

          if (match == false)
            return false;
          else
          {
            i++;
            index++;
          }
        }
      }

      // went through the pattern and no mismatch found, so it must be a match
      return true;
    }


    /// <summary>
    /// Search for a particular TokenType. Return -1 if not found.
    /// </summary>
    public int find_token(TokenType token_type, int start_index)
    {
      for(int i = start_index; i < tokens.Count; i++)
      {
        if (tokens[i].type == token_type)
          return i;
      }

      return -1;
    }


    /// <summary>
    /// Find a particular token, outside of parenthesis nesting. For example,
    /// looking for ':' while ignoring those that are enclosed by parenthesis.
    /// </summary>
    /// <returns>Returns -1 if token not found.</returns>
    public int find_token_outside_of_paren(TokenType token_type, int start_index, int end_index)
    {
      // The token is valid only if ALL of the following parenthesis
      // counters are zero.
      int left_brace_count = 0;
      int left_bracket_count = 0;      
      int left_paren_count = 0;

      int index = start_index;
      while(index <= end_index)
      {
        var token = tokens[index];

        if (token.type == token_type && left_brace_count == 0
          && left_bracket_count == 0 && left_paren_count == 0)
          return index;

        else if (token.type == TokenType.Left_Brace)
          left_brace_count++;
        else if (token.type == TokenType.Left_Bracket)
          left_bracket_count++;        
        else if (token.type == TokenType.Left_Paren)
          left_paren_count++;

        else if (token.type == TokenType.Right_Brace)
          left_brace_count--;
        else if (token.type == TokenType.Right_Bracket)
          left_bracket_count--;        
        else if (token.type == TokenType.Right_Paren)
          left_paren_count--;

        index++;
      }

      return -1;
    }


    /// <summary>
    /// Search for a particular TokenType pattern. Return -1 if not found.
    /// </summary>
    public int find_pattern(TokenType[] pattern, int start_index, int min_pattern_length)
    {
      int index = start_index;
      // while (index + pattern.Length <= tokens.Count) - mostly works, but there's TokenType.
      while (index <= tokens.Count - min_pattern_length)
      {
        if (match_pattern(pattern, index) == true)
          return index;

        index++;
      }

      return -1;
    }


    /// <summary>
    /// The "start_index" should point to '(', '[', or '{'. Look for the
    /// ending ')', ']', or '}'. Return -1 if not found.
    /// </summary>
    public int find_ending_token(int start_index)
    {
      var start_pattern = tokens[start_index].type;

      TokenType end_pattern = 0;
      if (start_pattern == TokenType.Left_Brace) end_pattern = TokenType.Right_Brace;
      else if (start_pattern == TokenType.Left_Bracket) end_pattern = TokenType.Right_Bracket;
      else if (start_pattern == TokenType.Left_Paren) end_pattern = TokenType.Right_Paren;

      // counter to keep track of start_pattern repeats
      int pattern_counter = 1; // for the character at tokens[start_index]

      for (int i = start_index + 1; i < tokens.Count; i++)
      {
        if (tokens[i].type == start_pattern) pattern_counter++;
        else if (tokens[i].type == end_pattern) pattern_counter--;

        if (pattern_counter == 0) return i;
      }

      return -1;
    }
    

    /// <summary>
    /// The "end_index" should point to ')', ']', or '}'. Look for the
    /// starting '(', '[', or '{'. Return -1 if not found.
    /// </summary>
    public int find_starting_token(int end_index)
    {
      var end_pattern = tokens[end_index].type;

      TokenType start_pattern = 0;
      if (end_pattern == TokenType.Right_Brace) start_pattern = TokenType.Left_Brace;
      else if (end_pattern == TokenType.Right_Bracket) start_pattern = TokenType.Left_Bracket;
      else if (end_pattern == TokenType.Right_Paren) start_pattern = TokenType.Left_Paren;

      // counter to keep track of end_pattern repeats
      int pattern_counter = 1; // for the character at tokens[end_index]

      for (int i = end_index - 1; i >= 0; i--)
      {
        if (tokens[i].type == start_pattern) pattern_counter--;
        else if (tokens[i].type == end_pattern) pattern_counter++;

        if (pattern_counter == 0) return i;
      }

      return -1;
    }


    public override string ToString()
    {
      return ToString(0, tokens.Count - 1);
    }


    public string ToString(int start_index, int end_index)
    {
      var sb = new StringBuilder();
      if (end_index > tokens.Count - 1) end_index = tokens.Count - 1;
      
      for (int i = 0; i < indentation; i++)
        sb.Append(' ');

      for (int i = start_index; i <= end_index; i++)
        sb.Append(tokens[i].ToString() + " ");

      return sb.ToString();
    }
  }

  
  enum TokenType
  {
    // These tokens are for pattern matching
    AnyOne, ZeroOrMore, OneOrMore, TwoOrMore, NotNextToken, BracketExpr,
    
    // Many of these are NOT implemented
    Add, Add_Equal, And, As, Assert, Assign, Bitwise_And,
    Bitwise_Not, Bitwise_Or, Bitwise_Xor, Break,
    Class, Colon, Comma, Continue, Def, Del, Divide, Divide_Equal,
    Double, Elif, Else, Equal, Except, Exponent, False,
    Finally, Floor_Divide, For, From, Global, Greater, Greater_or_Equal,
    Identifier, If, Import, In, Integer, Is, Is_Not, Lambda,
    Left_Brace, Left_Bracket, Left_Paren, Less, Less_or_Equal,
    Multiply, Multiply_Equal, None, Nonlocal, Not, Not_In, Not_Equal, Or, Pass, 
    Period, Raise, Remainder, Return, Right_Brace, Right_Bracket,
    Right_Paren, Shift_Left, Shift_Right, Semicolon, Slash, String,
    Subtract, Subtract_Equal, True, Try, While, With, Yield
  }

  class Token
  {
    public readonly TokenType type;
    public readonly object value;
        
    Token(TokenType type, object value)
    {
      this.type = type;
      this.value = value;
    }

    // Constant tokens - only one object of each type exist.
    // For example, all the "Add" tokens are really references to
    // the single "Constant.Add" token. Almost all token types are 
    // constants. The only non-constants are the data types -
    // such as string, integer, double.
    // 
    // For creation purposes, the "Constants" is (mostly) not necessary. 
    // It's simpler to just rely on "symbol_mappings" and "keyword_mappings". This 
    // class exists so to easily refer to tokens for the intermediate
    // code generation.
    public class Constants
    {
      static public readonly Token Add, Add_Equal, And, As, Assert, Assign, Bitwise_And;
      static public readonly Token Bitwise_Not, Bitwise_Or, Bitwise_Xor, Break;
      static public readonly Token Class, Colon, Comma, Continue, Def, Del, Divide, Divide_Equal;
      static public readonly Token Elif, Else, Equal, Except, Exponent, False;
      static public readonly Token Finally, Floor_Divide, For, From, Global, Greater, Greater_or_Equal;
      static public readonly Token If, Import, In, Is, Is_Not, Lambda;
      static public readonly Token Left_Brace, Left_Bracket, Left_Paren, Less, Less_or_Equal;
      static public readonly Token Multiply, Multiply_Equal, None, Nonlocal, Not, Not_In, Not_Equal, Or, Pass;
      static public readonly Token Period, Raise, Remainder, Return, Right_Brace, Right_Bracket;
      static public readonly Token Right_Paren, Shift_Left, Shift_Right, Semicolon, Slash;
      static public readonly Token Subtract, Subtract_Equal, True, Try, While, With, Yield;

      static Constants()
      {
        Add = new Token(TokenType.Add, '+');
        Add_Equal = new Token(TokenType.Add_Equal, "+=");
        And = new Token(TokenType.And, "and");
        As = new Token(TokenType.As, "as");
        Assert = new Token(TokenType.Assert, "assert");
        Assign = new Token(TokenType.Assign, '=');
        Bitwise_And = new Token(TokenType.Bitwise_And, '&');

        Bitwise_Not = new Token(TokenType.Bitwise_Not, '~');
        Bitwise_Or = new Token(TokenType.Bitwise_Or, '|');
        Bitwise_Xor = new Token(TokenType.Bitwise_Xor, '^');
        Break = new Token(TokenType.Break, "break");

        Class = new Token(TokenType.Class, "class");
        Colon = new Token(TokenType.Colon, ':');
        Comma = new Token(TokenType.Comma, ',');
        Continue = new Token(TokenType.Continue, "continue");
        Def = new Token(TokenType.Def, "def");
        Del = new Token(TokenType.Del, "del");
        Divide = new Token(TokenType.Divide, '/');
        Divide_Equal = new Token(TokenType.Divide_Equal, "/=");

        Elif = new Token(TokenType.Elif, "elif");
        Else = new Token(TokenType.Else, "else");
        Equal = new Token(TokenType.Equal, "==");
        Except = new Token(TokenType.Except, "except");
        Exponent = new Token(TokenType.Exponent, "**");
        False = new Token(TokenType.False, "False");

        Finally = new Token(TokenType.Finally, "finally");
        Floor_Divide = new Token(TokenType.Floor_Divide, "//");
        For = new Token(TokenType.For, "for");
        From = new Token(TokenType.From, "from");
        Global = new Token(TokenType.Global, "global");
        Greater = new Token(TokenType.Greater, '>');
        Greater_or_Equal = new Token(TokenType.Greater_or_Equal, ">=");

        If = new Token(TokenType.If, "if");
        Import = new Token(TokenType.Import, "import");
        In = new Token(TokenType.In, "in");
        Is = new Token(TokenType.Is, "is");
        Is_Not = new Token(TokenType.Is_Not, "is not");
        Lambda = new Token(TokenType.Lambda, "lambda");

        Left_Brace = new Token(TokenType.Left_Brace, '{');
        Left_Bracket = new Token(TokenType.Left_Bracket, '[');
        Left_Paren = new Token(TokenType.Left_Paren, '(');
        Less = new Token(TokenType.Less, '<');
        Less_or_Equal = new Token(TokenType.Less_or_Equal, "<=");

        Multiply = new Token(TokenType.Multiply, '*');
        Multiply_Equal = new Token(TokenType.Multiply_Equal, "*=");
        None = new Token(TokenType.None, "none");
        Nonlocal = new Token(TokenType.Nonlocal, "nonlocal");
        Not = new Token(TokenType.Not, "not");
        Not_In = new Token(TokenType.Not_In, "not in");
        Not_Equal = new Token(TokenType.Not_Equal, "!=");
        Or = new Token(TokenType.Or, "or");
        Pass = new Token(TokenType.Pass, "pass");

        Period = new Token(TokenType.Period, '.');
        Raise = new Token(TokenType.Raise, "raise");
        Remainder = new Token(TokenType.Remainder, '%');
        Return = new Token(TokenType.Return, "return");
        Right_Brace = new Token(TokenType.Right_Brace, '}');
        Right_Bracket = new Token(TokenType.Right_Bracket, ']');
        Right_Paren = new Token(TokenType.Right_Paren, ')');

        Shift_Left = new Token(TokenType.Shift_Left, "<<");
        Shift_Right = new Token(TokenType.Shift_Right, ">>");
        Semicolon = new Token(TokenType.Semicolon, ';');
        Slash = new Token(TokenType.Slash, '\\');

        Subtract = new Token(TokenType.Subtract, '-');
        Subtract_Equal = new Token(TokenType.Subtract_Equal, "-=");
        True = new Token(TokenType.True, "True");
        Try = new Token(TokenType.Try, "try");
        While = new Token(TokenType.While, "while");
        With = new Token(TokenType.With, "with");
        Yield = new Token(TokenType.Yield, "yield");
      }
    }

    // string token to token mappings
    static Dictionary<char, Token> symbol_mappings;
    static HashSet<char> two_char_symbols;
    static Dictionary<string, Token> keyword_mappings;
    

    static Token()
    {
      // Initialize static data structures
      // Initialize symbol_mappings
      symbol_mappings = new Dictionary<char, Token>();
      symbol_mappings.Add('+', Constants.Add);
      symbol_mappings.Add('=', Constants.Assign);
      symbol_mappings.Add('&', Constants.Bitwise_And);
      symbol_mappings.Add('~', Constants.Bitwise_Not);
      symbol_mappings.Add('|', Constants.Bitwise_Or);
      symbol_mappings.Add('^', Constants.Bitwise_Xor);
      symbol_mappings.Add(':', Constants.Colon);
      symbol_mappings.Add(',', Constants.Comma);
      symbol_mappings.Add('/', Constants.Divide);
      symbol_mappings.Add('>', Constants.Greater);
      symbol_mappings.Add('{', Constants.Left_Brace);
      symbol_mappings.Add('[', Constants.Left_Bracket);
      symbol_mappings.Add('(', Constants.Left_Paren);
      symbol_mappings.Add('<', Constants.Less);
      symbol_mappings.Add('*', Constants.Multiply);
      // symbol_mappings.Add('.', Constants.Period);
      symbol_mappings.Add('%', Constants.Remainder);
      symbol_mappings.Add('}', Constants.Right_Brace);
      symbol_mappings.Add(']', Constants.Right_Bracket);
      symbol_mappings.Add(')', Constants.Right_Paren);
      symbol_mappings.Add('-', Constants.Subtract);
      symbol_mappings.Add(';', Constants.Semicolon);
      symbol_mappings.Add('\\', Constants.Slash);

      // Initialize two_char_symbols
      // These characters can have a follow up character as a symbol
      two_char_symbols = new HashSet<char>();
      two_char_symbols.Add('*'); // **, *=
      two_char_symbols.Add('='); // ==
      two_char_symbols.Add('<'); // <=
      two_char_symbols.Add('>'); // >=
      two_char_symbols.Add('+'); // +=
      two_char_symbols.Add('-'); // -=
      two_char_symbols.Add('/'); // "//" - floor divide, /=

      // Initialize keyword_mappings
      keyword_mappings = new Dictionary<string, Token>();
      keyword_mappings.Add("and", Constants.And);
      keyword_mappings.Add("as", Constants.As);
      keyword_mappings.Add("assert", Constants.Assert);
      keyword_mappings.Add("break", Constants.Break);
      keyword_mappings.Add("class", Constants.Class);
      keyword_mappings.Add("continue", Constants.Continue);
      keyword_mappings.Add("def", Constants.Def);
      keyword_mappings.Add("del", Constants.Del);
      keyword_mappings.Add("elif", Constants.Elif);
      keyword_mappings.Add("else", Constants.Else);
      keyword_mappings.Add("except", Constants.Except);
      keyword_mappings.Add("False", Constants.False);
      keyword_mappings.Add("finally", Constants.Finally);
      keyword_mappings.Add("for", Constants.For);
      keyword_mappings.Add("from", Constants.From);
      keyword_mappings.Add("global", Constants.Global);
      keyword_mappings.Add("if", Constants.If);
      keyword_mappings.Add("import", Constants.Import);
      keyword_mappings.Add("in", Constants.In);
      keyword_mappings.Add("is", Constants.Is);
      keyword_mappings.Add("lambda", Constants.Lambda);
      keyword_mappings.Add("None", Constants.None);
      keyword_mappings.Add("nonlocal", Constants.Nonlocal);
      keyword_mappings.Add("not", Constants.Not);
      keyword_mappings.Add("or", Constants.Or);
      keyword_mappings.Add("pass", Constants.Pass);
      keyword_mappings.Add("raise", Constants.Raise);
      keyword_mappings.Add("return", Constants.Return);
      keyword_mappings.Add("True", Constants.True);
      keyword_mappings.Add("try", Constants.Try);
      keyword_mappings.Add("while", Constants.While);
      keyword_mappings.Add("with", Constants.With);
      keyword_mappings.Add("yield", Constants.Yield);
    }


    /// <summary>
    /// Try to parse the two characters as symbols. Will return a 
    /// symbol token, along with the length of the token if a 
    /// symbol is found. Will return null if no symbol found.
    /// </summary>
    static public (Token, int) parse_symbol(char current_char, char? second_char, int line_number)
    {
      if(symbol_mappings.ContainsKey(current_char))
      {
        if (two_char_symbols.Contains(current_char))
        {
          // check for special two character symbols
          if (current_char == '+' && second_char == '=')
            return (Constants.Add_Equal, 2);

          else if (current_char == '=' && second_char == '=')
            return (Constants.Equal, 2);

          else if (current_char == '*')
          {
            if (second_char == '*')
              return (Constants.Exponent, 2);

            else if (second_char == '=')
              return (Constants.Multiply_Equal, 2);
          }

          else if (current_char == '/')
          {
            if (second_char == '/')
              return (Constants.Floor_Divide, 2);

            else if (second_char == '=')
              return (Constants.Divide_Equal, 2);
          }

          else if (current_char == '>')
          {
            if (second_char == '=')
              return (Constants.Greater_or_Equal, 2);

            else if (second_char == '>')
              return (Constants.Shift_Right, 2);
          }

          else if (current_char == '<')
          {
            if (second_char == '=')
              return (Constants.Less_or_Equal, 2);

            else if (second_char == '<')
              return (Constants.Shift_Left, 2);
          }

          else if (current_char == '-' && second_char == '=')
            return (Constants.Subtract_Equal, 2);
        }

        // single character symbol case
        return (symbol_mappings[current_char], 1);
      }

      // special case, !=
      // If '!', then must have '='. Throw exception for having only '!'
      if (current_char == '!')
      {
        if (second_char == '=')
        {
          return (Constants.Not_Equal, 2);
        }
        else
        {
          throw new Exception("Source code error at line # " + (line_number + 1) 
            + ". The character '!' must be followed by '='.");
        }
      }

      // no symbol found
      return (null, 0);
    }


    /// <summary>
    /// Parse token as a keyword - as in "if" or "def". Will return null
    /// if unable to parse as keyword.
    /// </summary>
    static public Token parse_keyword(string token)
    {
      if (keyword_mappings.ContainsKey(token))
        return keyword_mappings[token];

      return null;
    }


    /// <summary>
    /// Try to parse a substring as a number - int or double. If successful,
    /// will return the length of the substring used. Tokens that start 
    /// with digit or period must be a number. So if fail to extract a 
    /// number, this function will throw an exception.
    /// </summary>
    static public (Token, int) parse_number(string substring, int line_number)
    {
      // The first character of "substring" should be a
      // '.' or a digit.

      // A state machine to recognize a number.
      // See documentation --> Design, Expression evaluation, Tokenizing Numbers
      // for detail.
      int state = 0;
      // 0 = digit, 1 = period, 2 = digit, 3 = 'e', 4 = sign, 5 = digit

      int index = 0;
      bool error = false;
      
      while(index < substring.Length && error == false)
      {
        char c = substring[index];

        if (state == 0) // digit
        {
          if (c == '.') state = 1;
          else if (c == 'e') state = 3;
          else if (char.IsDigit(c)) state = 0;
          else if (char.IsWhiteSpace(c)) break;
          else if (symbol_mappings.ContainsKey(c)) break;
          else error = true;
        }
        else if (state == 1) // period
        {
          if (char.IsDigit(c)) state = 2;
          else if (c == 'e') state = 3;
          else if (char.IsWhiteSpace(c)) break;
          else if (symbol_mappings.ContainsKey(c)) break;
          else error = true;
        }
        else if (state == 2) // digit
        {
          if (c == 'e') state = 3;
          else if (char.IsDigit(c)) state = 2;
          else if (char.IsWhiteSpace(c)) break;
          else if (symbol_mappings.ContainsKey(c)) break;
          else error = true;
        }
        else if (state == 3) // 'e'
        {
          if (char.IsDigit(c)) state = 5;
          else if (c == '+' || c == '-') state = 4;
          else if (char.IsWhiteSpace(c)) break;
          else if (symbol_mappings.ContainsKey(c)) break;
          else error = true;
        }
        else if (state == 4) // sign
        {
          if (char.IsDigit(c)) state = 5;
          else
          {
            error = true;
            break;
          }
        }
        else if (state == 5) // digit
        {
          if (char.IsDigit(c)) state = 5;
          else if (char.IsWhiteSpace(c)) break;
          else if (symbol_mappings.ContainsKey(c)) break;
          else error = true;
        }

        index++;
      }

      // Token length is index.
      int length = index;

      // ending in state 3, the 'e', is an error
      // ending in state 4, the '+' or '-', is an error
      if (state == 3 || state == 4) error = true;

      // catch the single period case as an error
      if (state == 1 && length == 1) error = true;

      if (error == false)
      {
        if (state == 0)
        {
          // Integer case
          int value;
          bool success = int.TryParse(substring.Substring(0, length), out value);

          if (success) return (new Token(TokenType.Integer, value), length);
          else error = true;
        }
        else if (state == 1 || state == 2 || state == 5)
        {
          // Double case
          double value;
          bool success = double.TryParse(substring.Substring(0, length), out value);

          if (success) return (new Token(TokenType.Double, value), length);
          else error = true;
        }
      }
      
      if (error)
      {
        throw new Exception("Source code error at line # "
          + (line_number + 1) + ". Unable to process \""
          + substring + "\" as a number.");
      }

      // code should not get here - just to satisfy the compiler
      throw new Exception("Source code error at line # "
          + (line_number + 1) + ". Unable to process \""
          + substring + "\" as a number.");
    }


    /// <summary>
    /// Parse token as a identifier by checking for name convention
    /// violation. Since this is the last resort for accepting a 
    /// token, this function will throw an exception if unable to 
    /// parse as an identifier.
    /// </summary>
    static public Token parse_identifier(string token, int line_number)
    {
      // Standard identifier processing.
      // Identifier cannot start with '$' sign, nor digit.
      char first_char = token[0];
      if (char.IsDigit(token[0]) || first_char == '$')
        throw new Exception("Source code error at line # "
              + (line_number + 1) + ". The identifier \"" + token 
              + "\" cannot start with a digit nor a dollar sign.");

      return new Token(TokenType.Identifier, token);
    }

    /// <summary>
    /// Create a string token using the provided "token" string. This
    /// doesn't do any check so it always succeeds.
    /// </summary>
    static public Token create_string_token(string token)
    {
      return new Token(TokenType.String, token);
    }

    /// <summary>
    /// Creates a system variable. This must start with a '$' to
    /// avoid conflict with the normal variables.
    /// </summary>
    static public Token create_system_var(string var_name)
    {
      if (var_name.StartsWith('$') == false)
        throw new Exception("Attemptting to create a system variable that does not start with a '$'.");

      return new Token(TokenType.Identifier, var_name);
    }

    /// <summary>
    /// Returns true if the given character 'c' mark the end of a
    /// token.
    /// </summary>
    static public bool is_token_end(char c)
    {
      if (char.IsWhiteSpace(c)) return true;
      if (symbol_mappings.ContainsKey(c)) return true;
      if (c == '!' || c == '#' || c == '"' || c == '\'') return true;

      return false;
    }


    public override string ToString()
    {
      if (type == TokenType.String)
        return "\"" + value.ToString() + "\"";

      else
        return value.ToString();
    }
  }
}
