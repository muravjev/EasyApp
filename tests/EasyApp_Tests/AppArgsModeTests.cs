namespace EasyApp
{
    public sealed class AppArgsModeTests
    {
        public sealed class Options
        {
            [Flag('h', "help", "Help screen.", IsBreaker = false)]
            public bool Help = false;

            [Option('f', "format", "Datetime format.")]
            public string Format = "yyyy-MM-dd";
        }

        [Test]
        public void EmptyArgsParsed()
        {
            var result = new AppArgs().Parse<Options>(new string[] { });

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
            var result = new AppArgs().Parse<Options>(new string[] { flag });

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
            var result = new AppArgs().Parse<Options>(option.Split(' '));

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.Format, Is.EqualTo("foo"));
            });
        }
    }
}