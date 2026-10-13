using ErrorHelper;
using Lexing;
using OLangAst.ClassMembers;
using OLangAst.EnumVariants;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace AstHelpers;

public class TypeHelper(IErrorHelper errorHelper)
{
    public IType GetMethodType(IVariableType? type, IDefinedType? typeContext)
    {
        if (type == null)
        {
            return PrimitiveTypes.VoidType;
        }

        return GetLocalType(type, typeContext);
    }

    public IType? TryGetLocalType(IVariableType? type, IDefinedType? typeContext)
    {
        if (type == null)
        {
            return null;
        }

        return GetLocalType(type, typeContext);
    }
    
    public IType GetLocalType(IVariableType type, IDefinedType? typeContext)
    {
        if (type is ArrayType arrayType)
        {
            var innerType = GetLocalType(arrayType.InnerType, typeContext);

            return GetArrayOfType(innerType);
        }

        if (type is ParameterizedType parameterizedType)
        {
            var genericType = GetLocalType(parameterizedType.GenericType, typeContext);
            var typeArguments = parameterizedType.TypeArguments.Select(x => GetLocalType(x, typeContext));

            if (genericType is not DefinedType { IsGeneric: true } definedType)
            {
                throw errorHelper.ShowErrorMessage($"Type `{genericType}` doesn't have type parameters.", type.Span);
            }

            return definedType.MakeConcrete(typeArguments.ToList());
        }
        
        return GetDefinedType(type.Name, typeContext) ?? throw errorHelper.ShowErrorMessage($"Unknown type: `{type.Name}`", type.Span);
    }

    private DefinedArray GetArrayOfType(IType type)
    {
        return new DefinedArray(type);
    }
    
    public IFunctionDefinition GetMethod(IDefinedType definedType, string name, List<IType> argumentTypes, SourceSpan span)
    {
        var methods = definedType.GetMethods();
        var matchingMethods = methods.Where(x => x.Name == name && x.GetParameterTypes().Index().All(y => TypeMatches(y.Item, argumentTypes, y.Index))).ToList();

        return matchingMethods.Count switch
        {
            0 => throw errorHelper.ShowErrorMessage($"Couldn't find method `{name}` with matching signature.", span),
            > 1 => throw errorHelper.ShowErrorMessage($"Couldn't uniquely identify method `{name}`.", span),
            _ => matchingMethods.Single()
        };
    }

    public FunctionDefinition CreateCustomMethod(DefinedType customDefinedType, MethodDeclaration methodDeclaration)
    {
        if (customDefinedType.TypeVariant == TypeVariant.Enum)
        {
            throw errorHelper.ShowErrorMessage($"Cannot define method on enum {customDefinedType.Name}", methodDeclaration.Span);
        }
        
        CheckForExistingMember(customDefinedType, methodDeclaration.Identifier, methodDeclaration.Span);
        var method = new FunctionDefinition(customDefinedType,
            GetMethodType(methodDeclaration.DeclaredType, customDefinedType),
            methodDeclaration.Identifier,
            methodDeclaration.Parameters.Select(x => new DeclaredParameter(x.Identifier, GetLocalType(x.DeclaredType, customDefinedType))),
            methodDeclaration.IsInstance);
        customDefinedType.Methods.Add(method);

        return method;
    }

    public DefinedEnumVariant CreateEnumVariant(DefinedType customDefinedType, EnumVariant enumVariant)
    {
        if (customDefinedType.TypeVariant != TypeVariant.Enum)
        {
            throw errorHelper.ShowErrorMessage($"Cannot define enum variant on non-enum {customDefinedType.Name}", enumVariant.Span);
        }
        
        CheckForExistingEnumVariant(customDefinedType, enumVariant.Name, enumVariant.Span);

        var definedEnumVariant = new DefinedEnumVariant(enumVariant.Name, enumVariant.Values.Select(x => GetLocalType(x, customDefinedType)).ToList());
        customDefinedType.EnumVariants.Add(enumVariant.Name, definedEnumVariant);

        return definedEnumVariant;
    }

    private void CheckForExistingEnumVariant(DefinedType definedType, string variantName, SourceSpan span)
    {
        if (definedType.EnumVariants.ContainsKey(variantName))
        {
            throw errorHelper.ShowErrorMessage($"Enum `{definedType.Name}` had variant `{variantName}` defined twice", span);
        }
    }

    public void CreateCustomField(DefinedType customDefinedType, FieldDeclaration fieldDeclaration)
    {
        CheckForExistingMember(customDefinedType, fieldDeclaration.Identifier, fieldDeclaration.Span);
        customDefinedType.Fields.Add(fieldDeclaration.Identifier, new FieldDefinition(GetLocalType(fieldDeclaration.Type, customDefinedType), fieldDeclaration.Identifier));
    }

    private void CheckForExistingMember(DefinedType definedType, string memberName, SourceSpan span)
    {
        var errorMessage = $"Member with name `{memberName}` was already declared";
        if (definedType.Methods.Any(x => x.Name == memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
        
        if (definedType.Fields.ContainsKey(memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
    }

    public DefinedType CreateCustomType(string name, TypeVariant typeVariant, List<string> typeParameters)
    {
        var userDefined = new DefinedType(name, typeVariant, typeParameters, false);
        _customTypes.Add(name, userDefined);
        
        return userDefined;
    }

    public void LoadAssembly(string assemblyName)
    {
        foreach (var (_, type) in CsAssemblyLoader.LoadDefinedTypesFromAssembly(assemblyName))
        {
            _loadedTypes[type.Name] = type;
        }
    }
    
    public IType GetInnerArrayType(IExpression array)
    {
        if (array.Type is not DefinedArray arrayType)
        {
            throw errorHelper.ShowErrorMessage($"Cannot get inner array type from non-array type {array.Type}", array.Span);
        }
        
        return arrayType.InnerType;
    }

    public IType? GetDefinedType(string name, IDefinedType? typeContext)
    {
        if (PrimitiveTypeMap.Keys.Any(x => x.ToString() == name))
        {
            return PrimitiveTypeMap.First(x => x.Key.ToString() == name).Value;
        }
        return _customTypes.GetValueOrDefault(name) ?? (IType?)_loadedTypes.GetValueOrDefault(name) ?? GetTypeParameter(name, typeContext);
    }

    private GenericType? GetTypeParameter(string name, IDefinedType? declaringType)
    {
        return (declaringType as DefinedType)?.TypeParameters.Any(x => x == name) ?? false ? new GenericType(name) : null;
    }

    private bool TypeMatches(IType type, List<IType> argumentTypes, int index)
    {
        return index < argumentTypes.Count && type.Equals(argumentTypes[index]);
    }

    private readonly Dictionary<string, DefinedType> _customTypes = new();
    private readonly Dictionary<string, DefinedType> _loadedTypes = new();

    public List<DefinedType> GetDefinedTypes()
    {
        return _customTypes.Values.ToList();
    }

    private static readonly Dictionary<PrimitiveVariableTypeEnum, DefinedType> PrimitiveTypeMap = new()
    {
        { PrimitiveVariableTypeEnum.Int, PrimitiveTypes.IntType },
        { PrimitiveVariableTypeEnum.Bool, PrimitiveTypes.BoolType },
        { PrimitiveVariableTypeEnum.Float, PrimitiveTypes.FloatType },
        { PrimitiveVariableTypeEnum.String, PrimitiveTypes.StringType }
    };
    
    // private static readonly Dictionary<Type, PrimitiveType> ReversePrimitiveTypeMap = PrimitiveTypeMap.ToDictionary(x => x.Value, x => x.Key);
}