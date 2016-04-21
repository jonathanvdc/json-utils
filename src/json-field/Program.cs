using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace jsonfield
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            bool show_help = false;
            int? arrIndex = null;

            var p = new OptionSet(); 
            p.Add<int>("i|index=", "specifies that the JSON dictionary element is the {n}-th element in a JSON array ({n} is a zero-based index)",
                v => arrIndex = v);
            p.Add("h|help", "show this message and exit", 
                v => show_help = v != null);

            List<string> extra;
            try 
            {
                extra = p.Parse(args);
            }
            catch (OptionException e) 
            {
                Console.Write("json-field: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("try `json-field --help' for more information");
                Environment.Exit(1);
                return;
            }

            if (show_help) 
            {
                ShowHelp(p);
                return;
            }

            // Parse the JSON dictionary
            Dictionary<string, object> dict;
            try
            {
                dict = ParseJSONDictionary(arrIndex);
            }
            catch (JsonException)
            {
                if (arrIndex.HasValue)
                {
                    LogFatalError("could not parse standard input as a JSON array of dictionaries");
                }
                else
                {
                    LogFatalError("could not parse standard input as a JSON dictionary");
                }
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                LogFatalError("array index '" + arrIndex.Value + "' was out of range");
                return;
            }

            if (dict == null)
            {
                LogFatalError("no input data");
            }

            var results = new List<object>();
            foreach (var name in extra)
            {
                if (!dict.ContainsKey(name))
                {
                    LogFatalError("JSON dictionary did not contain a field named '" + name + "'");
                    return;
                }
                results.Add(dict[name]);
            }

            foreach (var item in results)
            {
                Console.WriteLine(item);
            }
        }

        private static Dictionary<string, object> ParseJSONDictionary(int? ArrayIndex)
        {
            string input = Console.In.ReadToEnd();

            if (ArrayIndex.HasValue)
            {
                var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(input);
                return result == null ? null : result[ArrayIndex.Value];
            }
            else
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            }
        }

        public static void ShowHelp (OptionSet p)
        {
            Console.WriteLine("usage: json-field [options] field-names");
            Console.WriteLine("extracts named fields from a JSON dictionary, or an array of JSON dictionaries");
            Console.WriteLine("JSON is parsed from standard input");
            Console.WriteLine();
            Console.WriteLine("options:");
            p.WriteOptionDescriptions(Console.Out);
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
