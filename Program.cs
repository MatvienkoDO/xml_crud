using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;

namespace information_technologies_task
{
    class Program
    {
        static void Main(string[] args)
        {
            if (isHelp(args)) {
                Console.Write(GetHelpMessage());
                return;
            }

            const string dataFileName = "data.xml";
            string dataPath = Path.Combine(Environment.CurrentDirectory, dataFileName);
            XmlDocument document = LoadDocument(dataPath);

            if (isDelete(args)) {
                string id = GetArgumentValue(args, "-i");
                if (string.IsNullOrEmpty(id)) {
                    Console.WriteLine("You should specify id of document");
                    return;
                }

                XmlNode toDelete = document.DocumentElement.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(
                        node => node.Attributes.Cast<XmlAttribute>()
                            .Any(attr => attr.Name == "id" && attr.Value == id)
                    );
                if (toDelete == default(XmlNode)) {
                    Console.WriteLine("There is no document with such id");
                    return;
                }

                document.DocumentElement.RemoveChild(toDelete);
                document.Save(dataPath);
                return;
            }

            if (isCreate(args)) {
                string name = GetArgumentValue(args, "-n");
                if (string.IsNullOrEmpty(name)) {
                    Console.WriteLine("You should specify name of document");
                    return;
                }
                string text = GetArgumentValue(args, "-t");
                if (string.IsNullOrEmpty(text)) {
                    Console.WriteLine("You should specify text of document");
                    return;
                }
                string id = Guid.NewGuid().ToString();

                var idAttr = document.CreateAttribute("id");
                idAttr.Value = id;
                var nameAttr = document.CreateAttribute("name");
                nameAttr.Value = name;
                
                var newElement = document.CreateElement("Document");
                newElement.Attributes.Append(idAttr);
                newElement.Attributes.Append(nameAttr);
                newElement.InnerText = text;

                document.DocumentElement.AppendChild(newElement);
                document.Save(dataPath);
                return;
            }

            if (isUpdate(args)) {
                string id = GetArgumentValue(args, "-i");
                if (string.IsNullOrEmpty(id)) {
                    Console.WriteLine("You should specify id of document");
                    return;
                }
                string name = GetArgumentValue(args, "-n");
                string text = GetArgumentValue(args, "-t");
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(text)) {
                    Console.WriteLine("You should specify name or/and text of document");
                    return;
                }

                XmlNode toUpdate = document.DocumentElement.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(
                        node => node.Attributes.Cast<XmlAttribute>()
                            .Any(attr => attr.Name == "id" && attr.Value == id)
                    );
                if (toUpdate == default(XmlNode)) {
                    Console.WriteLine("There is no document with such id");
                    return;
                }

                if (!string.IsNullOrEmpty(name)) {
                    toUpdate.Attributes.GetNamedItem("name").Value = name;
                }
                if (!string.IsNullOrEmpty(text)) {
                    toUpdate.InnerText = text;
                }

                document.Save(dataPath);
                return;
            }

            if (isRead(args)) {
                string id = GetArgumentValue(args, "-i");
                if (string.IsNullOrEmpty(id)) {
                    Console.WriteLine("You should specify id of document");
                    return;
                }

                XmlNode toShow = document.DocumentElement.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(
                        node => node.Attributes.Cast<XmlAttribute>()
                            .Any(attr => attr.Name == "id" && attr.Value == id)
                    );
                if (toShow == default(XmlNode)) {
                    Console.WriteLine("There is no document with such id");
                    return;
                }

                string name = toShow.Attributes.Cast<XmlAttribute>()
                    .FirstOrDefault(attr => attr.Name == "name")?.Value;
                string value = toShow.InnerText;

                var sb = new StringBuilder()
                    .AppendLine("Document")
                    .Append("Id: ").AppendLine(id)
                    .Append("Name: ").AppendLine(name)
                    .Append("Text: ").AppendLine(value);

                Console.Write(sb.ToString());
                return;
            }

            Console.WriteLine("There are no actions corresponding to current arguments. (Help: -h, --help)");
        }

        private static XmlDocument LoadDocument(string dataPath) {            
            var document = new XmlDocument();

            try {
                document.Load(dataPath);
            } catch(FileNotFoundException e) {
                Console.WriteLine("File data.xml was not found. It should be located in current working directory");
                throw e;
            }

            return document;
        }

        private static string GetHelpMessage() {
            StringBuilder sb = new StringBuilder()
                .AppendLine("Help")
                .AppendLine("-h, --help - for show this help message")
                .AppendLine("-i <id> - specifies id of document to update")
                .AppendLine("-n <name> - specifies new name of document")
                .AppendLine("-t <text> - specifies new text of document")
                .AppendLine("-d - deletes document specified by id (-i)")
                .AppendLine()
                .AppendLine("For create specify id (-i <id>) and delete (-d)")
                .AppendLine("For read specify only id (-i <id>)")
                .AppendLine("For update specify id and name or/and text")
                .AppendLine("For delete specify delete argument (-d)");

            return sb.ToString();
        }

        private static bool isHelp(string[] args) {
            return args.Contains("-h") || args.Contains("--help");
        }

        private static bool isDelete(string[] args) {
            return args.Contains("-i") && args.Contains("-d");
        }

        private static bool isRead(string[] args) {
            return args.Contains("-i") && !args.Contains("-n") && !args.Contains("-t") && !args.Contains("-d");
        }

        private static bool isCreate(string[] args) {
            return args.Contains("-n") && args.Contains("-t") && !args.Contains("-i") && !args.Contains("-d");
        }

        private static bool isUpdate(string[] args) {
            return args.Contains("-i") && (args.Contains("-n") || args.Contains("-t")) && !args.Contains("-d");
        }

        private static string GetArgumentValue(string[] args, string arg) {
            int index = IndexOf(args, arg);
            int next = index + 1;
            if (index < 0 || next == args.Length) {
                return null;
            }

            string value = args[next];
            if (!string.IsNullOrEmpty(value) && value[0] == '-') {
                return null;
            }

            return value;
        }

        private static int IndexOf<T>(T[] array, T element) {
            int length = array.Length;

            for (int i = 0; i < length; ++i) {
                if (array[i].Equals(element)) {
                    return i;
                }
            }

            return -1;
        }
    }
}
