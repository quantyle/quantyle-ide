using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace X1.Compiler
{
  class ListExpression
  {
    Expression[] expressions;


    /// <summary>
    /// Constructs ListExpression from tokenized source code. 
    /// </summary>
    /// <param name="start_index">assumed to be a '[' character</param>
    /// <param name="end_index">assumed to be a ']' character</param>
    public ListExpression(TokenList token_list, int start_index, int end_index)
    {
      var expressions_list = new List<Expression>();

      int index = start_index + 1;

      while(index < end_index)
      {
        int end_comma_index = token_list.find_token_outside_of_paren(
          TokenType.Comma, index, end_index);

        if (end_comma_index < 0) end_comma_index = end_index;

        if (end_comma_index > index)
          expressions_list.Add(new Expression(token_list, index, end_comma_index - 1));

        index = end_comma_index + 1;
      }

      expressions = expressions_list.ToArray();
    }


    public ListValue eval(SymbolTable symbol_table)
    {
      var value_list = new List<Value>();

      foreach (var expression in expressions)
        value_list.Add(expression.eval(symbol_table));

      return new ListValue(value_list);
    }


    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append('[');

      for (int i = 0; i < expressions.Length; i++)
      {
        sb.Append(expressions[i]);

        if (i < expressions.Length - 1)
          sb.Append(", ");
      }

      sb.Append(']');
      return sb.ToString();
    }


  }

  
  
  class ListValue : Value
  {
    public readonly IList<Value> list = new List<Value>();

    public ValueType Type { get { return ValueType.List; } }

    delegate Value Method(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table);
    static Dictionary<string, Method> methods_look_up = new Dictionary<string, Method>();

    // for iteration:
    int current_iter = 0;


    static ListValue()
    {
      methods_look_up.Add("append", append);
      methods_look_up.Add("clear", clear);
      methods_look_up.Add("copy", copy);
      methods_look_up.Add("count", count);
      methods_look_up.Add("extend", extend);
      methods_look_up.Add("index", index);    
      methods_look_up.Add("insert", insert);
      methods_look_up.Add("pop", pop);
      methods_look_up.Add("remove", remove);
      methods_look_up.Add("reverse", reverse);
      methods_look_up.Add("sort", sort);
    }

    /*
     * This constructor can create "ListValue" from (source code) tokens.
     * This approach is currently disabled.
     * The current approach is to construct "ListExpression" from source
     * code tokens, then at run time produce ListValue objects.
    /// <summary>
    /// Constructs a ListValue, from source code starting at a given index. 
    /// The constructor will advance this index to the end of the list, 
    /// where the ']' is located.
    /// </summary>
    public ListValue(TokenList token_list, ref int start_index)
    {
      // Find the ending ']'
      int end_index = token_list.find_ending_token(start_index);
      if (end_index == -1)
        throw new Exception("Encountered unbalanced '[' ']' pairing.");

      // Go over tokens from "start_index" to "end_index"
      int index = start_index + 1;
      while (index < end_index)
      {
        var token = token_list[index];

        // Add "token" to list
        if (token.type == TokenType.Integer)
          list.Add(new DynamicValue((int)token.value));

        else if (token.type == TokenType.Double)
          list.Add(new DynamicValue((double)token.value));

        else if (token.type == TokenType.True)
          list.Add(new DynamicValue(true));

        else if (token.type == TokenType.False)
          list.Add(new DynamicValue(false));

        else if (token.type == TokenType.String)
          list.Add(new StringValue((string)token.value));

        else if (token.type == TokenType.None)
          list.Add(NoneValue.NONE);

        else if (token.type == TokenType.Left_Bracket)
        {
          // This is a nested list
          list.Add(new ListValue(token_list, ref index));

          if (end_index == -1)
            throw new Exception("Encountered unbalanced '[' ']' pairing.");
        }

        else
          throw new Exception("Unsupported item \"" + token.value + "\" used as list item.");

        // advance the token, go over (only) one comma
        index++;
        if (token_list[index].type == TokenType.Comma)
          index++;
      }

      start_index = end_index;
    }
    */
    
    public ListValue(List<Value> list)
    {
      this.list = list;
    }
    
    public ListValue() { }
    
    /// <summary>
    /// This constructor wraps a slice notation around an existing ListValue
    /// object.
    /// </summary>
    /// <param name="user_start">The start index as specified by the python 
    /// source code. Final start index may differ.</param>
    /// <param name="user_stop">The stop index as specified by the python 
    /// source code. Final stop index may differ.</param>
    public ListValue(ListValue list_value, int user_start, int user_stop, int step)
    {
      list = new Slice(list_value.list, user_start, user_stop, step);
    }


    public bool Equals(Value other)
    {
      if (other.Type == ValueType.List)
      {
        var other_list = (ListValue)other;
        int count = list.Count;

        if(count == other_list.Count)
        {
          for(int i = 0; i < count; i++)
          {
            if (list[i].Equals(other_list[i]) == false)
              return false;
          }
          // code arrives here if it survived all those comparisons.
          return true;
        }
      }      

      return false;
    }
    

    public Value operate(OperatorType op_type, Value val2)
    {
      if (op_type == OperatorType.In)
      {
        // "val2" is item to search for
        bool result = list.Contains(val2);
        return new DynamicValue(result);
      }
      else if (op_type == OperatorType.Not_In)
      {
        // "val2" is item to search for
        bool result = list.Contains(val2);
        return new DynamicValue(!result);
      }
      else if (op_type == OperatorType.Add)
      {
        if(val2.Type == ValueType.List)
        {
          // this + val2 list
          var list_value = new ListValue();
          list_value.add_list(this);
          list_value.add_list((ListValue)val2);

          return list_value;
        }
      }
      else if (op_type == OperatorType.Multiply)
      {
        if (val2.Type == ValueType.Dynamic)
        {
          var val2_dyn = (DynamicValue)val2;
          if (val2_dyn.value is int)
          {
            // duplicate val1 by val2 integer
            int duplication = (int)val2_dyn.value;
            return multiply(duplication);
          }
        }
      }
      else if (op_type == OperatorType.Equal)
        return new DynamicValue(Equals(val2));

      throw new Exception("Operator not implemented");
    }


    public Value call(string function_name, FunctionArguments arguments, 
      SymbolTable symbol_table)
    {
      if (methods_look_up.ContainsKey(function_name))
        return methods_look_up[function_name](list, arguments, symbol_table);

      // Iteration support:
      // Cheap acceleration by checking the first letter is risky here - 
      // as the user does directly work with List objects.
      if (function_name.Equals("next_element"))
      {
        // support next_element()
        if (current_iter < list.Count)
        {
          var return_val = shallow_copy(list[current_iter]);
          current_iter++;
          return return_val;
        }
        else
          return NoneValue.NONE;
      }
      else if (function_name.Equals("has_next_element"))
      {
        // support has_next_element()
        if (current_iter < list.Count) return new DynamicValue(true);
        else return new DynamicValue(false);
      }
      else if (function_name.Equals("reset"))
      {
        // support reset()
        current_iter = 0;
        return NoneValue.NONE;
      }
      
      return null;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append('[');

      int count = list.Count;

      for(int i = 0; i < count; i++)
      {
        append_value_to_string_builder(list[i], sb);

        if (i < count - 1)
          sb.Append(", ");
      }

      sb.Append(']');
      return sb.ToString();
    }


    /// <summary>
    /// Append a value to a string builder for print() display purpose.
    /// </summary>
    static public void append_value_to_string_builder(Value val, StringBuilder sb)
    {
      if (val.Type == ValueType.String)
      {
        sb.Append('\'');

        // sb.Append(val.ToString());
        // this almost works, but need to print escapes

        var str = ((StringValue)val).value;

        foreach (char c in str)
        {
          if (c == '\n') sb.Append("\\n");
          else if (c == '\\') sb.Append("\\\\");
          else if (c == '\t') sb.Append("\\t");
          else if (c == '\r') sb.Append("\\r");

          else if (c == '\a') sb.Append("\\x07");
          else if (c == '\b') sb.Append("\\x08");
          else if (c == '\f') sb.Append("\\x0c");
          else if (c == '\v') sb.Append("\\x0b");

          else sb.Append(c);
        }

        sb.Append('\'');
      }
      else
        sb.Append(val.ToString());
    }


    /// <summary>
    /// Returns the current list multiplied "n" times.
    /// </summary>
    public ListValue multiply(int n)
    {
      var result = new List<Value>();

      for (int i = 0; i < n; i++)
      {
        foreach (var item in list)
          result.Add(item);
      }

      return new ListValue(result);
    }
    

    /// <summary>
    /// Adds the content of "list_value" to this list, using shallow copy.
    /// </summary>
    public void add_list(ListValue list_value)
    {
      foreach (var value in list_value.list)
        list.Add(shallow_copy(value));
    }
        
    
    /// <summary>
    /// Return true if this "list" is really a "Slice" wrapper
    /// around a list.
    /// </summary>
    public bool is_slice()
    {
      if (list is Slice) return true;
      else return false;
    }


    /// <summary>
    /// Centralized location to have consistent shallow copy behavior. 
    /// </summary>
    public static Value shallow_copy(Value value)
    {
      // The current implementation of DynamicValue is to 
      // have its "value" be "readonly". Therefore the DynamicValue
      // is immutable, and the following is not necessary.
      // 
      //if (value.Type == ValueType.Dynamic)
      //{
      //  var dyn_value = (DynamicValue)value;
      //  return new DynamicValue(dyn_value.value);
      //}
      
      return value;
    }


    /// <summary>
    /// Make a shallow copy of the list.
    /// </summary>
    public ListValue get_shallow_copy()
    {
      var new_list = new List<Value>();

      foreach (var value in list)
        new_list.Add(shallow_copy(value));

      return new ListValue(new_list);
    }


    /// <summary>
    /// Make a shallow copy of the element at "index".
    /// </summary>
    public Value get_shallow_copy(int index)
    {
      return shallow_copy(list[index]);
    }


    // convenience properties to make this class look more like a list:
    // indexer, Count
    public Value this[int index]
    {
      get { return list[index]; }
      set { list[index] = value; }
    }


    public int Count
    {
      get { return list.Count; }
    }

    
    /// <summary>
    /// C# version of Python's list::append(...).
    /// </summary>
    static NoneValue append(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      cast_to_value_list(list, "append");
      
      var value = arguments.get_value_argument(0, null, null, symbol_table);

      list.Add(value);

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::clear().
    /// </summary>
    static NoneValue clear(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      cast_to_value_list(list, "clear");
      arguments.check_num_args(0);

      list.Clear();
      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::copy().
    /// </summary>
    static ListValue copy(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      var new_list = new List<Value>();
      foreach (var val in list)
        new_list.Add(shallow_copy(val));

      return new ListValue(new_list);
    }
    

    /// <summary>
    /// C# version of Python's list::count(...).
    /// </summary>
    static DynamicValue count(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);

      var value = arguments.get_value_argument(0, null, null, symbol_table);
      int count = 0;

      int length = list.Count; 
      for (int i = 0; i < length; i++)
      {
        if (list[i].Equals(value))
          count++;
      }

      return new DynamicValue(count);
    }


    /// <summary>
    /// C# version of Python's list::extend(...).
    /// </summary>
    static NoneValue extend(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      cast_to_value_list(list, "extend");

      var value = arguments.get_value_argument(0, null, null, symbol_table);

      if (value.Type == ValueType.String)
      {
        // add letters of the string to the list
        var str = ((StringValue)value).value;

        foreach(char c in str)
          list.Add(new StringValue(c.ToString()));
      }
      else if (value.Type == ValueType.List)
      {
        // add items of the list 
        var list_value = (ListValue)value;

        int count = list_value.Count;
        for (int i = 0; i < count; i++)
          list.Add(list_value[i]);
      }
      else if (value.Type == ValueType.Range)
      {
        var range_value = (RangeValue)value;

        if(range_value.step > 0)
        {
          for (int i = range_value.start; i < range_value.stop; i += range_value.step)
            list.Add(new DynamicValue(i));
        }
        else if(range_value.step < 0)
        {
          for (int i = range_value.start; i > range_value.stop; i += range_value.step)
            list.Add(new DynamicValue(i));
        }
      }
      else
        throw new Exception("The list::extend() method does not support argument of type \""
          + value.Type + "\".");

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::index(...).
    /// </summary>
    static DynamicValue index(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      int count = list.Count; 

      // get arguments
      var value = arguments.get_value_argument(0, null, null, symbol_table);
      int start = arguments.get_int_argument(1, null, 0, symbol_table);
      int end = arguments.get_int_argument(2, null, count, symbol_table);

      // arguments are allowed to be negative and out of bound
      if (start < 0) start = count + start;
      if (end < 0) end = count + end;

      if (start < 0) start = 0;
      if (end > count) end = count;

      for (int i = start; i < end; i++)
      {
        if (list[i].Equals(value))
          return new DynamicValue(i);
      }

      return new DynamicValue(-1); // The Python implementation throws an exception instead.
    }

    
    /// <summary>
    /// C# version of Python's list::insert(...).
    /// </summary>
    static NoneValue insert(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(2);
      cast_to_value_list(list, "insert");

      int index = arguments.get_int_argument(0, null, null, symbol_table);
      var value = arguments.get_value_argument(1, null, null, symbol_table);

      list.Insert(index, shallow_copy(value));

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::pop(...).
    /// </summary>
    static Value pop(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      cast_to_value_list(list, "pop");

      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(1);
      
      int index = arguments.get_int_argument(0, null, list.Count - 1, symbol_table);

      var value = list[index];
      list.RemoveAt(index);

      return value;
    }


    /// <summary>
    /// C# version of Python's list::remove(...).
    /// </summary>
    static Value remove(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      cast_to_value_list(list, "remove");
      arguments.check_num_args(1);

      var value = arguments.get_value_argument(0, null, null, symbol_table);
      bool removed = list.Remove(value);

      if (removed == false)
        throw new Exception("Item \"" + value + "\" was not found in the list.");

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::reverse(...).
    /// </summary>
    static NoneValue reverse(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      // i points to the start of the list, j points to the end of the list
      int i = 0, j = list.Count - 1;

      while(i < j)
      {
        // swap list[i] and list[j]
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;

        i++;
        j--;
      }

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's list::sort(...).
    /// </summary>
    static NoneValue sort(IList<Value> list, FunctionArguments arguments, SymbolTable symbol_table)
    {
      var value_list = cast_to_value_list(list, "sort");

      arguments.check_num_args_minimum(0); 
      arguments.check_num_args_maximum(1); // key not supported

      bool reverse = arguments.get_bool_argument(null, "reverse", false, symbol_table);

      if (reverse)
        value_list.Sort(reverse_compare_function);
      else
        value_list.Sort(compare_function);

      return NoneValue.NONE;
    }


    public static int compare_function(Value x, Value y)
    {
      var True = new DynamicValue(true);

      if (x.operate(OperatorType.Equal, y).Equals(True))
        return 0;
      else if (x.operate(OperatorType.Greater, y).Equals(True))
        return 1;
      else
        return -1;

      /*
       * Alternative implementation - probably faster, but 
       * then over time might lose consistency with operate(...)
      if (x.Type == ValueType.Dynamic && y.Type == ValueType.Dynamic)
      {
        var x_val = ((DynamicValue)x).value;
        var y_val = ((DynamicValue)y).value;

        if (x_val is bool || y_val is bool)
          throw new Exception("Comparison involving boolean values are not supported.");

        if (x_val > y_val) return 1;
        else if (x_val == y_val) return 0;
        else return -1;
      }
      else if (x.Type == ValueType.String && y.Type == ValueType.String)
      {
        var x_str = ((StringValue)x).value;
        var y_str = ((StringValue)y).value;

        return x_str.CompareTo(y_str);
      }

      throw new Exception("Comparison between type " + x.Type
        + " and " + y.Type + " is not supported.");
      */
    }

    static int reverse_compare_function(Value x, Value y)
    {
      return -1 * compare_function(x, y);
    }


    /// <summary>
    /// Try to cast the "i_list" interface object to an actual
    /// List&lt;Value> object. If the cast fails, throw an exception
    /// saying that the "method_name" is supported only for a real
    /// list, not the slice of a list.
    /// </summary>
    static List<Value> cast_to_value_list(IList<Value> i_list, string method_name)
    {
      if (i_list is List<Value>)
        return (List<Value>)i_list;

      throw new Exception("The method " + method_name + "() is only supported for a "
        + "real list, not the slice of a real list.");
    }

  }



  class SliceExpression
  {
    int line_number;

    // the source of the slice is ONE of the following (the unused variables are null)
    string var_name;
    StringValue str_literal;
    SliceExpression inner_slice_expr;

    // start, stop, step
    readonly int num_arguments = 0; // to differentiate s[2::] versus s[2]
    public readonly Expression start_expression;
    public readonly Expression stop_expression;
    public readonly Expression step_expression;


    /// <summary>
    /// Constructs SliceExpression from tokenized source code. 
    /// </summary>
    /// <param name="start_index">assumed to be a string or an identifier</param>
    /// <param name="end_index">assumed to be a ']' character</param>
    public SliceExpression(TokenList token_list, int start_index, int end_index)
    {
      line_number = token_list.line_number;

      // find the location of the start expression
      int bracket_start = token_list.find_starting_token(end_index);
      
      // decide on one of the following: "var_name", "str_literal", or "inner_slice_expr"      
      if (bracket_start > start_index + 1)
      {
        inner_slice_expr = new SliceExpression(token_list, start_index, bracket_start - 1);
      }
      else
      {
        var first_token = token_list[start_index];

        if (token_list[start_index].type == TokenType.String)
          str_literal = new StringValue((string)first_token.value);

        else if (token_list[start_index].type == TokenType.Identifier)
          var_name = (string)first_token.value;
      }

      // extract the "start_expression"
      int index = bracket_start + 1;

      int end_colon_index = token_list.find_token_outside_of_paren(
        TokenType.Colon, index, end_index);

      if (end_colon_index == -1) end_colon_index = end_index;

      if (index < end_colon_index)
        start_expression = new Expression(token_list, index, end_colon_index - 1);

      num_arguments = 1;
      index = end_colon_index + 1;

      // In the case of s[:], there are two arguments, start and stop, 
      // but the expressions themselves are null. So the number of 
      // arguments is detected by the colon location.
      if (end_colon_index < end_index)
        num_arguments = 2;

      // extract the "stop_expression"
      if (index < end_index)
      {
        end_colon_index = token_list.find_token_outside_of_paren(
          TokenType.Colon, index, end_index);

        if (end_colon_index == -1) end_colon_index = end_index;

        if (index < end_colon_index)
          stop_expression = new Expression(token_list, index, end_colon_index - 1);

        index = end_colon_index + 1;
      }

      if (end_colon_index < end_index)
        num_arguments = 3;

      // extract the "step_expression"
      if (index < end_index)
      {
        step_expression = new Expression(token_list, index, end_index - 1);
      }
    }


    public Value eval(SymbolTable symbol_table)
    {
      // decide on the source of the slice
      Value value = null;

      if (str_literal != null)
        value = str_literal;

      else if (var_name != null)
        value = symbol_table.get(var_name, line_number);

      else if (inner_slice_expr != null)
        value = inner_slice_expr.eval(symbol_table);

      if (value.Type == ValueType.String)
        return eval_string_slice((StringValue)value, symbol_table);

      if (value.Type == ValueType.List)
        return eval_list_slice((ListValue)value, symbol_table);

      if (value.Type == ValueType.Dictionary)
        return eval_dict_value((DictionaryValue)value, symbol_table);
            
      throw new Exception("The slice notation is not supported for type \""
            + value.Type + "\".");
    }


    /// <summary>
    /// Evaluate the slice expression for the case that the inner
    /// "value" is a list.
    /// </summary>
    /// <returns>returns either a single value or a wrapper around
    /// the inner list value</returns>
    Value eval_list_slice(ListValue value, SymbolTable symbol_table)
    {      
      // handle single value cases first
      if (num_arguments == 1)
      {
        int index = eval_int_expression(start_expression, symbol_table);

        if (index < 0) index = value.Count + index;

        if (index < 0 || index >= value.Count)
          throw new Exception("The list index of " + index + " is out of range.");

        return value[index];
      }

      // Handle range values
      (int start, int stop, int step) = get_slice_parameters(value.Count, symbol_table);

      // The slice of a list is still a ListValue - a "Slice" wrapper will be 
      // applied inside the constructor.
      return new ListValue(value, start, stop, step);
    }


    /// <summary>
    /// Return a substring of a string "value".
    /// </summary>
    /// <param name="str_value">a string value</param>
    /// <returns>returns a substring of "value"</returns>
    StringValue eval_string_slice(StringValue str_value, SymbolTable symbol_table)
    {
      int length = str_value.value.Length; // string length

      // handle single value cases first
      if (num_arguments == 1)
      {
        int index = eval_int_expression(start_expression, symbol_table);
        if (index < 0) index = length + index;

        if (index < 0 || index >= length)
          throw new Exception("The string index of " + index + " is out of range.");

        char c = str_value.value[index];
        return new StringValue(c.ToString());
      }

      // Handle range values
      (int start, int stop, int step) = get_slice_parameters(length, symbol_table);

      // Slice of string is immediately turned into a substring
      return StringValue.slice(str_value.value, start, stop, step);
    }


    /// <summary>
    /// Evaluate the slice expression for the case that the inner
    /// "value" is a dictionary.
    /// </summary>
    Value eval_dict_value(DictionaryValue value, SymbolTable symbol_table)
    {      
      // only single value cases are supported
      if (num_arguments == 1)
      {
        var key = start_expression.eval(symbol_table);

        if (value.dict.ContainsKey(key))
          return value.dict[key];

        throw new Exception("The dictionary key \"" + key + "\" cannot be found.");
      }

      throw new Exception("The dictionary type does not support complex slice expressions.");
    }


    /// <summary>
    /// Evaluates an expression, expecting an integer result. If not
    /// integer, throws an exception.
    /// </summary>
    int eval_int_expression(Expression expression, SymbolTable symbol_table)
    {
      var result = expression.eval(symbol_table);
      if (result.Type == ValueType.Dynamic)
      {
        var dyn_result = (DynamicValue)result;
        if (dyn_result.value is int)
          return (int)dyn_result.value;
      }

      throw new Exception("The slice notation expression \"" 
        + expression.ToString().Trim() + "\" evaluated to be \""
        + result.ToString() + "\", of the type \"" 
        + result.Type.ToString() + "\", but an integer value is expected.");
    }
    
    
    /// <summary>
    /// Returns (start, stop, step)
    /// </summary>
    /// <param name="length">length of the list or string being wrapped
    /// by the slice.</param>
    /// <returns></returns>
    (int, int, int) get_slice_parameters(int length, SymbolTable symbol_table)
    {
      // The default value of "start" and "end" depends on the sign of "step".
      // Therefore it's necessary to get "step" first.
      int step = 1;
      if (step_expression != null)
        step = eval_int_expression(step_expression, symbol_table);

      if (step == 0)
        throw new Exception("The slice operation's step value of 0 is illegal.");

      // Get the "start" parameter
      int start = 0; // default for step > 0
      if (step < 0) start = length - 1;

      if (start_expression != null)
      {
        start = eval_int_expression(start_expression, symbol_table);
        if (start < 0) start = length + start;
      }

      // Get the "stop" parameter
      int stop = length; // default for step > 0
      if (step < 0) stop = -1;

      if (stop_expression != null)
      {
        stop = eval_int_expression(stop_expression, symbol_table);
        if (stop < 0) stop = length + stop;
      }

      // Python slices are allowed to use invalid index values.
      // Shrink all values to fit the limit of the underlying list or string.
      if (step > 0)
      {
        if (start < 0) start = 0;
        if (stop > length) stop = length;
      }
      else if (step < 0)
      {
        if (start > length - 1) start = length - 1;
        if (stop < -1) stop = -1;
      }

      return (start, stop, step);
    }


    /// <summary>
    /// Execute: this = value;
    /// </summary>
    public void assign(Value value, SymbolTable symbol_table)
    {
      // Determine the "lvalue" that will be the target of the assignment.
      Value lvalue = null;

      if (var_name != null)
        lvalue = symbol_table.get(var_name, line_number);

      else if (inner_slice_expr != null)
        lvalue = inner_slice_expr.eval(symbol_table);

      if (str_literal != null)
        throw new Exception("Assigning to a string is not allowed.");

      // Handle assigning to a single value
      if (num_arguments == 1)
      {
        if (lvalue.Type == ValueType.List)
        {
          var list_lvalue = (ListValue)lvalue;

          int index = eval_int_expression(start_expression, symbol_table);
          if (index < 0) index = list_lvalue.Count + index;

          if (index < 0 || index >= list_lvalue.Count)
            throw new Exception("The list index of " + index + " is out of range.");

          list_lvalue[index] = value;
        }
        else if (lvalue.Type == ValueType.Dictionary)
        {
          var dict_lvalue = ((DictionaryValue)lvalue).dict;

          var key = start_expression.eval(symbol_table);
          dict_lvalue[key] = value;
        }
        else
          throw new Exception("Assignment to an index of type \"" + lvalue.Type
            + " \" is not supported.");
      }

      // Handle assigning to a range of values
      else
      {
        if (lvalue.Type == ValueType.List)
        {
          var list_lvalue = (ListValue)lvalue;
          (int start, int stop, int step) = get_slice_parameters(list_lvalue.Count, symbol_table);

          if (step == 1)
          {
            // insertion style assignment
            if (list_lvalue.is_slice())
              throw new Exception("Inserting a value into the slice of a slice is not supported.");

            insert_value(list_lvalue.list, value, start, stop);
          }
          else
          {
            // Item by item style assignment

            // Python behavior is to work only if 
            //   list_lvalue.is_slice() == false.
            // For slices, Python will fail silently (no effect, 
            // but no complaint either).
            //
            // Without checking for list_lvalue.is_slice(), the following
            // code will trigger assignment even if the underlying 
            // list_value is a slice.

            assign_items(list_lvalue.list, value, start, stop, step);
          }
        }
        else
          throw new Exception("Assignment to a slice of type \"" + lvalue.Type
            + " \" is not supported.");
      }
            
    }


    /// <summary>
    /// Insert "new_value" into "list", based on the slice parameters 
    /// "start" and "stop". The "step" has already been checked as 1.
    /// </summary>
    void insert_value(IList<Value> list, Value new_value, int start, int stop)
    {
      var new_list = new List<Value>();

      // add old_list[0] through old_list[start - 1]
      for (int i = 0; i < start; i++)
        new_list.Add(list[i]);

      // add new_value - which must be an iterable, per Python rules
      if (new_value.Type == ValueType.List)
      {
        var list_value = (ListValue)new_value;

        for (int i = 0; i < list_value.Count; i++)
          new_list.Add(list_value.get_shallow_copy(i));
      }
      else if (new_value.Type == ValueType.Range)
      {
        var range_value = (RangeValue)new_value;
        var range_list = range_value.get_list();

        for (int i = 0; i < range_list.Count; i++)
          new_list.Add(range_list[i]);
      }
      else if (new_value.Type == ValueType.String)
      {
        var str_value = (StringValue)new_value;
        string str = str_value.value;

        for (int i = 0; i < str.Length; i++)
          new_list.Add(new StringValue(str[i].ToString()));
      }
      else
        throw new Exception("Inserting a value of type \"" + new_value.Type
          + "\" into a list slice is not supported.");

      // add remainder of old_list
      if (stop < start) stop = start;

      for (int i = stop; i < list.Count; i++)
        new_list.Add(list[i]);
      
      // transfer new_list into old_list
      list.Clear();

      foreach (var i in new_list)
        list.Add(i);
    }


    /// <summary>
    /// Assign "new_items" to "list", on an item by item basis, according
    /// to indices determined by the slice notation "start, stop, step".
    /// </summary>
    void assign_items(IList<Value> list, Value new_items, int start, int stop, int step)
    {
      if (new_items.Type == ValueType.List)
      {
        var new_items_list = (ListValue)new_items;

        // length check
        int slice_length = compute_length(start, stop, step);
        int new_items_length = new_items_list.Count;

        if (slice_length != new_items_length)
          throw new Exception("Item by item assignment require that the size "
            + "of the slice, which is currently " + slice_length
            + ", match the size of the items list, which is currently "
            + new_items_length + ".");

        for(int i = 0; i < slice_length; i++)
        {
          int index = start + i * step;
          list[index] = new_items_list.get_shallow_copy(i);
        }
      }
      else
        throw new Exception("Item by item assignment into a slice only works for items inside lists.");
    }


    /// <summary>
    /// Compute the length of slice notation "start, stop, step".
    /// </summary>
    public static int compute_length(int start, int stop, int step)
    {
      int num_elements = 0;

      if (step > 0)
        num_elements = (int)Math.Floor((double)(stop - 1 - start) / step) + 1;
      // for positive step, the final value is (stop - 1)
      // (stop - 1 - start) is number of elements in addition to the starting value
      // +1 at the very end to include the starting element

      else if (step < 0)
        num_elements = (int)Math.Floor((double)(stop + 1 - start) / step) + 1;
      // for negative step, the final value is (stop + 1)

      if (num_elements < 0) num_elements = 0;

      return num_elements;
    }


    /// <summary>
    /// Implements the del statement on the slice
    /// </summary>
    public void del(SymbolTable symbol_table)
    {
      // Determine the "value" that will be the target of the del.
      Value value = null;

      if (var_name != null)
        value = symbol_table.get(var_name, line_number);

      else if (inner_slice_expr != null)
        value = inner_slice_expr.eval(symbol_table);
                  
      // Handle "del list slice"
      if (value.Type == ValueType.List)
      {
        var list_value = (ListValue)value;

        // del is not supported for slice of a slice
        if (list_value.is_slice())
          throw new Exception("del a slice of a slice is not supported.");

        // Handle del on single item
        if (num_arguments == 1)
        {
          int index = eval_int_expression(start_expression, symbol_table);
          if (index < 0) index = list_value.Count + index;

          if (index < 0 || index >= list_value.Count)
            throw new Exception("The list index of " + index + " is out of range.");

          list_value.list.RemoveAt(index);
          return;
        }

        // Handle del on multiple items
        else
        {
          (int start, int stop, int step) = get_slice_parameters(list_value.Count, symbol_table);

          // build up a new list, with some elements removed
          var new_list = new List<Value>();

          int count = list_value.Count;
          for (int i = 0; i < count; i++)
          {
            if (is_index_included(i, start, stop, step) == false)
              new_list.Add(list_value[i]);
          }

          // copy new_list back into list_value
          list_value.list.Clear();

          foreach (var val in new_list)
            list_value.list.Add(val);
        }
      }

      // Handle "del dictionary slice"
      else if (value.Type == ValueType.Dictionary)
      {
        var dict = ((DictionaryValue)value).dict;

        if (num_arguments != 1)
          throw new Exception("Only del dictionary[key] is supported.");

        var key = start_expression.eval(symbol_table);

        if (dict.ContainsKey(key))
          dict.Remove(key);

        else
          throw new Exception("The key \"" + key + "\" cannot be found.");
      }

      else 
        throw new Exception("del a slice of type \"" + value.Type + "\" is not supported.");
    }


    /// <summary>
    /// Returns true if "index" is covered by the slice notation
    /// [start:stop:step].
    /// </summary>
    bool is_index_included(int index, int start, int stop, int step)
    {
      // range check first
      if (step > 0)
      {
        if (index < start) return false;
        if (index >= stop) return false;
      }
      else if (step < 0)
      {
        if (index > start) return false;
        if (index <= stop) return false;
      }

      // remainder check
      int remainder = (index - start) % step;

      if (remainder == 0) return true;
      else return false;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();

      if (var_name != null) sb.Append(var_name);
      else if (str_literal != null)
      {
        sb.Append('"');
        sb.Append(str_literal.ToString());
        sb.Append('"');
      }
      else if (inner_slice_expr != null)
        sb.Append(inner_slice_expr.ToString());

      sb.Append('[');
      
      if (start_expression != null && stop_expression == null 
        && step_expression == null)
      {
        // single value case
        sb.Append(start_expression);
      }
      else
      {
        // range case
        if (start_expression != null)
          sb.Append(start_expression);

        sb.Append(':');
        if (stop_expression != null)
          sb.Append(stop_expression);

        sb.Append(':');
        if (step_expression != null)
          sb.Append(step_expression);
      }

      sb.Append(']');

      return sb.ToString();
    }

  }


  class Slice : IList<Value>
  {
    IList<Value> list;

    int start, stop, step;

    public Slice(IList<Value> list, int start, int stop, int step)
    {
      this.list = list;
      this.start = start;
      this.stop = stop;
      this.step = step;
    }
    

    /// <summary>
    /// Translate between the slice list's "index" and the "real_index"
    /// that is used to access the underlying "list".
    /// </summary>
    int real_index(int index)
    {
      int real_index = start + index * step;

      // check real_index versus "stop"
      if (step > 0)
      {
        if (real_index >= stop)
          throw new Exception("Index out of bound");
      }
      else if (step < 0)
      {
        if (real_index <= stop)
          throw new Exception("Index out of bound");
      }

      return real_index;
    }


    public Value this[int index]
    {
      get { return list[real_index(index)]; }
      set
      {
        list[real_index(index)] = value;
      }
    }


    public int Count
    {
      get
      {
        return SliceExpression.compute_length(start, stop, step);
      }
    }


    public bool IsReadOnly { get { return false; } }


    public void Add(Value item)
    {
      // Using Python's .append() on a slice seems to do nothing
      // to the underlying list, and the .append() itself 
      // returns "None".
      throw new Exception("Calling append() on a slice is not supported.");
    }

    public void Clear()
    {
      // Using Python's .clear() on a slice seems to do nothing
      // to the underlying list.
      throw new Exception("Calling clear() on a slice is not supported.");
    }

    public bool Contains(Value item)
    {
      int count = Count;

      for (int i = 0; i < count; i++)
        if (list[real_index(i)].Equals(item)) return true;

      return false;
    }

    public void CopyTo(Value[] array, int arrayIndex)
    {
      int count = Count;

      for (int i = 0; i < count; i++)
        array[arrayIndex] = list[real_index(i)];
    }

    public IEnumerator<Value> GetEnumerator()
    {
      return new SliceEnumerator(this);
    }

    public int IndexOf(Value item)
    {
      int count = Count;

      for (int i = 0; i < count; i++)
        if (list[real_index(i)].Equals(item)) return i;

      return -1;
    }

    public void Insert(int index, Value item)
    {
      throw new NotImplementedException();
    }

    public bool Remove(Value item)
    {
      // Using Python's .remove() on a slice seems to do nothing
      // to the underlying list.
      throw new Exception("Calling remove() on a slice is not supported.");
    }

    public void RemoveAt(int index)
    {
      throw new Exception("Calling remove() on a slice is not supported.");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }



  class SliceEnumerator : IEnumerator<Value>
  {
    Slice slice;
    int current_index = -1;

    public SliceEnumerator(Slice slice)
    {
      this.slice = slice;
    }

    public void Reset()
    {
      current_index = -1;
    }

    public bool MoveNext()
    {
      if (current_index < slice.Count - 1)
      {
        current_index++;
        return true;
      }
      else
        return false;
    }

    public Value Current
    {
      get { return slice[current_index]; }
    }

    object IEnumerator.Current
    {
      get { return slice[current_index]; }
    }

    public void Dispose() { }

  }



}
