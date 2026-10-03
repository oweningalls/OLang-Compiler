using AssemblyGeneration.Generation;
using AstHelpers;
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
    public void Compile(string program, IGenerator generator, IErrorHelper errorHelper, TypeHelper typeHelper, string filePath, string fileName)
    {
        var tokenizer = new Tokenizer();
        var tokens = tokenizer.Tokenize(program, errorHelper);

        var parser = new LrParser();
        var parseTable = new Lr1ParseTable(new OLangGrammar.OLangGrammar(), errorHelper);
        var programNode = (ProgramNode)parser.ParseProgram(parseTable, tokens, errorHelper);

        var ast = new OLangAstBuilder(errorHelper).ParseProgram(programNode);

        new TypeRegisterer(errorHelper, typeHelper).VisitProgram(ast);
        new TypeChecker(errorHelper, typeHelper).VisitProgram(ast);
        
        generator.GenerateProgram(ast, filePath, fileName);
    }
}