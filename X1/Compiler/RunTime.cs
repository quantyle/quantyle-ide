using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ValueType = X1.Compiler.ValueType;

namespace X1.Compiler
{
    public class RunTime
    {
        List<Statement> statements;
        internal Dictionary<string, int> user_def_function_locations;

        int prog_counter = 0; // program counter

        delegate void StatementHandler(Statement statement);
        StatementHandler[] statement_handlers;

        internal SymbolTable symbol_table = new SymbolTable();
        CompilerFunctions built_in_functions;

        Stack<FunctionCall> function_call_stack = new Stack<FunctionCall>();



        // Construct a run time. You can have multiple run times per script.

        // <param name="script">Contains the script source file.</param>
        // <param name="text_writer">Allows print() output to be redirected.</param>
        public RunTime(Script script, TextWriter text_writer = null)
        {
            statements = script.statements;
            user_def_function_locations = script.user_def_function_locations;
            built_in_functions = new CompilerFunctions(symbol_table, text_writer);

            // initialize "statement_handlers[]"
            statement_handlers = new StatementHandler[Enum.GetNames(typeof(StatementType)).Length];
            statement_handlers[(int)StatementType.Assign] = run_assign_statement;
            statement_handlers[(int)StatementType.Conditional] = run_conditional_statement;
            statement_handlers[(int)StatementType.Function_Arg_Assign] = run_function_arg_assign_statement;
            statement_handlers[(int)StatementType.Function_Call] = run_function_call_statement;
            statement_handlers[(int)StatementType.Global] = run_global_statement;
            statement_handlers[(int)StatementType.Keyword] = run_keyword_statement;
            statement_handlers[(int)StatementType.Jump] = run_jump_statement;
            statement_handlers[(int)StatementType.Return] = run_return_statement;
            statement_handlers[(int)StatementType.SliceAssign] = run_slice_assign_statement;
        }



        // By default, run until the end of the program. Returns "true"
        // if the end of the program is reached.

        public bool run(int? instructions_to_run = null)
        {
            while (prog_counter < statements.Count && prog_counter >= 0)
            {
                var statement = statements[prog_counter];

                // run "statement"
                try
                {
                    statement_handlers[(int)statement.Type](statement);
                }
                catch (Exception ex)
                {
                    throw new Exception("Execution error on line "
                            + (statement.LineNumber + 1) + ": " + statement.ToString()
                            + ". Error details: " + ex.Message);
                }

                // stop early if "instructions_to_run" is used
                if (instructions_to_run != null)
                {
                    instructions_to_run--;
                    if (instructions_to_run <= 0)
                    {
                        if (prog_counter < statements.Count) return false;
                        else return true;
                    }
                }
            }

            return true;
        }


        void run_assign_statement(Statement statement)
        {
            var s = (AssignStatement)statement;
            var result = s.expression.eval(symbol_table);

            // x = list --- special handling
            // For list slice, if system ($) variable, store the list slice 
            // to symbol table.
            // 
            // If user variable, meaning no $ start, make a copy of the
            // list and store that copy to the symbol table
            if (result.Type == ValueType.List && s.lvalue.StartsWith('$') == false)
            {
                // user variable case
                var list_result = (ListValue)result;

                if (list_result.is_slice())
                    result = list_result.get_shallow_copy();
            }

            symbol_table.store(s.lvalue, result);
            prog_counter++;
        }


        void run_conditional_statement(Statement statement)
        {
            var s = (ConditionalStatement)statement;
            var result = s.expression.eval(symbol_table);

            if (result.Type == ValueType.Dynamic)
            {
                var result_dyn = (DynamicValue)result;
                if (result_dyn.value is bool)
                {
                    bool result_val = (bool)result_dyn.value;
                    if (result_val) prog_counter = s.TrueAddr;
                    else prog_counter = s.FalseAddr;
                }
                else
                {
                    if (result_dyn.value != 0) prog_counter = s.TrueAddr;
                    else prog_counter = s.FalseAddr;
                }
            }
            else
                throw new Exception("The evaluation of the expression "
                  + s.expression.ToString() + " did not result in a boolean or numeric result.");
        }


