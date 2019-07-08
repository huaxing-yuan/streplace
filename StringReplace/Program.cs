using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StringReplace
{
    class Program
    {
        static void Main(string[] args)
        {
            //Args[0] = original resource files
            string sourceFile = args[0];

            //Args[1] = sourceCode folder
            var codeFolder = new DirectoryInfo(args[1]);


            string action = null;
            if (args.Length >= 3) action = args[2];

            XmlDocument xmlDoc = new XmlDocument();
            List<KeyValuePair<string, string>> keyvalues = new List<KeyValuePair<string, string>>();
            //read resources
            using (FileStream fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                xmlDoc.Load(fs);
                int nodeIndex = 0;
                var nodes = xmlDoc.LastChild.ChildNodes;

                foreach (XmlNode node in nodes)
                {
                    nodeIndex++;
                    if (node.LocalName == "String")
                    {
                        string value = node.InnerText;
                        string key = node.Attributes[0].Value;
                        keyvalues.Add(new KeyValuePair<string, string>(key, value));
                    }
                    else if (node.LocalName == "#comment")
                    {
                        keyvalues.Add(new KeyValuePair<string, string>("#comment", node.Value));
                    }
                }

            }

            //Replace xaml.cs files
            foreach (var file in codeFolder.GetFiles("*.xaml.cs", SearchOption.AllDirectories))
            {
                if (file.Name == "App.xaml.cs") continue;
                bool flag = false;
                string content = null;
                using (StreamReader sr = new StreamReader(file.FullName))
                {
                    content = sr.ReadToEnd();
                }
                foreach (var pair in keyvalues)
                {
                    string toFind = pair.Value.Replace("\n", "\\n").Replace("\"", "\\\"");
                    string original = "\"" + toFind + "\"";
                    string toReplace = $"this.FindStringResource(\"{pair.Key}\")";

                    if (content.Contains(original))
                    {
                        content = content.Replace(original, toReplace);
                        Console.WriteLine("  -" + pair.Key + ": " + pair.Value);
                        flag = true;
                    }
                }

                if (flag)
                {
                    Console.WriteLine("Replacement found in:" + file.FullName);
                    if (action == "RUN")
                    {
                        Console.WriteLine("UPDATING SOURCE CODE.");
                        using (StreamWriter sw = new StreamWriter(file.FullName))
                        {
                            sw.Write(content);
                        }
                    }
                }
            }

            string[] tags = { "Text=", "Header=", "CheckedText=", "UncheckedText=", "ToolTip=", "Content=", "Title=", "Caption="};

            foreach (var file in codeFolder.GetFiles("*.xaml", SearchOption.AllDirectories))
            {
                if (file.Name == "App.xaml") continue;
                bool flag = false;
                string content = null;
                using (StreamReader sr = new StreamReader(file.FullName))
                {
                    content = sr.ReadToEnd();
                }

                foreach(var pair in keyvalues)
                {
                    string toFind = pair.Value.Replace("\n", "&#13;");
                    foreach(var tag in tags)
                    {
                        string original = tag + "\"" + toFind + "\"";
                        string toReplace = tag + $"\"{{DynamicResource {pair.Key}}}\"";

                        if (content.Contains(original))
                        {
                            content = content.Replace(original, toReplace);
                            Console.WriteLine("  -" + pair.Key + ": " + pair.Value);
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    Console.WriteLine("Replacement found in:" + file.FullName);
                    if (action == "RUN")
                    {
                        Console.WriteLine("UPDATING SOURCE CODE.");
                        using (StreamWriter sw = new StreamWriter(file.FullName))
                        {
                            sw.Write(content);
                        }
                    }
                }
            }

            Console.WriteLine("Scan finished, press any key to continue");
            Console.ReadLine();
        }
    }
}
