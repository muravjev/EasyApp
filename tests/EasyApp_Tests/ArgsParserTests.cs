using System.Diagnostics;

namespace EasyApp
{
    public class ArgsParserTests
    {
        public class Options
        {
            [Flag('h', "help", "Help screen.", IsBreaker = false)]
            public bool Help = false;

            [Flag('v', "version", "Show version namber.", IsBreaker = false)]
            public bool Version = false;

            [Option(1, 'f', "format", "Datetime format.")]
            public string Format = "yyyy-MM-dd";

            [Value(2, "directory", "Directory to process.")]
            public string? Directory = null;

            [Value(3, "file", "File to process.")]
            public string? File = null;
        }

        [Test]
        public void Parsed()
        {
            var option1Value = "yyyy-MM-dd";

            var flag1 = "-h";
            var option1 = "-f " + option1Value;
            var value1 = "foo";
            var value2 = "bar";

            var parser = new AppArgs() as IAppArgs;
            var permutations = Utils.GetPermutations(new[] { flag1, option1, value1, value2 }, 4);

            foreach (var permutation in permutations)
            {
                var args = permutation.ToArray();

                if (Array.IndexOf(args, value1) > Array.IndexOf(args, value2))
                {
                    continue;
                }

                var commandLine = string.Join(" ", permutation);

                Debug.WriteLine(commandLine);

                args = commandLine.Split(" ");

                var result = parser.Parse<Options>(args);

                Assert.That(result.IsParsed, Is.EqualTo(true));

                var options = result.Options;

                Assert.Multiple(() =>
                {
                    Assert.That(options.Help, Is.EqualTo(true));
                    Assert.That(options.Format, Is.EqualTo(option1Value));
                    Assert.That(options.Directory, Is.EqualTo(value1));
                    Assert.That(options.File, Is.EqualTo(value2));
                });
            }
        }
    }
}