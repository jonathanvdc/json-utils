using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace jsonadd
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            bool show_help = false;

            var p = new OptionSet();
            p.Add("h|help", "show this message and exit", 
                v => show_help = v != null);

            List<string> extra;
            try 
            {
                extra = p.Parse(args);
            }
            catch (OptionException e) 
            {
                LogBadInput(e.Message);
                return;
            }

            if (show_help) 
            {
                ShowHelp(p);
                return;
            }

            if (extra.Count != 2)
            {
                LogBadInput("expected exactly two input arguments");
                return;
            }

            // Parse the JSON dictionary
            Dictionary<string, object> dict;
            try
            {
                string input = Console.In.ReadToEnd();
                dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            }
            catch (JsonException)
            {
                LogFatalError("could not parse standard input as a JSON dictionary");
                return;
            }

            if (dict == null)
            {
                LogFatalError("no input data");
            }

            string fieldName = extra[0];
            string fieldVal = extra[1];

            dict[fieldName] = fieldVal;

            Console.WriteLine(JsonConvert.SerializeObject(dict));
        }

        public static void ShowHelp (OptionSet p)
        {
            Console.WriteLine("usage: json-add field-name field-value");
            Console.WriteLine(
                "adds a single named field to a JSON dictionary, " +
                "or overwrites said field if it is already present");
            Console.WriteLine("JSON is parsed from standard input, and is written to standard output");
            Console.WriteLine();
            Console.WriteLine("options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void LogBadInput(string Message)
        {
            Console.Write("json-add: ");
            Console.WriteLine(Message);
            Console.WriteLine("try `json-add --help' for more information");
            Environment.Exit(1);
        }

        private static void LogError(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("error: ");
            Console.ResetColor();
            Console.WriteLine(Message);
        }

        private static void LogFatalError(string Message)
        {
            LogError(Message);
            Environment.Exit(1);
        }
    }
}
