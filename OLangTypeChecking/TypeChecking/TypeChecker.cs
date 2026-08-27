using ErrorHelper;
using Lexing;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Statements;
using OLangAst.Miscellaneous;
using OLangCompiler.Utility;
using OLangTypeChecking.TypeChecking.Types;

namespace OLangTypeChecking.TypeChecking;

public class TypeChecker(IErrorHelper errorHelper) : BaseOLangAstVisitor(errorHelper)
{
    public override Program VisitProgram(Program program)
    {
        _variableTypeStack = new ScopeTracker<string, IVariableType>();
        _functionTypeStack = new ScopeTracker<string, FunctionSignature>();

        return base.VisitProgram(program);
    }

    protected override Return VisitReturnStatement(Return returnStatement)
    {
        if (_currentFunctionReturnType is null && returnStatement.Value is not null)
        {
            throw ErrorHelper.ShowErrorMessage($"Void function cannot return a value.", returnStatement.Value.Span);
        }
        returnStatement = base.VisitReturnStatement(returnStatement);
        if (!_isInFunction)
        {
            throw ErrorHelper.ShowErrorMessage("Cannot return outside of a function.", returnStatement.Span);
        }

        if (returnStatement.Value == null)
        {
            return returnStatement;
        }

        var expressionType = GetExpressionType(returnStatement.Value);
        if (!expressionType.Equals(_currentFunctionReturnType))
        {
            throw ErrorHelper.ShowErrorMessage($"Function of type {_currentFunctionReturnType} cannot return expression of type {expressionType}.", returnStatement.Value.Span);
        }

        return returnStatement;
    }

    protected override ForLoop VisitForLoop(ForLoop forStatement)
    {
        BeginScope();
        forStatement.RangeStart = VisitExpression(forStatement.RangeStart);
        forStatement.RangeEnd = VisitExpression(forStatement.RangeEnd);
        var startType = GetExpressionType(forStatement.RangeStart);
        
        if (!startType.Equals(IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeStart.Span);
        }

        var endType = GetExpressionType(forStatement.RangeEnd);
        if (!endType.Equals(IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeEnd.Span);
        }

        RecordVariableType(forStatement.Identifier, IntType, forStatement.Span);
        forStatement.Body.Statements = VisitStatements(forStatement.Body.Statements);
        EndScope();

        return forStatement;
    }

    protected override IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement = base.VisitIfStatement(ifStatement);
        var conditionType = GetExpressionType(ifStatement.Predicate);
        if (!conditionType.Equals(BoolType))
        {
            throw ErrorHelper.ShowErrorMessage("If predicate must be a boolean", ifStatement.Predicate.Span);
        }

