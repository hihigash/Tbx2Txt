using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Tbx2Txt
{
    class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                new Option(new[] {"--inputFile", "-i"})
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option(new[] {"--outDir", "-o"})
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option(new []{"--from", "-f"})
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new []{"--to", "-t"})
                {
                    Argument = new Argument<string>(),
                    Required = false
                }
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string>(Run);
            return rootCommand.Invoke(args);
        }

        private static int Run(string inputFile, string outDir, string from="en-US", string to="ja-JP")
        {
            if (inputFile == null) throw new ArgumentNullException(nameof(inputFile));
            if (outDir == null) throw new ArgumentNullException(nameof(outDir));
            if (!File.Exists(inputFile)) throw new FileNotFoundException("tbx file is not found", inputFile);

            var baseFileName = Path.GetFileNameWithoutExtension(inputFile);
            using TextWriter writerFrom = new StreamWriter(Path.Combine(outDir, $"{baseFileName}-{from}.txt"));
            using TextWriter writerTo = new StreamWriter(Path.Combine(outDir, $"{baseFileName}-{to}.txt"));

            XDocument document = XDocument.Load(File.OpenText(inputFile));
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            foreach (var termEntry in document.Descendants("termEntry"))
            {
                var termFrom = termEntry.XPathSelectElement($"./langSet[@xml:lang='{from}']/ntig/termGrp/term", namespaceManager).Value;
                Debug.Assert(!string.IsNullOrWhiteSpace(termFrom));
                writerFrom.WriteLine(termFrom);

                var termTo = termEntry.XPathSelectElement($"./langSet[@xml:lang='{to}']/ntig/termGrp/term", namespaceManager).Value;
                Debug.Assert(!string.IsNullOrWhiteSpace(termTo));
                writerTo.WriteLine(termTo);
            }

            return 0;
        }
    }
}
