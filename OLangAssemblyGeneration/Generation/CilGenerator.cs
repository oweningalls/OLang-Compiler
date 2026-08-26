using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using ErrorHelper;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangCompiler.Utility;
using Parameter = OLangAst.Miscellaneous.Parameter;

namespace AssemblyGeneration.Generation;

public class CilGenerator(IErrorHelper errorHelper) : BaseOLangAstVisitor(errorHelper), IGenerator
{
    private IErrorHelper _errorHelper;
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

        program.Statements.Add(new ExitStatement(new IntLiteral(0)));
        var main = GenerateMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [], program.Statements);

        GenerateAssemblyFile(filePath, fileName, assembly, main);
        WriteRuntimeConfigFile(filePath, Path.GetFileNameWithoutExtension(fileName));
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
        var method = _type.DefineMethod(name, attributes, retType, parameters.Select(x => GetCsType(x.Type)).ToArray());
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

    private void WriteExit(int? value = null)
    {
        var exitMethod = typeof(Environment).GetMethod(nameof(Environment.Exit), [typeof(int)])!;
        if (value is { } num)
        {
            _il.Emit(OpCodes.Ldc_I4, num);
        }

        _il.Emit(OpCodes.Call, exitMethod);
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        var local = _il.DeclareLocal(GetCsType(declarationStatement.Value.Type!));
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
        var addExpression = new Add(identifierExpression, new IntLiteral(1));
        var finalStmt = new VariableAssignment(@for.Identifier, addExpression);

        scope.Statements.Add(finalStmt);

        var condition = new LessThan(identifierExpression, @for.RangeEnd);
        var whileStatement = new WhileLoop(condition, scope);

        VisitWhileLoop(whileStatement);
        EndScope();

        return @for;
    }

    // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
    // return values in rax and rdx (if needed)
    // preserve rbx, rbp, r12, r13, r14, and r15
    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        GenerateMethod(functionDeclaration.Identifier,
            MethodAttributes.Public | MethodAttributes.Static,
            functionDeclaration.Type == null ? typeof(void) : GetCsType(functionDeclaration.Type!),
            functionDeclaration.Parameters,
            functionDeclaration.Scope.Statements);

        return functionDeclaration;
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
        for (var i = 0; i < functionInvocation.Arguments.Count; i++)
        {
            functionInvocation.Arguments[i] = VisitExpression(functionInvocation.Arguments[i]);
        }

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

    protected override Not VisitNotExpression(Not notExpression)
    {
        notExpression = base.VisitNotExpression(notExpression);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return notExpression;
    }

    protected override IntLiteral VisitIntLiteral(IntLiteral intLiteral)
    {
        _il.Emit(OpCodes.Ldc_I4, intLiteral.Value);
        return intLiteral;
    }

    protected override BoolLiteral VisitBoolLiteral(BoolLiteral boolLiteral)
    {
        _il.Emit(boolLiteral.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

        return boolLiteral;
    }

    protected override StringLiteral VisitStringLiteral(StringLiteral stringLiteral)
    {
        _il.Emit(OpCodes.Ldstr, stringLiteral.Value);
        
        return stringLiteral;
    }


    protected override VariableAccess VisitVariableAccess(VariableAccess variableAccess)
    {
        variableAccess = base.VisitVariableAccess(variableAccess);
        EmitLocalAccess(variableAccess.Identifier);

        return variableAccess;
    }

    protected override Add VisitAddExpression(Add addExpression)
    {
        addExpression = base.VisitAddExpression(addExpression);
        _il.Emit(OpCodes.Add);

        return addExpression;
    }

    protected override And VisitAndExpression(And andExpression)
    {
        andExpression = base.VisitAndExpression(andExpression);
        _il.Emit(OpCodes.And);

        return andExpression;
    }

    protected override AreEqual VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        areEqualExpression = base.VisitAreEqualExpression(areEqualExpression);
        _il.Emit(OpCodes.Ceq);

        return areEqualExpression;
    }

    protected override GreaterOrEqual VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        greaterOrEqualExpression = base.VisitGreaterOrEqualExpression(greaterOrEqualExpression);
        _il.Emit(OpCodes.Clt);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return greaterOrEqualExpression;
    }

    protected override GreaterThan VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        greaterThanExpression = base.VisitGreaterThanExpression(greaterThanExpression);
        _il.Emit(OpCodes.Cgt);

        return greaterThanExpression;
    }

    protected override LessThan VisitLessThanExpression(LessThan lessThanExpression)
    {
        lessThanExpression = base.VisitLessThanExpression(lessThanExpression);
        _il.Emit(OpCodes.Clt);

        return lessThanExpression;
    }

    protected override LessThanOrEqual VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        lessThanOrEqualExpression = base.VisitLessThanOrEqualExpression(lessThanOrEqualExpression);
        _il.Emit(OpCodes.Cgt);
        _il.Emit(OpCodes.Ldc_I4_0);
        _il.Emit(OpCodes.Ceq);

        return lessThanOrEqualExpression;
    }

    protected override Multiply VisitMultiplyExpression(Multiply multiplyExpression)
    {
        multiplyExpression = base.VisitMultiplyExpression(multiplyExpression);
        _il.Emit(OpCodes.Mul);

        return multiplyExpression;
    }

    protected override Divide VisitDivideExpression(Divide divideExpression)
    {
        divideExpression = base.VisitDivideExpression(divideExpression);
        _il.Emit(OpCodes.Div);

        return divideExpression;
    }

    protected override NotEqual VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        notEqualExpression = base.VisitNotEqualExpression(notEqualExpression);
        _il.Emit(OpCodes.Ceq);
        _il.Emit(OpCodes.Ldc_I4_1);
        _il.Emit(OpCodes.Xor);

        return notEqualExpression;
    }

    protected override Or VisitOrExpression(Or orExpression)
    {
        orExpression = base.VisitOrExpression(orExpression);
        _il.Emit(OpCodes.Or);

        return orExpression;
    }

    protected override Subtract VisitSubtractExpression(Subtract subtractExpression)
    {
        subtractExpression = base.VisitSubtractExpression(subtractExpression);
        _il.Emit(OpCodes.Sub);

        return subtractExpression;
    }

    protected override Negate VisitNegate(Negate negate)
    {
        negate = base.VisitNegate(negate);
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

    private Type GetCsType(IVariableType type)
    {
        if (type is not PrimitiveVariableType primitiveVariableType)
        {
            throw ErrorHelper.UnknownVariant("type", type.GetType());
        }

        return PrimitiveTypeMap[primitiveVariableType.Type];
    }

    private static readonly Dictionary<PrimitiveVariableTypeEnum, Type> PrimitiveTypeMap = new()
    {
        { PrimitiveVariableTypeEnum.Int, typeof(int) },
        { PrimitiveVariableTypeEnum.Bool, typeof(bool) },
        { PrimitiveVariableTypeEnum.Float, typeof(float) },
        { PrimitiveVariableTypeEnum.String, typeof(string) },
    };
}