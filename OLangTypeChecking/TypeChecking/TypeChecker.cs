using AstHelpers;
using ErrorHelper;
using Lexing;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.Expressions;
using OLangAst.Statements;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;
using OLangHelpers;

namespace OLangTypeChecking.TypeChecking;

public class TypeChecker(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private ScopeTracker<string, IType> _variableTypeStack = null!;
    private ScopeTracker<string, FunctionSignature> _functionTypeStack = null!;
    private IType _currentFunctionReturnType;
    private DefinedType? _currentClass;
    private bool _isInInstanceMethod;
    
    public override Program VisitProgram(Program program)
    {
        _variableTypeStack = new ScopeTracker<string, IType>();
        _functionTypeStack = new ScopeTracker<string, FunctionSignature>();

        return base.VisitProgram(program);
    }

    protected override UsingStatement VisitUsingStatement(UsingStatement usingStatement)
    {
        typeHelper.LoadAssembly(usingStatement.Module);

        return usingStatement;
    }
    
    protected override ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentClass = typeHelper.GetDefinedType(classDeclaration.Identifier, null) as DefinedType ?? throw ErrorHelper.ShowErrorMessage($"Class `{classDeclaration.Identifier}` wasn't recognized", classDeclaration.Span);

        return (ClassDeclaration)base.VisitClassDeclaration(classDeclaration);
    }
    
    protected override EnumDeclaration VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        var type = typeHelper.GetDefinedType(enumDeclaration.Identifier, null) as DefinedType ?? throw ErrorHelper.ShowErrorMessage($"Enum `{enumDeclaration.Identifier}` wasn't recognized", enumDeclaration.Span);
        _currentClass = type;

        enumDeclaration = (EnumDeclaration)base.VisitEnumDeclaration(enumDeclaration);
        // foreach (var typeParameter in enumDeclaration.TypeParameters)
        // {
        //     type.ConcreteTypeParameters.Add(typeParameter);
        // }

        return enumDeclaration;
    }

    protected override Return VisitReturnStatement(Return returnStatement)
    {
        if (_currentFunctionReturnType.Equals(PrimitiveTypes.VoidType) && returnStatement.Value is not null)
        {
            throw ErrorHelper.ShowErrorMessage($"Void function cannot return a value.", returnStatement.Span);
        }
        returnStatement = base.VisitReturnStatement(returnStatement!);

        if (returnStatement.Value == null)
        {
            return returnStatement;
        }

        var expressionType = GetExpressionType(returnStatement.Value);
        returnStatement.Value = CastIfImplicitOrThrow(_currentFunctionReturnType, returnStatement.Value, $"Function of type {_currentFunctionReturnType} cannot return expression of type {expressionType}.", returnStatement.Value.Span);

        return returnStatement;
    }

    protected override ForLoop VisitForLoop(ForLoop forStatement)
    {
        BeginScope();
        forStatement.RangeStart = VisitExpression(forStatement.RangeStart);
        forStatement.RangeEnd = VisitExpression(forStatement.RangeEnd);
        var startType = GetExpressionType(forStatement.RangeStart);
        
        if (!startType.Equals(PrimitiveTypes.IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeStart.Span);
        }

        var endType = GetExpressionType(forStatement.RangeEnd);
        if (!endType.Equals(PrimitiveTypes.IntType))
        {
            throw ErrorHelper.ShowErrorMessage("Bounds of range must be integers", forStatement.RangeEnd.Span);
        }

        RecordVariableType(forStatement.Identifier, PrimitiveTypes.IntType, forStatement.Span);
        forStatement.Body.Statements = VisitStatements(forStatement.Body.Statements);
        EndScope();

        return forStatement;
    }

    protected override IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement = base.VisitIfStatement(ifStatement);
        var conditionType = GetExpressionType(ifStatement.Predicate);
        if (!conditionType.Equals(PrimitiveTypes.BoolType))
        {
            throw ErrorHelper.ShowErrorMessage("If predicate must be a boolean", ifStatement.Predicate.Span);
        }

        return ifStatement;
    }

    protected override WhileLoop VisitWhileLoop(WhileLoop whileStatement)
    {
        whileStatement = base.VisitWhileLoop(whileStatement);
        var whileConditionType = GetExpressionType(whileStatement.Predicate);
        if (!whileConditionType.Equals(PrimitiveTypes.BoolType))
        {
            throw ErrorHelper.ShowErrorMessage("If predicate must be a boolean", whileStatement.Predicate.Span);
        }

        return whileStatement;
    }

    protected override IStatement VisitVariableAssignment(VariableAssignment assignmentStatement)
    {
        if (_variableTypeStack.ContainsKey(assignmentStatement.Identifier))
        {
            assignmentStatement = (VariableAssignment)base.VisitVariableAssignment(assignmentStatement);
            var expressionType = GetExpressionType(assignmentStatement.Value);
            var variableType = GetVariableType(assignmentStatement.Identifier, assignmentStatement.Span);
            assignmentStatement.Value = CastIfImplicitOrThrow(variableType,
                assignmentStatement.Value,
                $"Cannot assign expression of type {expressionType} to variable {assignmentStatement.Identifier} of type {variableType}",
                assignmentStatement.Span);
        }
        
        if (_currentClass!.Fields.ContainsKey(assignmentStatement.Identifier))
        {
            var thisAccess = new ThisAccess();
            thisAccess = (ThisAccess)VisitThisAccess(thisAccess);
            var fieldAssignment = new FieldAssignment(thisAccess, assignmentStatement.Identifier, assignmentStatement.Value) { Span = assignmentStatement.Span };

            return VisitFieldAssignment(fieldAssignment);
        }

        return assignmentStatement;
    }
    
    protected override ArrayAssignment VisitArrayAssignment(ArrayAssignment assignmentStatement)
    {
        assignmentStatement = base.VisitArrayAssignment(assignmentStatement);
        var expressionType = GetExpressionType(assignmentStatement.Array) as DefinedArray;

        if (expressionType is null)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot index non-array type {expressionType}", assignmentStatement.Span);
        }

        var indexType = assignmentStatement.Index.Type!;
        if (!indexType.Equals(PrimitiveTypes.IntType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot use non-integer expression of type {indexType} as an index.", assignmentStatement.Index.Span);
        }

        var arrayType = typeHelper.GetInnerArrayType(assignmentStatement.Array);
        var valueType = assignmentStatement.Value.Type!;
        assignmentStatement.Value = CastIfImplicitOrThrow(valueType,
            assignmentStatement.Value,
            $"Cannot assign expression of type {valueType} to array of type {arrayType}",
            assignmentStatement.Span);

        return assignmentStatement;
    }

    protected override FieldAssignment VisitFieldAssignment(FieldAssignment fieldAssignment)
    {
        fieldAssignment = base.VisitFieldAssignment(fieldAssignment);
        var targetType = fieldAssignment.Target.Type!;

        if (targetType is DefinedType definedType &&  definedType.Fields.TryGetValue(fieldAssignment.Identifier, out _))
        {
            return fieldAssignment;
        }

        throw ErrorHelper.ShowErrorMessage($"Unrecognized member `{fieldAssignment.Identifier}` on expression of type `{targetType}`", fieldAssignment.Span);
    }

    protected override ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement = base.VisitExitStatement(exitStatement);
        var exitType = GetExpressionType(exitStatement.Expression);
        if (!exitType.Equals(PrimitiveTypes.IntType))
        {
            throw ErrorHelper.ShowErrorMessage($"Exit code has to be an integer, was {exitType}", exitStatement.Expression.Span);
        }

        return exitStatement;
    }
    
    protected override PrintStatement VisitPrintStatement(PrintStatement printStatement)
    {
        printStatement = base.VisitPrintStatement(printStatement);
        var expressionType = GetExpressionType(printStatement.Expression);
        if (!expressionType.Equals(PrimitiveTypes.StringType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot print non-string value, was {expressionType}", printStatement.Expression.Span);
        }

        return printStatement;
    }

    protected override IExpression VisitNotExpression(Not notExpression)
    {
        notExpression = (Not)base.VisitNotExpression(notExpression);
        var expType = GetExpressionType(notExpression);

        if (!expType.Equals(PrimitiveTypes.BoolType))
        {
            throw CannotApplyUnaryOperator("!", expType.ToString()!, notExpression.Span);
        }
        
        return notExpression;
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        var type = GetExpressionType(declarationStatement.Value);
        if (declarationStatement.DeclaredType is {} declaredType && !CanImplicitlyConvertTo(typeHelper.GetLocalType(declaredType, _currentClass), type))
        {
            throw ErrorHelper.ShowErrorMessage($"Expression type {type} does not match variable type {declarationStatement.DeclaredType}", declarationStatement.Span);
        }

        if (type.Equals(PrimitiveTypes.VoidType))
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot set variable to void value", declarationStatement.Span);
        }
    
        declarationStatement.VariableType = typeHelper.TryGetLocalType(declarationStatement.DeclaredType!, _currentClass) ?? type;
        declarationStatement.Value = MaybeImplicitCast(declarationStatement.VariableType!, declarationStatement.Value);
        RecordVariableType(declarationStatement.Identifier, declarationStatement.VariableType!, declarationStatement.Span);

        return declarationStatement;
    }

    private FunctionSignature GetSignatureFromFunctionDeclaration(FunctionDeclaration declaration)
    {
        return new FunctionSignature(typeHelper.GetMethodType(declaration.DeclaredType, _currentClass), declaration.Parameters.Select(x => typeHelper.GetLocalType(x.DeclaredType, _currentClass)));
    }

    protected override MethodDeclaration VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var typedParameters = methodDeclaration.Parameters.Select(x => new DeclaredParameter(x.Identifier, typeHelper.GetLocalType(x.DeclaredType, _currentClass))).ToList();
        methodDeclaration.Parameters.ForEach(x => x.Type = typeHelper.GetLocalType(x.DeclaredType, _currentClass));
        typeHelper.GetMethod(_currentClass!, methodDeclaration.Identifier, typedParameters.Select(x => x.Type).ToList(), methodDeclaration.Span);
    
        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, IType>();
    
        _variableTypeStack.SetValue("self", _currentClass!);
        foreach (var param in typedParameters)
        {
            _variableTypeStack.SetValue(param.Name, param.Type);
        }

        _isInInstanceMethod = methodDeclaration.IsInstance;
        var previousFunctionType = _currentFunctionReturnType;
        _currentFunctionReturnType = typeHelper.GetMethodType(methodDeclaration.DeclaredType, _currentClass);
    
        VisitScope(methodDeclaration.Scope);
        if (methodDeclaration.DeclaredType is not null)
        {
            CheckAllPathsReturnCorrectType(methodDeclaration.Scope);
        }
    
        _variableTypeStack = originalTypeStack;
        _currentFunctionReturnType = previousFunctionType;
    
        return methodDeclaration;
    }

    protected override IClassMember VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        _ = typeHelper.GetLocalType(fieldDeclaration.Type, _currentClass);

        return fieldDeclaration;
    }
    
    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var signature = GetSignatureFromFunctionDeclaration(functionDeclaration);
        _functionTypeStack.SetValue(functionDeclaration.Identifier, signature);
    
        var originalTypeStack = _variableTypeStack;
        _variableTypeStack = new ScopeTracker<string, IType>();
    
        foreach (var param in functionDeclaration.Parameters)
        {
            _variableTypeStack.SetValue(param.Identifier, typeHelper.GetLocalType(param.DeclaredType, _currentClass));
        }
    
        var previousFunctionType = _currentFunctionReturnType;
        _currentFunctionReturnType = GetReturnType(signature);
    
        VisitScope(functionDeclaration.Scope);
        if (functionDeclaration.DeclaredType is not null)
        {
            CheckAllPathsReturnCorrectType(functionDeclaration.Scope);
        }
    
        _variableTypeStack = originalTypeStack;
        _currentFunctionReturnType = previousFunctionType;
    
        return functionDeclaration;
    }

    private IType GetReturnType(FunctionSignature signature)
    {
        if (signature.ReturnType == null)
        {
            return PrimitiveTypes.VoidType;
        }
        return signature.ReturnType!;
    }

    protected override IExpression VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation.Arguments = functionInvocation.Arguments.Select(VisitExpression).ToList();
        if (!_functionTypeStack.ContainsKey(functionInvocation.Identifier))
        {
            var method = typeHelper.GetMethod(_currentClass!, functionInvocation.Identifier, functionInvocation.Arguments.Select(x => x.Type!).ToList(), functionInvocation.Span);

            var methodInvocation = new MethodInvocation(_currentClass!.Name, functionInvocation.Identifier, functionInvocation.Arguments) { Type = method.GetReturnType() };
            
            VisitMethodInvocationStatement(methodInvocation);

            return methodInvocation;
        }
        
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

        for (var i = 0; i < functionInvocation.Arguments.Count; i++)
        {
            var argument = functionInvocation.Arguments[i];
            var paramType = signature.ParameterTypes[i];
            functionInvocation.Arguments[i] = CastIfImplicitOrThrow(paramType,
                argument,
                $"Argument of type {argument.Type} passed to function '{functionInvocation.Identifier}' does not match declared parameter type {paramType}",
                argument.Span);
        }
        
        if (GetReturnType(signature) is { } returnType)
        {
            functionInvocation.Type = returnType;
        }

        return functionInvocation;
    }
    
    protected override MethodInvocation VisitMethodInvocation(MethodInvocation methodInvocation)
    {
        if (!methodInvocation.IsInstance)
        {
            if (typeHelper.GetDefinedType(methodInvocation.ClassName!, null) is not IDefinedType declaredClass)
            {
                throw ErrorHelper.ShowErrorMessage($"Unrecognized class name: `{methodInvocation.ClassName}`", methodInvocation.Span);
            }

            methodInvocation.SourceType = declaredClass;
        }
        
        methodInvocation = base.VisitMethodInvocation(methodInvocation);
        var targetType = methodInvocation.IsInstance ? methodInvocation.Expression?.Type : methodInvocation.SourceType!;
        if (targetType is not IDefinedType definedType)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot call methods on non-class type `{targetType}`.", methodInvocation.Span);
        }
        
        var functionDefinition = typeHelper.GetMethod(definedType, methodInvocation.Identifier, methodInvocation.Arguments.Select(x => x.Type!).ToList(), methodInvocation.Span);

        if (methodInvocation.IsInstance && !functionDefinition.IsInstance)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot call non-instance method `{methodInvocation.Identifier}` as instance method.", methodInvocation.Span);
        }
        
        if (!methodInvocation.IsInstance && functionDefinition.IsInstance)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot call instance method `{methodInvocation.Identifier}` as non-instance method.", methodInvocation.Span);
        }

        var parameterTypes = functionDefinition.GetParameterTypes();
        var paramCount = parameterTypes.Count;
        var argCount = methodInvocation.Arguments.Count;
        if (argCount != paramCount)
        {
            throw ErrorHelper.ShowErrorMessage($"Method '{methodInvocation.Identifier}' has {paramCount} parameters but is invoked with {argCount} arguments.", methodInvocation.Span);
        }

        for (var i = 0; i < methodInvocation.Arguments.Count; i++)
        {
            var argument = methodInvocation.Arguments[i];
            var paramType = parameterTypes[i];
            methodInvocation.Arguments[i] = CastIfImplicitOrThrow(paramType,
                argument,
                $"Argument of type `{argument.Type}` passed to function `{methodInvocation.Identifier}` does not match declared parameter type `{paramType}`",
                argument.Span);
        }

        methodInvocation.Type = functionDefinition.GetReturnType();

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
        var expType = GetExpressionType(negate);

        if (!IsMathType(expType))
        {
            throw CannotApplyUnaryOperator("-", expType.ToString()!, negate.Span);
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

        if (!lhsType.Equals(PrimitiveTypes.BoolType) || !rhsType.Equals(PrimitiveTypes.BoolType))
        {
            throw ErrorHelper.ShowErrorMessage($"Both sides of expression must be of type {PrimitiveVariableTypeEnum.Bool}, was {lhsType} and {rhsType}.", booleanBinaryExpression.Span);
        }
    }
    
    private void CheckBinaryComparisonExpression<T>(T binaryComparisonExpression) where T : BaseBinaryExpression
    {
        var lhsType = GetExpressionType(binaryComparisonExpression.Lhs);
        var rhsType = GetExpressionType(binaryComparisonExpression.Rhs);

        var resultingType = GetCommonType(lhsType, rhsType);
        if (resultingType == null)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot compare expressions of type {lhsType} and {rhsType}", binaryComparisonExpression.Span);
        }

        binaryComparisonExpression.Lhs = MaybeImplicitCast(lhsType, binaryComparisonExpression.Lhs);
        binaryComparisonExpression.Rhs = MaybeImplicitCast(lhsType, binaryComparisonExpression.Rhs);
    }

    private bool CanImplicitlyConvertTo(IType expectedType, IType actualType)
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
        if (resultingType is not {} definedType)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot apply operator `{oper}` to expressions of type {lhsType} and {rhsType}", mathExpression.Span);
        }

        mathExpression.Lhs = MaybeImplicitCast(definedType, mathExpression.Lhs);
        mathExpression.Rhs = MaybeImplicitCast(definedType, mathExpression.Rhs);
        
        MarkExpressionType(mathExpression, definedType);
    }

    private IExpression CastIfImplicitOrThrow(IType type, IExpression expression, string errorMessage, SourceSpan span)
    {
        if (CanImplicitlyConvertTo(type, expression.Type!))
        {
            return MaybeImplicitCast(type, expression);
        }

        throw ErrorHelper.ShowErrorMessage(errorMessage, span);
    }

    private IExpression MaybeImplicitCast(IType type, IExpression expression)
    {
        if (expression.Type!.Equals(type))
        {
            return expression;
        }

        return new Cast(new InternalDefinedType(type), expression) { Type = type };
    }

    private IType? GetCommonType(IType type1, IType type2)
    {
        if (type1.Equals(type2))
        {
            return type1;
        }

        if (IsMathType(type1) && IsMathType(type2))
        {
            return ResultingTypeFromMath(type1, type2);
        }

        return null;
    }

    private IType? ResultingTypeFromMath(IType type1, IType type2)
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
        if (addExpression.Lhs.Type!.Equals(PrimitiveTypes.StringType) && addExpression.Rhs.Type!.Equals(PrimitiveTypes.StringType))
        {
            MarkExpressionType(addExpression, PrimitiveTypes.StringType);
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
        var expType = cast.Value.Type!;
        var targetType = typeHelper.GetLocalType(cast.TargetType, _currentClass);
        if ((IsMathType(targetType) && IsMathType(expType)) || targetType.Equals(expType))
        {
            cast.Type = typeHelper.GetLocalType(cast.TargetType, _currentClass);
            return cast;
        }
        
        throw ErrorHelper.ShowErrorMessage($"Cannot cast expression of type `{expType}` to `{cast.TargetType}`.", cast.Span);
    }

    protected override IExpression VisitInstantiation(Instantiation instantiation)
    {
        var type = (typeHelper.GetDefinedType(instantiation.ClassName, null)
                    ?? throw ErrorHelper.ShowErrorMessage($"Unrecognized type `{instantiation.ClassName}`.", instantiation.Span)) as IDefinedType
                    ?? throw ErrorHelper.ShowErrorMessage($"Cannot instantiate generic type `{instantiation.ClassName}`.", instantiation.Span);
        
        instantiation.Type = type;
        instantiation = (Instantiation)base.VisitInstantiation(instantiation);

        var fields = GetFields(type);
        var missingInitializations = fields.Keys.Except(instantiation.FieldInitializations.Select(x => x.Key));

        if (missingInitializations.FirstOrDefault() is { } missingField)
        {
            throw ErrorHelper.ShowErrorMessage($"Member `{missingField}` on type `{type}` was not initialized.", instantiation.Span);
        }
        
        foreach (var (fieldName, value) in instantiation.FieldInitializations)
        {
            if (!fields.TryGetValue(fieldName, out var fieldType))
            {
                throw ErrorHelper.ShowErrorMessage($"Class `{type.Name}` does not have a field named `{fieldName}`.", value.Span);
            }

            if (!CanImplicitlyConvertTo(fieldType, value.Type!))
            {
                throw ErrorHelper.ShowErrorMessage($"Expression of type `{value.Type!}` cannot implicitly convert to type `{fieldType}`", value.Span);
            }
        }

        return instantiation;
    }

    private Dictionary<string, IType> GetFields(IDefinedType type)
    {
        return type switch
        {
            ConcreteType concreteType => new Dictionary<string, IType>(concreteType.Fields.Select(x => new KeyValuePair<string, IType>(x.Key, x.Value.Type))),
            DefinedType definedType => new Dictionary<string, IType>(definedType.Fields.Select(x => new KeyValuePair<string, IType>(x.Key, x.Value.Type))),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    protected override IExpression VisitArrayInstantiation(ArrayInstantiation instantiation)
    {
        instantiation = (ArrayInstantiation)base.VisitArrayInstantiation(instantiation);
        if (!instantiation.Size.Type!.Equals(PrimitiveTypes.IntType))
        {
            throw ErrorHelper.ShowErrorMessage($"Size of array must be int, found type `{instantiation.Size.Type}`.", instantiation.Span);
        }
        
        instantiation.Type = typeHelper.GetLocalType(instantiation.ArrayType, _currentClass);
        
        return instantiation;
    }
    
    protected override IExpression VisitArrayAccess(ArrayAccess arrayAccess)
    {
        arrayAccess = (ArrayAccess)base.VisitArrayAccess(arrayAccess);

        arrayAccess.Type = typeHelper.GetInnerArrayType(arrayAccess.Array);
        
        return arrayAccess;
    }

    protected override IExpression VisitFieldAccess(FieldAccess fieldAccess)
    {
        fieldAccess = (FieldAccess)base.VisitFieldAccess(fieldAccess);

        var type = fieldAccess.Expression.Type;
        if (type is not IDefinedType definedType)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot access fields from non-class type `{type}`", fieldAccess.Span);
        }

        if (!GetFields(definedType).TryGetValue(fieldAccess.FieldName, out var fieldType))
        {
            throw ErrorHelper.ShowErrorMessage($"Unrecognized member `{fieldAccess.FieldName}` on expression of type `{type}`", fieldAccess.Span);
        }
        
        fieldAccess.Type = fieldType;

        return fieldAccess;

    }


    private void MarkExpressionType(IExpression expression, IType type)
    {
        if (expression.Type is not null)
        {
            throw ErrorHelper.ShowErrorMessage("Expression already had type marked.", expression.Span);
        }
        expression.Type = type;
    }
    
    private IType GetExpressionType(IExpression expression)
    {
        if (expression is FunctionInvocation { Type: null } inv)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot get value from void function {inv.Identifier}", inv.Span);
        }

        return expression.Type!;
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
    
    private void RecordVariableType(string identifier, IType expressionType, SourceSpan span)
    {
        if (_variableTypeStack.ContainsKey(identifier))
        {
            throw ErrorHelper.ShowErrorMessage($"Identifier '{identifier}' already declared", span);
        }
    
        _variableTypeStack.SetValue(identifier, expressionType);
    }
    
    private IType GetVariableType(string identifier, SourceSpan span)
    {
        if (_variableTypeStack.ContainsKey(identifier))
        {
            return _variableTypeStack.GetValue(identifier);
        }
        
        if (_currentClass!.Fields.TryGetValue(identifier, out var field))
        {
            return field.Type;
        }
        
        throw ErrorHelper.ShowErrorMessage($"Unknown identifier '{identifier}'", span);
    }

    protected override IExpression VisitVariableAccess(VariableAccess variableAccess)
    {
        if (_variableTypeStack.ContainsKey(variableAccess.Identifier))
        {
            var type = GetVariableType(variableAccess.Identifier, variableAccess.Span);
            variableAccess.Type = type;

            return variableAccess;
        }
        
        throw ErrorHelper.ShowErrorMessage($"Unknown identifier '{variableAccess.Identifier}'", variableAccess.Span);
    }

    protected override IExpression VisitThisAccess(ThisAccess thisAccess)
    {
        thisAccess.Type = _currentClass!;

        return thisAccess;
    }

    protected override IExpression VisitEnumInstantiation(EnumInstantiation enumInstantiation)
    {
        enumInstantiation = (EnumInstantiation)base.VisitEnumInstantiation(enumInstantiation);
        var type = typeHelper.GetDefinedType(enumInstantiation.EnumName, null)!;
        if (type is not IDefinedType definedType || definedType.TypeVariant != TypeVariant.Enum)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot instantiate class `{enumInstantiation.EnumName}` as enum.", enumInstantiation.Span);
        }

        var enumVariant = CheckEnumVariantExists(enumInstantiation.VariantName, definedType, enumInstantiation.Span);
        var paramTypes = enumVariant switch
        {
            ConcreteEnumVariant concreteEnumVariant => concreteEnumVariant.Parameters.Cast<IType>().ToList(),
            DefinedEnumVariant definedEnumVariant => definedEnumVariant.Parameters,
            _ => throw new ArgumentOutOfRangeException(nameof(enumVariant))
        };

        var args = enumInstantiation.Arguments;

        if (args.Count != paramTypes.Count)
        {
            throw ErrorHelper.ShowErrorMessage($"Enum variant `{enumInstantiation.EnumName}::{enumInstantiation.VariantName}` expected {paramTypes.Count} arguments, found {args.Count}.", enumInstantiation.Span);
        }

        enumInstantiation.Arguments = args.Zip(paramTypes)
            .Select(x => CastIfImplicitOrThrow
                (
                    x.Second,
                    x.First,
                    $"Argument to enum variant `{enumInstantiation.EnumName}::{enumInstantiation.VariantName}` of type `{x.First.Type!}` cannot implicitly convert to defined parameter of type `{x.Second}`.",
                    x.First.Span
                )
            )
            .ToList();
        
        enumInstantiation.Type = definedType;

        return enumInstantiation;
    }

    private IEnumVariant CheckEnumVariantExists(string variantName, IDefinedType definedType, SourceSpan span)
    {
        var variant = TryGetEnumVariant(variantName, definedType);
        if (variant is null)
        {
            throw ErrorHelper.ShowErrorMessage($"Enum `{definedType.Name}` does not contain a variant named `{variantName}`.", span);
        }

        return variant;
    }

    private IEnumVariant? TryGetEnumVariant(string variantName, IDefinedType definedType)
    {
        return definedType switch
        {
            ConcreteType concreteType => concreteType.EnumVariants.GetValueOrDefault(variantName),
            DefinedType definedType1 => definedType1.EnumVariants.GetValueOrDefault(variantName),
            _ => throw new ArgumentOutOfRangeException(nameof(definedType))
        };
    }

    protected override IExpression VisitMatchExpression(MatchExpression matchExpression)
    {
        matchExpression.MatchTarget = VisitExpression(matchExpression.MatchTarget);

        var matchTargetType = matchExpression.MatchTarget.Type! as DefinedType;
        if (matchTargetType == null || matchTargetType.TypeVariant != TypeVariant.Enum)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot match expression of non-enum type `{matchTargetType}`.", matchExpression.MatchTarget.Span);
        }

        foreach (var arm in matchExpression.MatchArms)
        {
            var enumVariant = CheckEnumVariantExists(arm.VariantName, matchTargetType, arm.Span);
            var paramTypes = enumVariant switch
            {
                ConcreteEnumVariant concreteEnumVariant => concreteEnumVariant.Parameters.Cast<IType>().ToList(),
                DefinedEnumVariant definedEnumVariant => definedEnumVariant.Parameters,
                _ => throw new ArgumentOutOfRangeException(nameof(enumVariant))
            };

            var enumParameterCount = paramTypes.Count;
            var destructureVariableCount = arm.DestructureVariables.Count;
            if (enumParameterCount != destructureVariableCount)
            {
                throw ErrorHelper.ShowErrorMessage($"Enum destructure had {destructureVariableCount} variables, expected {enumParameterCount};", arm.Span);
            }
            
            BeginScope();
            
            foreach (var (variable, type) in arm.DestructureVariables.Zip(paramTypes))
            {
                RecordVariableType(variable, type, arm.Span);
            }

            arm.Value = VisitExpression(arm.Value);
            
            EndScope();
        }

        matchExpression.Type = GetCommonType(matchExpression.MatchArms.Select(x => x.Value.Type!));
        if (matchExpression.Type is not {} commonType)
        {
            throw ErrorHelper.ShowErrorMessage("Not all branches of match expression can implicitly convert to a common type.", matchExpression.Span);
        }

        matchExpression.MatchArms.ForEach(x => x.Value = MaybeImplicitCast(commonType, x.Value));
        
        return matchExpression;
    }

    protected override IExpression VisitSelfAccess(SelfAccess selfAccess)
    {
        if (!_isInInstanceMethod)
        {
            throw ErrorHelper.ShowErrorMessage($"Cannot access `self` outside of an instance method.", selfAccess.Span);
        }
        
        selfAccess.Type = _currentClass!;

        return selfAccess;
    }

    private IType? GetCommonType(IEnumerable<IType> types)
    {
        IType? matchExpressionType = null;
        foreach (var type in types)
        {
            if (matchExpressionType is not {} currentType)
            {
                matchExpressionType = type;
                continue;
            }

            if (currentType.Equals(type) || CanImplicitlyConvertTo(currentType, type))
            {
                continue;
            }
            
            if (CanImplicitlyConvertTo(type, currentType))
            {
                matchExpressionType = type;
                continue;
            }

            return null;
        }

        return matchExpressionType;
    }

    protected override MatchArm VisitMatchArm(MatchArm matchArm)
    {
        return matchArm;
    }

    private int? GetMathTypePrecedence(IType type)
    {
        if (type is not ConcreteType concreteType)
        {
            return null;
        }
        
        var idx = MathTypes.IndexOf(concreteType);
        return idx == -1 ? null : idx;
    }

    private bool IsMathType(IType type)
    {
        return MathTypes.Contains(type);
    }

    private static readonly List<ConcreteType> MathTypes = [PrimitiveTypes.IntType, PrimitiveTypes.FloatType];
}