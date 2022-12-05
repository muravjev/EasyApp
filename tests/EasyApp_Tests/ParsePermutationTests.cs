using System.Diagnostics;

namespace EasyApp
{
    public class ParsePermutationTests
    {
        public class Options
        {
            [Flag("f1", "Description", IsBreaker = false)]
            public bool F1 = false;

            [Option("o1", "Description")]
            public string? O1 = null;

            [Parameter(1, "p1", "Description")]
            public string? P1 = null;

            [Parameter(2, "p2", "Description")]
            public string? P2 = null;
        }

        internal static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        [Test]
        public void Parsed()
        {
            var o1Value = "yyyy-MM-dd";

            var f1 = "--f1";
            var o1 = "--o1 " + o1Value;
            var p1 = "foo";
            var p2 = "bar";

            var parser = new AppArgs() as IAppArgs;
            var permutations = GetPermutations(new[] { f1, o1, p1, p2 }, 4);

            foreach (var permutation in permutations)
            {
                var args = permutation.ToArray();

                //< Keep order of Parameters.

                if (Array.IndexOf(args, p1) > Array.IndexOf(args, p2))
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
                    Assert.That(options.F1, Is.EqualTo(true));
                    Assert.That(options.O1, Is.EqualTo(o1Value));
                    Assert.That(options.P1, Is.EqualTo(p1));
                    Assert.That(options.P2, Is.EqualTo(p2));
                });
            }
        }
    }
}