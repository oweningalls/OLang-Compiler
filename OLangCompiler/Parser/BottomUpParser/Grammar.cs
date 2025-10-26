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
using Void = OLangCompiler.Parser.ParseTree.FunctionType.Void;

namespace OLangCompiler.Parser.BottomUpParser;

public class Grammar : IGrammar
{
    private List<IGrammarRule> _rules = [
        // Prog
        GrammarRule.Create((IStmtListNode stmtList, EndOfInputToken _) => new ProgramNode(stmtList)), 
        
        // StmtList
        GrammarRule.Create(() => new EmptyStmtList()),
        GrammarRule.Create((IStatement statement, IStmtListNode statementList) => new StmtListWithStatement(statement, statementList)),
        
        // Stmt
        GrammarRule.Create((ExitToken _, IExpression expression) => new Exit(expression)),
        GrammarRule.Create((IVariableType variableType, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new Declaration(variableType, identifier, expression)),
        GrammarRule.Create((IdentifierToken identifier, IAssignmentOperator assignmentOperator, IExpression expression, SemicolonToken _) => new Assignment(identifier, assignmentOperator, expression)),
        GrammarRule.Create((ScopeNode scope) => new ScopeStatement(scope)),
        GrammarRule.Create((IfToken _, IExpression expression, ScopeNode scope, IElse elseNode) => new If(expression, scope, elseNode)),
        GrammarRule.Create((WhileToken _, IExpression expression, ScopeNode scope) => new While(expression, scope)),
        GrammarRule.Create((ForToken _, IdentifierToken identifier, InToken _, IExpression startExpression, RangeToken _, IExpression endExpression, ScopeNode scope) => new For(identifier, startExpression, endExpression, scope)),
        GrammarRule.Create((IFunctionType type, IdentifierToken identifier, LeftParenToken _, IParameterListNode parameterList, RightParenToken _, ScopeNode scope) => new FunctionDeclaration(type, identifier, parameterList, scope)),
        GrammarRule.Create((InvocationNode invocation) => new Invocation(invocation)),
        GrammarRule.Create((ReturnToken _, SemicolonToken _) => new Return()),
        GrammarRule.Create((ReturnToken _, IExpression expression, SemicolonToken _) => new ReturnValue(expression)),
        
        // ElseBlock
        GrammarRule.Create(() => new EmptyElse()),
        GrammarRule.Create((ElseToken _, ScopeNode scope) => new Else(scope)),
        
        // ParameterList
        GrammarRule.Create(() => new EmptyParameterList()),
        GrammarRule.Create((BaseToken type, IdentifierToken identifier, CommaToken _, IParameterListNode parameterList) => new ContinuedParameterList(type.ExpType!.Value, identifier, parameterList)),
        GrammarRule.Create((BaseToken type, IdentifierToken identifier) => new Parameter(type.ExpType!.Value, identifier)),
        
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
        GrammarRule.Create((IntTypeToken _) => new PrimitiveVariableType(new IntType())),
        GrammarRule.Create((BoolTypeToken _) => new PrimitiveVariableType(new BoolType())),
        GrammarRule.Create((FloatTypeToken _) => new PrimitiveVariableType(new FloatType())),
        
        // Expression
        // TODO: need to refactor to encode operator precedence into grammar
        // TODO: also make BinaryExpressionNode a thing
        // GrammarRule.Create((IExpressionNode lhs) => new ),
        
        // BinaryOperator
        
        // Term
        
        // FunctionInvocation
        
        GrammarRule.Create((IdentifierToken ident, LeftParenToken _, IArgumentList argumentList, RightParenToken _) => new InvocationNode(ident, argumentList)),
        ];
    
    public List<IGrammarRule> GetRules()
    {
        return _rules;
    }
}
