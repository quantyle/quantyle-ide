using System;
using System.Collections.Generic;
using System.Text;

namespace X1.Compiler
{
  class DictionaryExpression
  {
    Expression[] key_expressions;
    Expression[] value_expressions;


    /// <summary>
    /// Construction from source code. It's expected that the first token is
    /// '{' and the last token is '}'
    /// </summary>
    public DictionaryExpression(TokenList token_list, int start_index, int end_index)
    {
      var key_list = new List<Expression>();
      var value_list = new List<Expression>();

      int index = start_index + 1;

      while (index < end_index)
      {
        // Find the location of ','
        int end_comma_index = token_list.find_token_outside_of_paren(
          TokenType.Comma, index, end_index);
        if (end_comma_index < 0) end_comma_index = end_index;

        if (end_comma_index > index)
        {
          // Find the location of ':'
          int colon_index = token_list.find_token_outside_of_paren(
            TokenType.Colon, index + 1, end_comma_index - 1);

          // make sure there's at least one key token and one value token
          if ((colon_index > index) && (end_comma_index > colon_index + 1))
          {
            // Key expression is token_list[index ... colon_index - 1]
            // Value expression is token_list[colon_index + 1 ... end_comma_index - 1]
            key_list.Add(new Expression(token_list, index, colon_index - 1));
            value_list.Add(new Expression(token_list, colon_index + 1, end_comma_index - 1));
          }
        }

        index = end_comma_index + 1;
      }

      key_expressions = key_list.ToArray();
      value_expressions = value_list.ToArray();
    }


    public DictionaryValue eval(SymbolTable symbol_table)
    {
      var dict = new Dictionary<Value, Value>();

      for(int i = 0; i < key_expressions.Length; i++)
      {
        dict.Add(key_expressions[i].eval(symbol_table),
          value_expressions[i].eval(symbol_table));
      }

      return new DictionaryValue(dict);
    }


    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append('{');

      for (int i = 0; i < key_expressions.Length; i++)
      {
        sb.Append(key_expressions[i] + ": " + value_expressions[i]);

        if (i < key_expressions.Length - 1)
          sb.Append(", ");
      }

