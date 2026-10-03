using ErrorHelper;
using Lexing;
using OLangAst.ClassMembers;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace AstHelpers;

public class TypeHelper(IErrorHelper errorHelper)
{
    public DefinedType GetMethodType(IVariableType? type)
    {
        if (type == null)
        {
            return PrimitiveTypes.VoidType;
        }

        return GetLocalType(type);
    }

    public DefinedType? TryGetLocalType(IVariableType? type)
    {
        if (type == null)
        {
            return null;
        }

        return GetLocalType(type);
    }
    public DefinedType GetLocalType(IVariableType type)
    {
        if (type is ArrayType arrayType)
        {
            var innerType = GetLocalType(arrayType.InnerType);

            return GetArrayOfType(innerType);
        }
        
        return GetDefinedType(type.Name) ?? throw errorHelper.ShowErrorMessage($"Unknown type: `{type.Name}`", type.Span);
    }

    public DefinedType GetArrayOfType(DefinedType type)
    {
        return new DefinedType($"{type.Name}[]", false, isArray: true)
        {
            TypeParameters = [type]
        };
    }
    
    public FunctionDefinition GetMethod(DefinedType type, string name, List<DefinedType> argumentTypes, SourceSpan span)
    {
        var matchingMethods = type.Methods.Where(x => x.Name == name && x.Parameters.Index().All(y => TypeMatches(y.Item.Type, argumentTypes, y.Index))).ToList();

        return matchingMethods.Count switch
        {
            0 => throw errorHelper.ShowErrorMessage($"Couldn't find method `{name}` with matching signature.", span),
            > 1 => throw errorHelper.ShowErrorMessage($"Couldn't uniquely identify method `{name}`.", span),
            _ => matchingMethods.Single()
        };
    }

    public FunctionDefinition CreateCustomMethod(DefinedType customType, MethodDeclaration methodDeclaration)
    {
        CheckForExistingMember(customType, methodDeclaration.Identifier, methodDeclaration.Span);
        var method = new FunctionDefinition(customType, GetMethodType(methodDeclaration.DeclaredType), methodDeclaration.Identifier, methodDeclaration.Parameters.Select(x => new Parameter(x.Identifier, GetLocalType(x.DeclaredType))));
        customType.Methods.Add(method);

        return method;
    }

    public void CreateCustomField(DefinedType customType, FieldDeclaration fieldDeclaration)
    {
        CheckForExistingMember(customType, fieldDeclaration.Identifier, fieldDeclaration.Span);
        customType.Fields.Add(new FieldDefinition(GetLocalType(fieldDeclaration.Type), fieldDeclaration.Identifier));
    }

    private void CheckForExistingMember(DefinedType type, string memberName, SourceSpan span)
    {
        var errorMessage = $"Member with name `{memberName}` was already declared";
        if (type.Methods.Any(x => x.Name == memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
        
        if (type.Fields.Any(x => x.Name == memberName))
        {
            throw errorHelper.ShowErrorMessage(errorMessage, span);
        }
    }

    public DefinedType CreateCustomClass(string name, bool isStatic)
    {
        var userDefined = new DefinedType(name, isStatic);
        _customClasses.Add(name, userDefined);
        
        return userDefined;
    }

    public void LoadAssembly(string assemblyName)
    {
        foreach (var (_, type) in CsAssemblyLoader.LoadDefinedTypesFromAssembly(assemblyName))
        {
            _customClasses[type.Name] = type;
        }
    }
    
    public DefinedType GetInnerArrayType(IExpression array)
    {
        if (!array.Type!.Value.IsArray)
        {
            throw errorHelper.ShowErrorMessage($"Cannot get inner array type from non-array type {array.Type}", array.Span);
        }
        
        return array.Type.Value.TypeParameters.Single();
    }

    public DefinedType? GetDefinedType(string name)
    {
        if (PrimitiveTypeMap.Keys.Any(x => x.ToString() == name))
        {
            return PrimitiveTypeMap.First(x => x.Key.ToString() == name).Value;
        }
        return _customClasses.TryGetValue(name, out var value) ? value : null;
    }

    private bool TypeMatches(DefinedType type, List<DefinedType> argumentTypes, int index)
    {
        return index < argumentTypes.Count && type.Equals(argumentTypes[index]);
    }

    private readonly Dictionary<string, DefinedType> _customClasses = new();

    public List<DefinedType> GetDefinedClasses()
    {
        return _customClasses.Values.Where(x => !x.IsCs).ToList();
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