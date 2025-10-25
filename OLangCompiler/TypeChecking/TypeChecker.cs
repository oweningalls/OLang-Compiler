using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;
using OLangCompiler.Utility;

namespace OLangCompiler.TypeChecking;

public class TypeChecker
{
    private ErrorHelper _errorHelper;

    public void CheckTypes(ProgramNode program, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _variableTypeStack = new ScopeTracker<string, ExpressionType>();
        _functionTypeStack = new ScopeTracker<string, ExpressionType?>();

        CheckStmtListType(program.StmtList);
    }

    private void CheckStmtListType(IStmtListNode stmtList)
    {
        switch (stmtList)
        {
            case EmptyStmtList:
                break;
            case StmtListWithStatement stmtListWithStatement:
                CheckStatementType(stmtListWithStatement.Statement);
                CheckStmtListType(stmtListWithStatement.StmtList);
                break;
            default:
            {
                throw _errorHelper.UnknownVariant("statment list", stmtList.GetType());
            }
        }
    }

    private void CheckStatementType(IStatementNode statementNode)
    {
        switch (statementNode)
        {
            case DeclarationStatement declarationStatement:
                MarkAndCheckExpression(declarationStatement.Expression);
                var type = declarationStatement.Expression.ValueType;
                if (declarationStatement.ExpressionType != null && type != declarationStatement.ExpressionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Expression type {type} does not match variable type {declarationStatement.ExpressionType}", declarationStatement.Expression);
                }

                RecordExpressionType(declarationStatement.Identifier, type!.Value);
                break;
            case ExitStatement exitStatement:
                MarkAndCheckExpression(exitStatement.ExpressionNode);
                var exitType = exitStatement.ExpressionNode.ValueType;
                if (exitType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Exit code has to be an integer", exitStatement.ExpressionNode);
                }

                break;
            case AssignmentStatement assignmentStatement:
                MarkAndCheckExpression(assignmentStatement.Expression);
                var expressionType = assignmentStatement.Expression.ValueType;
                var variableType = GetVariableType(assignmentStatement.Identifier);
                if (variableType != expressionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot assign variable {assignmentStatement.Identifier.Identifier} of type {variableType} to expression of type {expressionType}", assignmentStatement.Expression);
                }

                break;
            case ScopeStatement scopeStatement:
                CheckScopeTypes(scopeStatement.Scope);
                break;
            case IfStatement ifStatement:
                MarkAndCheckExpression(ifStatement.Condition);
                var conditionType = ifStatement.Condition.ValueType;
                if (conditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", ifStatement.Condition);
                }

                CheckScopeTypes(ifStatement.Scope);
                break;
            case WhileStatement whileStatement:
                MarkAndCheckExpression(whileStatement.Condition);
                var whileConditionType = whileStatement.Condition.ValueType;
                if (whileConditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", whileStatement.Condition);
                }

                CheckScopeTypes(whileStatement.Scope);
                break;
            case ForStatement forStatement:
                BeginScope();
                MarkAndCheckExpression(forStatement.Start);
                var startType = forStatement.Start.ValueType;
                if (startType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Bounds of range must be integers", forStatement.Start);
                }

                MarkAndCheckExpression(forStatement.End);
                var endType = forStatement.End.ValueType;
                if (endType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Bounds of range must be integers", forStatement.End);
                }

                RecordExpressionType(forStatement.Identifier, ExpressionType.Int);
                CheckScopeTypes(forStatement.Scope);
                EndScope();
                break;
            case FunctionDeclarationStatement functionDeclaration:
                CheckFunctionDeclarationTypes(functionDeclaration);
                break;
            case InvocationStatement:
                break;
            case ReturnStatement returnStatement:
                if (!_isInFunction)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Cannot return outside of a function", returnStatement);
                }

                break;
            case ReturnValueStatement returnValueStatement:
                if (!_isInFunction)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Cannot return outside of a function", returnValueStatement);
                }

                MarkAndCheckExpression(returnValueStatement.Expression);
                expressionType = returnValueStatement.Expression.ValueType;
                if (expressionType != _currentFunctionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Function of type {_currentFunctionType.ToString()} cannot return expression of type {expressionType.ToString()}", returnValueStatement.Expression);
                }

