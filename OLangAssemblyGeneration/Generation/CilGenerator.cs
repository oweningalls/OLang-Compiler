using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangHelpers;
using Parameter = OLangAst.Miscellaneous.Parameter;

namespace AssemblyGeneration.Generation;

public class CilGenerator(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper), IGenerator
{
    private ILGenerator _il;
    private TypeBuilder _type;

    private Dictionary<string, MethodBuilder> _functions;
    private ScopeTracker<string, LocalBuilder> _variableTracker = null!;

    private ScopeTracker<string, int> _parameterTracker;

    public void GenerateProgram(Program program, string filePath, string fileName)
    {
        _variableTracker = new ScopeTracker<string, LocalBuilder>();
        _parameterTracker = new ScopeTracker<string, int>();
        _functions = [];
        
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(fileName), typeof(object).Assembly);
        var module = assembly.DefineDynamicModule("OLangProgram");
        _type = module.DefineType("Program");

        VisitProgram(program);

        if (!_functions.TryGetValue("Main", out var main))
        {
            throw ErrorHelper.ShowErrorMessage("Program must contain a method named 'Main'", program.Span);
        }
        GenerateAssemblyFile(filePath, fileName, assembly, main);
        WriteRuntimeConfigFile(filePath, Path.GetFileNameWithoutExtension(fileName));
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        GenerateMethod(methodDeclaration.Identifier, MethodAttributes.Private | MethodAttributes.Static, methodDeclaration.Type == null ? typeof(void) : typeHelper.GetCsType(methodDeclaration.Type), methodDeclaration.Parameters, methodDeclaration.Scope.Statements);