        return ifStatement;
    }

    protected override WhileLoop VisitWhileLoop(WhileLoop whileStatement)
    {
        whileStatement = base.VisitWhileLoop(whileStatement);
        var whileConditionType = GetExpressionType(whileStatement.Predicate);
        if (!whileConditionType.Equals(BoolType))
        {
            throw ErrorHelper.ShowErrorMessage("If predicate must be a boolean", whileStatement.Predicate.Span);
        }

        return whileStatement;
    }

    protected override VariableAssignment VisitVariableAssignment(VariableAssignment assignmentStatement)
    {
        assignmentStatement = base.VisitVariableAssignment(assignmentStatement);
        var expressionType = GetExpressionType(assignmentStatement.Value);
        var variableType = GetVariableType(assignmentStatement.Identifier, assignmentStatement.Span);
        if (!expressionType.Equals(variableType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot assign expression of type {expressionType} to variable {assignmentStatement.Identifier} of type {variableType}", assignmentStatement.Span);
        }

        return assignmentStatement;
    }

    protected override ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement = base.VisitExitStatement(exitStatement);
        var exitType = GetExpressionType(exitStatement.Expression);
        if (!exitType.Equals(IntType))
        {
            throw ErrorHelper.ShowErrorMessage($"Exit code has to be an integer, was {exitType}", exitStatement.Expression.Span);
        }

        return exitStatement;
    }
    
    protected override PrintStatement VisitPrintStatement(PrintStatement printStatement)
    {
        printStatement = base.VisitPrintStatement(printStatement);
        var expressionType = GetExpressionType(printStatement.Expression);
        if (!expressionType.Equals(StringType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot print non-string value, was {expressionType}", printStatement.Expression.Span);
        }

        return printStatement;
    }

    protected override Not VisitNotExpression(Not notExpression)
    {
        notExpression = base.VisitNotExpression(notExpression);
        var expType = GetExpressionType(notExpression.Value);

        if (!expType.Equals(BoolType))
        {
            throw CannotApplyUnaryOperator("!", expType.ToString(), notExpression.Span);
        }
        
        return notExpression;
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        var type = GetExpressionType(declarationStatement.Value);
        if (declarationStatement.Type is PrimitiveVariableType varType && !type.Equals(varType))
        {
            throw ErrorHelper.ShowErrorMessage($"Expression type {type} does not match variable type {varType.Type}", declarationStatement.Value.Span);
        }
    
        RecordVariableType(declarationStatement.Identifier, type, declarationStatement.Span);

        return declarationStatement;
    }

    private static FunctionSignature GetTypeFromFunctionDeclaration(FunctionDeclaration declaration)
    {
        return new FunctionSignature(declaration.Type, declaration.Parameters.Select(x => x.Type));
    }

    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var signature = GetTypeFromFunctionDeclaration(functionDeclaration);
        _functionTypeStack.SetValue(functionDeclaration.Identifier, signature);
    
        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, IVariableType>();
    
        foreach (var param in functionDeclaration.Parameters)
        {
            _variableTypeStack.SetValue(param.Identifier, param.Type);
        }
    
        var wasInFunction = _isInFunction;
        _isInFunction = true;
        var previousFunctionType = _currentFunctionReturnType;
        _currentFunctionReturnType = GetReturnType(signature);
    
        VisitScope(functionDeclaration.Scope);
        if (functionDeclaration.Type is not null)
        {
            CheckAllFunctionPathsReturnCorrectType(functionDeclaration);
        }
    
        _variableTypeStack = originalTypeStack;
        _currentFunctionReturnType = previousFunctionType;
    
        _isInFunction = wasInFunction;

        return functionDeclaration;
    }

    private IVariableType? GetReturnType(FunctionSignature signature)
    {
        if (signature.ReturnType == null)
        {
            return null;
        }
        return signature.ReturnType!;
    }

    protected override FunctionInvocation VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation = base.VisitFunctionInvocation(functionInvocation);
        if (!_functionTypeStack.ContainsKey(functionInvocation.Identifier))
        {
            throw ErrorHelper.ShowErrorMessage($"Unknown function: {functionInvocation.Identifier}", functionInvocation.Span);
        }

        var signature = _functionTypeStack.GetValue(functionInvocation.Identifier);
        var paramCount = signature.ParameterTypes.Count;
        var argCount = functionInvocation.Arguments.Count;
        if (argCount != paramCount)
        {
            throw ErrorHelper.ShowErrorMessage($"Function '{functionInvocation.Identifier}' has {paramCount} parameters but is invoked with {argCount} arguments.", functionInvocation.Span);
        }

        foreach (var (argument, paramType) in functionInvocation.Arguments.Zip(signature.ParameterTypes))
        {
            if (!argument.Type.Equals(paramType))
            {
                throw ErrorHelper.ShowErrorMessage($"Argument of type {argument.Type} passed to function '{functionInvocation.Identifier}' does not match declared parameter type {paramType}", argument.Span);
            }
        }
        
        if (GetReturnType(signature) is { } returnType)
        {
            functionInvocation.Type = returnType;
        }

        return functionInvocation;
    }
    
    private void CheckAllFunctionPathsReturnCorrectType(FunctionDeclaration declaration)
    {
        if (!DoesScopeAlwaysReturn(declaration.Scope))
        {
            throw ErrorHelper.ShowErrorMessage("Not all function paths return a value", declaration.Span);
        }
    }
    
    private bool DoesScopeAlwaysReturn(Scope scope)
    {
        foreach (var statement in scope.Statements)
        {
            if (statement is Return returnStatement)
            {
                return returnStatement.Value is not null;
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
    
    protected override Scope VisitScope(Scope scopeNode)
    {
        BeginScope();
        scopeNode = base.VisitScope(scopeNode);
        EndScope();

        return scopeNode;
    }
    
    protected override Negate VisitNegate(Negate negate)
    {
        negate = base.VisitNegate(negate);
        var expType = GetExpressionType(negate.Value);

        if (!expType.Equals(IntType))
        {
            throw CannotApplyUnaryOperator("-", expType.ToString(), negate.Span);
        }
        
        MarkExpressionType(negate, IntType);

        return negate;
    }

    private Exception CannotApplyUnaryOperator(string oper, string expressionType, SourceSpan span)
    {
        return ErrorHelper.ShowErrorMessage($"Cannot apply `{oper}` operator to expression of type {expressionType}", span);
    }

    private void CheckBooleanBinaryExpression<T>(T booleanBinaryExpression) where T : BaseBinaryExpression
    {
        var lhsType = GetExpressionType(booleanBinaryExpression.Lhs);
        var rhsType = GetExpressionType(booleanBinaryExpression.Rhs);

        if (!lhsType.Equals(BoolType) || !rhsType.Equals(BoolType))
        {
            throw ErrorHelper.ShowErrorMessage($"Both sides of expression must be {PrimitiveVariableTypeEnum.Bool} type, was {lhsType} and {rhsType}.", booleanBinaryExpression.Span);
        }
    }
    
    private void CheckBinaryComparisonExpression<T>(T binaryComparisonExpression) where T : BaseBinaryExpression
    {
        var lhsType = GetExpressionType(binaryComparisonExpression.Lhs);
        var rhsType = GetExpressionType(binaryComparisonExpression.Rhs);

        if (!lhsType.Equals(rhsType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot compare expressions of type {lhsType} and {rhsType}", binaryComparisonExpression.Span);
        }
    }
    
    private void CheckMathExpression<T>(T mathExpression, string oper) where T : BaseBinaryExpression
    {
        var lhsType = GetExpressionType(mathExpression.Lhs);
        var rhsType = GetExpressionType(mathExpression.Rhs);

        var addedType = ResultingTypeFromMath(lhsType, rhsType);
        if (addedType is null)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot apply operator `{oper}` to expressions of type {lhsType} and {rhsType}", mathExpression.Span);
        }
        
        MarkExpressionType(mathExpression, addedType);
    }

    private IVariableType? ResultingTypeFromMath(IVariableType type1, IVariableType type2)
    {
        if (!MathTypes.Any(x => x.Equals(type1)) || !MathTypes.Any(x => x.Equals(type2)))
        {
            return null;
        }

        if (type1.Equals(type2))
        {
            return type1;
        }

        return null;
    }

    protected override Add VisitAddExpression(Add addExpression)
    {
        addExpression = base.VisitAddExpression(addExpression);
        CheckMathExpression(addExpression, "+");

        return addExpression;
    }
    
    protected override And VisitAndExpression(And andExpression)
    {
        andExpression = base.VisitAndExpression(andExpression);
        CheckBooleanBinaryExpression(andExpression);

        return andExpression;
    }
    
    protected override AreEqual VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        areEqualExpression = base.VisitAreEqualExpression(areEqualExpression);
        CheckBinaryComparisonExpression(areEqualExpression);

        return areEqualExpression;
    }

    protected override GreaterOrEqual VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        greaterOrEqualExpression = base.VisitGreaterOrEqualExpression(greaterOrEqualExpression);
        CheckBinaryComparisonExpression(greaterOrEqualExpression);

        return greaterOrEqualExpression;
    }

    protected override GreaterThan VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        greaterThanExpression = base.VisitGreaterThanExpression(greaterThanExpression);
        CheckBinaryComparisonExpression(greaterThanExpression);

        return greaterThanExpression;
    }

    protected override LessThan VisitLessThanExpression(LessThan lessThanExpression)
    {
        lessThanExpression = base.VisitLessThanExpression(lessThanExpression);
        CheckBinaryComparisonExpression(lessThanExpression);

        return lessThanExpression;
    }

    protected override LessThanOrEqual VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        lessThanOrEqualExpression = base.VisitLessThanOrEqualExpression(lessThanOrEqualExpression);
        CheckBinaryComparisonExpression(lessThanOrEqualExpression);

        return lessThanOrEqualExpression;
    }

    protected override Multiply VisitMultiplyExpression(Multiply multiplyExpression)
    {
        multiplyExpression = base.VisitMultiplyExpression(multiplyExpression);
        CheckMathExpression(multiplyExpression, "+");

        return multiplyExpression;
    }

    protected override Divide VisitDivideExpression(Divide divideExpression)
    {
        divideExpression = base.VisitDivideExpression(divideExpression);
        CheckMathExpression(divideExpression, "/");

        return divideExpression;
    }

    protected override NotEqual VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        notEqualExpression = base.VisitNotEqualExpression(notEqualExpression);
        CheckBinaryComparisonExpression(notEqualExpression);

        return notEqualExpression;
    }

    protected override Or VisitOrExpression(Or orExpression)
    {
        orExpression = base.VisitOrExpression(orExpression);
        CheckBooleanBinaryExpression(orExpression);

        return orExpression;
    }

    protected override Subtract VisitSubtractExpression(Subtract subtractExpression)
    {
        subtractExpression = base.VisitSubtractExpression(subtractExpression);
        CheckMathExpression(subtractExpression, "-");

        return subtractExpression;
    }
    
    private void MarkExpressionType(IExpression expression, IVariableType type)
    {
        if (expression.Type is not null)
        {
            throw ErrorHelper.ShowErrorMessage("Expression already had type marked.", expression.Span);
        }
        expression.Type = type;
    }
    
    private IVariableType? GetExpressionType(IExpression expression)
    {
        if (expression is FunctionInvocation { Type: null } inv)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot get value from void function {inv.Identifier}", inv.Span);
        }

        return expression.Type;
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
    
    private ScopeTracker<string, IVariableType> _variableTypeStack = null!;
    private ScopeTracker<string, FunctionSignature> _functionTypeStack = null!;
    private IVariableType? _currentFunctionReturnType;
    private bool _isInFunction;
    
    private void RecordVariableType(string identifier, IVariableType expressionType, SourceSpan span)
    {
        if (_variableTypeStack.ContainsKey(identifier))
        {
            throw ErrorHelper.ShowErrorMessage($"Identifier '{identifier}' already declared", span);
        }
    
        _variableTypeStack.SetValue(identifier, expressionType);
    }
    
    private IVariableType GetVariableType(string identifier, SourceSpan span)
    {
        if (!_variableTypeStack.ContainsKey(identifier))
        {
            throw ErrorHelper.ShowErrorMessage($"Unknown identifier '{identifier}'", span);
        }
    
        return _variableTypeStack.GetValue(identifier);
    }
    
    protected override VariableAccess VisitVariableAccess(VariableAccess variableAccess)
    {
        var type = GetVariableType(variableAccess.Identifier, variableAccess.Span);
        variableAccess.Type = type;

        return variableAccess;
    }

    private static readonly PrimitiveVariableType BoolType = new(PrimitiveVariableTypeEnum.Bool);
    private static readonly PrimitiveVariableType IntType = new(PrimitiveVariableTypeEnum.Int);
    private static readonly PrimitiveVariableType FloatType = new(PrimitiveVariableTypeEnum.Float);
    private static readonly PrimitiveVariableType StringType = new(PrimitiveVariableTypeEnum.String);
    private static readonly IVariableType[] MathTypes = [IntType, FloatType];
}