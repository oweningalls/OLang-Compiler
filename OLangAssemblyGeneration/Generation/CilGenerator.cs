using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.EnumVariants;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangAst.TypeSystem;
using OLangHelpers;
using FieldDefinition = OLangAst.TypeSystem.FieldDefinition;

namespace AssemblyGeneration.Generation;

public class CilGenerator(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper), IGenerator
{
    private PersistedAssemblyBuilder _assemblyBuilder;
    private TypeBuilder _type;
    private ModuleBuilder _moduleBuilder;
    private ILGenerator _il;

    private Dictionary<string, MethodBuilder> _functions;
    private ScopeTracker<string, LocalBuilder> _variableTracker = null!;

    private ScopeTracker<string, int> _parameterTracker;

    public void GenerateProgram(Program program, string filePath, string fileName)
    {
        _variableTracker = new ScopeTracker<string, LocalBuilder>();
        _parameterTracker = new ScopeTracker<string, int>();
        _functions = [];
        _assemblyBuilder = new PersistedAssemblyBuilder(new AssemblyName("AssemblyName"), typeof(object).Assembly);
        _moduleBuilder = _assemblyBuilder.DefineDynamicModule("OLangProgram");

        var types = typeHelper.GetDefinedTypes();
        var definedClasses = types.Where(x => x.TypeVariant != TypeVariant.Enum).ToList();
        var definedEnums = types.Where(x => x.TypeVariant == TypeVariant.Enum).ToList();
        
        var classes = DefineClasses(definedClasses);
        var enums = DefineEnums(definedEnums);
        DefineMethods(definedClasses, classes);
        DefineFields(definedClasses, classes);
        
        VisitProgram(program);

        if (!_functions.TryGetValue("Main", out var main))
        {
            throw ErrorHelper.ShowErrorMessage("Program must contain a method named 'Main'", program.Span);
        }
        GenerateAssemblyFile(filePath, fileName, _assemblyBuilder, main);
        WriteRuntimeConfigFile(filePath, Path.GetFileNameWithoutExtension(fileName));
    }

    protected override UsingStatement VisitUsingStatement(UsingStatement usingStatement)
    {
        var types = CsAssemblyLoader.LoadDefinedTypesFromAssembly(usingStatement.Module);
        foreach (var (type, definedType) in types)
        {
            _loadedTypes[definedType] = type;
        }

        return usingStatement;
    }

    protected override ITypeDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _type = _definedTypes[classDeclaration.Type!.Value];
        classDeclaration = (ClassDeclaration)base.VisitClassDeclaration(classDeclaration);
        
        _type.CreateType();

