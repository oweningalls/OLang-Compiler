using OLangLexing;

namespace OLangTests;

public class SourceReaderTests
{
    [TestCase(0, 0, 0)]
    [TestCase(1, 0, 1)]
    [TestCase(2, 0, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(4, 1, 0)]
    [TestCase(8, 1, 4)]
    [TestCase(9, 2, 0)]
    [TestCase(10, 3, 0)]
    public void TestLineAndRelativeCharNumbers(int absoluteChar, int expectedLineNumber, int expectedRelativeCharNumber)
    {
        const string source = "012\n4567\n\n1";
        var reader = new SourceReader(source);
        
        Assert.That(reader.GetLineAndRelativeCharacterNumber(absoluteChar), Is.EqualTo((expectedLineNumber, expectedRelativeCharNumber)));
    }
}