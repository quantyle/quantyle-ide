using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X1.Compiler
{
  interface Statement
  {
    StatementType Type { get; }
    int Indentation { get; }
    int LineNumber { get; }
    void resolve_addresses();
  }

  enum StatementType { Assign, Conditional, Function_Arg_Assign,
    Function_Call, Global, Jump, Keyword, Return, SliceAssign }



  class AssignStatement : Statement
  {
    public StatementType Type { get { return StatementType.Assign; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly string lvalue;
    public readonly Expression expression;

    public AssignStatement(TokenList token_list)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      lvalue = (string)token_list[0].value;
      expression = new Expression(token_list, 2, token_list.Count - 1);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append(lvalue + " = " + expression);
      return sb.ToString();
    }
  }


  class ConditionalStatement : Statement
  {
    public StatementType Type { get { return StatementType.Conditional; } }
    public int Indentation { get; }
    public int LineNumber { get; }

    public Expression expression;

    public int TrueAddr { get; private set; }
    public int FalseAddr { get; private set; }

    /// <summary>
    /// Resolve any invalid (negative) addresses using "LabelAddresses".
    /// </summary>
    public void resolve_addresses()
    {
      if (TrueAddr < 0)
        TrueAddr = Parser.LabelAddresses.get_label_address(TrueAddr * -1);

      if (FalseAddr < 0)
        FalseAddr = Parser.LabelAddresses.get_label_address(FalseAddr * -1);
    }

    
    /// <summary>
    /// Conditional statement constructor.
    /// </summary>
    /// <param name="token_list">The conditional expression is assumed
    /// to be from the second index to the second last index.</param>
    /// <param name="true_addr">The address to jump to if the conditional
    /// expression is true.</param>
    /// <param name="false_addr">The address to jump to if the conditional
    /// expression is false.</param>
    public ConditionalStatement(TokenList token_list, int true_addr, int false_addr)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      expression = new Expression(token_list, 1, token_list.Count - 2);

      TrueAddr = true_addr;
      FalseAddr = false_addr;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append("if(" + expression + ") ; true_addr="
        + TrueAddr + " ; false_addr=" + FalseAddr);

      return sb.ToString();
    }
  }


  class FunctionArgAssignStatement : Statement
  {
    public StatementType Type { get { return StatementType.Function_Arg_Assign; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly string lvalue; // argument name
    public readonly int position; // argument position
    public readonly Expression default_expression = null;

    /// <summary>
    /// Constructs a Function Arg Assignment object by looking at a subset of
    /// the "def f(...)" token_list.
    /// </summary>
    /// <param name="position">Argument position - this function only looks at 
    /// token_list[start_index] through token_list[end_index], so the position 
    /// information needs to be externally provided.</param>
    public FunctionArgAssignStatement(int position, TokenList token_list, int start_index,
      int end_index)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      lvalue = (string)token_list[start_index].value;
      this.position = position;

      if (end_index - start_index >= 2)
        default_expression = new Expression(token_list, start_index + 2, end_index);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append(lvalue + " = function_arg[" + position + "]"); 

      if (default_expression != null)
        sb.Append(" or " + default_expression);

      return sb.ToString();
    }
  }


  class FunctionCallStatement : Statement
  {
    public StatementType Type { get { return StatementType.Function_Call; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly string object_name;
    public readonly string function_name;

    FunctionArguments arguments = new FunctionArguments();
    public FunctionArguments get_arguments() { return arguments; }


    public FunctionCallStatement(string object_name, string function_name, 
      FunctionArguments arguments, int indentation, int line_number)
    {
      this.object_name = object_name;
      this.function_name = function_name;
      if (arguments != null) this.arguments = arguments;
      Indentation = indentation;
      LineNumber = line_number;
    }
    

    public FunctionCallStatement(TokenList token_list)
    {
      try
      {
        Indentation = token_list.indentation;
        LineNumber = token_list.line_number;

        if (token_list[1].type == TokenType.Left_Paren)
        {
          // Handle f(...) style function
          var identifier = token_list[0].value.ToString();
          var period_index = identifier.IndexOf('.');

          if (period_index >= 0)
          {
            // identifier is of the type "x.y"
            object_name = identifier.Substring(0, period_index);
            function_name = identifier.Substring(period_index + 1);
          }
          else
          {
            // no period found
            object_name = null;
            function_name = identifier;
          }
          parse_function_arguments(token_list);
        }

        // At one point, slice notation such as "s[1:2:3]" was
        // implemented as a function call.
        //else if (token_list[1].type == TokenType.Left_Bracket)
        //{
        //  // Handle x[...] style function
        //  object_name = token_list[0].value.ToString();
        //  function_name = "slice";
        //  parse_slice(token_list);
        //}
        
      }
      catch (Exception ex)
      {
        throw new CompilerException("Source code error on line "
            + (token_list.line_number + 1) + ". Details: " + ex.Message, (token_list.line_number + 1));
      }
    }


    /// <summary>
    /// The "token_list" is assumed to be f ( ... ).
    /// </summary>
    void parse_function_arguments(TokenList token_list)
    {
      int index = 2; // start of the current argument

      while(index < token_list.Count)
      {
        // look for comma
        int comma_index = token_list.find_token_outside_of_paren(
          TokenType.Comma, index, token_list.Count - 1);

        // Handle final argument case
        if (comma_index < 0)
        {
          // the rest of this token_list is treated as one argument
          int right_paren_index = token_list.find_token_outside_of_paren(
            TokenType.Right_Paren, index, token_list.Count - 1);

          if (right_paren_index < 0)
            throw new Exception("Unable to locate the ending ')' in \"" + token_list + "\".");

          if (right_paren_index == index)
          {
            // The function might be f(), totally no argument at all.
            // In this case, the index = 2.
            if (index == 2) return;

            throw new Exception("Unable to parse the final argument for \"" + token_list + "\".");
          }
          else
          {
            arguments.add_argument(token_list, index, right_paren_index - 1);
            index = right_paren_index + 1;
          }
        }

        // Handle empty argument error, as in f(1,2,,3)
        else if (comma_index == index)
          throw new Exception("Empty argument detected inside \"" + token_list + "\".");

        // Handle the standard case
        else
        {
          arguments.add_argument(token_list, index, comma_index - 1);
          index = comma_index + 1;
        }
      }
    }


    /// <summary>
    /// The "token_list" is assumed to be x [ ... ].
    /// </summary>
    void parse_slice(TokenList token_list)
    {
      // counter for right bracket detection
      int bracket_counter = 1; // number of left brackets so far

      int start_index = 2;
      for (int i = start_index; i < token_list.Count; i++)
      {
        // keep track of left brackets versus right brackets
        if (token_list[i].type == TokenType.Left_Bracket)
          bracket_counter++;
        else if (token_list[i].type == TokenType.Right_Bracket)
          bracket_counter--;

        // a colon, or the bracket_counter hitting zero, indicates a function argument
        if (token_list[i].type == TokenType.Colon || bracket_counter <= 0)
        {
          // The slice notation has at most 3 arguments: start, end, step
          if (arguments.Count >= 3)
            throw new Exception("The slice notation can have at most 3 arguments: start, end, and step.");

          // add token_list[start_index, i-1] to the list of arguments
          if (i - start_index >= 1)
            arguments.add_argument(token_list, start_index, i - 1);
          else
          {
            // Handle the case where a default is being used, like x[:2]
            arguments.add_null_argument();
          }
          start_index = i + 1;
        }

        // bracket_counter hitting zero indicates end of the function
        if (bracket_counter == 0) break;
      }

      // The slice notation can have at most 3 arguments.
      if (arguments.Count > 3)
        throw new Exception("The slice notation can have at most 3 arguments.");
    }


    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      if (object_name != null)
        sb.Append(object_name + "." + function_name + "(" + arguments + ")");
      else
        sb.Append(function_name + "(" + arguments + ")");

      return sb.ToString();
    }
  }



  class GlobalStatement : Statement
  {
    public StatementType Type { get { return StatementType.Global; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public string[] Symbols { get; private set; }

    public GlobalStatement(TokenList token_list)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      // check global statement length
      if (token_list.Count < 2)
        throw new Exception("global statement must have at least one parameter.");

      // symbols are extracted from token_list[1] ... end
      var symbols = new List<string>();

      for (int i = 1; i < token_list.Count; i++)
      {
        if (token_list[i].type == TokenType.Identifier)
          symbols.Add((string)token_list[i].value);
      }

      if (symbols.Count < 1)
        throw new Exception("No variable name found for the global statement.");

      Symbols = symbols.ToArray();
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append("global ");

      foreach (var symbol in Symbols)
        sb.Append(symbol + ' ');

      return sb.ToString();
    }
  }



  class KeywordStatement : Statement
  {
    public StatementType Type { get { return StatementType.Keyword; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly Token keyword;

    // Use (at most) ONE of the following. The unused variables are null.
    public readonly string var_name;
    public readonly SliceExpression slice_expression;
    public readonly Expression expression;

    public KeywordStatement(TokenList token_list)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      // assume token_list[0] is keyword
      keyword = token_list[0];

      // process token_list[1:] - general procedure - var_name first, 
      // slice next, expression last
      if (token_list.Count < 2) return;

      if (token_list.Count == 2 && token_list[1].type == TokenType.Identifier)
        var_name = (string)token_list[1].value;

      else if (token_list.Count >= 5 && token_list[1].type == TokenType.Identifier
        && token_list[2].type == TokenType.Left_Bracket
        && token_list[-1].type == TokenType.Right_Bracket)

        slice_expression = new SliceExpression(token_list, 1, token_list.Count - 1);

      else
        expression = new Expression(token_list, 1, token_list.Count);

      // restrict (var_name, slice_expression, expression) choices
      bool supported = true;
      if (keyword.type == TokenType.Del && var_name == null && slice_expression == null)
        supported = false;

      if (supported == false) throw new Exception("Statement not supported.");
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append(keyword + " ");

      if (var_name != null) sb.Append(var_name);
      else if (slice_expression != null) sb.Append(slice_expression);
      else if (expression != null) sb.Append(expression);

      return sb.ToString();
    }
  
  }


  class JumpStatement : Statement
  {
    public StatementType Type { get { return StatementType.Jump; } }
    public int Indentation { get; }
    public int LineNumber { get; }

    public int Addr { get; private set; }
    public void resolve_addresses()
    {
      if (Addr < 0)
        Addr = Parser.LabelAddresses.get_label_address(Addr * -1);
    }

    public JumpStatement(int address, int indentation, int line_number)
    {
      Indentation = indentation;
      LineNumber = line_number;
      Addr = address;      
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append("jump to " + Addr);
      return sb.ToString();
    }
  }

    
  
  class ReturnStatement : Statement
  {
    public StatementType Type { get { return StatementType.Return; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly Expression expression;

    /// <summary>
    /// The return expression is constructed from token_list[1]
    /// through the end of token_list.
    /// </summary>
    public ReturnStatement(TokenList token_list)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      if (token_list.Count > 1)
        expression = new Expression(token_list, 1, token_list.Count - 1);
      else
        expression = new Expression(NoneValue.NONE, LineNumber);
    }

    /// <summary>
    /// This constructor sets the return expression to "None".
    /// </summary>
    public ReturnStatement(int indentation, int line_number)
    {
      Indentation = indentation;
      LineNumber = line_number;
      expression = new Expression(NoneValue.NONE, line_number);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append("return " + expression);

      return sb.ToString();
    }
  }



  class SliceAssignStatement : Statement
  {
    public StatementType Type { get { return StatementType.SliceAssign; } }
    public int Indentation { get; }
    public int LineNumber { get; }
    public void resolve_addresses() { }

    public readonly SliceExpression lvalue;
    public readonly Expression expression;


    public SliceAssignStatement(TokenList token_list)
    {
      Indentation = token_list.indentation;
      LineNumber = token_list.line_number;

      // The format is (slice expression) = (expression).
      int equal_sign_index = token_list.find_token_outside_of_paren(
        TokenType.Assign, 0, token_list.Count - 1);

      lvalue = new SliceExpression(token_list, 0, equal_sign_index - 1);
      expression = new Expression(token_list, equal_sign_index + 1, token_list.Count - 1);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < Indentation; i++)
        sb.Append(' ');

      sb.Append(lvalue + " = " + expression);
      return sb.ToString();
    }
  }



}