        return classDeclaration;
    }
    
    protected override ITypeDeclaration VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        _type = _definedTypes[enumDeclaration.Type!.Value];
        enumDeclaration = (EnumDeclaration)base.VisitEnumDeclaration(enumDeclaration);
        // already did CreateType() in DefineEnum
        
        return enumDeclaration;
    }

    protected override EnumVariant VisitEnumVariant(EnumVariant enumVariant)
    {
        _enumVariants[_type][enumVariant.Name].CreateType();

        return enumVariant;
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var method = (MethodBuilder)GetMethod(_type, methodDeclaration.Identifier, methodDeclaration.Parameters.Select(x => GetCsType(x.Type!.Value)));
        _functions[methodDeclaration.Identifier] = method;
        
        var originalIl = _il;
        _il = method.GetILGenerator();

        BeginScope();

        var isStatic = IsStatic(_type);
        for (var i = 0; i < methodDeclaration.Parameters.Count; i++)
        {
            SaveParameter(methodDeclaration.Parameters[i], isStatic ? i : i + 1);
        }

        VisitStatements(methodDeclaration.Scope.Statements);
        _il.Emit(OpCodes.Ret);

        EndScope();
        _il = originalIl;

        return methodDeclaration;
    }

    private static bool IsStatic(Type type)
    {
        return type.IsSealed && type.IsAbstract;
    }

    private void GenerateAssemblyFile(string filePath, string fileName, PersistedAssemblyBuilder assembly, MethodBuilder main)
    {
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
    
    private void GenerateFunction(string name, MethodAttributes attributes, Type? retType, List<ParameterNode> parameters, List<IStatement> statements)
    {
        var method = _type.DefineMethod(name, attributes, retType, parameters.Select(x => GetCsType(x.Type!.Value)).ToArray());
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
    }

    protected override ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement = base.VisitExitStatement(exitStatement);
        WriteExit();

        return exitStatement;
    }

    protected override IExpression VisitEnumInstantiation(EnumInstantiation enumInstantiation)
    {
        var enumType = GetCsType(enumInstantiation.Type!.Value);
        var constructor = _enumVariantConstructors[enumType][enumInstantiation.VariantName];
        
        _il.Emit(OpCodes.Newobj, constructor);

        return enumInstantiation;
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
        var local = _il.DeclareLocal(GetCsType(declarationStatement.VariableType!.Value));
        _variableTracker.SetValue(declarationStatement.Identifier, local);
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        EmitLocalSet(declarationStatement.Identifier);

        return declarationStatement;
    }

    protected override IStatement VisitVariableAssignment(VariableAssignment assignment)
    {
        assignment = (VariableAssignment)base.VisitVariableAssignment(assignment);
        EmitLocalSet(assignment.Identifier);

        return assignment;
    }

    protected override ArrayAssignment VisitArrayAssignment(ArrayAssignment assignmentStatement)
    {
        assignmentStatement = base.VisitArrayAssignment(assignmentStatement);
        var arrayType = GetCsType(typeHelper.GetInnerArrayType(assignmentStatement.Array));
        _il.Emit(OpCodes.Stelem, arrayType);

        return assignmentStatement;
    }
    
    protected override FieldAssignment VisitFieldAssignment(FieldAssignment fieldAssignment)
    {
        fieldAssignment = base.VisitFieldAssignment(fieldAssignment);
        var field = GetField(GetCsType(fieldAssignment.Target.Type!.Value), fieldAssignment.Identifier);
        _il.Emit(OpCodes.Stfld, field);

        return fieldAssignment;
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

        var declaration = new VariableDeclarationStatement(null, @for.Identifier, @for.RangeStart) { VariableType = PrimitiveTypes.IntType };
        VisitDeclarationStatement(declaration);
        var scope = @for.Body;
        var identifierExpression = new VariableAccess(@for.Identifier);
        var addExpression = new Add(identifierExpression, new IntLiteral(1)) { Type = PrimitiveTypes.IntType };
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
        GenerateFunction(functionDeclaration.Identifier,
            MethodAttributes.Public | MethodAttributes.Static,
            functionDeclaration.DeclaredType == null ? null : GetCsType(typeHelper.GetMethodType(functionDeclaration.DeclaredType)),
            functionDeclaration.Parameters,
            functionDeclaration.Scope.Statements);

        return functionDeclaration;
    }
    
    protected override MethodInvocation VisitMethodInvocationStatement(MethodInvocation methodInvocation)
    {
        methodInvocation = base.VisitMethodInvocationStatement(methodInvocation);

        var returnType = GetCsType(typeHelper.GetMethod(methodInvocation.Expression!.Type!.Value,
                methodInvocation.Identifier,
                methodInvocation.Arguments.Select(x => x.Type!.Value)
                    .ToList(), methodInvocation.Span)
            .ReturnType);
        if (returnType != typeof(void))
        {
            _il.Emit(OpCodes.Pop);
        }
        
        return methodInvocation;
    }
    protected override MethodInvocation VisitMethodInvocation(MethodInvocation methodInvocation)
    {
        methodInvocation = base.VisitMethodInvocation(methodInvocation);

        var expressionType = methodInvocation.SourceType ?? methodInvocation.Expression?.Type;
        var targetType = expressionType == null ? _type : GetCsType(expressionType.Value);

        if (expressionType != null && targetType.IsValueType)
        {
            GetReferenceToStackValue(methodInvocation.Expression);
        }

        var method = GetMethod(targetType, methodInvocation.Identifier, methodInvocation.Arguments.Select(x => GetCsType(x.Type!.Value)));
        _il.Emit(targetType.IsValueType || method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);

        return methodInvocation;
    }

    protected override IExpression VisitFieldAccess(FieldAccess fieldAccess)
    {
        fieldAccess = (FieldAccess)base.VisitFieldAccess(fieldAccess);

        var type = GetCsType(fieldAccess.Expression.Type!.Value);
        var field = GetField(type, fieldAccess.FieldName);
        _il.Emit(OpCodes.Ldfld, field);
        
        return fieldAccess;
    }

    private void GetReferenceToStackValue(IExpression expression)
    {
        var local = _il.DeclareLocal(GetCsType(expression.Type!.Value));
        var name = _generatedVariableCounter++.ToString();
        _variableTracker.SetValue(name, local);
        EmitLocalSet(name);
        
        _il.Emit(OpCodes.Ldloca, local);
    }

    protected override IStatement VisitFunctionInvocationStatement(FunctionInvocation functionInvocation)
    {
        functionInvocation = (FunctionInvocation)base.VisitFunctionInvocationStatement(functionInvocation);

        var returnType = _functions[functionInvocation.Identifier].ReturnType;
        if (returnType != typeof(void))
        {
            _il.Emit(OpCodes.Pop);
        }
        
        return functionInvocation;
    }
    protected override IExpression VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation = (FunctionInvocation)base.VisitFunctionInvocation(functionInvocation);

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
        if (cast.Type.Equals(PrimitiveTypes.IntType))
        {
            _il.Emit(OpCodes.Conv_I4);

            return cast;
        }

        if (cast.Type.Equals(PrimitiveTypes.FloatType))
        {
            _il.Emit(OpCodes.Conv_R4);

            return cast;
        }

        // can only change type between int and float for now
        throw ErrorHelper.ShowErrorMessage("Invalid cast.", cast.Span);
    }

    protected override IExpression VisitInstantiation(Instantiation instantiation)
    {
        instantiation = (Instantiation)base.VisitInstantiation(instantiation);
        var method = instantiation.Type!.Value.IsCs ? GetCsType(instantiation.Type!.Value).GetConstructor([])! : _definedTypeConstructors[instantiation.Type.Value];
        _il.Emit(OpCodes.Newobj, method);

        return instantiation;
    }

    protected override IExpression VisitArrayInstantiation(ArrayInstantiation instantiation)
    {
        instantiation = (ArrayInstantiation)base.VisitArrayInstantiation(instantiation);
        _il.Emit(OpCodes.Newarr, GetCsType(instantiation.Type!.Value.TypeParameters.Single()));

        return instantiation;
    }
    
    protected override IExpression VisitArrayAccess(ArrayAccess arrayAccess)
    {
        arrayAccess = (ArrayAccess)base.VisitArrayAccess(arrayAccess);

        _il.Emit(OpCodes.Ldelem, GetCsType(typeHelper.GetInnerArrayType(arrayAccess.Array)));
        
        return arrayAccess;
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
        if (addExpression.Type.Equals(PrimitiveTypes.StringType))
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

    protected override IExpression VisitThisAccess(ThisAccess thisAccess)
    {
        _il.Emit(OpCodes.Ldarg_0);

        return thisAccess;
    }
    
    private void SaveParameter(ParameterNode parameterNode, int index)
    {
        _parameterTracker.SetValue(parameterNode.Identifier, index);
    }

    private void EmitLocalAccess(string variableName)
    {
        // TODO: need to prefer the newest declared one in the case of nested scopes
        if (_parameterTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Ldarg, _parameterTracker.GetValue(variableName));
        }

        else if (_variableTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Ldloc, _variableTracker.GetValue(variableName));
        }
        else
        {
            throw new Exception();
        }
    }

    private void EmitLocalSet(string variableName)
    {
        // TODO: need to prefer the newest declared one in the case of nested scopes
        if (_parameterTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Starg, _parameterTracker.GetValue(variableName));
        }
        else if (_variableTracker.ContainsKey(variableName))
        {
            _il.Emit(OpCodes.Stloc, _variableTracker.GetValue(variableName));
        }
        else
        {
            throw new Exception();
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
    
    private int _generatedVariableCounter;
    
    public Type GetCsType(DefinedType type)
    {
        if (type.Equals(PrimitiveTypes.FloatType))
        {
            return typeof(float);
        }
        if (type.Equals(PrimitiveTypes.IntType))
        {
            return typeof(int);
        }
        if (type.Equals(PrimitiveTypes.BoolType))
        {
            return typeof(bool);
        }
        if (type.Equals(PrimitiveTypes.StringType))
        {
            return typeof(string);
        }

        if (type.Equals(PrimitiveTypes.VoidType))
        {
            return typeof(void);
        }
        if (type.IsCs)
        {
            return _loadedTypes[type];
        }

        if (type.IsArray)
        {
            var innerType = type.TypeParameters.Single();

            return GetCsType(innerType).MakeArrayType();

        }

        return _definedTypes[type];
    }
    
    private Dictionary<DefinedType, TypeBuilder> _definedTypes = new();
    private Dictionary<DefinedType, Type> _loadedTypes = new();
    private Dictionary<DefinedType, ConstructorBuilder> _definedTypeConstructors = new();
    private Dictionary<TypeBuilder, Dictionary<string, MethodBuilder>> _definedMethods = new();
    private Dictionary<TypeBuilder, Dictionary<string, FieldBuilder>> _definedFields = new();
    private Dictionary<Type, Dictionary<string, TypeBuilder>> _enumVariants = new();
    private Dictionary<Type, Dictionary<string, ConstructorBuilder>> _enumVariantConstructors = new();

    private List<TypeBuilder> DefineClasses(IEnumerable<DefinedType> classes)
    {
        return classes.Where(x => x.TypeVariant != TypeVariant.Enum).Select(DefineClass).ToList();
    }

    private List<TypeBuilder> DefineEnums(IEnumerable<DefinedType> enums)
    {
        return enums.Where(x => x.TypeVariant == TypeVariant.Enum).Select(DefineEnum).ToList();
    }

    private TypeBuilder DefineEnum(DefinedType type)
    {
        var attributes = TypeAttributes.Public | TypeAttributes.Abstract;
        
        var typeBuilder = _moduleBuilder.DefineType(type.Name, attributes);
        _definedTypes[type] = typeBuilder;
        _definedTypeConstructors[type] = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        typeBuilder.CreateType();
        _enumVariants[typeBuilder] = [];
        _enumVariantConstructors[typeBuilder] = [];

        attributes = TypeAttributes.Public | TypeAttributes.Sealed;
        foreach (var variant in type.EnumVariants)
        {
            var variantBuilder = _moduleBuilder.DefineType(variant.Name, attributes, typeBuilder);
            _enumVariants[typeBuilder][variant.Name] = variantBuilder;
            _enumVariantConstructors[typeBuilder][variant.Name] = variantBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        }
        
        return typeBuilder;
    }
    
    private TypeBuilder DefineClass(DefinedType type)
    {
        var attributes = TypeAttributes.Public;
        if (type.TypeVariant == TypeVariant.StaticClass)
        {
            attributes |= TypeAttributes.Abstract | TypeAttributes.Sealed;
        }
        
        var typeBuilder = _moduleBuilder.DefineType(type.Name, attributes);
        _definedTypes[type] = typeBuilder;
        _definedTypeConstructors[type] = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        
        return typeBuilder;
    }

    private void DefineMethods(List<DefinedType> classes, List<TypeBuilder> types)
    {
        classes.Zip(types).SelectMany(x => x.First.Methods.Select(method => (x.Second, method))).ToList().ForEach(x => DefineMethod(x.Second, x.method));
    }
    
    private void DefineFields(List<DefinedType> classes, List<TypeBuilder> types)
    {
        classes.Zip(types).SelectMany(x => x.First.Fields.Select(method => (x.Second, method))).ToList().ForEach(x => DefineField(x.Second, x.method));
    }

    private void DefineMethod(TypeBuilder type, FunctionDefinition definition)
    {
        var attributes = MethodAttributes.Public;
        if (definition.DeclaringType.TypeVariant == TypeVariant.StaticClass)
        {
           attributes |= MethodAttributes.Static;
        }
        
        var method = type.DefineMethod(definition.Name, attributes, definition.ReturnType.Equals(PrimitiveTypes.VoidType) ? null : GetCsType(definition.ReturnType), definition.Parameters.Select(x => GetCsType(x.Type)).ToArray());
        if (!_definedMethods.ContainsKey(type))
        {
            _definedMethods[type] = new Dictionary<string, MethodBuilder>(1);
        }
        
        _definedMethods[type][definition.Name] = method;
    }

    private void DefineField(TypeBuilder type, FieldDefinition field)
    {
        var attributes = FieldAttributes.Public;
        var defined = type.DefineField(field.Name, GetCsType(field.Type), attributes);
        
        if (!_definedFields.ContainsKey(type))
        {
            _definedFields[type] = new Dictionary<string, FieldBuilder>(1);
        }
        
        _definedFields[type][field.Name] = defined;
    }

    private MethodInfo GetMethod(Type type, string name, IEnumerable<Type> argumentTypes)
    {
        if (type is TypeBuilder builder && _definedMethods.GetValueOrDefault(builder) is {} value)
        {
            return value[name];
        }
        return type.GetMethod(name, argumentTypes.ToArray()) ?? throw new Exception();
    }
    
    private FieldInfo GetField(Type type, string name)
    {
        if (type is TypeBuilder builder && _definedFields.GetValueOrDefault(builder) is {} value)
        {
            return value[name];
        }
        return type.GetField(name) ?? throw new Exception();
    }
}