using AssemblyGeneration.Generation;
using OLangAst;
using OLangGrammar.ParseTree.Prog;
using OLangLexing;
using OLangTypeChecking.TypeChecking;
using Parser.Parser.BottomUpParser;
using Parser.Parser.BottomUpParser.Lr1;

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
        var reader = new SourceReader(program);
        var errorHelper = new OLangHelpers.ErrorHelper(reader);
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(program, errorHelper);

        var parser = new LrParser();
        var parseTable = new Lr1ParseTable(new OLangGrammar.OLangGrammar(), errorHelper);
        var programNode = (ProgramNode)parser.ParseProgram(parseTable, tokens, errorHelper);

        var ast = new OLangAstBuilder(errorHelper).ParseProgram(programNode);

        var typeChecker = new TypeChecker();
        typeChecker.CheckTypes(ast, errorHelper);

        var generator = new X86AssemblyGenerator();
        var assembly = generator.GenerateProgram(ast, errorHelper);

        return assembly;
    }
}