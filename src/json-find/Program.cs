using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace jsonfind
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            bool show_help = false;
            int arrIndex = 0;

            var p = new OptionSet(); 
            p.Add<int>("i|index=", "specifies that {n}-th matching dictionary element should be returned ({n} is a zero-based index)",
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
            List<Dictionary<string, object>> dicts;
            try
            {
                dicts = ParseJSONArray();
            }
            catch (JsonException)
            {
                LogFatalError("could not parse standard input as a JSON array of dictionaries");
                return;
            }

            if (dicts == null)
            {
                LogFatalError("no input data");
            }

            string fieldName = extra[0];
            string fieldVal = extra[1];

            var filtered = Filter(dicts, fieldName, fieldVal);

            if (filtered.Count == 0)
            {
                LogFatalError("no dictionary had a field named '" + fieldName + "' with value '" + fieldVal + "'");
                return;
            }

            if (filtered.Count <= arrIndex)
            {
                LogFatalError(
                    "found only '" + filtered.Count + "' dictionaries with fields named '" + 
                    fieldName + "' and value '" + fieldVal + "', but requested dictionary '" + 
                    arrIndex + "'");
                return;
            }

            Console.WriteLine(JsonConvert.SerializeObject(filtered[arrIndex]));
        }

        private static List<Dictionary<string, object>> Filter(
            IEnumerable<Dictionary<string, object>> Dictionaries, 
            string FieldName, string FieldValue)
        {
            var results = new List<Dictionary<string, object>>();
            foreach (var item in Dictionaries)
            {
                if (item.ContainsKey(FieldName) && item[FieldName].ToString() == FieldValue)
                    results.Add(item);
            }
            return results;
        }

        private static List<Dictionary<string, object>> ParseJSONArray()
        {
            string input = Console.In.ReadToEnd();

            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(input);
        }

        public static void ShowHelp (OptionSet p)
        {
            Console.WriteLine("usage: json-find [options] field-name field-value");
            Console.WriteLine("find the first dictionary that has a named field with a specific value");
            Console.WriteLine("JSON is parsed from standard input, and written to standard output");
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

        private static void LogBadInput(string Message)
        {
            Console.Write("json-pack: ");
            Console.WriteLine(Message);
            Console.WriteLine("try `json-pack --help' for more information");
            Environment.Exit(1);
        }
    }
}
