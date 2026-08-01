using OLangAst;
using OLangAst.Expressions;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;
using IExpression = OLangGrammar.ParseTree.Expression.IExpression;

namespace OLangTests;

public class AstConversionTests() : OLangAstBuilder(new NoOpErrorHelper())
{
    [Test]
    public void TestParseProgram()
    {
        var emptyScope = new ScopeStatement(new ScopeNode(new EmptyStmtList()));
        var parsed = new ProgramNode(new StmtListWithStatement(emptyScope, new StmtListWithStatement(emptyScope, new EmptyStmtList())));
        
        var converted = ParseProgram(parsed);
        var expected = new Program
        {
            Statements = [
                new Scope {Statements = []},
                new Scope {Statements = []}
            ]
        
        };

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_Equals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(new IdentifierToken(identifier), new Equals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var intLiteral = new IntLiteral { Value = 11 };
        var expected = new VariableAssignment { Identifier = identifier, Value = intLiteral };

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseTerm_IntLit()
    {
        var value = 11;
        var parsed = new OLangGrammar.ParseTree.Term.IntLiteral(new IntLiteralToken(value));

        var converted = ParseTerm(parsed);
        var expected = new IntLiteral { Value = 11 };

        AssertEquivalence(converted, expected);
    }

    private static IExpression GetIntLiteralExpression(int value)
    {
        return new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new OLangGrammar.ParseTree.Term.IntLiteral(new IntLiteralToken(value)))))))));
    }

    private static void AssertEquivalence<T>(T actual, T expected) where T : IAstNode 
    {
        Assert.That(ProgramComparer.AreEquivalent(actual, expected));
    }
}