        void run_function_arg_assign_statement(Statement statement)
        {
            if (function_call_stack.Count == 0)
                throw new Exception("Function argument assignment requested, but function call stack is empty.");

            var s = (FunctionArgAssignStatement)statement;

            var arguments = function_call_stack.Peek().arguments;

            Value default_value = null;
            if (s.default_expression != null)
                default_value = s.default_expression.eval(symbol_table);

            var value = arguments.get_value_argument(s.position, s.lvalue, default_value, symbol_table);

            symbol_table.store(s.lvalue, value);
            prog_counter++;
        }


        void run_function_call_statement(Statement statement)
        {
            var s = (FunctionCallStatement)statement;

            if (s.object_name != null)
            {
                // Handling calling a method call on a built-in object
                var obj = symbol_table.get(s.object_name, statement.LineNumber);
                var return_val = obj.call(s.function_name, s.get_arguments(), symbol_table);

                if (return_val == null)
                    throw new Exception("The operation \"" + s.object_name + "." + s.function_name
                      + "(" + s.get_arguments().ToString() + ")\" is not supported.");

                symbol_table.store("$return", return_val);
                prog_counter++;
            }

            else if (s.object_name == null && built_in_functions.implements_function(s.function_name))
            {
                // Handle built-in function
                var return_val = built_in_functions.run(s);

                if (return_val == null)
                    throw new Exception("The operation \"" + s.function_name + "("
                      + s.get_arguments().ToString() + ")\" is not supported.");

                symbol_table.store("$return", return_val);
                prog_counter++;
            }

            else if (s.object_name == null && user_def_function_locations.ContainsKey(s.function_name))
            {
                // Create a new entry on function_call_stack, then jump
                var function_call = new FunctionCall();
                function_call.return_addr = prog_counter + 1;
                function_call.arguments = s.get_arguments();

                const int function_call_limit = 200;
                if (function_call_stack.Count >= function_call_limit)
                {
                    throw new Exception("The built-in limit of " + function_call_limit
                      + " nested function calls has been reached.");
                }

                function_call_stack.Push(function_call);

                symbol_table.enter_function_call();

                prog_counter = user_def_function_locations[s.function_name];
            }

            else
                throw new Exception("Function not implemented.");
        }


        void run_global_statement(Statement statement)
        {
            var s = (GlobalStatement)statement;
            foreach (var symbol in s.Symbols)
                symbol_table.add_global_variable(symbol);

            prog_counter++;
        }


        void run_jump_statement(Statement statement)
        {
            var s = (JumpStatement)statement;
            prog_counter = s.Addr;
        }


        void run_keyword_statement(Statement statement)
        {
            var s = (KeywordStatement)statement;

            if (s.keyword.type == TokenType.Del)
            {
                if (s.var_name != null)
                    symbol_table.del(s.var_name, s.LineNumber);

                else if (s.slice_expression != null)
                    s.slice_expression.del(symbol_table);

                prog_counter++;
            }
            else
                throw new Exception("statement not supported");
        }


        void run_return_statement(Statement statement)
        {
            if (function_call_stack.Count == 0)
                throw new Exception("Return statement encountered, but function call stack is empty.");

            // put expression into $return variable
            var s = (ReturnStatement)statement;

            var return_value = s.expression.eval(symbol_table);

            symbol_table.exit_function_call();
            symbol_table.store("$return", return_value);

            // jump to the return address
            int return_addr = function_call_stack.Pop().return_addr;
            prog_counter = return_addr;
        }


        void run_slice_assign_statement(Statement statement)
        {
            var s = (SliceAssignStatement)statement;
            var result = s.expression.eval(symbol_table);

            s.lvalue.assign(result, symbol_table);

            prog_counter++;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("prog_counter=" + prog_counter + "  ");

            if (prog_counter < statements.Count)
                sb.AppendLine(statements[prog_counter].ToString());
            else
                sb.AppendLine("End of Program");

            sb.Append(symbol_table.ToString());

            return sb.ToString();
        }



        // Adds a user defined function to "built_in_functions".

        internal void add_function(string function_name, CompilerFunctions.CompilerFunction function)
        {
            built_in_functions.add_function(function_name, function);
        }
    }


    class SymbolTable
    {
        Dictionary<string, Value> global_symbol_table = new Dictionary<string, Value>();

