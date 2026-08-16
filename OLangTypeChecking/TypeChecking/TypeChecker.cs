using ErrorHelper;
using Lexing;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Statements;
using OLangAst.Miscellaneous;
using OLangCompiler.Utility;

namespace OLangTypeChecking.TypeChecking;

public class TypeChecker
{
    private IErrorHelper _errorHelper;

    public void CheckTypes(Program program, IErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _variableTypeStack = new ScopeTracker<string, PrimitiveVariableTypeEnum>();
        _functionTypeStack = new ScopeTracker<string, PrimitiveVariableTypeEnum?>();
        _expressionTypes = new Dictionary<IExpression, PrimitiveVariableTypeEnum>();
        
        CheckStmtListType(program.Statements);
    }
    
    private void CheckStmtListType(List<IStatement> stmtList)
    {
        foreach (var statement in stmtList)
        {
            CheckStatementType(statement);
        }
    }
    
    private void CheckStatementType(IStatement statement)
    {
        switch (statement)
        {
            case VariableDeclarationStatement declarationStatement:
                var type = GetExpressionType(declarationStatement.Value);
                if (declarationStatement.Type is PrimitiveVariableType varType && type != varType.Type)
                {
                    throw _errorHelper.ShowErrorMessage($"Expression type {type} does not match variable type {varType.Type}", declarationStatement.Value.Span);
                }
    
                RecordVariableType(declarationStatement.Identifier, type, declarationStatement.Span);
                break;
            case ExitStatement exitStatement:
                var exitType = GetExpressionType(exitStatement.Expression);
                if (exitType != PrimitiveVariableTypeEnum.Int)
                {
                    throw _errorHelper.ShowErrorMessage($"Exit code has to be an integer", exitStatement.Expression.Span);
                }
    
                break;
            case VariableAssignment assignmentStatement:
                var expressionType = GetExpressionType(assignmentStatement.Value);
                var variableType = GetVariableType(assignmentStatement.Identifier, assignmentStatement.Span);
                if (variableType != expressionType)
                {
                    throw _errorHelper.ShowErrorMessage($"Cannot assign expression of type {expressionType} to variable {assignmentStatement.Identifier} of type {variableType}", assignmentStatement.Span);
                }
    
                break;
            case Scope scopeStatement:
                CheckScopeTypes(scopeStatement);
                break;
            case IfStatement ifStatement:
                var conditionType = GetExpressionType(ifStatement.Predicate);
                if (conditionType != PrimitiveVariableTypeEnum.Bool)
                {
                    throw _errorHelper.ShowErrorMessage("If predicate must be a boolean", ifStatement.Predicate.Span);
                }
    
                CheckScopeTypes(ifStatement.Body);
                if (ifStatement.Else != null)
                {
                    CheckScopeTypes(ifStatement.Else);
                }
                break;
            case WhileLoop whileStatement:
                var whileConditionType = GetExpressionType(whileStatement.Predicate);
                if (whileConditionType != PrimitiveVariableTypeEnum.Bool)
                {
                    throw _errorHelper.ShowErrorMessage("If predicate must be a boolean", whileStatement.Predicate.Span);
                }
    
                CheckScopeTypes(whileStatement.Body);
                break;
            case ForLoop forStatement:
                BeginScope();
                var startType = GetExpressionType(forStatement.RangeStart);
                if (startType != PrimitiveVariableTypeEnum.Int)
                {
                    throw _errorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeStart.Span);
                }
    
                var endType = GetExpressionType(forStatement.RangeEnd);
                if (endType != PrimitiveVariableTypeEnum.Int)
                {
                    throw _errorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeEnd.Span);
                }
    
                RecordVariableType(forStatement.Identifier, PrimitiveVariableTypeEnum.Int, forStatement.Span);
                CheckScopeTypes(forStatement.Body);
                EndScope();
                break;
            case FunctionDeclaration functionDeclaration:
                CheckFunctionDeclarationTypes(functionDeclaration);
                break;
            case FunctionInvocation:
                break;
            case Return returnStatement:
                if (!_isInFunction)
                {
                    throw _errorHelper.ShowErrorMessage("Cannot return outside of a function", returnStatement.Span);
                }

                if (returnStatement.Value == null)
                {
                    return;
                }
                
                expressionType = GetExpressionType(returnStatement.Value);
                if (expressionType != _currentFunctionType)
                {
                    throw _errorHelper.ShowErrorMessage($"Function of type {_currentFunctionType.ToString()} cannot return expression of type {expressionType.ToString()}", returnStatement.Value.Span);
                }
    
