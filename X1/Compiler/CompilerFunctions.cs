using System;
using System.Collections.Generic;
using System.IO;
// using System.Text;

namespace X1.Compiler
{
  class CompilerFunctions
  {
    internal delegate Value CompilerFunction(FunctionArguments arguments, SymbolTable symbol_table);

    Dictionary<string, CompilerFunction> function_lookup = new Dictionary<string, CompilerFunction>();

    SymbolTable symbol_table;
    TextWriter text_writer;


    /// <param name="text_writer">Allows print() output to be redirected.</param>
    public CompilerFunctions(SymbolTable symbol_table, TextWriter text_writer = null)
    {
      this.symbol_table = symbol_table;
      this.text_writer = text_writer;

      function_lookup.Add("float", python_float);
      function_lookup.Add("int", python_int);
      function_lookup.Add("len", len);
      function_lookup.Add("list", list);
      function_lookup.Add("max", max);
      function_lookup.Add("min", min);
      // function_lookup.Add("print", print);
      function_lookup.Add("range", range);
      function_lookup.Add("str", str);
    }


    public void add_function(string function_name, CompilerFunction function)
    {
      function_lookup.Add(function_name, function);
    }


    /// <summary>
    /// Returns true if "function_name" is implemented within this class.
    /// </summary>
    public bool implements_function(string function_name)
    {
      if (function_lookup.ContainsKey(function_name)) return true;
      else return false;
    }


    public Value run(FunctionCallStatement function_call)
    {
      return function_lookup[function_call.function_name](function_call.get_arguments(), symbol_table);
    }


    Value len(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var val = arguments.get_value_argument(0, null, null, symbol_table);

      if (val.Type == ValueType.String)
      {
        var val_str = (StringValue)val;
        return new DynamicValue(val_str.value.Length);
      }
      else if (val.Type == ValueType.List)
      {
        var val_list = (ListValue)val;
        return new DynamicValue(val_list.Count);
      }
      else if (val.Type == ValueType.Dictionary)
      {
        var dict_val = (DictionaryValue)val;
        return new DynamicValue(dict_val.dict.Keys.Count);
      }
      else if (val.Type == ValueType.Enumerator)
      {
        var enum_val = (EnumeratorValue)val;
        return new DynamicValue(enum_val.Count);
      }
      else
        throw new Exception("len(" + val.ToString() + ") is not supported.");
    }


    Value list(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var val = arguments.get_value_argument(0, null, null, symbol_table);

      // Handle list(string)
      if (val.Type == ValueType.String)
      {
        string str = ((StringValue)val).value;
        var new_list = new List<Value>();

        foreach (char c in str)
          new_list.Add(new StringValue(c.ToString()));

        return new ListValue(new_list);
      }

      // Handle list(list)
      else if (val.Type == ValueType.List)
      {
        // return a shallow copy of "val"
        var list_val = (ListValue)val;
        return list_val.get_shallow_copy();
      }

      // Handle list(range)
      else if (val.Type == ValueType.Range)
      {
        var range_val = (RangeValue)val;
        var val_list = range_val.get_list();
        return new ListValue(val_list);
      }

      // Handle list(enumerator)
      else if (val.Type == ValueType.Enumerator)
      {
        var enumerator = ((EnumeratorValue)val).enumerator;
        var list = new List<Value>();

        enumerator.Reset();

        while (enumerator.MoveNext())
          list.Add(enumerator.Current);

        return new ListValue(list);
      }

      else
        throw new Exception("list(" + val.ToString() + ") is not supported.");
    }


    Value max(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);

      var list_arg = arguments.get_list_arguments(0, symbol_table);
      
      // A single string is a special case
      if (list_arg.Count == 1 && list_arg[0].Type == ValueType.String)
      {
        // return the smallest character of this string
        string str = ((StringValue)list_arg[0]).value;

        if (str.Length == 0)
          throw new Exception("min() is called on an empty sequence.");

        char largest_char = str[0];
        for(int i = 1; i < str.Length; i++)
        {
          if (str[i] > largest_char) largest_char = str[i];
        }

        return new StringValue(largest_char.ToString());
      }

      // Standard case
      Value largest = null;
      
      for(int i = 0; i < list_arg.Count; i++)
      {
        if(list_arg[i].Type == ValueType.List)
          // Note: This is NOT standard Python behavior
          largest = find_largest_in_list((ListValue)list_arg[i], largest);
        
        else
        {
          if (largest == null)
            largest = list_arg[i];
          else
          {
            // possible alternative:
            // var True = new DynamicValue(true);
            // if (list_arg[i].operate(OperatorType.Greater, largest).Equals(True))
            if (ListValue.compare_function(list_arg[i], largest) > 0)
              largest = list_arg[i];
          }
        }
      }

      if (largest == null)
        throw new Exception("max() is called on an empty sequence.");

      return ListValue.shallow_copy(largest);
    }

    Value find_largest_in_list(ListValue list_value, Value current_largest)
    {
      int count = list_value.Count;
      
      for (int i = 0; i < count; i++)
      {
        if (list_value[i].Type == ValueType.List)
        {
          current_largest = find_largest_in_list((ListValue)list_value[i], current_largest);
        }
        else
        {
          if (current_largest == null)
            current_largest = list_value[i];

          // possible alternative:
          // var True = new DynamicValue(true);
          // else if (list_value[i].operate(OperatorType.Greater, current_largest).Equals(True))
          else if (ListValue.compare_function(list_value[i], current_largest) > 0)
            current_largest = list_value[i];
        }
      }

      return current_largest;
    }


    Value min(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);

