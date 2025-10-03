using OLangCompiler.Tokens;

namespace OLang_Compiler;

public class OLangCompiler
{
    public static void Main()
    {
        var tokenizer = new Tokenizer();
        var contents = File.ReadAllText("test.ol");
        var tokens = tokenizer.Tokenize(contents);
    }
}