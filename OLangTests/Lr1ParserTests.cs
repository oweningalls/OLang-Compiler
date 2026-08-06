using Lexing;
using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.BottomUpParser.Lr1;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.FunctionInvocation;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangLexing;
using OLangTokens.Tokens;

namespace OLangTests;

public class Lr1ParserTests
{
    [Test]
    public void TestGrammarDoesntFail()
    {
        Assert.DoesNotThrow(() => new Lr1ParseTable(new TestGrammar(), new NoOpErrorHelper()));
    }
    
    [Test]
    public void TestAmbiguousFails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr1ParseTable(new AmbiguousGrammar(), new NoOpErrorHelper()));
        Assert.That(e.Message.Contains("conflict", StringComparison.CurrentCultureIgnoreCase), e.Message);
    }

    [Test]
    public void TestLr0GrammarWorks()
    {
        Lr1ParseTable? table = null;
        Assert.DoesNotThrow(() => table = new Lr1ParseTable(new Lr0Grammar(), new NoOpErrorHelper()));
    }

    [Test]
    public void TestOLangGrammar()
    {
        Assert.DoesNotThrow(() => new Lr1ParseTable(new OLangGrammar.OLangGrammar(), new NoOpErrorHelper()));
    }

    [Test]
    public void TestParseLr0Grammar()
    {
        var parser = new Lr1Parser();
        var input = new List<BaseToken>
        {
            new BoolLiteralToken(false), new IntLiteralToken(1)
        };
        
        var program = parser.ParseProgram(new Lr0Grammar(), input, new NoOpErrorHelper());
        var intNode = program as IntANode;
        Assert.That(intNode, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(intNode.BoolLiteralToken.Value, Is.False);
            Assert.That(intNode.IntLiteralToken.Value, Is.EqualTo(1));
        });
    }

    [Test]
    public void TestParseOLangGrammarSnippet()
    {
        var code = """
                   let a = 1;
                   int b() {return 2;}
                   exit a + b();
                   """;

        NonExpression GetIntLiteralExpression(int value)
        {
            return new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new IntLiteral(new IntLiteralToken(value)))))))));
        }

        var letAStatement = new LetDeclaration(new IdentifierToken("a"), GetIntLiteralExpression(1));

        var bScope = new ScopeNode(new StmtListWithStatement(new ReturnValue(GetIntLiteralExpression(2)), new EmptyStmtList()));
        var bDeclarationStatement = new FunctionDeclaration(new IntType(), new IdentifierToken("b"), new EmptyParameterList(), bScope);

        var aAccess = new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new IdentifierTerm(new IdentifierToken("a")))));
        var bInvocation = new NonMultExpression(new NonUnaryExpression(new FunctionInvocationTerm(new FunctionInvocation(new IdentifierToken("b"), new EmptyArgumentList()))));
        var exitStatement = new Exit(new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new Plus(aAccess, bInvocation))))));

        var expectedProgram = new ProgramNode(new StmtListWithStatement(letAStatement, new StmtListWithStatement(bDeclarationStatement, new StmtListWithStatement(exitStatement, new EmptyStmtList()))));

        var errorHelper = new OLangHelpers.ErrorHelper(new SourceReader(code));
        var tokens = new Tokenizer().Tokenize(code, errorHelper);
        var parser = new Lr1Parser();

        var actualProgram = parser.ParseProgram(new OLangGrammar.OLangGrammar(), tokens, errorHelper);
        
        Assert.That(ProgramComparer.AreEquivalent(expectedProgram, actualProgram));
    }
}