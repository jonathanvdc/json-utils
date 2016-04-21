using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;
using System.IO;

namespace jsonpack
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

            // Parse the JSON dictionaries
            var dicts = new List<Dictionary<string, object>>();
            foreach (var item in extra)
            {
                ParseDictionary(item, dicts);
            }

            Console.WriteLine(JsonConvert.SerializeObject(dicts));
        }

        private static string ReadFile(string FileName)
        {
            if (FileName == "-")
            {
                return Console.In.ReadToEnd();
            }

            if (!File.Exists(FileName))
            {
                LogFatalError("file '" + FileName + "' does not exist");
                return null;
            }

            try
            {
                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                LogFatalError("could not open file '" + FileName + "'");
                return null;
            }
        }

        private static void ParseDictionary(string FileName, List<Dictionary<string, object>> DictionaryList)
        {
            string input = ReadFile(FileName);
            Dictionary<string, object> dict;
            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            }
            catch (JsonException)
            {
                LogFatalError("could not parse standard input as a JSON dictionary");
                return;
            }

            if (dict == null)
            {
                LogFatalError("file '" + FileName + "' is empty");
                return;
            }

            DictionaryList.Add(dict);
        }

        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("usage: json-pack file...");
            Console.WriteLine("packs JSON dictionaries in a single JSON array");
            Console.WriteLine("JSON is parsed from files, and is written to standard output");
            Console.WriteLine();
            Console.WriteLine("options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void LogBadInput(string Message)
        {
            Console.Write("json-pack: ");
            Console.WriteLine(Message);
            Console.WriteLine("try `json-pack --help' for more information");
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
