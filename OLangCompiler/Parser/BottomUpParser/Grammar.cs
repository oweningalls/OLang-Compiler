using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.FunctionType;
using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Prog;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Parser.ParseTree.Stmt;
using OLangCompiler.Parser.ParseTree.StmtList;
using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser;

public class Grammar : IGrammar
{
    private List<IGrammarRule> _rules = [
        // Prog
        GrammarRule.Create((IStmtListNode stmtList, EndOfInputToken _) => new ProgramNode(stmtList)), 
        
        // StmtList
        GrammarRule.Create(() => new EmptyStmtList()),
        GrammarRule.Create((IStatementNode statement, IStmtListNode statementList) => new StmtListWithStatement(statement, statementList)),
        
        // Stmt
        GrammarRule.Create((ExitToken _, BaseExpressionNode expression) => new ExitStatement(expression)),
        GrammarRule.Create((IVariableType variableType, IdentifierToken identifier, EqualsToken _, BaseExpressionNode expression, SemicolonToken _) => new DeclarationStatement(variableType, identifier, expression)),
        GrammarRule.Create((IdentifierToken identifier, IAssignmentOperatorNode assignmentOperator, BaseExpressionNode expression, SemicolonToken _) => new AssignmentStatement(identifier, assignmentOperator, expression)),
        GrammarRule.Create((ScopeNode scope) => new ScopeStatement(scope)),
        GrammarRule.Create((IfToken _, BaseExpressionNode expression, ScopeNode scope, IElseNode elseNode) => new IfStatement(expression, scope, elseNode)),
        GrammarRule.Create((WhileToken _, BaseExpressionNode expression, ScopeNode scope) => new WhileStatement(expression, scope)),
        GrammarRule.Create((ForToken _, IdentifierToken identifier, InToken _, BaseExpressionNode startExpression, RangeToken _, BaseExpressionNode endExpression, ScopeNode scope) => new ForStatement(identifier, startExpression, endExpression, scope)),
        GrammarRule.Create((IFunctionType type, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, ScopeNode scope) => new FunctionDeclarationStatement(type, identifier, parameterList, scope)),
        GrammarRule.Create((InvocationNode invocation) => new InvocationStatement(invocation)),
        GrammarRule.Create((ReturnToken _, SemicolonToken _) => new ReturnStatement()),
        GrammarRule.Create((ReturnToken _, BaseExpressionNode expression, SemicolonToken _) => new ReturnValueStatement(expression)),
        
        // ElseBlock
        GrammarRule.Create(() => new EmptyElse()),
        GrammarRule.Create((ElseToken _, ScopeNode scope) => new ElseNode(scope)),
        
        // ParameterList
        GrammarRule.Create(() => new EmptyParameterList()),
        GrammarRule.Create((BaseTypeToken type, IdentifierToken identifier, CommaToken _, IParameterListNode parameterList) => new ContinuedParameterList(type.ExpType!.Value, identifier, parameterList)),
        GrammarRule.Create((BaseTypeToken type, IdentifierToken identifier) => new Parameter(type.ExpType!.Value, identifier)),
        
        // ArgumentList
        GrammarRule.Create(() => new EmptyArgumentList()),
        GrammarRule.Create((BaseExpressionNode expression, CommaToken _, IArgumentListNode parameterList) => new ContinuedArgumentList(expression, parameterList)),
        GrammarRule.Create((BaseExpressionNode expression) => new ExpressionArgumentList(expression)),
        
        // SetOperator
        GrammarRule.Create((EqualsToken _) => new EqualsNode()),
        GrammarRule.Create((PlusEqualsToken _) => new PlusEqualsNode()),
        GrammarRule.Create((MinusEqualsToken _) => new MinusEqualsNode()),
        GrammarRule.Create((TimesEqualsToken _) => new TimesEqualsNode()),
        GrammarRule.Create((DivideEqualsToken _) => new DivideEqualsNode()),
        
        // Scope
        GrammarRule.Create((LeftCurlyToken _, IStmtListNode stmtList, RightCurlyToken _) => new ScopeNode(stmtList)),
        
        // FunctionType
        GrammarRule.Create((VoidToken _) => new VoidFunctionType()),
        GrammarRule.Create((ITypeNode typeNode) => new PrimitiveFunctionType(typeNode)),
        
        // VariableType
        GrammarRule.Create((LetToken _) => new LetVariableType()),
        GrammarRule.Create((ITypeNode typeNode) => new PrimitiveVariableType(typeNode)),
        
        // Type
        GrammarRule.Create((IntTypeToken _) => new PrimitiveVariableType(new IntTypeNode())),
        GrammarRule.Create((BoolTypeToken _) => new PrimitiveVariableType(new BoolTypeNode())),
        GrammarRule.Create((FloatTypeToken _) => new PrimitiveVariableType(new FloatTypeNode())),
        
        // Expression
        // TODO: need to refactor to encode operator precedence into grammar
        // TODO: also make BinaryExpressionNode a thing
        // GrammarRule.Create((BaseExpressionNode lhs) => new ),
        
        // BinaryOperator
        
        // Term
        
        // FunctionInvocation
        
        GrammarRule.Create((IdentifierToken ident, LeftParenToken _, IArgumentListNode argumentList, RightParenToken _) => new InvocationNode(ident, argumentList)),
        ];
    
    public List<IGrammarRule> GetRules()
    {
        return _rules;
    }
}
