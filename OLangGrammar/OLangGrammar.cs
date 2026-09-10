using Lexing;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.ClassDeclaration;
using OLangGrammar.ParseTree.ClassDeclarationList;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.ClassMemberList;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.FunctionInvocation;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MethodInvocation;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;

namespace OLangGrammar;

public class OLangGrammar : IGrammar
{
    private static readonly List<BaseGrammarRule> Rules =
    [
        // Prog
        GrammarRule.Create((IClassDeclarationListNode list) => new ProgramNode(list)),
        
        // ClassDeclarationList
        GrammarRule.Create((IClassDeclaration classDeclaration) => new SingleClassClassList(classDeclaration)),
        GrammarRule.Create((IClassDeclaration classDeclaration, IClassDeclarationListNode classDeclarationList) => new ClassDeclarationListWithClass(classDeclaration, classDeclarationList)),
        
        // ClassDeclaration
        GrammarRule.Create((ClassToken _, IdentifierToken identifierToken, LeftCurlyToken _, IClassMemberListNode stmtList, RightCurlyToken _) => new ClassDeclaration(identifierToken, stmtList)),
        GrammarRule.Create((StaticToken _, ClassToken _, IdentifierToken identifierToken, LeftCurlyToken _, IClassMemberListNode stmtList, RightCurlyToken _) => new StaticClassDeclaration(identifierToken, stmtList)),
        
        // ClassMemberList
        GrammarRule.Create((IClassMember statement) => new SingleMemberClassMemberList(statement)),
        GrammarRule.Create((IClassMember statement, IClassMemberListNode statementList) => new ClassMemberListWithClassMember(statement, statementList)),
        
        // ClassMember
        GrammarRule.Create((IType type, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, IScopeNode scope) => new MethodDeclarationWithParameters(type, identifier, parameterList, scope)),
        GrammarRule.Create((VoidToken _, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, IScopeNode scope) => new VoidMethodDeclarationWithParameters(identifier, parameterList, scope)),
        GrammarRule.Create((IType type, IdentifierToken identifier, LeftParenToken _, RightParenToken _, IScopeNode scope) => new MethodDeclaration(type, identifier, scope)),
        GrammarRule.Create((VoidToken _, IdentifierToken identifier, LeftParenToken _, RightParenToken _, IScopeNode scope) => new VoidMethodDeclaration(identifier, scope)),

        // StmtList
        GrammarRule.Create((IStatement statement) => new SingleStatementStmtList(statement)),
        GrammarRule.Create((IStatement statement, IStmtListNode statementList) => new StmtListWithStatement(statement, statementList)),

        // Stmt
        GrammarRule.Create((ExitToken _, IExpression expression, SemicolonToken _) => new Exit(expression)),
        GrammarRule.Create((PrintToken _, IExpression expression, SemicolonToken _) => new Print(expression)),
        GrammarRule.Create((LetToken _, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new LetDeclaration(identifier, expression)),
        GrammarRule.Create((IType type, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new Declaration(type, identifier, expression)),
        GrammarRule.Create((IdentifierToken identifier, IAssignmentOperator assignmentOperator, IExpression expression, SemicolonToken _) => new Assignment(identifier, assignmentOperator, expression)),
        GrammarRule.Create((IScopeNode scope) => new ScopeStatement(scope)),
        GrammarRule.Create((IfToken _, IExpression expression, IScopeNode scope) => new If(expression, scope)),
        GrammarRule.Create((IfToken _, IExpression expression, IScopeNode scope, ElseToken _, IScopeNode elseScope) => new IfWithElse(expression, scope, elseScope)),
        GrammarRule.Create((WhileToken _, IExpression expression, IScopeNode scope) => new While(expression, scope)),
        GrammarRule.Create((ForToken _, IdentifierToken identifier, InToken _, IExpression startExpression, RangeToken _, IExpression endExpression, IScopeNode scope) => new For(identifier, startExpression, endExpression, scope)),
        GrammarRule.Create((IType type, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, IScopeNode scope) => new FunctionDeclarationWithParameters(type, identifier, parameterList, scope)),
        GrammarRule.Create((VoidToken _, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, IScopeNode scope) => new VoidFunctionDeclarationWithParameters(identifier, parameterList, scope)),
        GrammarRule.Create((IType type, IdentifierToken identifier, LeftParenToken _, RightParenToken _, IScopeNode scope) => new FunctionDeclaration(type, identifier, scope)),
        GrammarRule.Create((VoidToken _, IdentifierToken identifier, LeftParenToken _, RightParenToken _, IScopeNode scope) => new VoidFunctionDeclaration(identifier, scope)),
        GrammarRule.Create((IFunctionInvocation invocation, SemicolonToken _) => new Invocation(invocation)),
        GrammarRule.Create((IMethodInvocation invocation, SemicolonToken _) => new MethodInvocationStatement(invocation)),
        GrammarRule.Create((ReturnToken _, SemicolonToken _) => new Return()),
        GrammarRule.Create((ReturnToken _, IExpression expression, SemicolonToken _) => new ReturnValue(expression)),

        // ParameterList
        GrammarRule.Create((IType type, IdentifierToken identifier, CommaToken _, IParameterListNode parameterList) => new ContinuedParameterList(type, identifier, parameterList)),
        GrammarRule.Create((IType type, IdentifierToken identifier) => new Parameter(type, identifier)),

        // ArgumentList
        GrammarRule.Create((IExpression expression, CommaToken _, IArgumentList parameterList) => new ContinuedArgumentList(expression, parameterList)),
        GrammarRule.Create((IExpression expression) => new ExpressionArgumentList(expression)),

        // AssignmentOperator
        GrammarRule.Create((EqualsToken _) => new Equals()),
        GrammarRule.Create((PlusEqualsToken _) => new PlusEquals()),
        GrammarRule.Create((MinusEqualsToken _) => new MinusEquals()),
        GrammarRule.Create((TimesEqualsToken _) => new TimesEquals()),
        GrammarRule.Create((DivideEqualsToken _) => new DivideEquals()),

        // Scope
        GrammarRule.Create((LeftCurlyToken _, IStmtListNode stmtList, RightCurlyToken _) => new ScopeNode(stmtList)),
        GrammarRule.Create((LeftCurlyToken _, RightCurlyToken _) => new EmptyScope()),

        // Type
        GrammarRule.Create((IntTypeToken _) => new IntType()),
        GrammarRule.Create((BoolTypeToken _) => new BoolType()),
        GrammarRule.Create((FloatTypeToken _) => new FloatType()),
        GrammarRule.Create((StringTypeToken _) => new StringType()),

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
        GrammarRule.Create((IMultExpression lhs, TimesToken _, IUnaryExpression rhs) => new Mult(lhs, rhs)),
        GrammarRule.Create((IMultExpression lhs, DivideToken _, IUnaryExpression rhs) => new Div(lhs, rhs)),
        GrammarRule.Create((IUnaryExpression expression) => new NonMultExpression(expression)),

        // UnaryExpression
        GrammarRule.Create((NotToken _, ITerm term) => new Not(term)),
        GrammarRule.Create((MinusToken _, ITerm term) => new Negation(term)),
        GrammarRule.Create((ITerm term) => new NonUnaryExpression(term)),

        // Term
        GrammarRule.Create((IntLiteralToken intLit) => new IntLiteral(intLit)),
        GrammarRule.Create((FloatLiteralToken floatLit) => new FloatLiteral(floatLit)),
        GrammarRule.Create((BoolLiteralToken boolLit) => new BoolLiteral(boolLit)),
        GrammarRule.Create((StringLiteralToken stringLit) => new StringLiteral(stringLit)),
        GrammarRule.Create((IdentifierToken ident) => new IdentifierTerm(ident)),
        GrammarRule.Create((LeftParenToken _, IExpression expression, RightParenToken _) => new Paren(expression)),
        GrammarRule.Create((IFunctionInvocation functionInvocation) => new FunctionInvocationTerm(functionInvocation)),
        GrammarRule.Create((IMethodInvocation methodInvocation) => new MethodInvocationTerm(methodInvocation)),
        GrammarRule.Create((IType type, LeftParenToken _, IExpression value, RightParenToken _) => new CastTerm(type, value)),

        // FunctionInvocation
        GrammarRule.Create((IdentifierToken ident, LeftParenToken _, IArgumentList argumentList, RightParenToken _) => new FunctionInvocationWithArguments(ident, argumentList)),
        GrammarRule.Create((IdentifierToken ident, LeftParenToken _, RightParenToken _) => new FunctionInvocation(ident)),
        
        // MethodInvocation
        GrammarRule.Create((ITerm expression, DotToken _, IdentifierToken ident, LeftParenToken _, IArgumentList argumentList, RightParenToken _) => new MethodInvocationWithArguments(expression, ident, argumentList)),
        GrammarRule.Create((ITerm expression, DotToken _, IdentifierToken ident, LeftParenToken _, RightParenToken _) => new MethodInvocation(expression, ident)),
    ];

    public Type GetStartSymbol()
    {
        return typeof(IProgramNode);
    }

    public List<BaseGrammarRule> GetRules()
    {
        return Rules;
    }
}