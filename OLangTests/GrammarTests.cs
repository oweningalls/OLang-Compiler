using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Prog;
using OLangCompiler.Parser.ParseTree.Stmt;
using OLangCompiler.Parser.ParseTree.StmtList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;

namespace OLangTests;

public class GrammarTests
{
    [Test]
    public void ReduceProgramNode()
    {
        var rule = new Grammar().GetRules().First(x => x.GetLhsType() == typeof(ProgramNode));
        var stmtList = new EmptyStmtList();
        var eoi = new EndOfInputToken();
        var program = rule.ReduceRule([stmtList, eoi]);

        var programNode = program as ProgramNode;
        Assert.That(programNode, Is.Not.Null);
        Assert.That(programNode.StmtList, Is.EqualTo(stmtList));
    }
    
    [Test]
    public void ReduceDeclarationStatement()
    {
        var rule = new Grammar().GetRules().First(x => x.GetLhsType() == typeof(Declaration));
        var varType = new PrimitiveVariableType(new BoolType());
        var identifier = new IdentifierToken("test");
        var expression = new TermExpression(new IdentifierTerm("otherTest"));
        
        var decl = rule.ReduceRule([varType, identifier, new EqualsToken(), expression, new SemicolonToken()]);

        var declaration = decl as Declaration;
        Assert.That(declaration, Is.Not.Null);
        Assert.That(declaration.Type, Is.EqualTo(varType));
        Assert.That(declaration.Identifier, Is.EqualTo(identifier));
        Assert.That(declaration.Expression, Is.EqualTo(expression));
    }
}