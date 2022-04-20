using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X1.Compiler
{
  class Parser
  {
    internal List<Statement> statements = new List<Statement>();
    internal TokenList[] intermediate_code;
    internal Dictionary<string, int> user_def_function_locations = new Dictionary<string, int>();

    static TokenType[][] simplification_patterns;

    // handlers for the simplification patterns
    delegate void SimplificationHandler(LinkedListNode<TokenList> node, int pattern_start_index);
    static SimplificationHandler[] simplification_handlers;

    static readonly int repeating_patterns; // patterns starting at this index can repeat or nest
    static readonly int[] pattern_min_length; // minimum length of "simplification_patterns"

    // Temporary variable management    
    class TempVar
    {
      // parallel array to track highest variable numbers used at various indentations
      static List<int> indentations = new List<int>();
      static List<int> highest_var_numbers = new List<int>();

      static TempVar()
      {
        // starting condition is no var_number in use at indentation 0
        indentations.Add(0);
        highest_var_numbers.Add(-1);
      }

      public static string get_var_name(int indentation)
      {
        // find the index corresponding to equal or lower indentation
        int index = indentations.Count - 1;
        while(index >= 0)
        {
          if (indentations[index] <= indentation) break;
          index--;
        }

        // remove all higher indentation values
        if (index < indentations.Count - 1)
        {
          indentations.RemoveRange(index + 1, indentations.Count - index - 1);
          highest_var_numbers.RemoveRange(index + 1, highest_var_numbers.Count - index - 1);
        }

        // var_number is one higher than the highest number in use
        int var_number = highest_var_numbers[index] + 1;

        // create new indentation entry if necessary
        if (indentations[index] < indentation)
        {
          indentations.Add(indentation);
          highest_var_numbers.Add(var_number);
        }
        else
        {
          // Handle: indentations[index] == indentation
          // Record the new highest var number in use
          highest_var_numbers[index] = var_number;
        }
        
        string var_name = "$temp" + var_number;
        return var_name;
      }

      public static void reset(int indentation)
      {
        if (indentation == 0)
        {
          // base case - back to the starting condition
          if (indentations.Count > 1)
          {
            indentations.RemoveRange(1, indentations.Count - 1);
            highest_var_numbers.RemoveRange(1, highest_var_numbers.Count - 1);
          }
          // indentations[0] = 0; --- this should never change
          highest_var_numbers[0] = -1;
        }
        else
        {
          // Note this is for indentation > 0.
          // Find the index corresponding to equal or lower indentation
          int index = indentations.Count - 1;
          while (index >= 0)
          {
            if (indentations[index] <= indentation) break;
            index--;
          }

          // Remove all EQUAL and higher indentation values
          // At this point, "index" is at equal or lower indentation.
          if (indentations[index] < indentation)
            index++;

          // At this point, "index" should be at equal or higher indentation
          if (index < indentations.Count)
          {
            indentations.RemoveRange(index, indentations.Count - index);
            highest_var_numbers.RemoveRange(index, highest_var_numbers.Count - index);
          }
        }
      }

      static public string to_string()
      {
        var sb = new StringBuilder();
        sb.Append("Indentations".PadRight(15));
        sb.AppendLine("Highest Var Number in Use");

        for(int i = 0; i < indentations.Count; i++)
        {
          sb.Append("     " + indentations[i].ToString().PadRight(15));
          sb.AppendLine(highest_var_numbers[i].ToString());
        }

        return sb.ToString().TrimEnd();
      }
    }



    // Label address management
    public class LabelAddresses
    {
      static List<int> addresses = new List<int>();

      static LabelAddresses()
      {
        addresses.Add(-1); // index 0 is not used
      }

      /// <summary>
      /// Returns a label ID.
      /// </summary>
      public static int allocate_label()
      {
        addresses.Add(-1);
        return addresses.Count - 1;
      }

      public static void set_label_address(int label_id, int address)
      {
        addresses[label_id] = address;
      }

      public static int get_label_address(int label_id)
      {
        return addresses[label_id];
      }
    }


    static Parser()
    {
      var patterns = new List<TokenType[]>();
      var handlers = new List<SimplificationHandler>();

      // +=, -=, *=, /= patterns - simple version, such as: x += 1
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.Add_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.Subtract_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.Multiply_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.Divide_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);

      // +=, -=, *=, /= patterns - bracket expression version, such as x[2] += 1
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.BracketExpr, TokenType.Add_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.BracketExpr, TokenType.Subtract_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.BracketExpr, TokenType.Multiply_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.BracketExpr, TokenType.Divide_Equal, TokenType.OneOrMore });
      handlers.Add(simplify_plus_equal);

      // simplification of "while" statement - this must come before the 
      // "simplify_single_line_if" patterns, since "simplify_while(...)" will call
      // "simplify_single_line_if(...)", but not vice versa.
      patterns.Add(new TokenType[] { TokenType.While, TokenType.OneOrMore });
      handlers.Add(simplify_while);

      // single line "if", "elif", "else", "for", "while"
      patterns.Add(new TokenType[] { TokenType.If, TokenType.OneOrMore, TokenType.Colon, TokenType.OneOrMore });
      handlers.Add(simplify_single_line_if);
      patterns.Add(new TokenType[] { TokenType.Elif, TokenType.OneOrMore, TokenType.Colon, TokenType.OneOrMore });
      handlers.Add(simplify_single_line_if);
      patterns.Add(new TokenType[] { TokenType.Else, TokenType.Colon, TokenType.OneOrMore });
      handlers.Add(simplify_single_line_if);
      patterns.Add(new TokenType[] { TokenType.For, TokenType.OneOrMore, TokenType.Colon, TokenType.OneOrMore });
      handlers.Add(simplify_single_line_if);

      // factor out complex iterator variables in for loops
      // example: for i in x[1:3]: --> for i in $temp0
      patterns.Add(new TokenType[] { TokenType.For, TokenType.AnyOne, TokenType.In, TokenType.TwoOrMore, TokenType.Colon });
      handlers.Add(simplify_for_loop_iterator);
      
      // ******** repeating patterns start here 
      repeating_patterns = patterns.Count;

      // function call: f(...) that does not start at the very beginning
      patterns.Add(new TokenType[] { TokenType.NotNextToken, TokenType.Def, TokenType.Identifier, TokenType.Left_Paren });
      handlers.Add(simplify_function);     

      // string method call: "abc".isdigit(...)
      patterns.Add(new TokenType[] { TokenType.String, TokenType.Period, TokenType.Identifier, TokenType.Left_Paren });
      handlers.Add(simplify_string_method_call);

      // slice notation method call: x[2:].index(2)
      patterns.Add(new TokenType[] { TokenType.Identifier, TokenType.BracketExpr, TokenType.Period, TokenType.Identifier, TokenType.Left_Paren });
      handlers.Add(simplify_slice_method_call);           


      simplification_patterns = patterns.ToArray();
      simplification_handlers = handlers.ToArray();

      // Compute "pattern_min_length" - to avoid unnecessary pattern recognition attempts
      pattern_min_length = new int[simplification_patterns.Length];
      for(int i = 0; i < simplification_patterns.Length; i++)
      {
        int length = 0;
        for (int j = 0; j < simplification_patterns[i].Length; j++)
        {
          // increase length based on token type
          if (simplification_patterns[i][j] == TokenType.ZeroOrMore
            && simplification_patterns[i][j] == TokenType.NotNextToken)
          {
            // length is zero for these tokens
          }
          else if (simplification_patterns[i][j] == TokenType.TwoOrMore)
            length += 2; // this token has length 2 or more
          else
          {
            // most tokens carry a minimum length of 1
            length++;
          }
        }
        pattern_min_length[i] = length;
      }
    }
    
    
    public Parser(List<TokenList> token_lists, bool print_intermediate_code = false)
    {
      create_intermediate_code(token_lists);

      if (print_intermediate_code)
      {
        Console.WriteLine("Intermediate code:");
        foreach (var token_list in intermediate_code)
          Console.WriteLine(token_list.ToString());

        Console.WriteLine();
      }

      construct_statements(0, intermediate_code.Length - 1, null, null);

      // resolve addresses of the control flow (if, while, for) statements
      foreach (var statement in statements)
        statement.resolve_addresses();
    }


    void create_intermediate_code(List<TokenList> token_lists)
    {
      var i_code = new LinkedList<TokenList>();

      foreach (var code in token_lists)
      {
        TempVar.reset(code.indentation);
        var node = i_code.AddLast(code);
        check(node);
      }

      intermediate_code = i_code.ToArray();
    }


    /// <summary>
    /// Check the code at node for simplifications.
    /// </summary>
    /// <param name="token_list_node"></param>
    static void check(LinkedListNode<TokenList> node)
    {
      var token_list = node.Value;

      // check for "{}", "[]", and "()" imbalance.
      token_list.count_braces(excessive_left_token_error: true);
      token_list.count_brackets(excessive_left_token_error: true);
      token_list.count_parenthesis(excessive_left_token_error: true);

      // Handle patterns that start at the very beginning of the line
      for (int i = 0; i < repeating_patterns; i++)
      {
        if (node.Value.match_pattern(simplification_patterns[i], 0))
        {
          simplification_handlers[i](node, 0);
          break; // usually these patterns are one off. Exception is the "while"
          // loop's handling. In that case, "simplify_while(...)" calls 
          // "simplify_single_line_if(...)" directly.
        }
      }
      
      // Handle patterns that can repeat or nest
      int index = 0;
      while(index < node.Value.Count)
      {
        // match for various patterns, as "index" increases from left to right
        bool pattern_found = false;

        // go through the patterns:
        for (int i = repeating_patterns; i < simplification_patterns.Length; i++)
        {
          if (node.Value.match_pattern(simplification_patterns[i], index))
          {
            pattern_found = true;
            simplification_handlers[i](node, index);
            break;
          }
        }

        // Every time a new pattern is found, restart the pattern matching for 
        // the current line.
        if (pattern_found) index = 0;
        else index++;
      }
    }


    /// <summary>
    /// <para>"x += y" --> "x = x + y"</para>
    /// <para>"x -= y" --> "x = x - y"</para>
    /// </summary>
    static void simplify_plus_equal(LinkedListNode<TokenList> node, int pattern_start_index)
    {      
      var token_list = node.Value;

      // Find where the +=, -=, *=, or /= is:
      int equal_sign_location = pattern_start_index + 1;

      for (int i = pattern_start_index + 1; i < token_list.Count - 1; i++)
      {
        if (token_list[i].type == TokenType.Add_Equal
          || token_list[i].type == TokenType.Subtract_Equal
          || token_list[i].type == TokenType.Multiply_Equal
          || token_list[i].type == TokenType.Divide_Equal)
          equal_sign_location = i;
      }

      var new_code = new TokenList(token_list.line_number, token_list.indentation);

      new_code.add_token_list(token_list, 0, equal_sign_location - 1);  // x
      new_code.add_token(Token.Constants.Assign);                       // =
      new_code.add_token_list(token_list, 0, equal_sign_location - 1);  // x

      // choose between +, -, *, /
      if (token_list[equal_sign_location].type == TokenType.Add_Equal)
        new_code.add_token(Token.Constants.Add);                                  
      else if (token_list[equal_sign_location].type == TokenType.Subtract_Equal)
        new_code.add_token(Token.Constants.Subtract);
      else if (token_list[equal_sign_location].type == TokenType.Multiply_Equal)
        new_code.add_token(Token.Constants.Multiply);
      else if (token_list[equal_sign_location].type == TokenType.Divide_Equal)
        new_code.add_token(Token.Constants.Divide);

      new_code.add_token_list(token_list, equal_sign_location + 1);     // y

      node.Value = new_code;
    }


    /// <summary>
    /// Simplify "x: z" into "x:" and "z". The "x" can be something like
    /// "if", "elif", "else", "while", or "for".
    /// </summary>
    static void simplify_single_line_if(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var old_code = node.Value;
      int colon_index = old_code.find_token(TokenType.Colon, pattern_start_index);

      // The pre-colon line of code
      var pre_colon_code = new TokenList(old_code.line_number, old_code.indentation);
      pre_colon_code.add_token_list(old_code, 0, colon_index);

      // The post colon line of code
      var post_colon_code = new TokenList(old_code.line_number, old_code.indentation + 4);
      post_colon_code.add_token_list(old_code, colon_index + 1, old_code.Count - 1);

      node.Value = pre_colon_code;
      var post_colon_node = node.List.AddAfter(node, post_colon_code);
      check(post_colon_node);
    }


    /// <summary>
    /// Simplify "while x: z" into "while", "while x: z". Will trigger
    /// "simplify_single_line_if" as needed to break the one line
    /// "while" into two lines.
    /// </summary>
    static void simplify_while(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var while_statement = node.Value;

      // Create a "while" code before the current code.
      var while_start = new TokenList(while_statement.line_number, while_statement.indentation);
      while_start.add_token(Token.Constants.While);
      node.List.AddBefore(node, while_start);

      // Call "simplify_single_line_if" if needed
      bool single_line_while = while_statement.match_pattern(
        new TokenType[] { TokenType.While, TokenType.OneOrMore, TokenType.Colon, TokenType.OneOrMore }, 0);

      if (single_line_while)
        simplify_single_line_if(node, 0);
    }


    /// <summary>
    /// Simplify "for x in y:" into "$temp0 = y", and "for x in $temp0:".
    /// This applies only if y is two or more tokens.
    /// </summary>
    static void simplify_for_loop_iterator(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var old_code = node.Value;

      // find the extent of y, the iterator variable
      int iterator_start = old_code.find_token(TokenType.In, 0) + 1;
      int iterator_end = old_code.find_token_outside_of_paren(
        TokenType.Colon, iterator_start + 1, old_code.Count - 1) - 1;
            
      // new code:
      // $temp0 = y      
      var iterator_assign = new TokenList(old_code.line_number, old_code.indentation);      
      var temp_var = Token.create_system_var(TempVar.get_var_name(old_code.indentation));

      iterator_assign.add_token(temp_var);
      iterator_assign.add_token(Token.Constants.Assign);
      iterator_assign.add_token_list(old_code, iterator_start, iterator_end);

      // for x in $temp0:
      var new_code = new TokenList(old_code.line_number, old_code.indentation);

      new_code.add_token_list(old_code, 0, iterator_start - 1);
      new_code.add_token(temp_var);
      new_code.add_token_list(old_code, iterator_end + 1);

      // insert new code into the LinkedList
      var iterator_assign_node = node.List.AddBefore(node, iterator_assign);
      node.Value = new_code;

      // The caller, which is check(...), will repeat its check on the current
      // node (which is new_code). This function needs to call check(...) 
      // on any other newly generated node.
      check(iterator_assign_node);
    }


    /// <summary>
    /// Simplify "x f() y" into "f()", "$temp0 = $return", and "x $temp0 y"
    /// </summary>
    static void simplify_function(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var old_code = node.Value;
      int function_start = pattern_start_index + 1;

      // new code:
      // f()
      var f_call = new TokenList(old_code.line_number, old_code.indentation);

      int function_end = old_code.find_ending_token(function_start + 1);
      if (function_end < function_start)
        throw new CompilerException("Source code error on line "
          + (old_code.line_number + 1) + ". The closing ')' cannot be found.", (old_code.line_number + 1));

      f_call.add_token_list(old_code, function_start, function_end);

      // $temp0 = $return
      var return_assignment = new TokenList(old_code.line_number, old_code.indentation);
      var temp_var = Token.create_system_var(TempVar.get_var_name(old_code.indentation));

      return_assignment.add_token(temp_var);
      return_assignment.add_token(Token.Constants.Assign);
      return_assignment.add_token(Token.create_system_var("$return"));

      // x $temp0 y
      var new_code = new TokenList(old_code.line_number, old_code.indentation);

      new_code.add_token_list(old_code, 0, function_start - 1);
      new_code.add_token(temp_var);
      new_code.add_token_list(old_code, function_end + 1);

      // insert new code into the LinkedList
      var f_call_node = node.List.AddBefore(node, f_call);
      node.List.AddBefore(node, return_assignment);
      node.Value = new_code;

      // The caller, which is check(...), will repeat its check on the current
      // node (which is new_code). This function needs to call check(...) 
      // on any other newly generated node.
      check(f_call_node);
    }


    /// <summary>
    /// Simplify {x "str".f( y} into {$temp0 = "str"; x $temp0.f ( y}
    /// </summary>
    static void simplify_string_method_call(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var old_code = node.Value;
      int string_location = pattern_start_index;

      // new code:
      // $temp0 = str
      var temp_var = Token.create_system_var(TempVar.get_var_name(old_code.indentation));

      var str_assignment = new TokenList(old_code.line_number, old_code.indentation);
      str_assignment.add_token(temp_var);
      str_assignment.add_token(Token.Constants.Assign);
      str_assignment.add_token(old_code[string_location]);

      // x $temp0.f (
      // The new method_call, "$temp0.f", is a single token.
      string method_call = temp_var.value.ToString() + '.' + old_code[string_location + 2].value.ToString();

      var new_code = new TokenList(old_code.line_number, old_code.indentation);

      new_code.add_token_list(old_code, 0, string_location - 1);
      new_code.add_token(Token.create_system_var(method_call));
      new_code.add_token_list(old_code, string_location + 3);

      // insert new code into the LinkedList
      node.List.AddBefore(node, str_assignment);
      node.Value = new_code;
    }


    /// <summary>
    /// Simplify {x s[...] . f( y} into {$temp0 = s[...]; x $temp0.f ( y}
    /// </summary>
    static void simplify_slice_method_call(LinkedListNode<TokenList> node, int pattern_start_index)
    {
      var old_code = node.Value;
      int slice_start = pattern_start_index;
      int slice_end = old_code.find_ending_token(slice_start + 1);

      // keep searching for more '[', so to handle "x[:][0:3]" situation
      while(slice_end < old_code.Count -1 
        && old_code[slice_end + 1].type == TokenType.Left_Bracket)
      {
        slice_end = old_code.find_ending_token(slice_end + 1);
      }

      // new code:
      // $temp0 = s[...]
      var temp_var = Token.create_system_var(TempVar.get_var_name(old_code.indentation));

      var slice_assignment = new TokenList(old_code.line_number, old_code.indentation);
      slice_assignment.add_token(temp_var);
      slice_assignment.add_token(Token.Constants.Assign);
      for (int i = slice_start; i <= slice_end; i++)
        slice_assignment.add_token(old_code[i]);

      // x $temp0.f (
      // The new method_call, "$temp0.f", is a single token.
      string method_call = temp_var.value.ToString() + '.' + old_code[slice_end + 2].value.ToString();

      var new_code = new TokenList(old_code.line_number, old_code.indentation);

      new_code.add_token_list(old_code, 0, slice_start - 1);
      new_code.add_token(Token.create_system_var(method_call));
      new_code.add_token_list(old_code, slice_end + 3);

      // insert new code into the LinkedList
      var slice_assignment_node = node.List.AddBefore(node, slice_assignment);
      node.Value = new_code;

      // The caller, which is check(...), will repeat its check on the current
      // node (which is new_code). This function needs to call check(...) 
      // on any other newly generated node.
      check(slice_assignment_node);
    }


    /// <summary>
    /// Parse intermediate_code from "start_index" to "end_index".
    /// Add to "statements".
    /// </summary>
    /// <param name="break_addr">The jump target for the "break" statement.</param>
    /// <param name="continue_addr">The jump target for the "continue" statement.</param>
    void construct_statements(int start_index, int end_index, int? break_addr, 
      int? continue_addr)
    {
      int index = start_index;

      while (index <= end_index)
      {
        var current_line = intermediate_code[index];
        bool not_processed = true;

        try
        {
          // x = expression
          if (not_processed && current_line.Count >= 3)
          {
            if (current_line[0].type == TokenType.Identifier
              && current_line[1].type == TokenType.Assign)
            {
              statements.Add(new AssignStatement(current_line));
              not_processed = false;
              index++;
            }
          }

          // x[0] = expression
          TokenType[] slice_assignment_pattern = {
            TokenType.Identifier, TokenType.BracketExpr, TokenType.Assign };

          if (not_processed && current_line.match_pattern(slice_assignment_pattern, 0))
          {
            statements.Add(new SliceAssignStatement(current_line));
            not_processed = false;
            index++;
          }

          // f(), f( expression )
          if (not_processed && current_line.Count >= 3)
          {
            if (current_line[0].type == TokenType.Identifier
              && current_line[1].type == TokenType.Left_Paren
              && current_line[-1].type == TokenType.Right_Paren)
            {
              statements.Add(new FunctionCallStatement(current_line));
              not_processed = false;
              index++;
            }
          }

          // x[expression]
          if (not_processed && current_line.Count >= 4)
          {
            if (current_line[0].type == TokenType.Identifier
              && current_line[1].type == TokenType.Left_Bracket
              && current_line[-1].type == TokenType.Right_Bracket)
            {
              statements.Add(new FunctionCallStatement(current_line));
              not_processed = false;
              index++;
            }
          }

          // if statement
          if (not_processed && current_line[0].type == TokenType.If)
          {
            process_if_statement(ref index, break_addr, continue_addr);
            not_processed = false;
          }

          // while statement
          if (not_processed && current_line[0].type == TokenType.While)
          {
            process_while_statement(ref index);
            not_processed = false;
          }

          // break statement
          if (not_processed && current_line[0].type == TokenType.Break)
          {
            if (break_addr == null)
              throw new Exception("\"break\" statement used without enclosing while or for statement.");

            statements.Add(new JumpStatement(break_addr.Value, current_line.indentation, current_line.line_number));
            not_processed = false;
            index++;
          }

          // continue statement
          if (not_processed && current_line[0].type == TokenType.Continue)
          {
            if (continue_addr == null)
              throw new Exception("\"continue\" statement used without enclosing while or for statement.");

            statements.Add(new JumpStatement(continue_addr.Value, current_line.indentation, current_line.line_number));
            not_processed = false;
            index++;
          }

          // for statement
          if (not_processed && current_line[0].type == TokenType.For)
          {
            process_for_statement(ref index);
            not_processed = false;
          }

          // function definition statement
          if (not_processed && current_line[0].type == TokenType.Def)
          {
            process_def_statement(ref index);
            not_processed = false;
          }

          // return statement
          if (not_processed && current_line[0].type == TokenType.Return)
          {
            statements.Add(new ReturnStatement(current_line));
            not_processed = false;
            index++;
          }

          // global statement
          if (not_processed && current_line[0].type == TokenType.Global)
          {
            statements.Add(new GlobalStatement(current_line));
            not_processed = false;
            index++;
          }

          // other keywords
          if (not_processed)
          {
            if (current_line[0].type == TokenType.Del)
            {
              statements.Add(new KeywordStatement(current_line));
              not_processed = false;
              index++;
            }
          }
          
          if (not_processed)
            throw new Exception("Statement not supported.");
        }
        catch (Exception ex)
        {
          throw new CompilerException("Source code error on line "
                + (current_line.line_number + 1) + ". Unable to process \""
                + current_line.ToString() + "\" into an expression. Details: "
                + ex.Message, (current_line.line_number + 1));
        }
      }
    }


    /// <summary>
    /// Process the "if" statement at "intermediate_code[index]".
    /// </summary>
    void process_if_statement(ref int index, int? break_addr, int? continue_addr)
    {
      int end_if_final_label = LabelAddresses.allocate_label();
      bool final_if_block = false;

      while(final_if_block == false)
      {
        // "index" points to the current "if" line
        // Find the end of the current if block
        int end_index = find_next_line_with_equal_or_less_indentation(index);
          
        var current_if_line = intermediate_code[index];

        TokenList next_block_line = null;
        if(end_index < intermediate_code.Length)
          next_block_line = intermediate_code[end_index];

        // Determine whether "next_block_line" is a continuation of the "if" statement.
        if (next_block_line == null)
          final_if_block = true; // end of the program 

        else if (next_block_line[0].type != TokenType.Elif
          && next_block_line[0].type != TokenType.Else)
          final_if_block = true; // something other than "elif" and "else"

        else if (next_block_line.indentation < current_if_line.indentation)
          final_if_block = true; // end of if block due to indentation

        // end_if_final --- label ID for the final "if" block's end address
        // end_if --- label ID for the current "if" block's end address
        int end_if_label = end_if_final_label;
        if (final_if_block == false)
          end_if_label = LabelAddresses.allocate_label();

        // Generate statement for the current if line
        if (current_if_line[0].type == TokenType.If
          || current_if_line[0].type == TokenType.Elif)
        {
          int true_addr = statements.Count + 1;
          statements.Add(new ConditionalStatement(current_if_line, true_addr, -1 * end_if_label));
        }
        else if (current_if_line[0].type == TokenType.Else)
        {
          // do nothing
        }
        
        // Check for empty "if" body block
        if (end_index - index <= 1)
        {
          if (current_if_line[0].type == TokenType.If)
            throw new Exception("The if statement body is empty.");
          else if (current_if_line[0].type == TokenType.Elif)
            throw new Exception("The elif statement body is empty.");
          else
            throw new Exception("The else statement body is empty.");
        }

        // Generate statements for the "if" block body
        construct_statements(index + 1, end_index - 1, break_addr, continue_addr);

        // Generate statement for the end of the "if" block
        if (final_if_block == false)
        {
          int indentation = intermediate_code[end_index - 1].indentation;
          int line_number = intermediate_code[end_index - 1].line_number;
          statements.Add(new JumpStatement(-1 * end_if_final_label, indentation, line_number));
        }

        // Record the current address as the "end_if" address
        LabelAddresses.set_label_address(end_if_label, statements.Count);

        // Set "index" for the future
        index = end_index;
      }
      
      LabelAddresses.set_label_address(end_if_final_label, statements.Count);
    }


    /// <summary>
    /// Process the "while" statement at "intermediate_code[index]".
    /// </summary>
    void process_while_statement(ref int index)
    {
      int start_while = statements.Count;
      int end_while_label = LabelAddresses.allocate_label();

      int start_index = index + 1;

      // "start_index" is pointing to the first "while" statement, which marks
      // the start of the while loop. The "while conditional:" line is further down.
      // Search for the "while conditional:" line.
      int conditional_index = start_index;
      while(conditional_index < intermediate_code.Length)
      {
        if (intermediate_code[conditional_index][0].type == TokenType.While)
          break;
        conditional_index++;
      }

      // Generate statement for the code from "start_index" to 
      // "conditional_index"-1, if necessary
      if (start_index < conditional_index)
      {
        construct_statements(start_index, conditional_index - 1, -1 * end_while_label, start_while);
      }

      // Generate statement for the "while conditional:" line
      var conditional_line = intermediate_code[conditional_index];
      int true_addr = statements.Count + 1;
      statements.Add(new ConditionalStatement(conditional_line, true_addr, -1 * end_while_label));

      // Find the end of the "while" body
      int end_index = find_next_line_with_equal_or_less_indentation(conditional_index);

      // Check for empty "while" body block
      if (end_index - conditional_index <= 1)
        throw new Exception("The while statement body is empty.");

      // Generate statement for the "while" body
      construct_statements(conditional_index + 1, end_index - 1, break_addr:-1*end_while_label, 
        continue_addr:start_while);

      // Generate statement for the end of the "while" body
      int indentation = conditional_line.indentation + 4;
      int line_number = intermediate_code[end_index - 1].line_number;
      statements.Add(new JumpStatement(start_while, indentation, line_number));

      index = end_index;
      LabelAddresses.set_label_address(end_while_label, statements.Count);
    }


    /// <summary>
    /// Process the "for" statement at "intermediate_code[index]".
    /// </summary>
    void process_for_statement(ref int index)
    {
      var for_statement = intermediate_code[index];

      // extract the iterator and iteration variable
      var iteration_var = for_statement[1];
      var iterator = for_statement[3];

      // add statement to reset the iterator
      statements.Add(new FunctionCallStatement(iterator.ToString(), "reset", null,
        for_statement.indentation, for_statement.line_number));

      // mark the start of the for statement
      int start_for = statements.Count;

      // call iterator.has_next_element()
      statements.Add(new FunctionCallStatement(iterator.ToString(), "has_next_element", null,
        for_statement.indentation, for_statement.line_number));
      
      // add conditional for the "for" decision
      int end_for_label = LabelAddresses.allocate_label();

      var for_condition = new TokenList(for_statement.line_number, for_statement.indentation);
      for_condition.add_token(Token.Constants.If);
      for_condition.add_token(Token.create_system_var("$return"));
      for_condition.add_token(Token.Constants.Colon);

      int true_addr = statements.Count + 1;

      statements.Add(new ConditionalStatement(for_condition, true_addr, -1 * end_for_label));

      // call iterator.next_element()
      statements.Add(new FunctionCallStatement(iterator.ToString(), "next_element", null,
        for_statement.indentation, for_statement.line_number));

      // iteration_var = iterator
      var assign_iteration_var = new TokenList(for_statement.line_number, for_statement.indentation);
      assign_iteration_var.add_token(iteration_var);
      assign_iteration_var.add_token(Token.Constants.Assign);
      assign_iteration_var.add_token(Token.create_system_var("$return"));

      statements.Add(new AssignStatement(assign_iteration_var));

      // identify the for loop body
      int end_index = find_next_line_with_equal_or_less_indentation(index);

      // Check for empty "for" body block
      if (end_index - index <= 1)
        throw new Exception("The for statement body is empty.");

      // generate code for "for" loop body
      construct_statements(index + 1, end_index - 1, break_addr:-1*end_for_label, 
        continue_addr:start_for);

      // generate Jump at the end of the "for" loop body
      int indentation = for_statement.indentation + 4;
      int line_number = intermediate_code[end_index - 1].line_number;
      statements.Add(new JumpStatement(start_for, indentation, line_number));
      
      index = end_index;
      LabelAddresses.set_label_address(end_for_label, statements.Count);
    }
    

    /// <summary>
    /// Process the "for" statement at "intermediate_code[index]".
    /// </summary>
    void process_def_statement(ref int index)
    {
      var def_statement = intermediate_code[index];

      // A jump statement that goes to the end of the function
      int end_func_label = LabelAddresses.allocate_label();
      statements.Add(new JumpStatement(-1 * end_func_label, def_statement.indentation, def_statement.line_number));

      // Syntax and assumption check
      if (def_statement[1].type != TokenType.Identifier)
        throw new Exception("Function definition syntax error. \"" 
          + def_statement[1].ToString() + "\" not an identifier.");

      if (def_statement[2].type != TokenType.Left_Paren)
        throw new Exception("Function definition syntax error. \"" 
          + def_statement[2].ToString() + "\" is expected to be '('.");

      if (def_statement[-1].type != TokenType.Colon)
        throw new Exception("Function definition syntax error. \"" 
          + "The function definition does not end with ':'.");

      if (def_statement[-2].type != TokenType.Right_Paren)
        throw new Exception("Function definition syntax error. \"" 
          + "The second last character is expected to be ')'.");

      // Process function name
      string function_name = (string)def_statement[1].value;

      if (user_def_function_locations.ContainsKey(function_name))
        throw new Exception("The function name \"" + function_name + "\" is already in use.");

      user_def_function_locations.Add(function_name, statements.Count);

      // Create a function argument assignment statement for each argument
      int arg_index = 3; // def f ( arg0 -- start at arg0, token #3
      int position_number = 0;
      
      while(arg_index < def_statement.Count - 2)
      {
        // check that arg_index is an identifier token
        if (def_statement[arg_index].type != TokenType.Identifier)
          throw new Exception("Syntax error in function arguments. Expecting \""
            + def_statement[arg_index].ToString() + "\" to be a variable name.");

        // simple argument case - without default argument, it's either [arg0 ,] or [arg0 )]
        if (def_statement[arg_index + 1].type == TokenType.Comma
          || def_statement[arg_index + 1].type == TokenType.Right_Paren)
        {
          statements.Add(new FunctionArgAssignStatement(position_number, def_statement, arg_index, arg_index));
          arg_index += 2;
          position_number++;
        }

        // simple argument plus type hint: [arg0 : str]
        else if (def_statement[arg_index + 1].type == TokenType.Colon
          && def_statement[arg_index + 2].type == TokenType.Identifier
          && (def_statement[arg_index + 3].type == TokenType.Comma
          || def_statement[arg_index + 3].type == TokenType.Right_Paren))
        {
          statements.Add(new FunctionArgAssignStatement(position_number, def_statement, arg_index, arg_index));
          arg_index += 4;
          position_number++;
        }
        
        // default argument case
        else if (def_statement[arg_index + 1].type == TokenType.Assign)
        {
          // The end of the default argument could be a ',' or a ')'.
          // In the case of ')', need to track '(' and ')' balance - since
          // "x=(1+2)*3" is allowed in the default argument.
          int arg_end_index = arg_index + 2;
          int paren_balance = 0; // +1 for every '(' seen
          
          while (arg_end_index < def_statement.Count - 1)
          {
            // t = current token
            var t = def_statement[arg_end_index];

            if (t.type == TokenType.Comma)
              break;
            else if (t.type == TokenType.Left_Paren)
              paren_balance++;
            else if (t.type == TokenType.Right_Paren)
            {
              paren_balance--;
              if (paren_balance < 0) break;
            }

            arg_end_index++;
          }

          // Look for errors with "arg_end_index"          
          // Check for parenthesis imbalance
          if (def_statement[arg_end_index].type == TokenType.Comma
            && paren_balance != 0)
            throw new Exception("The default argument has parenthesis imbalance.");

          if (def_statement[arg_end_index].type == TokenType.Right_Paren
            && paren_balance != -1)
            throw new Exception("The default argument has parenthesis imbalance.");

          // Check empty default statement
          if (arg_end_index == arg_index + 2)
            throw new Exception("The default argument statement is empty.");

          // Add argument assignment statement
          statements.Add(new FunctionArgAssignStatement(position_number, def_statement, arg_index, arg_end_index - 1));
          arg_index = arg_end_index + 1;
          position_number++;
        }

        else
        {
          throw new Exception("Syntax error in function arguments.");
        }
      }

      // Look for the end of the function block
      int end_index = find_next_line_with_equal_or_less_indentation(index);

      // Check for empty "def" body block
      if (end_index - index <= 1)
        throw new Exception("The " + def_statement.ToString().Trim() 
          + " statement body is empty.");

      // Generate statements for the function body
      construct_statements(index + 1, end_index - 1, null, null);

      // If there is no return statement, add one
      var last_func_statement = statements[statements.Count - 1];

      if (last_func_statement.Type != StatementType.Return)
        statements.Add(new ReturnStatement(last_func_statement.Indentation, last_func_statement.LineNumber));

      index = end_index;
      LabelAddresses.set_label_address(end_func_label, statements.Count);
    }
    

    /// <summary>
    /// Starting at line "intermediate_code[index]", scan forward and 
    /// find the next line that has equal or less indentation.
    /// </summary>
    int find_next_line_with_equal_or_less_indentation(int index)
    {
      int end_index = index + 1;
      int current_indentation = intermediate_code[index].indentation;

      while (end_index < intermediate_code.Length)
      {
        if (intermediate_code[end_index].indentation <= current_indentation)
          return end_index;

        end_index++;
      }
      return end_index;
    }

  }
}
