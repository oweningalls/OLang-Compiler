using OLangCompiler.Generation;
using OLangCompiler.Tokens;

namespace OLangCompiler;

public static class OLangCompiler
{
    public static void Main()
    {
        var fileName = "test.ol";
        if (!fileName.EndsWith(".ol"))
        {
            throw new Exception($"Input file {fileName} must end with .ol");
        }

        var baseName = fileName.Substring(0, fileName.Length - ".ol".Length);
        var tokenizer = new Tokenizer();
        var contents = File.ReadAllText(fileName);
        var tokens = tokenizer.Tokenize(contents);

        var parser = new Parser.Parser();
        var program = parser.ParseProgram(tokens);

        var generator = new AssemblyGenerator();
        var assembly = generator.GenerateProgram(program);
        
        File.WriteAllText($"{baseName}.asm", assembly);
    }
}