        // local symbol tables for use with functions
        Stack<Dictionary<string, Value>> local_symbol_table_stack = new Stack<Dictionary<string, Value>>();
        Stack<HashSet<string>> global_symbols_stack = new Stack<HashSet<string>>();

        // buffers for "local_symbol_table_stack" and "global_symbols_stack"
        const int nested_function_limit = 1000;
        int nested_function = 0;
        Dictionary<string, Value>[] local_symbol_table_buffer = new Dictionary<string, Value>[5];
        HashSet<string>[] global_symbols_buffer = new HashSet<string>[5];

        // Symbol table's "side job" of providing stacks for Expression::eval()
        const int nested_eval_limit = 20;
        int nested_eval = 0; // increase per eval() call
        Stack<Value>[] eval_stacks = new Stack<Value>[5];


        public SymbolTable()
        {
            for (int i = 0; i < eval_stacks.Length; i++)
                eval_stacks[i] = new Stack<Value>();
        }



        // Retrieve the variable "symbol" from the symbol table.

        public Value get(string symbol, int line_number)
        {
            if (use_global(symbol) == false)
            {
                // try local symbol table, but don't throw an error if it doesn't exist
                var table = local_symbol_table_stack.Peek();
                if (table.ContainsKey(symbol))
                    return table[symbol];
            }

            // The Python behavior is to always try the global symbol table if 
            // a variable cannot be found in the local symbol table.
            if (global_symbol_table.ContainsKey(symbol))
                return global_symbol_table[symbol];

            throw new Exception("Execution error on line "
              + (line_number + 1) + ". Unable to find \""
              + symbol + "\" in the symbol table.");
        }


        public void store(string symbol, Value value)
        {
            if (use_global(symbol))
                global_symbol_table[symbol] = value;
            else
                local_symbol_table_stack.Peek()[symbol] = value;
        }



        // Delete the variable "symbol" from the symbol table.

        public void del(string symbol, int line_number)
        {
            if (use_global(symbol) == false)
            {
                // try local symbol table, but don't throw an error if it doesn't exist
                var table = local_symbol_table_stack.Peek();
                if (table.ContainsKey(symbol))
                {
                    table.Remove(symbol);
                    return;
                }
            }

            // The Python behavior is to always try the global symbol table if 
            // a variable cannot be found in the local symbol table.
            if (global_symbol_table.ContainsKey(symbol))
            {
                global_symbol_table.Remove(symbol);
                return;
            }

            throw new Exception("Execution error on line "
              + (line_number + 1) + ". Unable to find \""
              + symbol + "\" in the symbol table.");
        }



        // Decision function to whether use the global symbol table, or the local
        // symbol table. This does not follow the CPython implementation
        // exactly.

        bool use_global(string symbol)
        {
            if (local_symbol_table_stack.Count > 0)
            {
                // if there is a local symbol table
                if (global_symbols_stack.Peek().Contains(symbol))
                    // if the variable has been explicitly mentioned as global
                    return true;
                else
                    return false;
            }
            else
                // no local symbol table
                return true;
        }



        // Add entries to "local_symbol_table_stack" and "global_symbols_stack".

        public void enter_function_call()
        {
            // The basic implementation is:
            // local_symbol_table_stack.Push(new Dictionary<string, Value>());
            // global_symbols_stack.Push(new HashSet<string>());
            // 
            // The rest of this function is about re-using objects allocated in the past.

            // check for too many nested function calls
            if (nested_function >= nested_function_limit)
                throw new Exception("The built-in limit of " + nested_function_limit
                    + " nested functions has been exceeded.");

            if (nested_function < local_symbol_table_buffer.Length)
            {
                // use the buffered "local_symbol_table" and "global_symbols"
                if (local_symbol_table_buffer[nested_function] == null)
                {
                    local_symbol_table_buffer[nested_function] = new Dictionary<string, Value>();
                    global_symbols_buffer[nested_function] = new HashSet<string>();
                }

                local_symbol_table_stack.Push(local_symbol_table_buffer[nested_function]);
                global_symbols_stack.Push(global_symbols_buffer[nested_function]);
            }
            else
            {
                // make new dictionary and hash set
                local_symbol_table_stack.Push(new Dictionary<string, Value>());
                global_symbols_stack.Push(new HashSet<string>());
            }

            nested_function++;
        }



