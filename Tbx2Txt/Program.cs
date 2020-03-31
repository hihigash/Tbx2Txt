using System;
using System.Collections.Generic;
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
                new Option<string>(new[] {"--inputFile", "-i"})
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option<string>(new[] {"--outDir", "-o"})
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option<string>(new []{"--from", "-f"})
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option<string>(new []{"--to", "-t"})
                {
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option<bool>("--tsv")
                {
                    Required = false
                }
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool>(Run);
            return rootCommand.Invoke(args);
        }

        private static int Run(string inputFile, string outDir, string from="en-US", string to="ja-JP", bool tsv=false)
        {
            if (inputFile == null) throw new ArgumentNullException(nameof(inputFile));
            if (outDir == null) throw new ArgumentNullException(nameof(outDir));
            if (!File.Exists(inputFile)) throw new FileNotFoundException("tbx file is not found", inputFile);

            var terms = new List<Tuple<string, string>>();

            XDocument document = XDocument.Load(File.OpenText(inputFile));
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            foreach (var termEntry in document.Descendants("termEntry"))
            {
                var termFrom = termEntry.XPathSelectElement($"./langSet[@xml:lang='{from}']/ntig/termGrp/term", namespaceManager).Value;
                Debug.Assert(!string.IsNullOrWhiteSpace(termFrom));

                var termTo = termEntry.XPathSelectElement($"./langSet[@xml:lang='{to}']/ntig/termGrp/term", namespaceManager).Value;
                Debug.Assert(!string.IsNullOrWhiteSpace(termTo));

                terms.Add(new Tuple<string, string>(termFrom,termTo));
            }

            var baseFileName = Path.GetFileNameWithoutExtension(inputFile);
            if (tsv)
            {
                using TextWriter writer = new StreamWriter(Path.Combine(outDir, $"{baseFileName}.tsv"));
                foreach (var term in terms)
                {
                    writer.WriteLine($"{term.Item1}\t{term.Item2}");
                }
            }
            else
            {
                using TextWriter writerFrom = new StreamWriter(Path.Combine(outDir, $"{baseFileName}-{from}.txt"));
                using TextWriter writerTo = new StreamWriter(Path.Combine(outDir, $"{baseFileName}-{to}.txt"));

                foreach (var term in terms)
                {
                    writerFrom.WriteLine(term.Item1);
                    writerTo.WriteLine(term.Item2);
                }
            }

            return 0;
        }
    }
}
