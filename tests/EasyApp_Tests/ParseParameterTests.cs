namespace EasyApp
{
    public sealed class ParseParameterTests
    {
        public sealed class Options<T>
        {
            [Parameter("p1", "Option.")]
            public T? P1 = default;
        }

        private static Result<Options<T>> parse<T>(string arg)
        {
            return new AppArgs().Parse<Options<T>>("--", arg);
        }

        [Test]
        [TestCase("true", true)]
        [TestCase("True", true)]
        [TestCase("TRUE", true)]
        [TestCase("false", false)]
        [TestCase("False", false)]
        [TestCase("FALSE", false)]
        [TestCase("  true  ", true)]
        public void BoolParsed(string arg, bool value)
        {
            var result = parse<bool>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("foo", "foo")]
        [TestCase("  foo  ", "  foo  ")]
        public void StringParsed(string arg, string value)
        {
            var result = parse<string>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }
        public enum Foo
        {
            Bar
        }

        [Test]
        [TestCase("bar", Foo.Bar)]
        [TestCase("Bar", Foo.Bar)]
        [TestCase(" BAR ", Foo.Bar)]
        public void EnumParsed(string arg, Foo value)
        {
            var result = parse<Foo>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("baz")]
        public void EnumFailed(string arg)
        {
            var result = parse<Foo>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(false));
                Assert.That(result.Options.P1, Is.EqualTo(default(Foo)));
                Assert.That(result.Exception, Is.TypeOf<AppException>());
            });
        }

        [Test]
        [TestCase("a", 'a')]
        [TestCase("  a ", 'a')]
        public void CharParsed(string arg, char value)
        {
            var result = parse<char>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("1975-12-01 15:03", "1975-12-01 15:03")]
        [TestCase("1975-12-01T15:03", "1975-12-01 15:03")]
        [TestCase("12/1/1975 15:03", "1975-12-01 15:03")]
        [TestCase("1975-12-01", "1975-12-01")]
        [TestCase("15:03", "15:03")]
        public void CharParsed(string arg, string value)
        {
            var result = parse<DateTime>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(DateTime.Parse(value)));
            });
        }

        [Test]
        [TestCase("81a130d2-502f-4cf1-a376-63edeb000e9f", "81a130d2-502f-4cf1-a376-63edeb000e9f")]
        [TestCase("{81a130d2-502f-4cf1-a376-63edeb000e9f}", "81a130d2-502f-4cf1-a376-63edeb000e9f")]
        [TestCase("{0x81a130d2,0x502f,0x4cf1,{0xa3,0x76,0x63,0xed,0xeb,0x00,0x0e,0x9f}}", "81a130d2-502f-4cf1-a376-63edeb000e9f")]
        public void GuidParsed(string arg, string value)
        {
            var result = parse<Guid>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(Guid.Parse(value)));
            });
        }

        [Test]
        [TestCase("1.2", 1.2d)]
        [TestCase("-1.063E-02", -1.063E-02d)]
        [TestCase("-4320.64", -4320.64d)]
        public void DoubleParsed(string arg, double value)
        {
            var result = parse<double>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("10", 10)]
        [TestCase("+3", 3)]
        [TestCase("0x4", 4)]
        [TestCase("  +12 ", 12)]
        public void ByteParsed(string arg, byte value)
        {
            var result = parse<byte>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("257")]
        [TestCase("-4")]
        public void ByteFailed(string arg)
        {
            var result = parse<byte>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(false));
                Assert.That(result.Options.P1, Is.EqualTo(default(byte)));
                Assert.That(result.Exception, Is.TypeOf<AppException>());
            });
        }

        [Test]
        [TestCase("10", 10)]
        [TestCase("+3", 3)]
        [TestCase("0x4", 4)]
        [TestCase("  +12 ", 12)]
        public void IntParsed(string arg, int value)
        {
            var result = parse<int>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(true));
                Assert.That(result.Options.P1, Is.EqualTo(value));
            });
        }

        [Test]
        [TestCase("12333333333")]
        [TestCase("-11111111111")]
        public void IntFailed(string arg)
        {
            var result = parse<int>(arg);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsParsed, Is.EqualTo(false));
                Assert.That(result.Options.P1, Is.EqualTo(default(int)));
                Assert.That(result.Exception, Is.TypeOf<AppException>());
            });
        }
    }
}