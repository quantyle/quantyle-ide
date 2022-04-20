using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X1.Compiler
{
  interface Value : IEquatable<Value>
  {
    ValueType Type { get; }
    Value operate(OperatorType op_type, Value val2);
    Value call(string function_name, FunctionArguments arguments, 
      SymbolTable symbol_table);
  }

  enum ValueType { Dictionary, Dynamic, Enumerator, List,
    None, Range, String, UserDefined }
  

  class DynamicValue : Value
  {
    public readonly dynamic value;

    delegate dynamic OpHandler(dynamic x, dynamic y);
    static OpHandler[] op_handlers;


    public DynamicValue(dynamic value) { this.value = value; }


    static DynamicValue()
    {
      int length = Enum.GetNames(typeof(OperatorType)).Length;
      op_handlers = new OpHandler[length];

      // Register the operation handlers
      op_handlers[(int)OperatorType.Add] = (x, y) => x + y;
      op_handlers[(int)OperatorType.And] = (x, y) => x && y;
      op_handlers[(int)OperatorType.Bitwise_And] = (x, y) => x & y;
      op_handlers[(int)OperatorType.Bitwise_Not] = (x, y) => ~x;
      op_handlers[(int)OperatorType.Bitwise_Or] = (x, y) => x | y;
      op_handlers[(int)OperatorType.Bitwise_Xor] = (x, y) => x ^ y;
      op_handlers[(int)OperatorType.Divide] = (x, y) => (double)x / y;
      op_handlers[(int)OperatorType.Equal] = (x, y) => x == y;
      op_handlers[(int)OperatorType.Exponent] = (x, y) => Math.Pow(x, y);
      op_handlers[(int)OperatorType.Floor_Divide] = (x, y) => Math.Floor((double)x / y);
      op_handlers[(int)OperatorType.Greater] = (x, y) => x > y;
      op_handlers[(int)OperatorType.Greater_or_Equal] = (x, y) => x >= y;
      op_handlers[(int)OperatorType.Less] = (x, y) => x < y;
      op_handlers[(int)OperatorType.Less_or_Equal] = (x, y) => x <= y;
      op_handlers[(int)OperatorType.Multiply] = (x, y) => x * y;
      op_handlers[(int)OperatorType.Negate] = (x, y) => -1 * x;
      op_handlers[(int)OperatorType.Not] = (x, y) => !x;
      op_handlers[(int)OperatorType.Not_Equal] = (x, y) => x != y;
      op_handlers[(int)OperatorType.Or] = (x, y) => x || y;
      op_handlers[(int)OperatorType.Remainder] = (x, y) => x % y;
      op_handlers[(int)OperatorType.Shift_Left] = (x, y) => x << y;
      op_handlers[(int)OperatorType.Shift_Right] = (x, y) => x >> y;
      op_handlers[(int)OperatorType.Subtract] = (x, y) => x - y;
    }


    ValueType Value.Type
    {
      get { return ValueType.Dynamic; }
    }


    public override string ToString() { return value.ToString(); }


    public bool Equals(Value other)
    {
      if(other.Type == ValueType.Dynamic)
      {
        var other_dyn = (DynamicValue)other;

        // Return value == other_dyn.value; --- mostly works
        // except for comparing "bool" to numbers. In this case,
        // the dynamic runtime will throw an exception. But this
        // has to be supported because Python lists can hold mix types.

        bool val1_is_bool = value is bool;
        bool val2_is_bool = other_dyn.value is bool;

        // Block boolean to number comparisons
        if (val1_is_bool && val2_is_bool == false) return false;
        if (val1_is_bool == false && val2_is_bool) return false;

        // Standard comparison
        return value == other_dyn.value;
      }

      return false;
    }


    public override int GetHashCode()
    {
      return value.GetHashCode();
    }


    public Value operate(OperatorType op_type, Value val2)
    {
      if (val2 == null)
      {
        // Unary operator case
        var result = op_handlers[(int)op_type](value, null);
        return new DynamicValue(result);
      }
      else
      {
        // Binary operator case
        // support Dynamic op dynamic
        if (val2.Type == ValueType.Dynamic)
        {
          var val2_dv = (DynamicValue)val2;
          var result = op_handlers[(int)op_type](value, val2_dv.value);
          return new DynamicValue(result);
        }

        if (op_type == OperatorType.Equal)
          return new DynamicValue(Equals(val2));

        // support dynamic op string
        else if (val2.Type == ValueType.String)
        {
          var str2 = (StringValue)val2;

          // support int * str
          if (op_type == OperatorType.Multiply)
          {
            if (value is int)
            {
              return str2.multiply((int)value);
            }
          }
        }

        // support dynamic op list
        else if (val2.Type == ValueType.List)
        {
          var list2 = (ListValue)val2;

          // support int * list
          if (op_type == OperatorType.Multiply)
          {
            if (value is int)
            {
              return list2.multiply((int)value);
            }
          }
        }
      }

      return null;
    }


    public Value call(string function_name, FunctionArguments arguments, 
      SymbolTable symbol_table)
    {
      throw new Exception("Not supported.");
    }
  }



  class NoneValue : Value
  {
    // This object is a constant. There is only one "NoneValue" object:
    public readonly static NoneValue NONE;

    NoneValue() { }
    static NoneValue() { NONE = new NoneValue(); }

    public ValueType Type
    {
      get { return ValueType.None; }
    }

    public override string ToString() { return "None"; }


    public bool Equals(Value other)
    {
      if (other.Type == ValueType.None)
        return true;

      return false;
    }


    public Value operate(OperatorType op_type, Value val2)
    {
      if (op_type == OperatorType.Equal)
        return new DynamicValue(Equals(val2));

      return null;
    }


    public Value call(string function_name, FunctionArguments arguments,
      SymbolTable symbol_table)
    {
      return null;
    }
  }



  class RangeValue : Value
  {
    public ValueType Type { get { return ValueType.Range; } }

    public Value operate(OperatorType op_type, Value val2)
    {
      return null;
    }

    public readonly int start, stop, step;
    int current;


    public RangeValue(int start, int stop, int step = 1)
    {
      this.start = start;
      this.stop = stop;
      this.step = step;

      if (step == 0)
        throw new Exception("Using a step value of 0 is illegal.");

      current = start;
    }


    public bool Equals(Value other)
    {
      if (other.Type == ValueType.Range)
      {
        var other_range = (RangeValue)other;
        if (start == other_range.start && stop == other_range.stop
          && step == other_range.step)
          return true;
      }

      return false;
    }


    public Value call(string function_name, FunctionArguments arguments,
      SymbolTable symbol_table)
    {
      // full function names are: next_element(), has_next_element(), and reset()
      // cheap acceleration - just check the first letter of "function_name"
      if(function_name[0] == 'n')
      {
        // Handle next_element()
        if (current < stop)
        {
          var return_val = new DynamicValue(current);
          current += step;
          return return_val;
        }
        else
          return NoneValue.NONE;
      }
      else if (function_name[0] == 'h')
      {
        // Handle has_next_element()
        if (current < stop) return new DynamicValue(true);
        else return new DynamicValue(false);
      }
      else if (function_name[0] == 'r')
      {
        // Handle reset()
        current = start;
        return NoneValue.NONE;
      }

      return null;
    }


    public List<Value> get_list()
    {
      var list_value = new List<Value>();

      if (step > 0)
      {
        for (int i = start; i < stop; i += step)
          list_value.Add(new DynamicValue(i));
      }
      else if (step < 0)
      {
        for (int i = start; i > stop; i += step)
          list_value.Add(new DynamicValue(i));
      }

      return list_value;
    }

  }



  class StringValue : Value
  {
    public readonly string value;

    public StringValue(string value) { this.value = value; }

    public ValueType Type { get { return ValueType.String; } }

    public override string ToString() { return value; }
    
    delegate Value Method(string value, FunctionArguments arguments, SymbolTable symbol_table);
    static Dictionary<string, Method> methods_look_up = new Dictionary<string, Method>();

    // for iteration:
    int current_iter = 0;


    static StringValue()
    {
      methods_look_up.Add("capitalize", capitalize);
      methods_look_up.Add("center", center);
      methods_look_up.Add("count", count);
      methods_look_up.Add("endswith", endswith);
      methods_look_up.Add("find", find);
      methods_look_up.Add("isalnum", isalnum);
      methods_look_up.Add("isalpha", isalpha);
      methods_look_up.Add("isdecimal", isdecimal);
      methods_look_up.Add("islower", islower);
      methods_look_up.Add("isnumeric", isnumeric);
      methods_look_up.Add("isspace", isspace);
      methods_look_up.Add("isupper", isupper);
      methods_look_up.Add("join", join);
      methods_look_up.Add("ljust", ljust);
      methods_look_up.Add("lower", lower);
      methods_look_up.Add("lstrip", lstrip);
      methods_look_up.Add("partition", partition);
      methods_look_up.Add("replace", replace);
      methods_look_up.Add("rfind", rfind);
      methods_look_up.Add("rjust", rjust);
      methods_look_up.Add("rpartition", rpartition);
      methods_look_up.Add("rsplit", rsplit);
      methods_look_up.Add("rstrip", rstrip);
      methods_look_up.Add("split", split);
      methods_look_up.Add("splitlines", splitlines);
      methods_look_up.Add("startswith", startswith);
      methods_look_up.Add("strip", strip);
      methods_look_up.Add("upper", upper);
      methods_look_up.Add("zfill", zfill);      
    }


    public bool Equals(Value other)
    {
      if (other.Type == ValueType.String)
      {
        var other_str = (StringValue)other;
        return value.Equals(other_str.value);
      }

      return false;
    }
    

    public override int GetHashCode()
    {
      return value.GetHashCode();
    }


    public Value operate(OperatorType op_type, Value val2)
    {
      // Handle +
      if (op_type == OperatorType.Add)
        return new StringValue(value + val2.ToString());

      // Handle comparison operators ==, <, <=, >, >=, !=
      else if (op_type == OperatorType.Equal || op_type == OperatorType.Less
        || op_type == OperatorType.Less_or_Equal || op_type == OperatorType.Greater
        || op_type == OperatorType.Greater_or_Equal || op_type == OperatorType.Not_Equal)
      {
        if (val2.Type == ValueType.String)
        {
          var val2_str = val2.ToString();
          var diff = string.Compare(value, val2_str);

          bool result = false;

          if (op_type == OperatorType.Equal && diff == 0)
            result = true;
          else if (op_type == OperatorType.Less && diff < 0)
            result = true;
          else if (op_type == OperatorType.Less_or_Equal && diff <= 0)
            result = true;
          else if (op_type == OperatorType.Greater && diff > 0)
            result = true;
          else if (op_type == OperatorType.Greater_or_Equal && diff >= 0)
            result = true;
          else if (op_type == OperatorType.Not_Equal && diff != 0)
            result = true;

          return new DynamicValue(result);
        }

        if (op_type == OperatorType.Equal)
          return new DynamicValue(Equals(val2));
      }

      // Handle "in" and "not in"
      else if (op_type == OperatorType.In || op_type == OperatorType.Not_In)
      {
        // search for val2 inside value
        var val2_str = val2.ToString();
        int index = value.IndexOf(val2_str);

        bool return_val = true; // this "return_val" is for "in"
        if (index < 0) return_val = false;

        // reverse the "return_val" for "not in"
        if (op_type == OperatorType.Not_In)
          return_val = !return_val;

        return new DynamicValue(return_val);
      }

      // Handle "*"
      else if (op_type == OperatorType.Multiply && val2.Type == ValueType.Dynamic)
      {
        var d = (DynamicValue)val2;
        if (d.value is int)
        {
          int dup = (int)d.value;
          return multiply(dup);
        }
      }

      return null;
    }


    public Value call(string function_name, FunctionArguments arguments, 
      SymbolTable symbol_table)
    {
      if (methods_look_up.ContainsKey(function_name))
        return methods_look_up[function_name](value, arguments, symbol_table);

      // Iteration support:
      // Cheap acceleration by checking the first letter is risky here - 
      // as the user does directly work with String objects.
      if (function_name.Equals("next_element"))
      {
        // support next_element()
        if (current_iter < value.Length)
        {
          var return_val = new StringValue(value[current_iter].ToString());
          current_iter++;
          return return_val;
        }
        else
          return NoneValue.NONE;
      }
      else if (function_name.Equals("has_next_element"))
      {
        // support has_next_element()
        if (current_iter < value.Length) return new DynamicValue(true);
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


    /// <summary>
    /// Returns substring for string[start:stop:step].
    /// </summary>
    public static StringValue slice(string value, int start, int stop, int step)
    {
      var sb = new StringBuilder();

      if (step > 0)
      {
        // build up string with an increasing step index
        if (start < 0) start = 0;
        if (stop > value.Length) stop = value.Length;

        for(int i = start; i < stop; i += step)
          sb.Append(value[i]);
      }
      else
      {
        // build up string with a decreasing step index
        if (start > value.Length - 1) start = value.Length - 1;
        if (stop < -1) stop = -1;

        for(int i = start; i > stop; i += step)
          sb.Append(value[i]);
      }

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::capitalize().
    /// </summary>
    static Value capitalize(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      if (value.Length == 0) return new StringValue("");

      var sb = new StringBuilder();
      sb.Append(char.ToUpper(value[0]));

      for (int i = 1; i < value.Length; i++)
        sb.Append(char.ToLower(value[i]));

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::center(...).
    /// </summary>
    static Value center(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(2);

      int width = arguments.get_int_argument(0, null, null, symbol_table);
      string fillchar = arguments.get_string_argument(1, null, " ", symbol_table);

      if (width <= value.Length) return new StringValue(value);

      if (fillchar.Length != 1)
        throw new Exception("The second argument to string::center() must be exactly one character long.");

      int left_pad = (width - value.Length) / 2;
      int right_pad = width - value.Length - left_pad;

      var sb = new StringBuilder();

      for (int i = 0; i < left_pad; i++)
        sb.Append(fillchar[0]);

      sb.Append(value);

      for (int i = 0; i < right_pad; i++)
        sb.Append(fillchar[0]);

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::count(...).
    /// </summary>
    static Value count(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      string str = arguments.get_string_argument(0, null, null, symbol_table);
      int start = arguments.get_int_argument(1, null, 0, symbol_table);
      int end = arguments.get_int_argument(2, null, value.Length, symbol_table);

      if (end > value.Length) end = value.Length;

      int count = 0;
      int index = start;
      while(index >= 0 && index < value.Length)
      {
        // search for "str" starting at "index".
        index = value.IndexOf(str, index);
        if (index >= 0)
        {
          if (index + str.Length <= end)
          {
            count++;
            index++; // continue search at next character
          }
          else
            break; // the result is out of bound (beyond end)
        }
      }

      return new DynamicValue(count);
    }


    /// <summary>
    /// C# version of Python's string::endswith(...).
    /// </summary>
    static Value endswith(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      string suffix = arguments.get_string_argument(0, null, null, symbol_table);

      if (arguments.Count == 1)
      {
        // Handle the simple case
        return new DynamicValue(value.EndsWith(suffix));
      }
      else
      {
        // Handle the 2 and 3 argument cases
        // The "start" must exist
        int start = arguments.get_int_argument(1, null, null, symbol_table);
        int end = arguments.get_int_argument(2, null, value.Length, symbol_table);

        if (end > value.Length) end = value.Length;
        return new DynamicValue(value.Substring(start, end - start).EndsWith(suffix));
      }
    }


    /// <summary>
    /// C# version of Python's string::find(...).
    /// </summary>
    static Value find(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      string sub = arguments.get_string_argument(0, null, null, symbol_table);

      if (arguments.Count == 1)
      {
        // Handle the simple case
        return new DynamicValue(value.IndexOf(sub));
      }
      else
      {
        // Handle the 2 and 3 argument cases
        // The "start" must exist
        int start = arguments.get_int_argument(1, null, null, symbol_table);
        int end = arguments.get_int_argument(2, null, value.Length, symbol_table);

        if (end > value.Length) end = value.Length;
        
        for(int i = start; i < end - sub.Length + 1; i++)
        {
          bool match = true;

          for (int j = 0; j < sub.Length; j++)
          {
            if(value[i+j] != sub[j])
            {
              match = false;
              break;
            }
          }

          if (match) return new DynamicValue(i);
        }

        return new DynamicValue(-1); // no match
      }
    }


    /// <summary>
    /// C# version of Python's string::isalnum(...).
    /// </summary>
    static Value isalnum(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      foreach(var c in value)
        if(char.IsLetter(c) == false && char.IsNumber(c) == false)
          return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::isalpha(...).
    /// </summary>
    static Value isalpha(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      foreach(var c in value)
        if(char.IsLetter(c) == false)
          return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::isdecimal(...).
    /// </summary>
    static Value isdecimal(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      foreach(var c in value)
        if(char.IsDigit(c) == false)
          return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::islower(...).
    /// </summary>
    static Value islower(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      bool has_letter = false;

      foreach(var c in value)
      {
        if (char.IsLetter(c))
        {
          has_letter = true;
          if(char.IsLower(c) == false)
            return new DynamicValue(false);
        }
      }

      if (has_letter == false) return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::isnumeric(...).
    /// </summary>
    static Value isnumeric(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      foreach(var c in value)
        if(char.IsNumber(c) == false)
          return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::isspace(...).
    /// </summary>
    static Value isspace(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      foreach(var c in value)
        if(char.IsWhiteSpace(c) == false)
          return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::isupper(...).
    /// </summary>
    static Value isupper(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);
      if (value.Length == 0) return new DynamicValue(false);

      bool has_letter = false;

      foreach(var c in value)
      {
        if (char.IsLetter(c))
        {
          has_letter = true;
          if(char.IsUpper(c) == false)
            return new DynamicValue(false);
        }
      }

      if (has_letter == false) return new DynamicValue(false);

      // Code gets here if all characters passed testing.
      return new DynamicValue(true); 
    }


    /// <summary>
    /// C# version of Python's string::join(...).
    /// </summary>
    static Value join(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);
      var arg = arguments.get_value_argument(0, null, null, symbol_table);

      var sb = new StringBuilder();

      if (arg.Type == ValueType.String)
      {
        string str = ((StringValue)arg).value;
        
        for(int i = 0; i < str.Length; i++)
        {
          sb.Append(str[i]);
          if (i < str.Length - 1) sb.Append(value);
        }
      }
      else if (arg.Type == ValueType.List)
      {
        var list = (ListValue)arg;
        int count = list.Count;

        for(int i = 0; i < count; i++)
        {
          // This is NOT the Python implementation.
          // The Python implementation is to complain if it's not a string.
          sb.Append(list[i].ToString());
          if (i < count - 1) sb.Append(value);
        }
      }
      else
        throw new Exception("The string::join() does not support type " + arg.Type + ".");
      
      return new StringValue(sb.ToString());
    }

       
    /// <summary>
    /// C# version of Python's string::ljust(...).
    /// </summary>
    static Value ljust(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);

      int width = arguments.get_int_argument(0, null, null, symbol_table);
      string fillchar = arguments.get_string_argument(1, null, " ", symbol_table);

      if (width <= value.Length) return new StringValue(value);

      if (fillchar.Length != 1)
        throw new Exception("The second argument to string::ljust() must be exactly one character long.");

      int right_pad = width - value.Length;

      var sb = new StringBuilder();
      sb.Append(value);

      for (int i = 0; i < right_pad; i++)
        sb.Append(fillchar[0]);

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::lower(...).
    /// </summary>
    static Value lower(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      var sb = new StringBuilder();
      foreach (var c in value)
        sb.Append(char.ToLower(c));

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::lstrip(...).
    /// </summary>
    static Value lstrip(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(1);

      if (arguments.Count == 0)
      {
        // Handle no parameter case first
        return new StringValue(value.TrimStart());
      }

      string chars = arguments.get_string_argument(0, null, " ", symbol_table);
      var char_array = chars.ToArray();
      
      return new StringValue(value.TrimStart(char_array));
    }


    /// <summary>
    /// C# version of Python's string::partition(...).
    /// </summary>
    static Value partition(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);

      string sep = arguments.get_string_argument(0, null, null, symbol_table);

      if (sep.Length == 0)
        throw new Exception("The string::partition() received an empty separator.");
      
      int index = value.IndexOf(sep);

      var list = new List<Value>();

      if (index < 0)
      {
        list.Add(new StringValue(value));
        list.Add(new StringValue(""));
        list.Add(new StringValue(""));
      }
      else
      {
        list.Add(new StringValue(value.Substring(0, index)));
        list.Add(new StringValue(value.Substring(index, sep.Length)));
        list.Add(new StringValue(value.Substring(index + sep.Length)));
      }

      return new ListValue(list);
    }
    

    /// <summary>
    /// C# version of Python's string::replace(...).
    /// </summary>
    static Value replace(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(2);
      arguments.check_num_args_maximum(3);

      var old_str = arguments.get_string_argument(0, null, null, symbol_table);
      var new_str = arguments.get_string_argument(1, null, null, symbol_table);

      if (arguments.Count == 2)
      {
        // Handle easier case first
        return new StringValue(value.Replace(old_str, new_str));
      }

      // Handle 3 parameter case here
      int count = arguments.get_int_argument(2, null, null, symbol_table);

      var sb = new StringBuilder();
      int index = 0;

      while(index < value.Length)
      {
        bool match = true; // for value[index + i] == old_str[i] comparison

        for(int i = 0; i < old_str.Length; i++)
        {
          if (index + i >= value.Length)
          {
            match = false;
            break;
          }
          // compare value[index + i] with old_str[i]
          if (value[index + i] != old_str[i])
          {
            match = false;
            break;
          }
        }

        if (match && count > 0)
        {
          sb.Append(new_str);
          count--;
          index += old_str.Length;
        }
        else
        {
          sb.Append(value[index]);
          index++;
        }        
      }

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::rfind(...).
    /// </summary>
    static Value rfind(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      string sub = arguments.get_string_argument(0, null, null, symbol_table);

      if (arguments.Count == 1)
      {
        // Handle the simple case
        return new DynamicValue(value.LastIndexOf(sub));
      }
      else
      {
        // Handle the 2 and 3 argument cases
        // The "start" must exist
        int start = arguments.get_int_argument(1, null, null, symbol_table);
        int end = arguments.get_int_argument(2, null, value.Length, symbol_table);

        if (end > value.Length) end = value.Length;
        
        for(int i = end - sub.Length; i >= start ; i--)
        {
          bool match = true;

          for (int j = 0; j < sub.Length; j++)
          {
            if(value[i+j] != sub[j])
            {
              match = false;
              break;
            }
          }

          if (match) return new DynamicValue(i);
        }

        return new DynamicValue(-1); // no match
      }
    }


    /// <summary>
    /// C# version of Python's string::rjust(...).
    /// </summary>
    static Value rjust(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);

      int width = arguments.get_int_argument(0, null, null, symbol_table);
      string fillchar = arguments.get_string_argument(1, null, " ", symbol_table);

      if (width <= value.Length) return new StringValue(value);

      if (fillchar.Length != 1)
        throw new Exception("The second argument to string::rjust() must be exactly one character long.");

      int left_pad = width - value.Length;

      var sb = new StringBuilder();

      for (int i = 0; i < left_pad; i++)
        sb.Append(fillchar[0]);

      sb.Append(value);

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::rpartition(...).
    /// </summary>
    static Value rpartition(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);

      string sep = arguments.get_string_argument(0, null, null, symbol_table);

      if (sep.Length == 0)
        throw new Exception("The string::rpartition() received an empty separator.");

      int index = value.LastIndexOf(sep);

      var list = new List<Value>();

      if (index < 0)
      {
        list.Add(new StringValue(value));
        list.Add(new StringValue(""));
        list.Add(new StringValue(""));
      }
      else
      {
        list.Add(new StringValue(value.Substring(0, index)));
        list.Add(new StringValue(value.Substring(index, sep.Length)));
        list.Add(new StringValue(value.Substring(index + sep.Length)));
      }

      return new ListValue(list);
    }


    /// <summary>
    /// C# version of Python's string::rsplit(...).
    /// </summary>
    static ListValue rsplit(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(2);

      string sep;
      try
      {
        sep = arguments.get_string_argument(0, "sep", null, symbol_table);
        if (sep.Length == 0)
          throw new Exception("The string::rsplit() received an empty separator.");
      }
      catch
      {
        sep = null; // default value for "sep" is null
      }

      int max_split = arguments.get_int_argument(1, "maxsplit", -1, symbol_table);

      string[] result = null;
      
      if (sep == null)
      {
        if (max_split < 0)
          // sep = white spaces, no limit on max split
          result = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        else
        {
          // sep = white spaces, max_split stated
          var results_list = new List<string>();
          
          int end_index = value.Length - 1;
          int index = end_index;
          int splits = 0;
          
          while(index >= 0 && splits < max_split)
          {
            // search for whitespace
            while (index >= 0 && char.IsWhiteSpace(value[index]) == false)
              index--;

            // The "index" is pointing to whitespace (or -1)
            // The token is from "index+1" to "end_index"
            results_list.Add(value.Substring(index + 1, end_index - index));
            splits++;

            // prepare for next iteration of the loop
            // In this version of rsplit, consecutive white spaces 
            // don't contribute "" to tokens
            while (index >= 0 && char.IsWhiteSpace(value[index]))
              index--;

            end_index = index;
          }

          // add left over string
          if (index >= 0)
            results_list.Add(value.Substring(0, index + 1));

          results_list.Reverse();
          result = results_list.ToArray();
        }
      }
      else
      {
        if (max_split < 0)
          // sep stated, no limit on max split
          result = value.Split(sep);
        else
        {
          // sep stated, max_split stated
          var results_list = new List<string>();
          
          int end_index = value.Length - 1;
          int index = end_index;
          int splits = 0;
          
          while(index >= 0 && splits < max_split)
          {
            // search for sep
            index = value.LastIndexOf(sep, end_index);

            // The "index" is pointing to the first character of "sep" (or -1)
            if (index < 0)
            {
              results_list.Add(value.Substring(0, end_index + 1));
            }
            else
            {
              // The token is from "index+sep.Length" to "end_index"
              // token length = end_index - (index+sep.Length) + 1
              results_list.Add(value.Substring(index + sep.Length, 
                end_index - index - sep.Length + 1));
            }
            
            splits++;

            // prepare for next iteration of the loop
            index--;
            end_index = index;
          }

          // add left over string
          if (index >= 0)
          {
            results_list.Add(value.Substring(0, index + 1));
          }
          else
          {
            // Code gets here if maxsplit is set higher than necessary, so 
            // "index--" went below zero without finding any more separators.

            // Alternatively, the final separator is found at the very 
            // start of "value". Then index-- goes below zero and the preceding
            // while loop terminated.

            // In this version of rsplit(), if the string starts with
            // "sep", then that separator produces an empty string
            if (value.StartsWith(sep))
              results_list.Add("");
          }
          

          results_list.Reverse();
          result = results_list.ToArray();
        }
      }

      var list = new List<Value>();
      foreach (var str in result)
        list.Add(new StringValue(str));

      return new ListValue(list);
    }


    /// <summary>
    /// C# version of Python's string::rstrip(...).
    /// </summary>
    static Value rstrip(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(1);

      if (arguments.Count == 0)
      {
        // Handle no parameter case first
        return new StringValue(value.TrimEnd());
      }

      string chars = arguments.get_string_argument(0, null, " ", symbol_table);
      var char_array = chars.ToArray();
      
      return new StringValue(value.TrimEnd(char_array));
    }


    /// <summary>
    /// C# version of Python's string::split(...).
    /// </summary>
    static ListValue split(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(2);

      string sep;
      try
      {
        sep = arguments.get_string_argument(0, "sep", null, symbol_table);
        if (sep.Length == 0)
          throw new Exception("The string::split() received an empty separator.");
      }
      catch
      {
        sep = null; // default value for "sep" is null
      }

      int max_split = arguments.get_int_argument(1, "maxsplit", -1, symbol_table);

      string[] result = null;
      
      if (sep == null)
      {
        if (max_split < 0)
          // sep = white spaces, no limit on max split
          result = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        else
          // sep = white spaces, max_split stated
          result = value.Split((char[])null, max_split + 1, StringSplitOptions.RemoveEmptyEntries);
      }
      else
      {
        if (max_split < 0)
          // sep stated, no limit on max split
          result = value.Split(sep);
        else
          // sep stated, max_split stated
          result = value.Split(sep, max_split + 1);
      }

      var list = new List<Value>();
      foreach (var str in result)
        list.Add(new StringValue(str));

      return new ListValue(list);
    }
    

    /// <summary>
    /// C# version of Python's string::splitlines(...).
    /// </summary>
    static ListValue splitlines(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(1);

      bool keepends = arguments.get_bool_argument(0, "keepends", false, symbol_table);

      string[] result = null;

      var separator = new string[] {"\r\n", "\n", "\r", "\v", "\f"};
      
      if (keepends)
      {
        var results_list = new List<string>();
        int index = 0;
        int start_index = 0; // start of current token
        
        while(index < value.Length)
        {
          // advance "index" until a match is found
          int match = match_str_to_pattern(value, index, separator); 
          
          while(match == -1 && index < value.Length)
          {
            index++;
            match = match_str_to_pattern(value, index, separator);
          }

          if (index < value.Length)
            index += separator[match].Length;

          // add value[start_index ... index - 1] to results_list
          results_list.Add(value.Substring(start_index, index - start_index));

          // prepare for the next loop
          start_index = index;
        }

        result = results_list.ToArray();
      }
      else
      {
        result = value.Split(separator, StringSplitOptions.None);
      }

      var list = new List<Value>();
      foreach (var str in result)
        list.Add(new StringValue(str));

      return new ListValue(list);
    }
    
    /// <summary>
    /// Match "value[index]" against "pattern[]". Return the 
    /// index of the pattern that got matched. Return -1
    /// if no pattern match.
    /// </summary>
    static int match_str_to_pattern(string value, int index, string[] pattern)
    {
      for (int i = 0; i < pattern.Length; i++)
      {
        // match "value[index]" against pattern[i]        

        // (index + pattern[i].Length - 1) at most (value.Length - 1)
        if (index + pattern[i].Length <= value.Length)
        {
          bool match = true;
          for (int j = 0; j < pattern[i].Length; j++)
          {
            if (value[index + j] != pattern[i][j])
            {
              match = false;
              break;
            }
          }

          if (match) return i;
        }
      }      

      return -1;
    }


    /// <summary>
    /// C# version of Python's string::startswith(...).
    /// </summary>
    static Value startswith(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(1);
      arguments.check_num_args_maximum(3);

      string prefix = arguments.get_string_argument(0, null, null, symbol_table);

      if (arguments.Count == 1)
      {
        // Handle the simple case
        return new DynamicValue(value.StartsWith(prefix));
      }
      else
      {
        // Handle the 2 and 3 argument cases
        // The "start" must exist
        int start = arguments.get_int_argument(1, null, null, symbol_table);
        int end = arguments.get_int_argument(2, null, value.Length, symbol_table);

        if (end > value.Length) end = value.Length;
        return new DynamicValue(value.Substring(start, end - start).StartsWith(prefix));
      }
    }


    /// <summary>
    /// C# version of Python's string::strip(...).
    /// </summary>
    static Value strip(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args_minimum(0);
      arguments.check_num_args_maximum(1);

      if (arguments.Count == 0)
      {
        // Handle no parameter case first
        return new StringValue(value.Trim());
      }

      string chars = arguments.get_string_argument(0, null, " ", symbol_table);
      var char_array = chars.ToArray();
      
      return new StringValue(value.Trim(char_array));
    }


    /// <summary>
    /// C# version of Python's string::upper(...).
    /// </summary>
    static Value upper(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(0);

      var sb = new StringBuilder();
      foreach (var c in value)
        sb.Append(char.ToUpper(c));

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// C# version of Python's string::zfill(...).
    /// </summary>
    static Value zfill(string value, FunctionArguments arguments, SymbolTable symbol_table)
    {
      arguments.check_num_args(1);

      int width = arguments.get_int_argument(0, null, null, symbol_table);

      if (width <= value.Length) return new StringValue(value);

      int left_pad = width - value.Length;

      var sb = new StringBuilder();
      int index = 0;

      // add in leading + or -
      if (value[0] == '+' || value[0] == '-')
      {
        sb.Append(value[0]);
        index = 1;
      }

      // add padding
      for (int i = 0; i < left_pad; i++)
        sb.Append('0');

      // add rest of the string
      sb.Append(value.Substring(index));

      return new StringValue(sb.ToString());
    }


    /// <summary>
    /// Returns the current string multiplied "n" times.
    /// </summary>
    public StringValue multiply(int n)
    {
      var sb = new StringBuilder();

      for (int i = 0; i < n; i++)
        sb.Append(value);

      return new StringValue(sb.ToString());
    }
  }


}
