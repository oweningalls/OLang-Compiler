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

        foreach (var statement in program.Statements)
        {
            CheckStatementType(statement);
        }
    }

    private void CheckStatementType(IStatementNode statementNode)
    {
        switch (statementNode)
        {
            case DeclarationStatement declarationStatement:
                var type = GetTypeFromExpression(declarationStatement.Expression);
                if (declarationStatement.ExpressionType != null && type != declarationStatement.ExpressionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Expression type {type} does not match variable type {declarationStatement.ExpressionType}", declarationStatement.Expression);
                }

                RecordExpressionType(declarationStatement.Identifier, type);
                break;
            case ExitStatement exitStatement:
                var exitType = GetTypeFromExpression(exitStatement.ExpressionNode);
                if (exitType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Exit code has to be an integer", exitStatement.ExpressionNode);
                }

                break;
            case AssignmentStatement assignmentStatement:
                var expressionType = GetTypeFromExpression(assignmentStatement.Expression);
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
                var conditionType = GetTypeFromExpression(ifStatement.Condition);
                if (conditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", ifStatement.Condition);
                }

                CheckScopeTypes(ifStatement.Scope);
                break;
            case WhileStatement whileStatement:
                var whileConditionType = GetTypeFromExpression(whileStatement.Condition);
                if (whileConditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", whileStatement.Condition);
                }

                CheckScopeTypes(whileStatement.Scope);
                break;
            case ForStatement forStatement:
                BeginScope();
                var startType = GetTypeFromExpression(forStatement.Start);
                if (startType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Bounds of range must be integers", forStatement.Start);
                }

                var endType = GetTypeFromExpression(forStatement.End);
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
            case InvocationStatement invocationStatement:
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

                expressionType = GetTypeFromExpression(returnValueStatement.Expression);
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

        foreach ((ExpressionType, IdentifierToken) val in functionDeclaration.Parameters.Parameters)
        {
            var (type, ident) = val;
            
            _variableTypeStack.SetValue(ident.Identifier, type);
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
        // TODO: update after implementing else
        foreach (var statement in declarationStatement.Scope.Statements)
        {
            if (statement is ReturnValueStatement or ExitStatement)
            {
                return;
            }
        }

        throw _errorHelper.ShowErrorMessageAtNode("Not all function paths return a value", declarationStatement);
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

    private ExpressionType GetTypeFromExpression(IExpressionNode expression)
    {
        switch (expression)
        {
            case TermExpression term:
                return GetTypeFromTerm(term.Term);
            case NotExpression notExpression:
                var notTermType = GetTypeFromExpression(notExpression.Expression);
                if (notTermType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot negate a non-boolean value of type {notTermType}", notExpression.Expression);
                }

                return ExpressionType.Bool;
            case BaseBinaryExpressionNode binaryExpression:
            {
                var lhsType = GetTypeFromExpression(binaryExpression.Lhs);
                var rhsType = GetTypeFromExpression(binaryExpression.Rhs);

                if (binaryExpression is BaseComparisonExpressionNode)
                {
                    if (lhsType != rhsType)
                    {
                        throw _errorHelper.ShowErrorMessageAtNode($"Cannot compare expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                    }

                    return ExpressionType.Bool;
                }

                var type = GetTypeOfBinaryExpression(lhsType, rhsType);
                if (type == null)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                }

                return type.Value;
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
            BaseComparisonExpressionNode => "compare",
            _ => throw _errorHelper.UnknownVariant("binary expression", binaryExpressionNode.GetType())
        };
    }

    private ExpressionType GetTypeFromTerm(ITermNode term)
    {
        return term switch
        {
            IdentifierTerm identifierTerm => GetVariableType(identifierTerm),
            ParenTerm parenTerm => GetTypeFromExpression(parenTerm.Expression),
            BoolLiteralTerm => ExpressionType.Bool,
            IntLiteralTerm => ExpressionType.Int,
            InvocationTerm invocation => GetTypeFromInvocation(invocation.InvocationNode) ??
                                         throw _errorHelper.ShowErrorMessageAtNode("Cannot get value from void function", invocation.InvocationNode),
            _ => throw _errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    private ExpressionType? GetTypeFromInvocation(InvocationNode invocation)
    {
        if (!_functionTypeStack.ContainsKey(invocation.Identifier.Identifier))
        {
            throw _errorHelper.ShowErrorMessageAtNode($"Unknown function: {invocation.Identifier}", invocation);
        }

        return _functionTypeStack.GetValue(invocation.Identifier.Identifier);
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
    private ExpressionType? _currentFunctionType = null;
    private bool _isInFunction = false;

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