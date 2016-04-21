using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;

namespace jsonflatten
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

            if (extra.Count > 0)
            {
                LogBadInput("did not expect any input arguments");
                return;
            }

            // Parse the JSON dictionary
            object dicts;
            try
            {
                string input = Console.In.ReadToEnd();
                dicts = JsonConvert.DeserializeObject(input);
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

            var flat = Flatten(dicts);

            Console.WriteLine(JsonConvert.SerializeObject(flat));
        }

        private static Dictionary<string, object> Flatten(Newtonsoft.Json.Linq.JObject Dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in Dictionary.Properties())
            {
                var flat = Flatten(pair.Value);
                if (flat is Dictionary<string, object>)
                {
                    foreach (var item in (Dictionary<string, object>)flat)
                    {
                        result[item.Key] = item.Value;
                    }
                }
                else
                {
                    result[pair.Name] = flat;
                }
            }
            return result;
        }

        private static object Flatten(object Input)
        {
            if (Input is Newtonsoft.Json.Linq.JObject)
            {
                return Flatten((Newtonsoft.Json.Linq.JObject)Input);
            }
            else if (Input is Newtonsoft.Json.Linq.JArray)
            {
                return ((Newtonsoft.Json.Linq.JArray)Input).Select(Flatten).ToArray();
            }
            else
            {
                return Input;
            }
        }

        public static void ShowHelp (OptionSet p)
        {
            Console.WriteLine("usage: json-flatten [options]");
            Console.WriteLine("recursively flattens JSON dictionaries");
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
