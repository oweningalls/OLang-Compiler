using OLangCompiler.Tokens;

namespace OLangCompiler;

public static class OLangCompiler
{
    public static void Main()
    {
        var tokenizer = new Tokenizer();
        var contents = File.ReadAllText("test.ol");
        var tokens = tokenizer.Tokenize(contents);

        var parser = new Parser.Parser();
        var program = parser.ParseProgram(tokens);
    }
}