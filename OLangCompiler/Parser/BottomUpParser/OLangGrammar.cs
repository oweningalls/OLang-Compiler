using OLangCompiler.Parser.ParseTree.AddExpression;
using OLangCompiler.Parser.ParseTree.AndExpression;
using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.EqualityExpression;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.FunctionInvocation;
using OLangCompiler.Parser.ParseTree.FunctionType;
using OLangCompiler.Parser.ParseTree.GreaterExpression;
using OLangCompiler.Parser.ParseTree.MultExpression;
using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Prog;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Parser.ParseTree.Stmt;
using OLangCompiler.Parser.ParseTree.StmtList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Parser.ParseTree.UnaryExpression;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;
using FunctionInvocation = OLangCompiler.Parser.ParseTree.FunctionInvocation.FunctionInvocation;
using Void = OLangCompiler.Parser.ParseTree.FunctionType.Void;

namespace OLangCompiler.Parser.BottomUpParser;

public class OLangGrammar : IGrammar
{
    private List<BaseGrammarRule> _rules =
    [
        // Prog
        GrammarRule.Create((IStmtListNode stmtList) => new ProgramNode(stmtList)),

        // StmtList
        GrammarRule.Create(() => new EmptyStmtList()),
        GrammarRule.Create((IStatement statement, IStmtListNode statementList) => new StmtListWithStatement(statement, statementList)),

        // Stmt
        GrammarRule.Create((ExitToken _, IExpression expression) => new Exit(expression)),
        GrammarRule.Create((IVariableType variableType, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new Declaration(variableType, identifier, expression)),
        GrammarRule.Create((IdentifierToken identifier, IAssignmentOperator assignmentOperator, IExpression expression, SemicolonToken _) => new Assignment(identifier, assignmentOperator, expression)),
        GrammarRule.Create((IScopeNode scope) => new ScopeStatement(scope)),
        GrammarRule.Create((IfToken _, IExpression expression, IScopeNode scope, IElse elseNode) => new If(expression, scope, elseNode)),
        GrammarRule.Create((WhileToken _, IExpression expression, IScopeNode scope) => new While(expression, scope)),
        GrammarRule.Create((ForToken _, IdentifierToken identifier, InToken _, IExpression startExpression, RangeToken _, IExpression endExpression, IScopeNode scope) => new For(identifier, startExpression, endExpression, scope)),
        GrammarRule.Create((IFunctionType type, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, IScopeNode scope) => new FunctionDeclaration(type, identifier, parameterList, scope)),
        GrammarRule.Create((IFunctionInvocation invocation) => new Invocation(invocation)),
        GrammarRule.Create((ReturnToken _, SemicolonToken _) => new Return()),
        GrammarRule.Create((ReturnToken _, IExpression expression, SemicolonToken _) => new ReturnValue(expression)),

        // ElseBlock
        GrammarRule.Create(() => new EmptyElse()),
        GrammarRule.Create((ElseToken _, IScopeNode scope) => new Else(scope)),

        // ParameterList
        GrammarRule.Create(() => new EmptyParameterList()),
        GrammarRule.Create((IType type, IdentifierToken identifier, CommaToken _, IParameterListNode parameterList) => new ContinuedParameterList(type, identifier, parameterList)),
        GrammarRule.Create((IType type, IdentifierToken identifier) => new Parameter(type, identifier)),

        // ArgumentList
        GrammarRule.Create(() => new EmptyArgumentList()),
        GrammarRule.Create((IExpression expression, CommaToken _, IArgumentList parameterList) => new ContinuedArgumentList(expression, parameterList)),
        GrammarRule.Create((IExpression expression) => new ExpressionArgumentList(expression)),

        // SetOperator
        GrammarRule.Create((EqualsToken _) => new Equals()),
        GrammarRule.Create((PlusEqualsToken _) => new PlusEquals()),
        GrammarRule.Create((MinusEqualsToken _) => new MinusEquals()),
        GrammarRule.Create((TimesEqualsToken _) => new TimesEquals()),
        GrammarRule.Create((DivideEqualsToken _) => new DivideEquals()),

        // Scope
        GrammarRule.Create((LeftCurlyToken _, IStmtListNode stmtList, RightCurlyToken _) => new ScopeNode(stmtList)),

        // FunctionType
        GrammarRule.Create((VoidToken _) => new Void()),
        GrammarRule.Create((IType typeNode) => new PrimitiveFunctionType(typeNode)),

        // VariableType
        GrammarRule.Create((LetToken _) => new Let()),
        GrammarRule.Create((IType typeNode) => new PrimitiveVariableType(typeNode)),

        // Type
        GrammarRule.Create((IntTypeToken _) => new IntType()),
        GrammarRule.Create((BoolTypeToken _) => new BoolType()),
        GrammarRule.Create((FloatTypeToken _) => new FloatType()),

        // Expression
        GrammarRule.Create((IExpression lhs, BooleanOrToken _, IAndExpression rhs) => new Expression(lhs, rhs)),
        GrammarRule.Create((IAndExpression expression) => new NonExpression(expression)),

        // AndExpression
        GrammarRule.Create((IAndExpression lhs, BooleanAndToken _, IEqualityExpression rhs) => new AndExpression(lhs, rhs)),
        GrammarRule.Create((IEqualityExpression expression) => new NonAnd(expression)),

        // EqualityExpression
        GrammarRule.Create((IEqualityExpression lhs, DoubleEqualsToken _, IGreaterExpression rhs) => new DoubleEquals(lhs, rhs)),
        GrammarRule.Create((IEqualityExpression lhs, NotEqualToken _, IGreaterExpression rhs) => new NotEqual(lhs, rhs)),
        GrammarRule.Create((IGreaterExpression expression) => new NonEquality(expression)),

        // GreaterExpression
        GrammarRule.Create((IGreaterExpression lhs, GreaterToken _, IAddExpression rhs) => new Greater(lhs, rhs)),
        GrammarRule.Create((IGreaterExpression lhs, GreaterOrEqualToken _, IAddExpression rhs) => new GreaterOrEqual(lhs, rhs)),
        GrammarRule.Create((IGreaterExpression lhs, LessToken _, IAddExpression rhs) => new Less(lhs, rhs)),
        GrammarRule.Create((IGreaterExpression lhs, LessOrEqualToken _, IAddExpression rhs) => new LessOrEqual(lhs, rhs)),
        GrammarRule.Create((IAddExpression expression) => new NonGreaterExpression(expression)),

        // AddExpression
        GrammarRule.Create((IAddExpression lhs, PlusToken _, IMultExpression rhs) => new Plus(lhs, rhs)),
        GrammarRule.Create((IAddExpression lhs, MinusToken _, IMultExpression rhs) => new Minus(lhs, rhs)),
        GrammarRule.Create((IMultExpression expression) => new NonAddExpression(expression)),

        // MultExpression
        GrammarRule.Create((IMultExpression lhs, MinusToken _, IUnaryExpression rhs) => new Mult(lhs, rhs)),
        GrammarRule.Create((IMultExpression lhs, MinusToken _, IUnaryExpression rhs) => new Div(lhs, rhs)),
        GrammarRule.Create((IUnaryExpression expression) => new NonMultExpression(expression)),

        // UnaryExpression
        GrammarRule.Create((NotToken _, ITerm term) => new Not(term)),
        GrammarRule.Create((ITerm term) => new NonUnaryExpression(term)),

        // Term
        GrammarRule.Create((IntLiteralToken intLit) => new IntLiteral(intLit)),
        GrammarRule.Create((FloatLiteralToken floatLit) => new FloatLiteral(floatLit)),
        GrammarRule.Create((BoolLiteralToken boolLit) => new BoolLiteral(boolLit)),
        GrammarRule.Create((IdentifierToken ident) => new IdentifierTerm(ident)),
        GrammarRule.Create((LeftParenToken _, IExpression expression, RightParenToken _) => new Paren(expression)),
        GrammarRule.Create((IFunctionInvocation ident) => new Invocation(ident)),

        // FunctionInvocation
        GrammarRule.Create((IdentifierToken ident, LeftParenToken _, IArgumentList argumentList, RightParenToken _) => new FunctionInvocation(ident, argumentList)),
    ];

    public Type GetStartSymbol()
    {
        return typeof(ProgramNode);
    }

    public List<BaseGrammarRule> GetRules()
    {
        return _rules;
    }
}