        // Remove entries from "local_symbol_table_stack" and "global_symbols_stack".

        public void exit_function_call()
        {
            local_symbol_table_stack.Pop();
            global_symbols_stack.Pop();

            nested_function--;
            if (nested_function < local_symbol_table_buffer.Length)
            {
                local_symbol_table_buffer[nested_function].Clear();
                global_symbols_buffer[nested_function].Clear();
            }
        }


        public void add_global_variable(string symbol)
        {
            if (local_symbol_table_stack.Count == 0)
                throw new Exception("Attempting to add a global variable when the program is "
                  + "already running in global scope.");

            var global_symbols = global_symbols_stack.Peek();
            global_symbols.Add(symbol);
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            // Print "global_symbol_table"
            sb.Append("Global vars: ");
            print_symbol_table(global_symbol_table, sb);

            // Print top of "local_symbol_table_stack"
            if (local_symbol_table_stack.Count > 0)
            {
                sb.AppendLine();
                sb.Append("Local vars: ");
                print_symbol_table(local_symbol_table_stack.Peek(), sb);
            }

            return sb.ToString();
        }



        // Convert "symbol_table" to string and append to "StringBuilder sb".

        void print_symbol_table(Dictionary<string, Value> symbol_table, StringBuilder sb)
        {
            // Print in alphabetical order rather than dictionary order ??
            // but I don't really use this that much...

            foreach (var symbol in symbol_table.Keys)
            {
                var value = symbol_table[symbol];
                if (value.Type == ValueType.String)
                    sb.Append(symbol + "=\"" + value + "\"  ");
                else
                    sb.Append(symbol + "=" + value + "  ");
            }
        }



        // This is for use with expression::eval() only. 
        // Allocates a new "Stack &lt;Value>" object, retrieving from an
        // internal buffer to speed things up. Also keeps track
        // of nested expression eval() call depth.

        public Stack<Value> get_eval_stack()
        {
            if (nested_eval >= nested_eval_limit)
            {
                throw new Exception("The built-in limit of " + nested_eval_limit
                  + " nested expressions has been exceeded.");
            }

            Stack<Value> return_var;

            if (nested_eval < eval_stacks.Length)
                return_var = eval_stacks[nested_eval];
            else
                return_var = new Stack<Value>();

            nested_eval++;

            return return_var;
        }



        // Buffer the stack object to be re-used by "get_eval_stack()".

        // <param name="stack"></param>
        public void return_eval_stack()
        {
            nested_eval--;

            if (nested_eval < eval_stacks.Length)
                eval_stacks[nested_eval].Clear();
        }


    }



    class FunctionArguments
    {
        List<Expression> positional_args = new List<Expression>();
        Dictionary<string, Expression> named_args = new Dictionary<string, Expression>();


        // Add a portion of the token list as an argument. Will detect
        // name arguments.

        public void add_argument(TokenList token_list, int start_index, int end_index)
        {
            // Detect named arguments - they start with: [Identifier, =]
            if (end_index - start_index >= 2
              && token_list[start_index].type == TokenType.Identifier
              && token_list[start_index + 1].type == TokenType.Assign)
            {
                // Handle name arguments
                var name = (string)token_list[start_index].value;
                var expression = new Expression(token_list, start_index + 2, end_index);
                named_args.Add(name, expression);
            }
            else
            {
                // Handle (standard) positional arguments.
                // Positional arguments are not allowed after named arguments.
                if (named_args.Count > 0)
                    throw new Exception("Positional argument must precede keyword arguments");

                var expression = new Expression(token_list, start_index, end_index);
                positional_args.Add(expression);
            }
        }



        // Adds an argument that is of type "Value".

        public void add_value_argument(Value value, int line_number)
        {
            positional_args.Add(new Expression(value, line_number));
        }



        // Adds a null to "positional_args".
        public void add_null_argument()
        {
            positional_args.Add(null);
            // This is for creating a hole in "positional_args".

            // Use case: s2[1::1] - this resolve to the function call
            // slice(1, null, 1). The "null" in the middle is needed.
            // This value is the length of the string (or list) s2, 
            // which is unknown at parse time.
        }



        // Retrieve an int argument based on the parameters given. Will
        // throw exception if the retrieval fails.