                break;
            default:
                throw _errorHelper.UnknownVariant("statement", statementNode.GetType());
        }
    }

    private void CheckFunctionDeclarationTypes(FunctionDeclarationStatement functionDeclaration)
    {
        _functionTypeStack.SetValue(functionDeclaration.Identifier.Identifier, functionDeclaration.Type);

        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, ExpressionType>();

        var i = 0;
        var parameterList = functionDeclaration.Parameters;
        while (parameterList is Parameter param)
        {
            var type = param.Type;
            var ident = param.Identifier;
            
            _variableTypeStack.SetValue(ident.Identifier, type);
            if (param is ContinuedParameterList continued)
            {
                parameterList = continued.ParameterList;
            }
            else
            {
                break;
            }
            i += 1;
        }

        var wasInFunction = _isInFunction;
        _isInFunction = true;
        var previousFunctionType = _currentFunctionType;
        _currentFunctionType = functionDeclaration.Type;

        CheckScopeTypes(functionDeclaration.Scope);
        if (functionDeclaration.Type != null)
        {
            CheckAllFunctionPathsReturnCorrectType(functionDeclaration);
        }

        _variableTypeStack = originalTypeStack;
        _currentFunctionType = previousFunctionType;

        _isInFunction = wasInFunction;
    }

    private void CheckAllFunctionPathsReturnCorrectType(FunctionDeclarationStatement declarationStatement)
    {
        if (!DoesScopeAlwaysReturn(declarationStatement.Scope))
        {
            throw _errorHelper.ShowErrorMessageAtNode("Not all function paths return a value", declarationStatement);
        }
    }

    private bool DoesScopeAlwaysReturn(ScopeNode scope)
    {
        foreach (var statement in scope.Statements)
        {
            if (statement is ReturnValueStatement or ExitStatement)
            {
                return true;
            }

            if (statement is not IfStatement ifStatement) continue;
            if (ifStatement.ElseBlock == null)
            {
                continue;
            }

            if (DoesScopeAlwaysReturn(ifStatement.Scope) && DoesScopeAlwaysReturn(ifStatement.ElseBlock.Scope))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckScopeTypes(ScopeNode scopeNode)
    {
        BeginScope();
        foreach (var statement in scopeNode.Statements)
        {
            CheckStatementType(statement);
        }

        EndScope();
    }

    private void MarkAndCheckExpression(BaseExpressionNode expression)
    {
        switch (expression)
        {
            case TermExpression term:
                MarkAndCheckTerm(term.Term);
                term.ValueType = term.Term.ValueType;
                break;
            case NotExpression notExpression:
                MarkAndCheckExpression(notExpression.Expression);
                var notTermType = notExpression.Expression.ValueType;
                if (notTermType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot negate a non-boolean value of type {notTermType}", notExpression.Expression);
                }

                expression.ValueType = ExpressionType.Bool;
                break;
            case BaseBinaryExpressionNode binaryExpression:
            {
                MarkAndCheckExpression(binaryExpression.Lhs);
                var lhsType = binaryExpression.Lhs.ValueType;
                MarkAndCheckExpression(binaryExpression.Rhs);
                var rhsType = binaryExpression.Rhs.ValueType;

                if (binaryExpression is BooleanAndExpression)
                {
                    if (lhsType != ExpressionType.Bool || rhsType != ExpressionType.Bool)
                    {
                        throw _errorHelper.ShowErrorMessageAtNode($"Cannot && expressions of type {lhsType} and {rhsType}", binaryExpression);
                    }

                    expression.ValueType = ExpressionType.Bool;
                    break;
                }
                
                if (binaryExpression is BooleanOrExpression)
                {
                    if (lhsType != ExpressionType.Bool || rhsType != ExpressionType.Bool)
                    {
                        throw _errorHelper.ShowErrorMessageAtNode($"Cannot || expressions of type {lhsType} and {rhsType}", binaryExpression);
                    }

                    expression.ValueType = ExpressionType.Bool;
                    break;
                }

                if (binaryExpression is BaseComparisonExpressionNode)
                {
                    if (GetTypeOfBinaryExpression(lhsType!.Value, rhsType!.Value) == null)
                    {
                        throw _errorHelper.ShowErrorMessageAtNode($"Cannot compare expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                    }

                    expression.ValueType = ExpressionType.Bool;
                    break;
                }

                var type = GetTypeOfBinaryExpression(lhsType!.Value, rhsType!.Value);
                if (type == null)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                }

                expression.ValueType = type.Value;
                break;
            }
            default:
                throw _errorHelper.UnknownVariant("binary expression", expression.GetType());
        }
    }

    private string GetNameOfOperator(BaseBinaryExpressionNode binaryExpressionNode)
    {
        return binaryExpressionNode switch
        {
            AddExpression => "add",
            SubtractExpression => "subtract",
            TimesExpression => "multiply",
            DivideExpression => "divide",
            BooleanAndExpression => "logically and",
            BaseComparisonExpressionNode => "compare",
            _ => throw _errorHelper.UnknownVariant("binary expression", binaryExpressionNode.GetType())
        };
    }

    private void MarkAndCheckTerm(BaseTermNode term)
    {
        switch (term)
        {
            case IdentifierTerm identifierTerm:
                term.ValueType = GetVariableType(identifierTerm);
                break;
            case ParenTerm parenTerm:
                MarkAndCheckExpression(parenTerm.Expression);
                term.ValueType = parenTerm.Expression.ValueType;
                break;
            case BoolLiteralTerm:
                term.ValueType = ExpressionType.Bool;
                break;
            case IntLiteralTerm:
                term.ValueType = ExpressionType.Int;
                break;
            case FloatLiteralTerm:
                term.ValueType = ExpressionType.Float;
                break;
            case InvocationTerm invocation:
                MarkAndCheckInvocation(invocation.InvocationNode);
                term.ValueType = invocation.InvocationNode.ValueType ?? throw _errorHelper.ShowErrorMessageAtNode("Cannot get value from void function", invocation.InvocationNode);
                break;
            default:
                throw _errorHelper.UnknownVariant("term", term.GetType());
        }
    }

    private void MarkAndCheckInvocation(InvocationNode invocation)
    {
        if (!_functionTypeStack.ContainsKey(invocation.Identifier.Identifier))
        {
            throw _errorHelper.ShowErrorMessageAtNode($"Unknown function: {invocation.Identifier}", invocation);
        }

        invocation.ValueType = _functionTypeStack.GetValue(invocation.Identifier.Identifier);
    }

    private ExpressionType? GetTypeOfBinaryExpression(ExpressionType type1, ExpressionType type2)
    {
        if (type1 == type2)
        {
            return type1;
        }

        return null;
    }

    private void BeginScope()
    {
        _variableTypeStack.BeginScope();
        _functionTypeStack.BeginScope();
    }

    private void EndScope()
    {
        _variableTypeStack.EndScope();
        _functionTypeStack.EndScope();
    }

    private ScopeTracker<string, ExpressionType> _variableTypeStack = null!;
    private ScopeTracker<string, ExpressionType?> _functionTypeStack = null!;
    private ExpressionType? _currentFunctionType;
    private bool _isInFunction;

    private void RecordExpressionType(IdentifierToken identifierToken, ExpressionType expressionType)
    {
        if (_variableTypeStack.ContainsKey(identifierToken.Identifier))
        {
            throw _errorHelper.ShowErrorMessageAtToken($"Identifier '{identifierToken.Identifier}' already declared", identifierToken);
        }

        _variableTypeStack.SetValue(identifierToken.Identifier, expressionType);
    }

    private ExpressionType GetVariableType(IdentifierToken identifierToken)
    {
        if (!_variableTypeStack.ContainsKey(identifierToken.Identifier))
        {
            throw _errorHelper.ShowErrorMessageAtToken($"Unknown identifier '{identifierToken.Identifier}'", identifierToken);
        }

        return _variableTypeStack.GetValue(identifierToken.Identifier);
    }

    private ExpressionType GetVariableType(IdentifierTerm identifierTerm)
    {
        if (!_variableTypeStack.ContainsKey(identifierTerm.Identifier))
        {
            throw _errorHelper.ShowErrorMessageAtNode($"Unknown identifier '{identifierTerm.Identifier}'", identifierTerm);
        }

        return _variableTypeStack.GetValue(identifierTerm.Identifier);
    }
}