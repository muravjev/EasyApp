namespace EasyApp
{
    public sealed class ParseRequiredTests
    {
        public class Options
        {
            [Flag("f1", "Description", true)]
            public bool F1 = false;

            [Flag("f2", "Description")]
            public bool F2 = false;

            [Option("o1", "Description")]
            public string? O1 = null;

            [Parameter("p1", "Description")]
            public string? P1 = null;
        }

        [Test]
        public void ParsedNoArgs()
        {
            var result = Utilities.Parse<Options>();

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.IsHelp, Is.EqualTo(true));
                Assert.That(result.Options.F1, Is.EqualTo(false));
                Assert.That(result.Options.F2, Is.EqualTo(false));
                Assert.That(result.Options.O1, Is.EqualTo(null));
                Assert.That(result.Options.P1, Is.EqualTo(null));
            });
        }

        [Test]
        public void ParsedBreaked()
        {
            var result = Utilities.Parse<Options>("--f1");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.IsHelp, Is.EqualTo(true));
                Assert.That(result.Options.F1, Is.EqualTo(true));
                Assert.That(result.Options.F2, Is.EqualTo(false));
                Assert.That(result.Options.O1, Is.EqualTo(null));
                Assert.That(result.Options.P1, Is.EqualTo(null));
            });
        }

        [Test]
        public void ParsedRequired()
        {
            var result = Utilities.Parse<Options>("--o1", "ovalue", "pvalue");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.IsHelp, Is.EqualTo(false));
                Assert.That(result.Options.F1, Is.EqualTo(false));
                Assert.That(result.Options.F2, Is.EqualTo(false));
                Assert.That(result.Options.O1, Is.EqualTo("ovalue"));
                Assert.That(result.Options.P1, Is.EqualTo("pvalue"));
            });
        }

        [Test]
        [TestCase(false, false, false, null, "pvalue", "pvalue")]
        [TestCase(false, false, false, "ovalue", null, "--o1", "ovalue")]
        public void FailedWithMissedRequiredFields(bool breaker, bool f1, bool f2, string? o1, string? p1, params string[] args)
        {
            var result = Utilities.Parse<Options>(args);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(false));
                Assert.That(result.Exception, Is.TypeOf<EasyAppException>());
                Assert.That(result.IsHelp, Is.EqualTo(breaker));
                Assert.That(result.Options.F1, Is.EqualTo(f1));
                Assert.That(result.Options.F2, Is.EqualTo(f2));
                Assert.That(result.Options.O1, Is.EqualTo(o1));
                Assert.That(result.Options.P1, Is.EqualTo(p1));
            });
        }
    }
}