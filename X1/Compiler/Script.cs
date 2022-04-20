using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace X1.Compiler
{
    public class Script
    {
        public class LogSettings
        {
            public bool print_tokens = false;
            public bool print_intermediate_code = false;
            public bool print_statements = false;
        }
        LogSettings log_settings = new LogSettings();

        string[] source; // source code, one line per string
        internal List<Statement> statements;
        internal Dictionary<string, int> user_def_function_locations;

        public Script(string source_code, LogSettings log_settings = null)
        {
            if (log_settings != null) this.log_settings = log_settings;
            else log_settings = this.log_settings;

            // Break "source" into lines. 
            source = Regex.Split(source_code, "\r\n|\r|\n");

            // Remove white space at the end of the lines.
            for (int i = 0; i < source.Length; i++)
                source[i] = source[i].TrimEnd();

            // Tokenize
            var tokenizer = new Tokenizer(source);

            // Console.WriteLine("============== TOKENIZER FINISHED ===========");

            if (log_settings.print_tokens)
            {
                Console.WriteLine("Tokenization results:");
                Console.WriteLine(tokenizer.ToString());
                Console.WriteLine();
            }

            // Parse
            var parser = new Parser(tokenizer.token_lists, log_settings.print_intermediate_code);
            statements = parser.statements;
            user_def_function_locations = parser.user_def_function_locations;

            // Console.WriteLine("============== PARSER FINISHED ===========");

            if (log_settings.print_statements)
            {
                Console.WriteLine("Program statements:");

                int max_addr_length = statements.Count.ToString().Length;

                for (int i = 0; i < statements.Count; i++)
                {
                    string addr = i.ToString().PadLeft(max_addr_length);
                    Console.WriteLine(addr + ' ' + statements[i].ToString());
                }
                Console.WriteLine("");
            }
        }


        static public Script load_from_file(string file_name, LogSettings log_settings = null)
        {
            var source = System.IO.File.ReadAllText(file_name, Encoding.UTF8);
            return new Script(source, log_settings);
        }

        static public Script load_from_string(string file_name, LogSettings log_settings = null)
        {
            var source = System.IO.File.ReadAllText(file_name, Encoding.UTF8);
            return new Script(source, log_settings);
        }
    }




}
