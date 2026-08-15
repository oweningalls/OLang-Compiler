using Lexing;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;
using IExpression = OLangGrammar.ParseTree.Expression.IExpression;
using Return = OLangGrammar.ParseTree.Stmt.Return;

namespace OLangTests;

public class AstConversionTests() : OLangAstBuilder(new NoOpErrorHelper())
{
    [Test]
    public void TestParseProgram()
    {
        var emptyScope = new ScopeStatement(GetEmptyScope());
        var parsed = new ProgramNode(new StmtListWithStatement(emptyScope, new SingleStatementStmtList(emptyScope)));
        
        var converted = ParseProgram(parsed);
        var expected = new Program([
            new Scope([]),
            new Scope([])
        ]);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_Equals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(GetIdentifier(identifier), new Equals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var intLiteral = new IntLiteral(value);
        var expected = new VariableAssignment(identifier, intLiteral);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_PlusEquals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(GetIdentifier(identifier), new PlusEquals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var expected = new VariableAssignment(identifier, new Add(new VariableAccess(identifier), new IntLiteral(value)));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_MinusEquals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(GetIdentifier(identifier), new MinusEquals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var expected = new VariableAssignment(identifier, new Subtract(new VariableAccess(identifier), new IntLiteral(value)));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_TimesEquals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(GetIdentifier(identifier), new TimesEquals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var expected = new VariableAssignment(identifier, new Multiply(new VariableAccess(identifier), new IntLiteral(value)));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseAssignment_DivideEquals()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Assignment(GetIdentifier(identifier), new DivideEquals(), GetIntLiteralExpression(value));

        var converted = ParseAssignment(parsed);
        var expected = new VariableAssignment(identifier, new Divide(new VariableAccess(identifier), new IntLiteral(value)));

        AssertEquivalence(converted, expected);
    }

    [Test]
    public void TestParseStatement_LetDeclaration()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new LetDeclaration(GetIdentifier(identifier), GetIntLiteralExpression(value));

        var converted = ParseStatement(parsed);
        var expected = new VariableDeclarationStatement(null, identifier, new IntLiteral(value));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_TypedDeclaration()
    {
        var value = 321;
        var identifier = "a";
        var parsed = new Declaration(new IntType(), GetIdentifier(identifier), GetIntLiteralExpression(value));

        var converted = ParseStatement(parsed);
        var expected = new VariableDeclarationStatement(new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int), identifier, new IntLiteral(value));

        AssertEquivalence(converted, expected);
    }

    private static IdentifierToken GetIdentifier(string identifier)
    {
        return new IdentifierToken(identifier);
    }

    [Test]
    public void TestParseStatement_ExitStatement()
    {
        var value = 321;
        var parsed = new Exit(GetIntLiteralExpression(value));

        var converted = ParseStatement(parsed);
        var expected = new ExitStatement(new IntLiteral(value));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_ForLoop()
    {
        var ident = "a";
        var start = 0;
        var end = 1;
        var parsed = new For(GetIdentifier(ident), GetIntLiteralExpression(start), GetIntLiteralExpression(end), GetEmptyScope());

        var converted = ParseStatement(parsed);
        var expected = new ForLoop(ident, new IntLiteral(start), new IntLiteral(end), new Scope([]));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_WhileLoop()
    {
        var start = 0;
        var end = 1;
        var parsed = new While(GetBoolLiteralExpression(true), GetEmptyScope());

        var converted = ParseStatement(parsed);
        var expected = new WhileLoop(new BoolLiteral(true), new Scope([]));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_FunctionDeclaration()
    {
        var start = 0;
        var end = 1;
        var ident = "a";
        var parsed = new FunctionDeclarationWithParameters(new BoolType(), GetIdentifier(ident), new EmptyParameterList(), GetEmptyScope());

        var converted = ParseStatement(parsed);
        var expected = new OLangAst.Statements.FunctionDeclaration(new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool), ident, []);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_VoidFunctionDeclaration()
    {
        var start = 0;
        var end = 1;
        var ident = "a";
        var parsed = new VoidFunctionDeclarationWithParameters(GetIdentifier(ident), new EmptyParameterList(), GetEmptyScope());

        var converted = ParseStatement(parsed);
        var expected = new OLangAst.Statements.FunctionDeclaration(null, ident, []);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_IfStatement()
    {
        var ident = "a";
        var start = 0;
        var end = 1;
        var parsed = new If(GetBoolLiteralExpression(true), GetEmptyScope());

        var converted = ParseStatement(parsed);
        var expected = new IfStatement(new BoolLiteral(true), new Scope([]), null);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_Return()
    {
        var parsed = new Return();

        var converted = ParseStatement(parsed);
        var expected = new OLangAst.Statements.Return(null);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseStatement_ReturnValue()
    {
        var value = 321;
        var parsed = new ReturnValue(GetIntLiteralExpression(value));

        var converted = ParseStatement(parsed);
        var expected = new OLangAst.Statements.Return(new IntLiteral(value));

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseScope_EmptyScope()
    {
        var parsed = new EmptyScope();

        var converted = ParseScope(parsed);
        var expected = new Scope([]);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseScope_ScopeWithValue()
    {
        var parsed = new ScopeNode(new SingleStatementStmtList(new ScopeStatement(GetEmptyScope())));

        var converted = ParseScope(parsed);
        var expected = new Scope([new Scope([])]);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void TestParseTerm_IntLit()
    {
        var value = 11;
        var parsed = new OLangGrammar.ParseTree.Term.IntLiteral(new IntLiteralToken(value));

        var converted = ParseTerm(parsed);
        var expected = new IntLiteral(11);

        AssertEquivalence(converted, expected);
    }
    
    [Test]
    public void SourcesFromIndividualTokens_GetPreserved()
    {
        var intLiteral = new OLangGrammar.ParseTree.Term.IntLiteral(new IntLiteralToken(1)) { Span = new SourceSpan(1, 2) };

        var parsed = ParseTerm(intLiteral);
        
        Assert.That(parsed.Span.Start, Is.EqualTo(intLiteral.Span.Start));
        Assert.That(parsed.Span.Length, Is.EqualTo(intLiteral.Span.Length));
    }

    private static IExpression GetIntLiteralExpression(int value)
    {
        return new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new OLangGrammar.ParseTree.Term.IntLiteral(new IntLiteralToken(value)))))))));
    }

    private static IExpression GetBoolLiteralExpression(bool value)
    {
        return new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new OLangGrammar.ParseTree.Term.BoolLiteral(new BoolLiteralToken(value)))))))));
    }

    private static IScopeNode GetEmptyScope()
    {
        return new EmptyScope();
    }

    private static void AssertEquivalence<T>(T actual, T expected) where T : IAstNode 
    {
        Assert.That(ProgramComparer.AreEquivalent(actual, expected));
    }
}
