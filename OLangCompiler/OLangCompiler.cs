using OLangCompiler.Generation;
using OLangCompiler.Parser;
using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.ParseTree.Prog;
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
        var errorHelper = new ErrorHelper(program);
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(program, errorHelper);

        IParser parser = new OLangParser();
        var programNode = (ProgramNode)parser.ParseProgram(new OLangGrammar(), tokens, errorHelper);

        var typeChecker = new TypeChecker();
        typeChecker.CheckTypes(programNode, errorHelper);

        var generator = new AssemblyGenerator();
        var assembly = generator.GenerateProgram(programNode, errorHelper);

        return assembly;
    }
}