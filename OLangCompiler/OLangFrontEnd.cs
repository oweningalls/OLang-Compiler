using AssemblyGeneration.Generation;
using ErrorHelper;
using OLangAst;
using OLangGrammar.ParseTree.Prog;
using OLangLexing;
using OLangTypeChecking.TypeChecking;
using Parser.Parser.BottomUpParser;
using Parser.Parser.BottomUpParser.Lr1;

namespace OLangCompiler;

public class OLangFrontEnd : IOLangFrontEnd
{
    public string Compile(string program, IGenerator generator, IErrorHelper errorHelper)
    {
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(program, errorHelper);

        var parser = new LrParser();
        var parseTable = new Lr1ParseTable(new OLangGrammar.OLangGrammar(), errorHelper);
        var programNode = (ProgramNode)parser.ParseProgram(parseTable, tokens, errorHelper);

        var ast = new OLangAstBuilder(errorHelper).ParseProgram(programNode);

        var typeChecker = new TypeChecker();
        typeChecker.CheckTypes(ast, errorHelper);
        
        var assembly = generator.GenerateProgram(ast, errorHelper);

        return assembly;
    }
}