        // <param name="round_double">If set to true, even if the argument
        // is a double, return a rounded version of it.</param>
        public int get_int_argument(int? arg_position, string arg_name,
          int? default_arg, SymbolTable symbol_table, bool round_double = false)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
            {
                // Argument retrieved, but need to check the type before returning.
                if (arg.Type == ValueType.Dynamic)
                {
                    var arg_dyn = (DynamicValue)arg;

                    // Return "arg_dyn.value" if it's an integer, or if rounding is allowed
                    if (arg_dyn.value is int) return (int)arg_dyn.value;

                    else if (arg_dyn.value is double && round_double == true)
                    {
                        int return_val = (int)Math.Round((double)arg_dyn.value);
                        return return_val;
                    }

                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting an integer but "
                      + ((object)arg_dyn.value).ToString() + " was not accepted as an integer.");
                }
                else
                    // arg.Type is not Dynamic
                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting an int but got "
                      + arg.Type + " instead.");
            }
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg.Value;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return 0; // this should not happen
                }
            }
        }


        // Retrieve a double argument based on the parameters given. Will
        // throw exception if the retrieval fails.
        public double get_double_argument(int? arg_position, string arg_name,
          double? default_arg, SymbolTable symbol_table)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
            {
                // Argument retrieved, but need to check the type before returning.
                if (arg.Type == ValueType.Dynamic)
                {
                    var arg_dyn = (DynamicValue)arg;

                    // Return "arg_dyn.value" if it's an integer or double
                    if (arg_dyn.value is double) return arg_dyn.value;
                    else if (arg_dyn.value is int) return (double)arg_dyn.value;

                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a double but "
                      + ((object)arg_dyn.value).ToString() + " was not accepted as a double.");
                }
                else
                    // arg.Type is not Dynamic
                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a double but got "
                      + arg.Type + " instead.");
            }
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg.Value;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return 0; // this should not happen
                }
            }
        }

        public decimal get_decimal_argument(int? arg_position, string arg_name,
          decimal? default_arg, SymbolTable symbol_table)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
            {
                // Argument retrieved, but need to check the type before returning.
                if (arg.Type == ValueType.Dynamic)
                {
                    var arg_dyn = (DynamicValue)arg;

                    // Return "arg_dyn.value" if it's an integer or double
                    if (arg_dyn.value is decimal) return arg_dyn.value;
                    else if (arg_dyn.value is int) return (decimal)arg_dyn.value;

                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a decimal but "
                      + ((object)arg_dyn.value).ToString() + " was not accepted as a decimal.");
                }
                else
                    // arg.Type is not Dynamic
                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a decimal but got "
                      + arg.Type + " instead.");
            }
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg.Value;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return 0; // this should not happen
                }
            }
        }



        // Retrieve a bool argument based on the parameters given. Will
        // throw exception if the retrieval fails.
        public bool get_bool_argument(int? arg_position, string arg_name,
          bool? default_arg, SymbolTable symbol_table)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
            {
                // Argument retrieved, but need to check the type before returning.
                if (arg.Type == ValueType.Dynamic)
                {
                    var arg_dyn = (DynamicValue)arg;

                    // Return "arg_dyn.value" if it's an bool
                    if (arg_dyn.value is bool) return (bool)arg_dyn.value;

                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a boolean but "
                      + ((object)arg_dyn.value).ToString() + " was not accepted as a boolean.");
                }
                else
                    // arg.Type is not Dynamic
                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a boolean but got "
                      + arg.Type + " instead.");
            }
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg.Value;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return false; // this should not happen
                }
            }
        }



        // Retrieve a string argument based on the parameters given. Will
        // throw exception if the retrieval fails.
        public string get_string_argument(int? arg_position, string arg_name,
          string default_arg, SymbolTable symbol_table)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
            {
                // Argument retrieved, but need to check the type before returning.
                if (arg.Type == ValueType.String)
                    return ((StringValue)arg).value;
                else
                    throw new Exception("The argument \"" + arg_name
                      + "\" did not evaluate to the correct type. Expecting a string but got "
                      + arg.Type + " instead.");
            }
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return null; // this should not happen
                }
            }
        }


        // Retrieve a Value argument based on the parameters given. Will
        // throw exception if the retrieval fails.
        public Value get_value_argument(int? arg_position, string arg_name,
          Value default_arg, SymbolTable symbol_table)
        {
            Value arg = get_argument(arg_position, arg_name, symbol_table);

            if (arg != null)
                return arg;
            else
            {
                // Argument cannot be retrieved
                if (default_arg != null)
                    return default_arg;
                else
                {
                    throw_failed_arg_retrieval_error(arg_position, arg_name);
                    return null; // this should not happen
                }
            }
        }


        // Retrieve arguments starting at a certain position. This is like 
        // getting (1, 2, 3) from f(1, 2, 3). This is NOT getting a list
        // at a particular argument position.
        public List<Value> get_list_arguments(int arg_position, SymbolTable symbol_table)
        {
            var return_val = new List<Value>();

            for (int i = arg_position; i < positional_args.Count; i++)
            {
                var result = positional_args[i].eval(symbol_table);
                return_val.Add(result);
            }

            return return_val;
        }



        // Check that the total number of arguments equal to "n". 
        // If not, thrown an error.
        public void check_num_args(int n)
        {
            if (Count != n)
                throw new Exception("Incorrect number of arguments. Expecting "
                  + n + " arguments but found " + Count + ".");
        }



        // Check that the total number of arguments >= "n". 
        // If not, thrown an error.
        public void check_num_args_minimum(int n)
        {
            if (Count < n)
                throw new Exception("Incorrect number of arguments. Expecting at least "
                  + n + " arguments but found " + Count + ".");
        }



        // Check that the total number of arguments <= "n". 
        // If not, thrown an error.
        public void check_num_args_maximum(int n)
        {
            if (Count > n)
                throw new Exception("Incorrect number of arguments. Expecting at most "
                  + n + " arguments but found " + Count + ".");
        }


        // If the argument is present in both "positional_args" and 
        // "named_args", then throw an error.
        void check_duplicate_args(int? arg_position, string arg_name)
        {
            if (arg_position < positional_args.Count && arg_name != null
              && named_args.ContainsKey(arg_name))
                throw new Exception("The \"" + arg_name + "\" argument is present both "
                  + "at position number " + arg_position + " and as a named argument.");
        }



        // Attempt to retrieve the argument at "arg_position", named "arg_name". 
        Value get_argument(int? arg_position, string arg_name, SymbolTable symbol_table)
        {
            // If both "arg_position" and "arg_name" are valid, report as error.
            check_duplicate_args(arg_position, arg_name);

            if (arg_position < positional_args.Count)
            {
                // Handle positional argument
                var expr = positional_args[arg_position.Value];

                if (expr == null) return null;
                else return expr.eval(symbol_table);
            }

            else if (arg_name != null && named_args.ContainsKey(arg_name))
            {
                // Handle named argument
                var expr = named_args[arg_name];

                if (expr == null) return null;
                else return expr.eval(symbol_table);
            }

            else
                return null; // Argument not found
        }



        // Throw an exception stating that a given argument cannot be retrieved.
        void throw_failed_arg_retrieval_error(int? arg_position, string arg_name)
        {
            if (arg_position != null)
                throw new Exception("The argument at position " + arg_position.Value
                  + " cannot be retrieved.");
            else if (arg_name != null)
                throw new Exception("The argument \"" + arg_name + "\" cannot be retrieved.");
            else
                // Code should not reach here - arguments should have a position or a name.
                throw new Exception("Function argument cannot be retrieved.");
        }


        public int CountIncludeNulls
        {
            get { return positional_args.Count + named_args.Count; }
        }


        public int Count
        {
            get
            {
                // This version of count includes only non-null arguments.
                int count = 0;
                foreach (var arg in positional_args)
                    if (arg != null) count++;

                foreach (var key in named_args.Keys)
                    if (named_args[key] != null) count++;

                return count;
            }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var arg in positional_args)
            {
                if (arg != null)
                    sb.Append(arg.ToString() + ", ");
                else
                    sb.Append("null, ");
            }

            foreach (var key in named_args.Keys)
                sb.Append(key + "=" + named_args[key].ToString() + ", ");

            return sb.ToString().TrimEnd(' ', ',');
        }
    }



    class FunctionCall
    {
        public int return_addr;
        public FunctionArguments arguments;
    }



}