        return methodDeclaration;
    }

    private void GenerateAssemblyFile(string filePath, string fileName, PersistedAssemblyBuilder assembly, MethodBuilder main)
    {
        _type.CreateType();
        var metadata = assembly.GenerateMetadata(out var ilStream, out var fieldData);
        var peHeader = new PEHeaderBuilder(
            imageCharacteristics: Characteristics.ExecutableImage | Characteristics.LargeAddressAware
        );

        var peBuilder = new ManagedPEBuilder(
            header: peHeader,
            metadataRootBuilder: new MetadataRootBuilder(metadata),
            ilStream: ilStream,
            mappedFieldData: fieldData,
            entryPoint: MetadataTokens.MethodDefinitionHandle(
                main.MetadataToken
            )
        );
        using var stream = File.Create(Path.Combine(filePath, fileName));

        var blob = new BlobBuilder();

        peBuilder.Serialize(blob);
        blob.WriteContentTo(stream);
    }

    private const string RuntimeConfigTemplate = "Assets/runtimeconfigtemplate.json";
    
    private void WriteRuntimeConfigFile(string targetFilePath, string assemblyName)
    {
        
        var sourcePath = FindConfigLocation();
        var targetPath = Path.Combine(targetFilePath, $"{assemblyName}.runtimeconfig.json");
        File.Copy(sourcePath, targetPath, true);
    }

    private string FindConfigLocation()
    {
        var assemblyPath = typeof(CilGenerator).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
        
        return Path.Combine(assemblyDir, RuntimeConfigTemplate);
    }

    private MethodBuilder GenerateMethod(string name, MethodAttributes attributes, Type? retType, List<Parameter> parameters, List<IStatement> statements)
    {
        var method = _type.DefineMethod(name, attributes, retType, parameters.Select(x => typeHelper.GetCsType(x.Type)).ToArray());
        _functions[name] = method;
        
        var originalIl = _il;
        _il = method.GetILGenerator();

        BeginScope();
        for (var i = 0; i < parameters.Count; i++)
        {
            SaveParameter(parameters[i], i);
        }

        VisitStatements(statements);
        _il.Emit(OpCodes.Ret);

        EndScope();
        _il = originalIl;

        return method;
    }

    protected override ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement = base.VisitExitStatement(exitStatement);
        WriteExit();

        return exitStatement;
    }

    private static readonly MethodInfo ExitMethod = typeof(Environment).GetMethod(nameof(Environment.Exit), [typeof(int)])!;
    
    private void WriteExit(int? value = null)
    {
        if (value is { } num)
        {
            _il.Emit(OpCodes.Ldc_I4, num);
        }

        _il.Emit(OpCodes.Call, ExitMethod);
    }
    
    private static readonly MethodInfo PrintMethod = typeof(Console).GetMethod(nameof(Console.Write), [typeof(string)])!;
    
    protected override PrintStatement VisitPrintStatement(PrintStatement printStatement)
    {
        printStatement = base.VisitPrintStatement(printStatement);
        _il.Emit(OpCodes.Call, PrintMethod);

        return printStatement;
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        var local = _il.DeclareLocal(typeHelper.GetCsType(declarationStatement.Type!));
        _variableTracker.SetValue(declarationStatement.Identifier, local);
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        EmitLocalSet(declarationStatement.Identifier);

        return declarationStatement;
    }

    protected override VariableAssignment VisitVariableAssignment(VariableAssignment assignment)
    {
        assignment = base.VisitVariableAssignment(assignment);
        EmitLocalSet(assignment.Identifier);

        return assignment;
    }

    protected override IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement.Predicate = VisitExpression(ifStatement.Predicate);
        var ifEndLabel = _il.DefineLabel();

        _il.Emit(OpCodes.Brfalse, ifEndLabel);

        ifStatement.Body = VisitScope(ifStatement.Body);
        if (ifStatement.Else is { } elseScope)
        {
            var elseEndLabel = _il.DefineLabel();
            _il.Emit(OpCodes.Br, elseEndLabel);

            EmitLabel(ifEndLabel);
            ifStatement.Else = VisitScope(elseScope);

            EmitLabel(elseEndLabel);
        }
        else
        {
            EmitLabel(ifEndLabel);
        }

        return ifStatement;
    }

    protected override WhileLoop VisitWhileLoop(WhileLoop @while)
    {
        var whileBegin = _il.DefineLabel();
        var whileEnd = _il.DefineLabel();

        EmitLabel(whileBegin);

        @while.Predicate = VisitExpression(@while.Predicate);
        _il.Emit(OpCodes.Brfalse, whileEnd);

        @while.Body = VisitScope(@while.Body);

        _il.Emit(OpCodes.Br, whileBegin);
        EmitLabel(whileEnd);

        return @while;
    }

    protected override ForLoop VisitForLoop(ForLoop @for)
    {
        BeginScope();

        var declaration = new VariableDeclarationStatement(new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int), @for.Identifier, @for.RangeStart);
        VisitDeclarationStatement(declaration);
        var scope = @for.Body;
        var identifierExpression = new VariableAccess(@for.Identifier);
        var addExpression = new Add(identifierExpression, new IntLiteral(1)) { Type = PrimitiveVariableType.IntType };
        var finalStmt = new VariableAssignment(@for.Identifier, addExpression);

        scope.Statements.Add(finalStmt);

        var condition = new LessThan(identifierExpression, @for.RangeEnd);
        var whileStatement = new WhileLoop(condition, scope);

        VisitWhileLoop(whileStatement);
        EndScope();

        return @for;
    }

    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        GenerateMethod(functionDeclaration.Identifier,
            MethodAttributes.Public | MethodAttributes.Static,
            functionDeclaration.Type == null ? typeof(void) : typeHelper.GetCsType(functionDeclaration.Type!),
            functionDeclaration.Parameters,
            functionDeclaration.Scope.Statements);

        return functionDeclaration;
    }
    
    protected override MethodInvocation VisitMethodInvocationStatement(MethodInvocation methodInvocation)
    {
        methodInvocation = base.VisitMethodInvocationStatement(methodInvocation);

        var returnType = typeHelper.GetMethod(methodInvocation.Expression.Type, methodInvocation.Identifier, methodInvocation.Arguments.Select(x => x.Type).ToList()).ReturnType;
        if (returnType != typeof(void))
        {
            _il.Emit(OpCodes.Pop);
        }
        
        return methodInvocation;
    }
    protected override MethodInvocation VisitMethodInvocation(MethodInvocation methodInvocation)
    {
        methodInvocation = base.VisitMethodInvocation(methodInvocation);

        if (typeHelper.GetCsType(methodInvocation.Expression.Type).IsValueType)
        {
            GetReferenceToStackValue(methodInvocation.Expression);
        }

        _il.Emit(OpCodes.Call, typeHelper.GetMethod(methodInvocation.Expression.Type, methodInvocation.Identifier, methodInvocation.Arguments.Select(x => x.Type).ToList()));

        return methodInvocation;
    }

    private void GetReferenceToStackValue(IExpression expression)
    {
        var local = _il.DeclareLocal(typeHelper.GetCsType(expression.Type));
        var name = GeneratedVariableCounter++.ToString();
        _variableTracker.SetValue(name, local);
        EmitLocalSet(name);
        
        _il.Emit(OpCodes.Ldloca, local);
    }

    protected override FunctionInvocation VisitFunctionInvocationStatement(FunctionInvocation functionInvocation)
    {
        functionInvocation = base.VisitFunctionInvocationStatement(functionInvocation);

        var returnType = _functions[functionInvocation.Identifier].ReturnType;
        if (returnType != typeof(void))
        {
            _il.Emit(OpCodes.Pop);
        }
        
        return functionInvocation;
    }
    protected override FunctionInvocation VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation = base.VisitFunctionInvocation(functionInvocation);

        _il.Emit(OpCodes.Call, _functions[functionInvocation.Identifier]);

        return functionInvocation;
    }

    protected override Scope VisitScope(Scope scopeNode)
    {
        BeginScope();
        VisitStatements(scopeNode.Statements);

        EndScope();

        return scopeNode;
    }

    protected override Return VisitReturnStatement(Return returnStatement)
    {
        base.VisitReturnStatement(returnStatement);
        _il.Emit(OpCodes.Ret);

        return returnStatement;
    }

    protected override IExpression VisitNotExpression(Not notExpression)
    {
        notExpression = (Not)base.VisitNotExpression(notExpression);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return notExpression;
    }

    protected override IExpression VisitIntLiteral(IntLiteral intLiteral)
    {
        _il.Emit(OpCodes.Ldc_I4, intLiteral.Value);
        return intLiteral;
    }

    protected override IExpression VisitBoolLiteral(BoolLiteral boolLiteral)
    {
        _il.Emit(boolLiteral.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

        return boolLiteral;
    }

    protected override IExpression VisitStringLiteral(StringLiteral stringLiteral)
    {
        _il.Emit(OpCodes.Ldstr, stringLiteral.Value);
        
        return stringLiteral;
    }

    protected override IExpression VisitFloatLiteral(FloatLiteral floatLiteral)
    {
        _il.Emit(OpCodes.Ldc_R4, floatLiteral.Value);

        return floatLiteral;
    }

    protected override IExpression VisitCast(Cast cast)
    {
        cast = (Cast)base.VisitCast(cast);
        if (cast.TargetType.Equals(PrimitiveVariableType.IntType))
        {
            _il.Emit(OpCodes.Conv_I4);

            return cast;
        }

        if (cast.TargetType.Equals(PrimitiveVariableType.FloatType))
        {
            _il.Emit(OpCodes.Conv_R4);

            return cast;
        }

        // can only change type between int and float for now
        throw ErrorHelper.ShowErrorMessage("Invalid cast.", cast.Span);
    }

    protected override VariableAccess VisitVariableAccess(VariableAccess variableAccess)
    {
        variableAccess = (VariableAccess)base.VisitVariableAccess(variableAccess);
        EmitLocalAccess(variableAccess.Identifier);

        return variableAccess;
    }

    protected override Add VisitAddExpression(Add addExpression)
    {
        addExpression = (Add)base.VisitAddExpression(addExpression);
        if (addExpression.Type.Equals(PrimitiveVariableType.StringType))
        {
            EmitConcat();
        }
        else
        {
            _il.Emit(OpCodes.Add);
        }

        return addExpression;
    }

    private static readonly MethodInfo Concat = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])!;
    
    private void EmitConcat()
    {
        _il.Emit(OpCodes.Call, Concat);
    }

    protected override And VisitAndExpression(And andExpression)
    {
        andExpression = (And)base.VisitAndExpression(andExpression);
        _il.Emit(OpCodes.And);

        return andExpression;
    }

    protected override AreEqual VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        areEqualExpression = (AreEqual)base.VisitAreEqualExpression(areEqualExpression);
        _il.Emit(OpCodes.Ceq);

        return areEqualExpression;
    }

    protected override GreaterOrEqual VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        greaterOrEqualExpression = (GreaterOrEqual)base.VisitGreaterOrEqualExpression(greaterOrEqualExpression);
        _il.Emit(OpCodes.Clt);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return greaterOrEqualExpression;
    }

    protected override GreaterThan VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        greaterThanExpression = (GreaterThan)base.VisitGreaterThanExpression(greaterThanExpression);
        _il.Emit(OpCodes.Cgt);

        return greaterThanExpression;
    }

    protected override LessThan VisitLessThanExpression(LessThan lessThanExpression)
    {
        lessThanExpression = (LessThan)base.VisitLessThanExpression(lessThanExpression);
        _il.Emit(OpCodes.Clt);

        return lessThanExpression;
    }

    protected override LessThanOrEqual VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        lessThanOrEqualExpression = (LessThanOrEqual)base.VisitLessThanOrEqualExpression(lessThanOrEqualExpression);
        _il.Emit(OpCodes.Cgt);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return lessThanOrEqualExpression;
    }

    protected override Multiply VisitMultiplyExpression(Multiply multiplyExpression)
    {
        multiplyExpression = (Multiply)base.VisitMultiplyExpression(multiplyExpression);
        _il.Emit(OpCodes.Mul);

        return multiplyExpression;
    }

    protected override Divide VisitDivideExpression(Divide divideExpression)
    {
        divideExpression = (Divide)base.VisitDivideExpression(divideExpression);
        _il.Emit(OpCodes.Div);

        return divideExpression;
    }

    protected override NotEqual VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        notEqualExpression = (NotEqual)base.VisitNotEqualExpression(notEqualExpression);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Ldc_I4_1);
        _il.Emit(OpCodes.Xor);

        return notEqualExpression;
    }

    protected override Or VisitOrExpression(Or orExpression)
    {
        orExpression = (Or)base.VisitOrExpression(orExpression);
        _il.Emit(OpCodes.Or);

        return orExpression;
    }

    protected override Subtract VisitSubtractExpression(Subtract subtractExpression)
    {
        subtractExpression = (Subtract)base.VisitSubtractExpression(subtractExpression);
        _il.Emit(OpCodes.Sub);

        return subtractExpression;
    }

    protected override Negate VisitNegate(Negate negate)
    {
        negate = (Negate)base.VisitNegate(negate);
        _il.Emit(OpCodes.Neg);

        return negate;
    }

    private void SaveParameter(Parameter parameter, int index)
    {
        _parameterTracker.SetValue(parameter.Identifier, index);
    }

    private void EmitLocalAccess(string variableName)
    {
        // TODO: need to prefer the newest declared one in the case of nested scopes
        if (_parameterTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Ldarg, _parameterTracker.GetValue(variableName));
        }

        if (_variableTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Ldloc, _variableTracker.GetValue(variableName));
        }
    }

    private void EmitLocalSet(string variableName)
    {
        // TODO: need to prefer the newest declared one in the case of nested scopes
        if (_parameterTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Starg, _parameterTracker.GetValue(variableName));
        }

        if (_variableTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Stloc, _variableTracker.GetValue(variableName));
        }
    }

    private void BeginScope()
    {
        _variableTracker.BeginScope();
        _parameterTracker.BeginScope();
    }

    private void EndScope()
    {
        _variableTracker.EndScope();
        _parameterTracker.EndScope();
    }

    private void EmitLabel(Label label)
    {
        _il.MarkLabel(label);
    }
    
    private int GeneratedVariableCounter = 0;
}