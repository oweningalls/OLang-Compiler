using Lexing;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;

namespace OLangTests;

public class OLangGrammarTests
{
    [Test]
    public void ReduceProgramNode()
    {
        var rule = new OLangGrammar.OLangGrammar().GetRules().First(x => x.GetLhsType() == typeof(IProgramNode));
        var stmtList = new EmptyStmtList();
        var program = rule.ReduceRule([stmtList]);

        var programNode = program as ProgramNode;
        Assert.That(programNode, Is.Not.Null);
        Assert.That(programNode.StmtList, Is.EqualTo(stmtList));
    }
    
    [Test]
    public void ReduceDeclarationStatement()
    {
        var rule = GrammarRule.Create((IType type, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new Declaration(type, identifier, expression));
        var varType = new BoolType();
        var identifier = new IdentifierToken("test");
        var expression = new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new IdentifierTerm(new IdentifierToken("ident2")))))))));
        
        var decl = rule.ReduceRule([varType, identifier, new EqualsToken(), expression, new SemicolonToken()]);

        var declaration = decl as Declaration;
        Assert.That(declaration, Is.Not.Null);
        Assert.That(declaration.Type, Is.EqualTo(varType));
        Assert.That(declaration.Identifier, Is.EqualTo(identifier));
        Assert.That(declaration.Expression, Is.EqualTo(expression));
    }
}