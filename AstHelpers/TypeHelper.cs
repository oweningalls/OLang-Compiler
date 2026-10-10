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
    public ConcreteType GetMethodType(IVariableType? type)
    {
        if (type == null)
        {
            return PrimitiveTypes.VoidType;
        }

        return GetLocalType(type);
    }

    public ConcreteType? TryGetLocalType(IVariableType? type)
    {
        if (type == null)
        {
            return null;
        }

        return GetLocalType(type);
    }
    public ConcreteType GetLocalType(IVariableType type)
    {
        if (type is ArrayType arrayType)
        {
            var innerType = GetLocalType(arrayType.InnerType);

            return GetArrayOfType(innerType);
        }
        
        return GetDefinedType(type.Name) ?? throw errorHelper.ShowErrorMessage($"Unknown type: `{type.Name}`", type.Span);
    }

    private ConcreteType GetArrayOfType(ConcreteType type)
    {
        return new ConcreteType($"{type.Name}[]", type.TypeVariant, isArray: true)
        {
            ConcreteTypeParameters = [type]
        };
    }
    
    public FunctionDefinition GetMethod(ConcreteType type, string name, List<ConcreteType> argumentTypes, SourceSpan span)
    {
        var matchingMethods = type.Methods.Where(x => x.Name == name && x.Parameters.Index().All(y => TypeMatches(y.Item.Type, argumentTypes, y.Index))).ToList();

        return matchingMethods.Count switch
        {
            0 => throw errorHelper.ShowErrorMessage($"Couldn't find method `{name}` with matching signature.", span),
            > 1 => throw errorHelper.ShowErrorMessage($"Couldn't uniquely identify method `{name}`.", span),
            _ => matchingMethods.Single()
        };
    }

    public FunctionDefinition CreateCustomMethod(ConcreteType customType, MethodDeclaration methodDeclaration)
    {
        if (customType.TypeVariant == TypeVariant.Enum)
        {
            throw errorHelper.ShowErrorMessage($"Cannot define method on enum {customType.Name}", methodDeclaration.Span);
        }
        
        CheckForExistingMember(customType, methodDeclaration.Identifier, methodDeclaration.Span);
        var method = new FunctionDefinition(customType,
            GetMethodType(methodDeclaration.DeclaredType),
            methodDeclaration.Identifier,
            methodDeclaration.Parameters.Select(x => new Parameter(x.Identifier, GetLocalType(x.DeclaredType))),
            methodDeclaration.IsInstance);
        customType.Methods.Add(method);

        return method;
    }

    public DefinedEnumVariant CreateEnumVariant(ConcreteType customType, EnumVariant enumVariant)
    {
        if (customType.TypeVariant != TypeVariant.Enum)
        {
            throw errorHelper.ShowErrorMessage($"Cannot define enum variant on non-enum {customType.Name}", enumVariant.Span);
        }
        
        CheckForExistingEnumVariant(customType, enumVariant.Name, enumVariant.Span);

        var definedEnumVariant = new DefinedEnumVariant(enumVariant.Name, enumVariant.Values.Select(GetLocalType).ToList());
        customType.EnumVariants.Add(enumVariant.Name, definedEnumVariant);

        return definedEnumVariant;
    }

    private void CheckForExistingEnumVariant(ConcreteType type, string variantName, SourceSpan span)
    {
        if (type.EnumVariants.ContainsKey(variantName))
        {
            throw errorHelper.ShowErrorMessage($"Enum `{type.Name}` had variant `{variantName}` defined twice", span);
        }
    }

    public void CreateCustomField(ConcreteType customType, FieldDeclaration fieldDeclaration)
    {
        CheckForExistingMember(customType, fieldDeclaration.Identifier, fieldDeclaration.Span);
        customType.Fields.Add(fieldDeclaration.Identifier, new FieldDefinition(GetLocalType(fieldDeclaration.Type), fieldDeclaration.Identifier));
    }

    private void CheckForExistingMember(ConcreteType type, string memberName, SourceSpan span)
    {
        var errorMessage = $"Member with name `{memberName}` was already declared";
        if (type.Methods.Any(x => x.Name == memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
        
        if (type.Fields.ContainsKey(memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
    }

    public ConcreteType CreateCustomType(string name, TypeVariant typeVariant, List<string> typeParameters)
    {
        var userDefined = new ConcreteType(name, typeVariant);
        _customTypes.Add(name, userDefined);
        
        return userDefined;
    }

    public void LoadAssembly(string assemblyName)
    {
        foreach (var (_, type) in CsAssemblyLoader.LoadDefinedTypesFromAssembly(assemblyName))
        {
            _customTypes[type.Name] = type;
        }
    }
    
    public ConcreteType GetInnerArrayType(IExpression array)
    {
        if (!array.Type!.Value.IsArray)
        {
            throw errorHelper.ShowErrorMessage($"Cannot get inner array type from non-array type {array.Type}", array.Span);
        }
        
        return array.Type.Value.ConcreteTypeParameters.Single();
    }

    public ConcreteType? GetDefinedType(string name)
    {
        if (PrimitiveTypeMap.Keys.Any(x => x.ToString() == name))
        {
            return PrimitiveTypeMap.First(x => x.Key.ToString() == name).Value;
        }
        return _customTypes.TryGetValue(name, out var value) ? value : null;
    }

    private bool TypeMatches(ConcreteType type, List<ConcreteType> argumentTypes, int index)
    {
        return index < argumentTypes.Count && type.Equals(argumentTypes[index]);
    }

    private readonly Dictionary<string, ConcreteType> _customTypes = new();

    public List<ConcreteType> GetDefinedTypes()
    {
        return _customTypes.Values.Where(x => !x.IsCs).ToList();
    }

    private static readonly Dictionary<PrimitiveVariableTypeEnum, ConcreteType> PrimitiveTypeMap = new()
    {
        { PrimitiveVariableTypeEnum.Int, PrimitiveTypes.IntType },
        { PrimitiveVariableTypeEnum.Bool, PrimitiveTypes.BoolType },
        { PrimitiveVariableTypeEnum.Float, PrimitiveTypes.FloatType },
        { PrimitiveVariableTypeEnum.String, PrimitiveTypes.StringType }
    };
    
    // private static readonly Dictionary<Type, PrimitiveType> ReversePrimitiveTypeMap = PrimitiveTypeMap.ToDictionary(x => x.Value, x => x.Key);
}