                break;
            default:
                throw _errorHelper.UnknownVariant("statement", statement.GetType());
        }
    }
    
    private PrimitiveVariableTypeEnum? GetTypeFromFunctionType(IVariableType type)
    {
        if (type is not PrimitiveVariableType primitiveType)
        {
            return null;
        }
    
        return primitiveType.Type;
    }
    
    private void CheckFunctionDeclarationTypes(FunctionDeclaration functionDeclaration)
    {
        if (functionDeclaration.Type != null)
        {
            _functionTypeStack.SetValue(functionDeclaration.Identifier, GetTypeFromFunctionType(functionDeclaration.Type));
        }
    
        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, PrimitiveVariableTypeEnum>();
    
        foreach (var param in functionDeclaration.Parameters)
        {
            _variableTypeStack.SetValue(param.Identifier, ((PrimitiveVariableType)param.Type).Type);
        }
    
        var wasInFunction = _isInFunction;
        _isInFunction = true;
        var previousFunctionType = _currentFunctionType;
        _currentFunctionType = GetTypeFromFunctionType(functionDeclaration.Type);
    
        CheckScopeTypes(functionDeclaration.Scope);
        if (functionDeclaration.Type != null)
        {
            CheckAllFunctionPathsReturnCorrectType(functionDeclaration);
        }
    
        _variableTypeStack = originalTypeStack;
        _currentFunctionType = previousFunctionType;
    
        _isInFunction = wasInFunction;
    }
    
    private void CheckAllFunctionPathsReturnCorrectType(FunctionDeclaration declaration)
    {
        if (!DoesScopeAlwaysReturn(declaration.Scope))
        {
            throw _errorHelper.ShowErrorMessage("Not all function paths return a value", declaration.Span);
        }
    }
    
    private bool DoesScopeAlwaysReturn(Scope scope)
    {
        foreach (var statement in scope.Statements)
        {
            if (statement is Return returnStatement)
            {
                return returnStatement.Value != null;
            }

            if (statement is ExitStatement)
            {
                return true;
            }
    
            if (statement is not IfStatement ifStatement) continue;
            if (ifStatement.Else is not {} elseScope)
            {
                continue;
            }
    
            if (DoesScopeAlwaysReturn(ifStatement.Body) && DoesScopeAlwaysReturn(elseScope))
            {
                return true;
            }
        }
    
        return false;
    }
    
    private void CheckScopeTypes(Scope scopeNode)
    {
        BeginScope();
        CheckStmtListType(scopeNode.Statements);
    
        EndScope();
    }
    
    private PrimitiveVariableTypeEnum DetermineExpressionType(IExpression expression)
    {
        switch (expression)
        {
            case Not notExpression:
                var notTermType = GetExpressionType(notExpression.Value);
                if (notTermType != PrimitiveVariableTypeEnum.Bool)
                {
                    throw _errorHelper.ShowErrorMessage($"Cannot negate a non-boolean value of type {notTermType}", notExpression.Value.Span);
                }
    
                return PrimitiveVariableTypeEnum.Bool;
            case BaseBinaryExpression binaryExpression:
            {
                return GetBinaryExpressionType(binaryExpression);
            }
            case VariableAccess variableAccess:
                return GetVariableType(variableAccess);
            case BoolLiteral:
                return PrimitiveVariableTypeEnum.Bool;
            case IntLiteral:
                return PrimitiveVariableTypeEnum.Int;
            case FloatLiteral:
                return PrimitiveVariableTypeEnum.Float;
            case FunctionInvocation invocation:
                return MarkAndCheckInvocation(invocation) ?? throw _errorHelper.ShowErrorMessage("Cannot get value from void function", invocation.Span);
            case Negate negate:
                var valueType = GetExpressionType(negate.Value);
                if (valueType != PrimitiveVariableTypeEnum.Int)
                {
                    throw _errorHelper.ShowErrorMessage($"Cannot negate expression of type {valueType}", negate.Span);
                }
                
                return PrimitiveVariableTypeEnum.Int;
            default:
                throw _errorHelper.UnknownVariant("binary expression", expression.GetType());
        }
    }

    private PrimitiveVariableTypeEnum GetBinaryExpressionType(BaseBinaryExpression binaryExpression)
    {
        var lhsType = GetExpressionType(binaryExpression.Lhs);
        var rhsType = GetExpressionType(binaryExpression.Rhs);

        PrimitiveVariableTypeEnum? type = binaryExpression switch
        {
            And when lhsType != PrimitiveVariableTypeEnum.Bool || rhsType != PrimitiveVariableTypeEnum.Bool => throw _errorHelper.ShowErrorMessage($"Cannot && expressions of type {lhsType} and {rhsType}", binaryExpression.Span),
            And => PrimitiveVariableTypeEnum.Bool,
            AreEqual or GreaterOrEqual or GreaterThan or LessThan or LessThanOrEqual or NotEqual => CheckComparisonBinaryExpression(binaryExpression),
            Or when lhsType != PrimitiveVariableTypeEnum.Bool || rhsType != PrimitiveVariableTypeEnum.Bool =>
                throw _errorHelper.ShowErrorMessage($"Cannot || expressions of type {lhsType} and {rhsType}", binaryExpression.Span),
            Or => PrimitiveVariableTypeEnum.Bool,
            _ => null
        };
        
        type ??= GetTypeOfBinaryExpression(lhsType, rhsType);
        if (type == null)
        {
            throw _errorHelper.ShowErrorMessage($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}", binaryExpression.Span);
        }
    
        return type.Value;
    }

    private void MarkExpressionType(IExpression expression, PrimitiveVariableTypeEnum type)
    {
        if (!_expressionTypes.TryAdd(expression, type))
        {
            throw _errorHelper.ShowErrorMessage("Expression was already marked", expression.Span);
        }
    }

    private PrimitiveVariableTypeEnum GetExpressionType(IExpression expression)
    {
        if (_expressionTypes.TryGetValue(expression, out var value))
        {
            return value;
        }

        var type = DetermineExpressionType(expression);
        MarkExpressionType(expression, type);

        return type;
    }
    
    private string GetNameOfOperator(BaseBinaryExpression binaryExpressionExpression)
    {
        return binaryExpressionExpression switch
        {
            Add => "add",
            Subtract => "subtract",
            Multiply => "multiply",
            Divide => "divide",
            And => "logically and",
            _ => throw _errorHelper.UnknownVariant("binary expression", binaryExpressionExpression.GetType())
        };
    }
    
    private PrimitiveVariableTypeEnum? MarkAndCheckInvocation(FunctionInvocation invocation)
    {
        if (!_functionTypeStack.ContainsKey(invocation.Identifier))
        {
            throw _errorHelper.ShowErrorMessage($"Unknown function: {invocation.Identifier}", invocation.Span);
        }
    
        return _functionTypeStack.GetValue(invocation.Identifier);
    }

    private PrimitiveVariableTypeEnum CheckComparisonBinaryExpression(BaseBinaryExpression expression)
    {
        var lhsType = GetExpressionType(expression.Lhs);
        var rhsType = GetExpressionType(expression.Rhs);

        if (lhsType != rhsType)
        {
            throw _errorHelper.ShowErrorMessage($"Cannot compare expressions of type {lhsType} and {rhsType}", expression.Span);
        }

        return PrimitiveVariableTypeEnum.Bool;
    }
    
    private PrimitiveVariableTypeEnum? GetTypeOfBinaryExpression(PrimitiveVariableTypeEnum type1, PrimitiveVariableTypeEnum type2)
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
    
    private ScopeTracker<string, PrimitiveVariableTypeEnum> _variableTypeStack = null!;
    private ScopeTracker<string, PrimitiveVariableTypeEnum?> _functionTypeStack = null!;
    private PrimitiveVariableTypeEnum? _currentFunctionType;
    private Dictionary<IExpression, PrimitiveVariableTypeEnum> _expressionTypes;
    private bool _isInFunction;
    
    private void RecordVariableType(string identifier, PrimitiveVariableTypeEnum expressionType, SourceSpan span)
    {
        if (_variableTypeStack.ContainsKey(identifier))
        {
            throw _errorHelper.ShowErrorMessage($"Identifier '{identifier}' already declared", span);
        }
    
        _variableTypeStack.SetValue(identifier, expressionType);
    }
    
    private PrimitiveVariableTypeEnum GetVariableType(string identifier, SourceSpan span)
    {
        if (!_variableTypeStack.ContainsKey(identifier))
        {
            throw _errorHelper.ShowErrorMessage($"Unknown identifier '{identifier}'", span);
        }
    
        return _variableTypeStack.GetValue(identifier);
    }
    
    private PrimitiveVariableTypeEnum GetVariableType(VariableAccess variableAccess)
    {
        if (!_variableTypeStack.ContainsKey(variableAccess.Identifier))
        {
            throw _errorHelper.ShowErrorMessage($"Unknown identifier '{variableAccess.Identifier}'", variableAccess.Span);
        }
    
        return _variableTypeStack.GetValue(variableAccess.Identifier);
    }
}