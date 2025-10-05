using OLangCompiler.Generation;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking;

namespace OLangCompiler;

public static class OLangCompiler
{
    public static void Main()
    {
        CompileFile("test.ol");
    }
    
    public static void CompileFile(string fileName, string? outputFile = null)
    {
        if (!fileName.EndsWith(".ol"))
        {
            throw new Exception($"Input file {fileName} must end with .ol");
        }

        var baseName = fileName.Substring(0, fileName.Length - ".ol".Length);
        outputFile ??= $"{baseName}.asm";
        var contents = File.ReadAllText(fileName);

        var assembly = GenerateAssembly(contents);
        File.WriteAllText(outputFile, assembly);
    }

    public static string GenerateAssembly(string program)
    {
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(program);

        var parser = new Parser.Parser();
        var programNode = parser.ParseProgram(tokens);

        var typeChecker = new TypeChecker();
        typeChecker.CheckTypes(programNode);

        var generator = new AssemblyGenerator();
        var assembly = generator.GenerateProgram(programNode);

        return assembly;
    }
}