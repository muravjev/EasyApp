namespace EasyApp
{
    public sealed class ParseModeTests
    {
        public sealed class Options
        {
            [Flag('h', "help", "Help screen.")]
            public bool Help = false;

            [Option('f', "format", "Datetime format.")]
            public string Format = "yyyy-MM-dd";
        }

        [Test]
        public void EmptyArgsParsed()
        {
            var result = Utilities.Parse<Options>();

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.Help, Is.EqualTo(false));
                Assert.That(result.Options.Format, Is.EqualTo("yyyy-MM-dd"));
            });
        }


        [Test]
        [TestCase("-h")]
        [TestCase("--help")]
        [TestCase("/h")]
        [TestCase("/help")]
        public void FlagParsed(string flag)
        {
            var result = Utilities.Parse<Options>(flag);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.Help, Is.EqualTo(true));
            });
        }

        [Test]
        [TestCase("-f foo")]
        [TestCase("-f=foo")]
        [TestCase("-f:foo")]
        [TestCase("--format foo")]
        [TestCase("--format=foo")]
        [TestCase("--format:foo")]
        [TestCase("/f foo")]
        [TestCase("/f=foo")]
        [TestCase("/f:foo")]
        [TestCase("/format foo")]
        [TestCase("/format=foo")]
        [TestCase("/format:foo")]
        public void OptionParsed(string option)
        {
            var result = Utilities.Parse<Options>(option.Split(' '));

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.Format, Is.EqualTo("foo"));
            });
        }
    }
}