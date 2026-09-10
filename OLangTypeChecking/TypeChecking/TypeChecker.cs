using AstHelpers;
using ErrorHelper;
using Lexing;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.Expressions;
using OLangAst.Statements;
using OLangAst.Miscellaneous;
using OLangHelpers;

namespace OLangTypeChecking.TypeChecking;

public class TypeChecker(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private ScopeTracker<string, IVariableType> _variableTypeStack = null!;
    private ScopeTracker<string, FunctionSignature> _functionTypeStack = null!;
    private IVariableType? _currentFunctionReturnType;
    private CustomClass? _currentClass;
    private bool _isInFunction;
    
    public override Program VisitProgram(Program program)
    {
        _variableTypeStack = new ScopeTracker<string, IVariableType>();
        _functionTypeStack = new ScopeTracker<string, FunctionSignature>();

        return base.VisitProgram(program);
    }

    protected override ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentClass = typeHelper.GetCustomClass(classDeclaration.Identifier) ?? throw ErrorHelper.ShowErrorMessage($"Class `{classDeclaration.Identifier}` wasn't recognized", classDeclaration.Span);;

        return base.VisitClassDeclaration(classDeclaration);
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
        
        if (!startType.Equals(PrimitiveVariableType.IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeStart.Span);
        }

        var endType = GetExpressionType(forStatement.RangeEnd);
        if (!endType.Equals(PrimitiveVariableType.IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeEnd.Span);
        }

        RecordVariableType(forStatement.Identifier, PrimitiveVariableType.IntType, forStatement.Span);
        forStatement.Body.Statements = VisitStatements(forStatement.Body.Statements);
        EndScope();

        return forStatement;
    }

    protected override IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement = base.VisitIfStatement(ifStatement);
        var conditionType = GetExpressionType(ifStatement.Predicate);
        if (!conditionType.Equals(PrimitiveVariableType.BoolType))
        {
            throw ErrorHelper.ShowErrorMessage("If predicate must be a boolean", ifStatement.Predicate.Span);
        }

        return ifStatement;
    }

    protected override WhileLoop VisitWhileLoop(WhileLoop whileStatement)
    {
        whileStatement = base.VisitWhileLoop(whileStatement);
        var whileConditionType = GetExpressionType(whileStatement.Predicate);
        if (!whileConditionType.Equals(PrimitiveVariableType.BoolType))
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
        if (!exitType.Equals(PrimitiveVariableType.IntType))
        {
            throw ErrorHelper.ShowErrorMessage($"Exit code has to be an integer, was {exitType}", exitStatement.Expression.Span);
        }

        return exitStatement;
    }
    
    protected override PrintStatement VisitPrintStatement(PrintStatement printStatement)
    {
        printStatement = base.VisitPrintStatement(printStatement);
        var expressionType = GetExpressionType(printStatement.Expression);
        if (!expressionType.Equals(PrimitiveVariableType.StringType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot print non-string value, was {expressionType}", printStatement.Expression.Span);
        }

        return printStatement;
    }

    protected override IExpression VisitNotExpression(Not notExpression)
    {
        notExpression = (Not)base.VisitNotExpression(notExpression);
        var expType = GetExpressionType(notExpression.Value);

        if (!expType.Equals(PrimitiveVariableType.BoolType))
        {
            throw CannotApplyUnaryOperator("!", expType.ToString(), notExpression.Span);
        }
        
        return notExpression;
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        var type = GetExpressionType(declarationStatement.Value);
        if (declarationStatement.Type != null && !CanImplicitlyConvertTo(declarationStatement.Type, type))
        {
            throw ErrorHelper.ShowErrorMessage($"Expression type {type} does not match variable type {declarationStatement.Type}", declarationStatement.Value.Span);
        }
    
        declarationStatement.Type ??= type;
        declarationStatement.Value = MaybeImplicitCast(declarationStatement.Type, declarationStatement.Value);
        RecordVariableType(declarationStatement.Identifier, declarationStatement.Type, declarationStatement.Span);

        return declarationStatement;
    }

    private static FunctionSignature GetTypeFromFunctionDeclaration(FunctionDeclaration declaration)
    {
        return new FunctionSignature(declaration.Type, declaration.Parameters.Select(x => x.Type));
    }

    protected override MethodDeclaration VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        typeHelper.GetMethod(_currentClass!, methodDeclaration.Identifier, methodDeclaration.Parameters.Select(x => x.Type).ToList());
    
        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, IVariableType>();
    
        foreach (var param in methodDeclaration.Parameters)
        {
            _variableTypeStack.SetValue(param.Identifier, param.Type);
        }
    
        var wasInFunction = _isInFunction;
        _isInFunction = true;
        var previousFunctionType = _currentFunctionReturnType;
        _currentFunctionReturnType = methodDeclaration.Type;
    
        VisitScope(methodDeclaration.Scope);
        if (methodDeclaration.Type is not null)
        {
            CheckAllPathsReturnCorrectType(methodDeclaration.Scope);
        }
    
        _variableTypeStack = originalTypeStack;
        _currentFunctionReturnType = previousFunctionType;
    
        _isInFunction = wasInFunction;
    
        return methodDeclaration;
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
            CheckAllPathsReturnCorrectType(functionDeclaration.Scope);
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

    protected override IExpression VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        if (!_functionTypeStack.ContainsKey(functionInvocation.Identifier))
        {
            typeHelper.GetMethod(_currentClass, functionInvocation.Identifier, functionInvocation.Arguments.Select(x => x.Type).ToList()); // just to throw if the method doesn't exist

            var methodInvocation = new MethodInvocation(null, functionInvocation.Identifier, functionInvocation.Arguments);
            
            VisitMethodInvocationStatement(methodInvocation);

            return methodInvocation;
        }
        
        functionInvocation = (FunctionInvocation)base.VisitFunctionInvocation(functionInvocation);
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
    
    protected override MethodInvocation VisitMethodInvocation(MethodInvocation methodInvocation)
    {
        methodInvocation = base.VisitMethodInvocation(methodInvocation);
        var methodInfo = typeHelper.GetMethod(methodInvocation.Expression?.Type ?? _currentClass, methodInvocation.Identifier, methodInvocation.Arguments.Select(x => x.Type).ToList())
                         ?? throw ErrorHelper.ShowErrorMessage($"Cannot resolve method `{methodInvocation.Identifier}`", methodInvocation.Span);

        var paramCount = methodInfo.GetParameters().Length;
        var argCount = methodInvocation.Arguments.Count;
        if (argCount != paramCount)
        {
            throw ErrorHelper.ShowErrorMessage($"Function '{methodInvocation.Identifier}' has {paramCount} parameters but is invoked with {argCount} arguments.", methodInvocation.Span);
        }

        foreach (var (argument, paramType) in methodInvocation.Arguments.Zip(methodInfo.GetParameters().Select(x => x.ParameterType)))
        {
            if (typeHelper.GetCsType(argument.Type) != paramType)
            {
                throw ErrorHelper.ShowErrorMessage($"Argument of type {argument.Type} passed to function '{methodInvocation.Identifier}' does not match declared parameter type {paramType}", argument.Span);
            }
        }

        if (methodInfo.ReturnType != typeof(void))
        {
            methodInvocation.Type = typeHelper.GetLocalType(methodInfo.ReturnType);
        }

        return methodInvocation;
    }
    
    private void CheckAllPathsReturnCorrectType(Scope scope)
    {
        if (!DoesScopeAlwaysReturn(scope))
        {
            throw ErrorHelper.ShowErrorMessage("Not all function paths return a value", scope.Span);
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
    
    protected override IExpression VisitNegate(Negate negate)
    {
        negate = (Negate)base.VisitNegate(negate);
        var expType = GetExpressionType(negate.Value);

        if (!IsMathType(expType))
        {
            throw CannotApplyUnaryOperator("-", expType.ToString(), negate.Span);
        }
        
        MarkExpressionType(negate, expType);

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

        if (!lhsType.Equals(PrimitiveVariableType.BoolType) || !rhsType.Equals(PrimitiveVariableType.BoolType))
        {
            throw ErrorHelper.ShowErrorMessage($"Both sides of expression must be of type {PrimitiveVariableTypeEnum.Bool}, was {lhsType} and {rhsType}.", booleanBinaryExpression.Span);
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

    private bool CanImplicitlyConvertTo(IVariableType expectedType, IVariableType actualType)
    {
        if (expectedType.Equals(actualType))
        {
            return true;
        }
        
        var expectedPrecedence = GetMathTypePrecedence(expectedType);
        var actualPrecedence = GetMathTypePrecedence(actualType);

        if (expectedPrecedence == null || actualPrecedence == null)
        {
            return false;
        }

        return expectedPrecedence >= actualPrecedence;
    }
    
    private void CheckMathExpression<T>(T mathExpression, string oper) where T : BaseBinaryExpression
    {
        var lhsType = GetExpressionType(mathExpression.Lhs);
        var rhsType = GetExpressionType(mathExpression.Rhs);

        var resultingType = ResultingTypeFromMath(lhsType, rhsType);
        if (resultingType is null)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot apply operator `{oper}` to expressions of type {lhsType} and {rhsType}", mathExpression.Span);
        }

        mathExpression.Lhs = MaybeImplicitCast(resultingType, mathExpression.Lhs);
        mathExpression.Rhs = MaybeImplicitCast(resultingType, mathExpression.Rhs);
        
        MarkExpressionType(mathExpression, resultingType);
    }

    private IExpression MaybeImplicitCast(IVariableType type, IExpression expression)
    {
        if (expression.Type.Equals(type))
        {
            return expression;
        }

        return new Cast(type, expression);
    }

    private IVariableType? ResultingTypeFromMath(IVariableType type1, IVariableType type2)
    {
        var t1Precedence = GetMathTypePrecedence(type1);
        var t2Precedence = GetMathTypePrecedence(type2);
        if (t1Precedence is not {} t1PrecedenceValue || t2Precedence is not {} t2PrecedenceValue)
        {
            return null;
        }

        if (type1.Equals(type2))
        {
            return type1;
        }

        return MathTypes[Math.Max(t1PrecedenceValue, t2PrecedenceValue)];
    }

    protected override IExpression VisitAddExpression(Add addExpression)
    {
        addExpression = (Add)base.VisitAddExpression(addExpression);
        if (addExpression.Lhs.Type.Equals(PrimitiveVariableType.StringType) && addExpression.Rhs.Type.Equals(PrimitiveVariableType.StringType))
        {
            MarkExpressionType(addExpression, PrimitiveVariableType.StringType);
        }
        else
        {
            CheckMathExpression(addExpression, "+");
        }

        return addExpression;
    }
    
    protected override IExpression VisitAndExpression(And andExpression)
    {
        andExpression = (And)base.VisitAndExpression(andExpression);
        CheckBooleanBinaryExpression(andExpression);

        return andExpression;
    }
    
    protected override IExpression VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        areEqualExpression = (AreEqual)base.VisitAreEqualExpression(areEqualExpression);
        CheckBinaryComparisonExpression(areEqualExpression);

        return areEqualExpression;
    }

    protected override IExpression VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        greaterOrEqualExpression = (GreaterOrEqual)base.VisitGreaterOrEqualExpression(greaterOrEqualExpression);
        CheckBinaryComparisonExpression(greaterOrEqualExpression);

        return greaterOrEqualExpression;
    }

    protected override IExpression VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        greaterThanExpression = (GreaterThan)base.VisitGreaterThanExpression(greaterThanExpression);
        CheckBinaryComparisonExpression(greaterThanExpression);

        return greaterThanExpression;
    }

    protected override IExpression VisitLessThanExpression(LessThan lessThanExpression)
    {
        lessThanExpression = (LessThan)base.VisitLessThanExpression(lessThanExpression);
        CheckBinaryComparisonExpression(lessThanExpression);

        return lessThanExpression;
    }

    protected override IExpression VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        lessThanOrEqualExpression = (LessThanOrEqual)base.VisitLessThanOrEqualExpression(lessThanOrEqualExpression);
        CheckBinaryComparisonExpression(lessThanOrEqualExpression);

        return lessThanOrEqualExpression;
    }

    protected override IExpression VisitMultiplyExpression(Multiply multiplyExpression)
    {
        multiplyExpression = (Multiply)base.VisitMultiplyExpression(multiplyExpression);
        CheckMathExpression(multiplyExpression, "*");

        return multiplyExpression;
    }

    protected override IExpression VisitDivideExpression(Divide divideExpression)
    {
        divideExpression = (Divide)base.VisitDivideExpression(divideExpression);
        CheckMathExpression(divideExpression, "/");

        return divideExpression;
    }

    protected override IExpression VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        notEqualExpression = (NotEqual)base.VisitNotEqualExpression(notEqualExpression);
        CheckBinaryComparisonExpression(notEqualExpression);

        return notEqualExpression;
    }

    protected override IExpression VisitOrExpression(Or orExpression)
    {
        orExpression = (Or)base.VisitOrExpression(orExpression);
        CheckBooleanBinaryExpression(orExpression);

        return orExpression;
    }

    protected override IExpression VisitSubtractExpression(Subtract subtractExpression)
    {
        subtractExpression = (Subtract)base.VisitSubtractExpression(subtractExpression);
        CheckMathExpression(subtractExpression, "-");

        return subtractExpression;
    }
    
    protected override IExpression VisitCast(Cast cast)
    {
        cast = (Cast)base.VisitCast(cast);
        var expType = cast.Value.Type;
        if (IsMathType(cast.TargetType) && IsMathType(expType) || cast.TargetType.Equals(expType))
        {
            return cast;
        }
        
        throw ErrorHelper.ShowErrorMessage($"Cannot cast expression of type `{expType}` to `{cast.TargetType}`.", cast.Span);
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

    private int? GetMathTypePrecedence(IVariableType type)
    {
        if (type is not PrimitiveVariableType prim)
        {
            throw ErrorHelper.UnknownVariant("math type", type.GetType());
        }

        var idx = MathTypes.IndexOf(prim);
        return idx == -1 ? null : idx;
    }

    private bool IsMathType(IVariableType type)
    {
        return MathTypes.Contains(type);
    }

    private static readonly List<IVariableType> MathTypes = [PrimitiveVariableType.IntType, PrimitiveVariableType.FloatType];
}