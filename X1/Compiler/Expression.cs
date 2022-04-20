using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X1.Compiler
{
  class Expression
  {
    int line_number;
    ExpressionTerm[] terms;

    /// <summary>
    /// An expression constructed from a subset of the token list.
    /// The tokens will be placed in RPN order.
    /// </summary>
    public Expression(TokenList token_list, int start_index, int end_index)
    {
      line_number = token_list.line_number;

      // convert the Token objects in "token_list" to ExpressionTerm 
      // objects in "source_expression_terms".
      var source_expression_terms = new List<ExpressionTerm>();

      int index = start_index;
      while (index != end_index + 1)
      {
        // only landing on "end_index + 1" is the correct termination
        if (index > end_index + 1)
        {
          throw new CompilerException("Source code error on line "
              + (line_number + 1) + ". Failed to process \""
              + token_list.ToString(start_index, end_index).Trim()
              + "\" into a valid expression.", (line_number + 1));
        }

        process_expression_term(token_list, ref index, source_expression_terms, 
          start_index, end_index);
      }

      // Reorder the terms in "source_expression_terms" to be in RPN.
      var terms_list = new List<ExpressionTerm>(); // "source_expression_terms" in RPN order.
      var op_stack = new Stack<Operator>(); // stack for RPN ordering

      for (int i = 0; i < source_expression_terms.Count; i++)
      {
        var source_term = source_expression_terms[i];

        if (source_term.type != ExpressionTerm.Type.Operator)
        {
          terms_list.Add(source_term);
        }
        else if (source_term.type == ExpressionTerm.Type.Operator)
        {
          var op = (Operator)source_term.value;

          // left parenthesis handling
          if (op.type == OperatorType.Left_Paren)
          {
            op_stack.Push(op);
          }

          // right parenthesis handling
          else if (op.type == OperatorType.Right_Paren)
          {
            pop_op_stack(op_stack, -1, terms_list, cancel_left_paren: true);
          }

          // Standard operator handling
          else
          {
            // Pop stack if top of the stack has higher or equal precedence 
            if (op_stack.Count > 0)
            {
              if (op_stack.Peek().precedence() >= op.precedence())
              {
                pop_op_stack(op_stack, op.precedence(), terms_list);
              }
            }

            // push current operator onto the stack
            op_stack.Push(op);
          }
        }
      }

      // pop the operator stack and add those operators to "terms"
      pop_op_stack(op_stack, -1, terms_list);

      if (op_stack.Count > 0)
      {
        var op = op_stack.Peek();

        if (op.type == OperatorType.Left_Paren)
          throw new CompilerException("Source code error on line "
              + (line_number + 1) + ". Left over '(' not balanced out by ')'.", (line_number + 1));

        else
          throw new CompilerException("Source code error on line "
                + (line_number + 1) + ". Unable to parse expression.", (line_number + 1));
      }

      terms = terms_list.ToArray();
    }


    /// <summary>
    /// An expression that is just a single value.
    /// </summary>
    public Expression(Value value, int line_number)
    {
      this.line_number = line_number;

      terms = new ExpressionTerm[1];
      terms[0] = ExpressionTerm.create_value(value);
    }


    /// <summary>
    /// Add the token at "token_list[index]" to "source_expression_terms", and
    /// advance the index.
    /// </summary>
    /// <param name="start_index">the very first index of the expression within token_list</param>
    /// <param name="end_index">how far can the expression continue</param>
    void process_expression_term(TokenList token_list, ref int index,
      List<ExpressionTerm> source_expression_terms, int start_index, 
      int end_index)
    {
      var token = token_list[index];
      if (token.type == TokenType.Identifier)
      {
        // check for "identifier [ x ]" case
        if (index + 3 <= end_index
          && token_list[index + 1].type == TokenType.Left_Bracket)
        {
          // slice expression
          // find the end ']' character
          int end_bracket = token_list.find_ending_token(index + 1);
          
          if (end_bracket < 0 || end_bracket > end_index)
            throw new Exception("Unable to find the ending ']' while processing the "
              + "bracket notation for \"" + token.ToString() + "\".");

          // continue looking for more '[' - to handle cases like x[1][2]
          while (end_bracket + 1 < token_list.Count
            && token_list[end_bracket + 1].type == TokenType.Left_Bracket)
          {
            end_bracket = token_list.find_ending_token(end_bracket + 1);

            if (end_bracket < 0 || end_bracket > end_index)
              throw new Exception("Unable to find the ending ']' while processing the "
                + "bracket notation for \"" + token.ToString() + "\".");
          }

          var slice_expr = new SliceExpression(token_list, index, end_bracket);
          source_expression_terms.Add(ExpressionTerm.create_slice_expression(slice_expr));
          index = end_bracket + 1;
        }
        else
        {
          // stand alone identifier
          source_expression_terms.Add(ExpressionTerm.create_identifier((string)token.value));
          index++;
        }
      }
      else if (token.type == TokenType.Integer)
      {
        source_expression_terms.Add(ExpressionTerm.create_value(new DynamicValue((int)token.value)));
        index++;
      }
      else if (token.type == TokenType.Double)
      {
        source_expression_terms.Add(ExpressionTerm.create_value(new DynamicValue((double)token.value)));
        index++;
      }
      else if (token.type == TokenType.String)
      {
        // check for "string [ x ]" case
        if (index + 3 <= end_index
          && token_list[index + 1].type == TokenType.Left_Bracket)
        {
          // slice expression
          // find the end ']' character
          int end_bracket = token_list.find_ending_token(index + 1);
          if (end_bracket < index || end_bracket > end_index)
            throw new Exception("Unable to find the ending ']' while processing the "
              + "bracket notation for \"" + token.ToString() + "\".");

          var slice_expr = new SliceExpression(token_list, index, end_bracket);
          source_expression_terms.Add(ExpressionTerm.create_slice_expression(slice_expr));
          index = end_bracket + 1;
        }
        else
        {
          // stand alone string
          source_expression_terms.Add(ExpressionTerm.create_value(new StringValue((string)token.value)));
          index++;
        }
      }
      else if (token.type == TokenType.None)
      {
        source_expression_terms.Add(ExpressionTerm.create_value(NoneValue.NONE));
        index++;
      }
      else if (token.type == TokenType.True)
      {
        source_expression_terms.Add(ExpressionTerm.create_value(new DynamicValue(true)));
        index++;
      }
      else if (token.type == TokenType.False)
      {
        source_expression_terms.Add(ExpressionTerm.create_value(new DynamicValue(false)));
        index++;
      }
      else if (token.type == TokenType.Left_Bracket)
      {
        // find the end bracket, but it has to be "end_index" or earlier
        int end_bracket_index = token_list.find_token_outside_of_paren(
          TokenType.Right_Bracket, index + 1, end_index);

        if (end_bracket_index == -1)
          throw new Exception("Unable to find the ending ']' in the phrase \""
            + token_list.ToString(index, end_index) + "\".");

        var list_expression = new ListExpression(token_list, index, end_bracket_index);

        source_expression_terms.Add(ExpressionTerm.create_list_expression(list_expression));
        index = end_bracket_index + 1;
      }
      else if (token.type == TokenType.Left_Brace)
      {
        // find the end '}'
        int end_brace_index = token_list.find_token_outside_of_paren(
          TokenType.Right_Brace, index + 1, end_index);

        if (end_brace_index == -1)
          throw new Exception("Unable to find the ending '}' in the phrase \""
            + token_list.ToString(index, end_index) + "\".");

        var dict_expression = new DictionaryExpression(token_list, index, end_brace_index);

        source_expression_terms.Add(ExpressionTerm.create_dict_expression(dict_expression));
        index = end_brace_index + 1;
      }
      else
      {
        // token must be a supported operator
        var op = Operator.parse_token(token);
        if (op != null)
        {
          // detect redundant +, as in 2 + + 3
          if (op.type == OperatorType.Add)
          {
            if (index == start_index || is_active_operator(token_list[index - 1]))
            {
              // skip over the redundant '+'
              index++;
              return; 
            }
          }

          // detect negation -, as in 2 + - 3
          if (op.type == OperatorType.Subtract)
          {
            if ((index == start_index) || is_active_operator(token_list[index - 1]))
            {
              op = Operator.Constants.Negate;
            }
          }

          source_expression_terms.Add(ExpressionTerm.create_operator(op));
          index++;
        }
        else
          // op == null case, unable to convert "token" to "op"
          throw new CompilerException("Source code error on line "
            + (line_number + 1) + ". Unsupported operator \""
            + token.ToString() + "\".", (line_number + 1));
      }
    }


    /// <summary>
    /// Pop the given "op_stack" as long as the stack operator's precedence
    /// >= the given "precedence". These popped operators are moved
    /// onto the "terms_list". The popping always stops at '('. 
    /// If "cancel_left_paren" is true, then the '(' will be 
    /// removed from the stack.
    /// </summary>
    /// <param name="cancel_left_paren">The caller sees a ')' and sets this
    /// parameter to true to cancel out the '(' on the stack.</param>
    void pop_op_stack(Stack<Operator> op_stack, int precedence,
      List<ExpressionTerm> terms_list, bool cancel_left_paren = false)
    {
      while (op_stack.Count > 0)
      {
        var op = op_stack.Peek();

        if (op.type == OperatorType.Left_Paren)
        {
          // Handle '(' case. 
          // Always stop at '(', but sometimes cancel it out.
          if (cancel_left_paren) op_stack.Pop();
          return;
        }

        // Standard, non '(' case:
        if (op.precedence() >= precedence)
        {
          op_stack.Pop();
          terms_list.Add(ExpressionTerm.create_operator(op));
        }
        else
          return;        
      }
    }


    /// <summary>
    /// Return true if token is an "active" operator. This is for the 
    /// purpose of unary '+' and '-' detection, so ')' doesn't 
    /// count as an "active" operator.
    /// </summary>
    bool is_active_operator(Token token)
    {
      // Try to parse token as operator - failure to parse means it's not 
      // an operator.
      var op = Operator.parse_token(token);
      if (op == null) return false;

      // Certain operators don't count - they don't change the meaning of '+' and '-'
      if (op.type == OperatorType.Right_Paren)
        return false;

      return true;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();

      for(int i = 0; i < terms.Length; i++)
      {
        sb.Append(terms[i].ToString());
        if (i < terms.Length - 1) sb.Append(' ');
      }

      return sb.ToString();
    }


    /// <summary>
    /// Evaluate the expression and return a single value.
    /// </summary>
    public Value eval(SymbolTable symbol_table)
    {
      // old approach: new stack object each eval(...) call
      // var eval_stack = new Stack<Value>();
      //
      // new approach: reuse the "eval_stack" to be slightly faster
      var eval_stack = symbol_table.get_eval_stack();

      foreach(var term in terms)
      {
        if (term.type == ExpressionTerm.Type.Identifier)
        {
          // Handle identifier term - get value from symbol table and push onto stack.
          var value_obj = symbol_table.get((string)term.value, line_number);
          eval_stack.Push(value_obj);
        }
        else if (term.type == ExpressionTerm.Type.SliceExpression
          || term.type == ExpressionTerm.Type.ListExpression
          || term.type == ExpressionTerm.Type.DictionaryExpression)
        {
          var value_obj = term.eval(symbol_table);
          eval_stack.Push(value_obj);
        }
        else if (term.type == ExpressionTerm.Type.Value)
        {
          // Handle value term - push it onto stack.
          var value_obj = (Value)term.value;
          eval_stack.Push(value_obj);
        }
        else
        {
          // Handle operator term
          var op = (Operator)term.value;

          if (op.type == OperatorType.Negate || op.type == OperatorType.Not
            || op.type == OperatorType.Bitwise_Not)
          {
            // unary operator cases
            // check stack size
            if (eval_stack.Count < 1)
              throw new Exception("Execution error on line "
                + (line_number + 1) + ". The stack is out of "
                + "arguments while executing " + op.ToString() + ".");

            // apply the operator
            var val1 = eval_stack.Pop();
            try
            {
              var result = val1.operate(op.type, null);
              eval_stack.Push(result);
            }
            catch (Exception ex)
            {
              throw new Exception("Execution error on line "
                + (line_number + 1) + ". The evaluation of "
                + val1.ToString() + " " + op.ToString() 
                + " failed. Details: " + ex.Message);
            }
          }
          else
          {
            // binary operator cases 
            // check stack size
            if (eval_stack.Count < 2)
              throw new Exception("Execution error on line "
                + (line_number + 1) + ". The stack is out of "
                + "arguments while executing " + op.ToString() + ".");

            // apply the operator
            var val2 = eval_stack.Pop();
            var val1 = eval_stack.Pop();
            try
            {
              Value result = null;
              // Usually it's val1.operate(op.type, val2)
              // But invoke using val2 for some operators
              if (op.type == OperatorType.In || op.type == OperatorType.Not_In)
              {
                // reverse operate() invoke
                result = val2.operate(op.type, val1);
              }
              else
                // standard operate() invoke
                result = val1.operate(op.type, val2);

              if (result == null)
                throw new Exception("Operation not supported");

              eval_stack.Push(result);
            }
            catch(Exception ex)
            {
              throw new Exception("Execution error on line "
                + (line_number + 1) + ". The evaluation of "
                + val1.ToString() + " " + op.ToString() + " "
                + val2.ToString() + " failed. Details: "  + ex.Message);
            }
          }
        }
      } // end of: foreach(var term in terms)

      // there should be only 1 argument left on the stack
      if (eval_stack.Count != 1)
        throw new Exception("Execution error on line "
          + (line_number + 1) + ". The stack does not have "
          + "exactly one argument left at the end of "
          + "expression evaluation.");

      var return_val = eval_stack.Pop();
      symbol_table.return_eval_stack();

      return return_val;
    }



  }


  class ExpressionTerm
  {
    internal enum Type { Value, Identifier, Operator, SliceExpression,
      ListExpression, DictionaryExpression }

    public readonly Type type;
    public readonly object value;

    // Better use factory functions - there's an identifier type that is a string,
    // but there is also a value type that is also a string. Easy to confuse if using
    // constructor.
    private ExpressionTerm (Type type, object value) { this.type = type; this.value = value; }
        
    public static ExpressionTerm create_value(Value value)
    {
      return new ExpressionTerm(Type.Value, value);
    }

    public static ExpressionTerm create_identifier(string identifier)
    {
      return new ExpressionTerm(Type.Identifier, identifier);
    }

    public static ExpressionTerm create_list_expression(ListExpression expr)
    {
      return new ExpressionTerm(Type.ListExpression, expr);
    }

    public static ExpressionTerm create_dict_expression(DictionaryExpression expr)
    {
      return new ExpressionTerm(Type.DictionaryExpression, expr);
    }
    
    public static ExpressionTerm create_operator(Operator op)
    {
      return new ExpressionTerm(Type.Operator, op);
    }

    public static ExpressionTerm create_slice_expression(SliceExpression expr)
    {
      return new ExpressionTerm(Type.SliceExpression, expr);
    }

    public Value eval(SymbolTable symbol_table)
    {
      if (type == Type.SliceExpression)
        return ((SliceExpression)value).eval(symbol_table);
      else if (type == Type.ListExpression)
        return ((ListExpression)value).eval(symbol_table);
      else if (type == Type.DictionaryExpression)
        return ((DictionaryExpression)value).eval(symbol_table);
      
      throw new Exception("Evaluation of \"" + type.ToString() 
        + "\" expression term is not supported.");
    }

    public override string ToString()
    {
      // Identify "StringValue" value objects and put quotes around it
      if (type == Type.Value)
      {
        if (((Value)value).Type == ValueType.String)
        {
          // Code gets here means "value" is a "StringValue" object.
          return '"' + value.ToString() + '"';
        }
      }
      
      return value.ToString();
    }
  }





  enum OperatorType
  {
    // unary operators: Not, Negate, Bitwise_Not

    Or, And, Not,
    Bitwise_Or, Bitwise_Xor, Bitwise_And,
    Shift_Left, Shift_Right,

    In, Not_In, Is, Is_Not, Less, Less_or_Equal,
    Greater, Greater_or_Equal, Not_Equal, Equal,

    Add, Subtract,
    Multiply, Divide, Floor_Divide, Remainder,

    Negate, Bitwise_Not, Exponent,

    Left_Paren, Right_Paren
  }

  class Operator
  {
    public readonly OperatorType type;

    Operator(OperatorType type) { this.type = type; }

    // To map from Token to Operator:
    static Dictionary<TokenType, Operator> token_to_operator;

    // All operators are constants. The only "Add" operator
    // is the one stored in "token_to_operator".

    // precedence information (index is OperatorType cast to int)
    static int[] operator_precedences;

    // Convenience constants
    public class Constants
    {
      // Most operators are inferred from Tokens. However, the '-'
      // token defaults to the "Subtract" operator. Negation is
      // inferred during the "Expression()" constructor.
      public static readonly Operator Negate;

      static Constants()
      {
        Negate = new Operator(OperatorType.Negate);
      }
    }

    static Operator()
    {
      // Initiate "token_to_operator"
      token_to_operator = new Dictionary<TokenType, Operator>();
      token_to_operator.Add(TokenType.Or, new Operator(OperatorType.Or));
      token_to_operator.Add(TokenType.And, new Operator(OperatorType.And));
      token_to_operator.Add(TokenType.Not, new Operator(OperatorType.Not));

      token_to_operator.Add(TokenType.Bitwise_Or, new Operator(OperatorType.Bitwise_Or));
      token_to_operator.Add(TokenType.Bitwise_Xor, new Operator(OperatorType.Bitwise_Xor));
      token_to_operator.Add(TokenType.Bitwise_And, new Operator(OperatorType.Bitwise_And));

      token_to_operator.Add(TokenType.Shift_Left, new Operator(OperatorType.Shift_Left));
      token_to_operator.Add(TokenType.Shift_Right, new Operator(OperatorType.Shift_Right));

      token_to_operator.Add(TokenType.In, new Operator(OperatorType.In));
      token_to_operator.Add(TokenType.Not_In, new Operator(OperatorType.Not_In));
      token_to_operator.Add(TokenType.Is, new Operator(OperatorType.Is));
      token_to_operator.Add(TokenType.Is_Not, new Operator(OperatorType.Is_Not));
      token_to_operator.Add(TokenType.Less, new Operator(OperatorType.Less));
      token_to_operator.Add(TokenType.Less_or_Equal, new Operator(OperatorType.Less_or_Equal));
      token_to_operator.Add(TokenType.Greater, new Operator(OperatorType.Greater));
      token_to_operator.Add(TokenType.Greater_or_Equal, new Operator(OperatorType.Greater_or_Equal));
      token_to_operator.Add(TokenType.Not_Equal, new Operator(OperatorType.Not_Equal));
      token_to_operator.Add(TokenType.Equal, new Operator(OperatorType.Equal));

      token_to_operator.Add(TokenType.Add, new Operator(OperatorType.Add));
      token_to_operator.Add(TokenType.Subtract, new Operator(OperatorType.Subtract));
      token_to_operator.Add(TokenType.Multiply, new Operator(OperatorType.Multiply));
      token_to_operator.Add(TokenType.Divide, new Operator(OperatorType.Divide));
      token_to_operator.Add(TokenType.Floor_Divide, new Operator(OperatorType.Floor_Divide));
      token_to_operator.Add(TokenType.Remainder, new Operator(OperatorType.Remainder));

      token_to_operator.Add(TokenType.Bitwise_Not, new Operator(OperatorType.Bitwise_Not));
      token_to_operator.Add(TokenType.Exponent, new Operator(OperatorType.Exponent));

      token_to_operator.Add(TokenType.Left_Paren, new Operator(OperatorType.Left_Paren));
      token_to_operator.Add(TokenType.Right_Paren, new Operator(OperatorType.Right_Paren));

      // Initiate "operator_precedences"
      // see: https://docs.python.org/3/reference/expressions.html#operator-precedence
      int length = Enum.GetNames(typeof(OperatorType)).Length;
      operator_precedences = new int[length];
      operator_precedences[(int)OperatorType.Left_Paren] = -1;

      operator_precedences[(int)OperatorType.Or] = 3;
      operator_precedences[(int)OperatorType.And] = 4;
      operator_precedences[(int)OperatorType.Not] = 5;

      operator_precedences[(int)OperatorType.In] = 6;
      operator_precedences[(int)OperatorType.Not_In] = 6;
      operator_precedences[(int)OperatorType.Is] = 6;
      operator_precedences[(int)OperatorType.Is_Not] = 6;
      operator_precedences[(int)OperatorType.Less] = 6;
      operator_precedences[(int)OperatorType.Less_or_Equal] = 6;
      operator_precedences[(int)OperatorType.Greater] = 6;
      operator_precedences[(int)OperatorType.Greater_or_Equal] = 6;
      operator_precedences[(int)OperatorType.Not_Equal] = 6;
      operator_precedences[(int)OperatorType.Equal] = 6;
      
      operator_precedences[(int)OperatorType.Bitwise_Or] = 7;
      operator_precedences[(int)OperatorType.Bitwise_Xor] = 8;
      operator_precedences[(int)OperatorType.Bitwise_And] = 9;

      operator_precedences[(int)OperatorType.Shift_Left] = 10;
      operator_precedences[(int)OperatorType.Shift_Right] = 10;

      operator_precedences[(int)OperatorType.Add] = 11;
      operator_precedences[(int)OperatorType.Subtract] = 11;

      operator_precedences[(int)OperatorType.Multiply] = 12;
      operator_precedences[(int)OperatorType.Divide] = 12;
      operator_precedences[(int)OperatorType.Floor_Divide] = 12;
      operator_precedences[(int)OperatorType.Remainder] = 12;

      operator_precedences[(int)OperatorType.Bitwise_Not] = 13;
      operator_precedences[(int)OperatorType.Negate] = 13;

      operator_precedences[(int)OperatorType.Exponent] = 14;
    }

    /// <summary>
    /// Returns operator precedence.
    /// </summary>
    public int precedence()
    {
      return operator_precedences[(int)type];
    }

    public override string ToString()
    {
      return type.ToString();
    }


    /// <summary>
    /// Try to create an Operator object corresponding to the "token".
    /// Returns null if there is no corresponding Operator object.
    /// </summary>
    static public Operator parse_token(Token token)
    {
      if (token_to_operator.ContainsKey(token.type))
        return token_to_operator[token.type];

      return null;
    }
  }
}