      var list_arg = arguments.get_list_arguments(0, symbol_table);
      
      // A single string is a special case
      if (list_arg.Count == 1 && list_arg[0].Type == ValueType.String)
      {
        // return the smallest character of this string
        string str = ((StringValue)list_arg[0]).value;

        if (str.Length == 0)
          throw new Exception("min() is called on an empty sequence.");

        char smallest_char = str[0];
        for(int i = 1; i < str.Length; i++)
        {
          if (str[i] < smallest_char) smallest_char = str[i];
        }

        return new StringValue(smallest_char.ToString());
      }

      // Standard case
      Value smallest = null;
      
      for(int i = 0; i < list_arg.Count; i++)
      {
        if(list_arg[i].Type == ValueType.List)
          // Note: This is NOT standard Python behavior
          smallest = find_smallest_in_list((ListValue)list_arg[i], smallest);
        
        else
        {
          if (smallest == null)
            smallest = list_arg[i];
          else
          {
            // possible alternative:
            // var True = new DynamicValue(true);
            // if (list_arg[i].operate(OperatorType.Less, smallest).Equals(True))
            if (ListValue.compare_function(list_arg[i], smallest) < 0)
              smallest = list_arg[i];
          }
        }
      }

      if (smallest == null)
        throw new Exception("min() is called on an empty sequence.");

      return ListValue.shallow_copy(smallest);
    }

    Value find_smallest_in_list(ListValue list_value, Value current_smallest)
    {
      int count = list_value.Count;
      
      for (int i = 0; i < count; i++)
      {
        if (list_value[i].Type == ValueType.List)
        {
          current_smallest = find_smallest_in_list((ListValue)list_value[i], current_smallest);
        }
        else
        {
          if (current_smallest == null)
            current_smallest = list_value[i];

          // possible alternative:
          // var True = new DynamicValue(true);
          // else if (list_value[i].operate(OperatorType.Less, current_smallest).Equals(True))
          else if (ListValue.compare_function(list_value[i], current_smallest) < 0)
            current_smallest = list_value[i];
        }
      }

      return current_smallest;
    }


    // Value print(FunctionArguments arguments, SymbolTable symbol_table)
    // {
    //   string sep = arguments.get_string_argument(null, "sep", " ", symbol_table);

    //   // string end = arguments.get_string_argument(null, "end", "\n", symbol_table);
    //   string end = arguments.get_string_argument(null, "end", Environment.NewLine, symbol_table);

    //   var list_arg = arguments.get_list_arguments(0, symbol_table);
            
    //   var sb = new StringBuilder();

    //   for(int i = 0; i < list_arg.Count; i++)
    //   {
    //     sb.Append(list_arg[i].ToString());

    //     if (i < list_arg.Count - 1)
    //       sb.Append(sep);
    //   }

    //   sb.Append(end);

    //   // console_output = sb.ToString();

    //   Console.Write("PYTHON print: {0}", sb.ToString());

    //   if (text_writer != null)
    //     text_writer.Write(sb.ToString());


    //   return NoneValue.NONE;
    // }


    Value python_float(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var val = arguments.get_value_argument(0, null, null, symbol_table);


      if (val.Type == ValueType.Dynamic)
      {
        var val_dyn = (DynamicValue)val;
        if (val_dyn.value is int)
          return new DynamicValue((double)val_dyn.value);
        else if (val_dyn.value is double)
          return val_dyn;
        else if (val_dyn.value is bool)
        {
          if (val_dyn.value == true) return new DynamicValue(1.0d);
          if (val_dyn.value == false) return new DynamicValue(0.0d);
        }
      }
      else if (val.Type == ValueType.String)
      {
        var val_str = (StringValue)val;
        double result;
        bool success = double.TryParse(val_str.value, out result);

        if (success) return new DynamicValue(result);
        else
          throw new Exception("Failed to convert " + val_str.value + " to float.");
      }
      else
        throw new Exception("float(" + val.Type + ") is not supported.");


      throw new Exception("float() used in an unsupported way.");
    }


    Value python_int(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var val = arguments.get_value_argument(0, null, null, symbol_table);


      if (val.Type == ValueType.Dynamic)
      {
        var val_dyn = (DynamicValue)val;
        if (val_dyn.value is int)
          return val_dyn;

        else if (val_dyn.value is double)
        {
          if (val_dyn.value >= 0)
            return new DynamicValue((int)Math.Floor(val_dyn.value));
          else
            return new DynamicValue((int)Math.Floor(val_dyn.value) + 1);
        }

        else if (val_dyn.value is bool)
        {
          if (val_dyn.value == true) return new DynamicValue(1);
          if (val_dyn.value == false) return new DynamicValue(0);
        }
      }
      else if (val.Type == ValueType.String)
      {
        var val_str = (StringValue)val;
        int result;
        bool success = int.TryParse(val_str.value, out result);

        if (success) return new DynamicValue(result);
        else
          throw new Exception("Failed to convert " + val_str.value + " to int.");
      }
      else
        throw new Exception("int(" + val.Type + ") is not supported.");
      
      throw new Exception("int() used in an unsupported way.");
    }

    
    Value range(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(2);
      arguments.check_num_args_maximum(3);

      int start = arguments.get_int_argument(0, null, null, symbol_table, round_double: true);
      int stop = arguments.get_int_argument(1, null, null, symbol_table, round_double: true);
      int step = arguments.get_int_argument(2, null, 1, symbol_table, round_double: true);

      return new RangeValue(start, stop, step);
    }


    Value str(FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var val = arguments.get_value_argument(0, null, null, symbol_table);
      return new StringValue(val.ToString());
    }


  }
}