      sb.Append('}');
      return sb.ToString();
    }

  }



  class DictionaryValue : Value
  {
    public readonly Dictionary<Value, Value> dict = new Dictionary<Value, Value>();

    public ValueType Type { get { return ValueType.Dictionary; } }

    delegate Value Method(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table);
    static Dictionary<string, Method> methods_look_up = new Dictionary<string, Method>();


    public DictionaryValue(Dictionary<Value, Value> dict)
    {
      this.dict = dict;
    }
    
    static DictionaryValue()
    {
      methods_look_up.Add("clear", clear);
      methods_look_up.Add("copy", copy);
      methods_look_up.Add("get", get);
      methods_look_up.Add("keys", keys);
      methods_look_up.Add("pop", pop);
      methods_look_up.Add("setdefault", setdefault);
      methods_look_up.Add("update", update);
      methods_look_up.Add("values", values);
    }


    public bool Equals(Value other)
    {
      if (other.Type == ValueType.Dictionary)
      {
        var other_dict = ((DictionaryValue)other).dict;

        foreach(var key in dict.Keys)
        {
          if (other_dict.ContainsKey(key)
            && dict[key].Equals(other_dict[key]))
          {
            // pass
          }
          else
            return false;
        }

        // code gets here if it survives all comparisons
        return true;
      }

      return false;
    }


    public override string ToString()
    {
      var sb = new StringBuilder();

      sb.Append('{');

      int total = dict.Keys.Count;
      int count = 0;

      foreach(var key in dict.Keys)
      {
        var val = dict[key];

        ListValue.append_value_to_string_builder(key, sb);
        sb.Append(": ");
        ListValue.append_value_to_string_builder(val, sb);

        count++;
        if (count < total) sb.Append(", ");
      }

      sb.Append('}');

      return sb.ToString();
    }


    public Value call(string function_name, FunctionArguments arguments, SymbolTable symbol_table)
    {
      if (methods_look_up.ContainsKey(function_name))
        return methods_look_up[function_name](dict, arguments, symbol_table);

      return null;
    }


    public Value operate(OperatorType op_type, Value val2)
    {
      if (op_type == OperatorType.Equal)
        return new DynamicValue(Equals(val2));

      else if (op_type == OperatorType.Not_Equal)
        return new DynamicValue(!Equals(val2));

      else if (op_type == OperatorType.In)
        return new DynamicValue(dict.ContainsKey(val2));

      else if (op_type == OperatorType.Not_In)
        return new DynamicValue(!dict.ContainsKey(val2));

      return null;
    }


    /// <summary>
    /// C# version of Python's dictionary::clear().
    /// </summary>
    static NoneValue clear(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      dict.Clear();

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's dictionary::copy().
    /// </summary>
    static DictionaryValue copy(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      var d2 = new Dictionary<Value, Value>();

      foreach(var key in dict.Keys)
        d2.Add(ListValue.shallow_copy(key), ListValue.shallow_copy(dict[key]));
      
      return new DictionaryValue(d2);
    }


    /// <summary>
    /// C# version of Python's dictionary::get().
    /// </summary>
    static Value get(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(2);

      var key = arguments.get_value_argument(0, null, null, symbol_table);
      var default_val = arguments.get_value_argument(1, null, NoneValue.NONE, symbol_table);

      if (dict.ContainsKey(key)) return dict[key];
      else return default_val;
    }


    /// <summary>
    /// C# version of Python's dictionary::keys().
    /// </summary>
    static EnumeratorValue keys(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      int count = dict.Keys.Count;
      var enumerator = dict.Keys.GetEnumerator();

      return new EnumeratorValue(enumerator, count);
    }


    /// <summary>
    /// C# version of Python's dictionary::pop().
    /// </summary>
    static Value pop(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(2);

      var key = arguments.get_value_argument(0, null, null, symbol_table);
      var default_val = arguments.get_value_argument(1, null, NoneValue.NONE, symbol_table);

      if (dict.ContainsKey(key))
      {
        var return_val = dict[key];
        dict.Remove(key);
        return return_val;
      }
      else return default_val;
    }


    /// <summary>
    /// C# version of Python's dictionary::setdefault().
    /// </summary>
    static Value setdefault(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(2);

      var key = arguments.get_value_argument(0, null, null, symbol_table);
      var default_val = arguments.get_value_argument(1, null, NoneValue.NONE, symbol_table);

      if (dict.ContainsKey(key)) return dict[key];
      else
      {
        dict[key] = default_val;
        return default_val;
      }
    }


    /// <summary>
    /// C# version of Python's dictionary::update(). This version of
    /// update() expects a single dictionary.
    /// </summary>
    static NoneValue update(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);

      var d2 = arguments.get_value_argument(0, null, null, symbol_table);

      if (d2.Type != ValueType.Dictionary)
        throw new Exception("This version of dictionary::update(dict) requires a dictionary argument.");

      var dict2 = ((DictionaryValue)d2).dict;

      foreach (var key in dict2.Keys)
        dict[key] = ListValue.shallow_copy(dict2[key]);

      return NoneValue.NONE;
    }


    /// <summary>
    /// C# version of Python's dictionary::values().
    /// </summary>
    static EnumeratorValue values(Dictionary<Value, Value> dict, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      int count = dict.Values.Count;
      var enumerator = dict.Values.GetEnumerator();

      return new EnumeratorValue(enumerator, count);
    }

  }



  /// <summary>
  /// Wrapper around C#'s IEnumerator&lt;Value>, for use in Python.
  /// </summary>
  class EnumeratorValue : Value
  {
    public readonly IEnumerator<Value> enumerator;
    public readonly int Count;

    bool has_next_element = false;

    public ValueType Type { get { return ValueType.Enumerator; } }


    public EnumeratorValue(IEnumerator<Value> enumerator, int count)
    {
      this.enumerator = enumerator;
      Count = count;

      has_next_element = enumerator.MoveNext();
    }
        

    public Value call(string function_name, FunctionArguments arguments, SymbolTable symbol_table)
    {
      // full function names are: next_element(), has_next_element(), and reset()
      // cheap acceleration - just check the first letter of "function_name"
      if(function_name[0] == 'n')
      {
        // Handle next_element()

        // The C# IEnumerator<> interface works in opposite order of 
        // the iterator interface for this interpreter. The C# interface
        // move first, then check the "MoveNext()" for whether that 
        // next element actually exist.
        //
        // So the return value is the "current" one, then move to be ready 
        // for future call to "has_next_element()" and "next_element()".
        var return_val = enumerator.Current;
        if (return_val == null) return_val = NoneValue.NONE;

        has_next_element = enumerator.MoveNext(); // prepare for next loop

        return return_val;
      }
      else if (function_name[0] == 'h')
      {
        // Handle has_next_element()
        return new DynamicValue(has_next_element);
      }
      else if (function_name[0] == 'r')
      {
        // Handle reset()
        enumerator.Reset();

        has_next_element = enumerator.MoveNext();

        return NoneValue.NONE;
      }

      return null;
    }


    public bool Equals(Value other)
    {
      return false;
    }


    public Value operate(OperatorType op_type, Value val2)
    {
      return null;
    }
  }



}
