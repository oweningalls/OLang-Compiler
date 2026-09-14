using ErrorHelper;
using Lexing;
using OLangAst.ClassMembers;
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
        return type switch
        {
            CustomType userDefinedClass => GetDefinedType(userDefinedClass.Name) ?? throw errorHelper.ShowErrorMessage($"Unknown type: `{type.Name}", type.Span),
            PrimitiveVariableType primitiveVariableType => PrimitiveTypeMap[primitiveVariableType.Type],
            _ => throw errorHelper.UnknownVariant("type", type.GetType())
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
        // arguments for instance method
        // new List<IVariableType> { customClass }.Concat(methodDeclaration.Parameters.Select(x => x.Type)).Select(GetCsType).ToArray()
        var method = new FunctionDefinition(GetMethodType(methodDeclaration.DeclaredType), methodDeclaration.Identifier, methodDeclaration.Parameters.Select(x => new Parameter(x.Identifier, GetLocalType(x.DeclaredType))));
        customType.Methods.Add(method);

        return method;
    }

    public DefinedType CreateCustomClass(string name, bool isStatic)
    {
        var userDefined = new DefinedType(name, isStatic);
        _customClasses.Add(name, userDefined);
        
        return userDefined;
    }

    public DefinedType? GetDefinedType(string name)
    {
        return _customClasses.GetValueOrDefault(name);
    }

    private bool TypeMatches(DefinedType type, List<DefinedType> argumentTypes, int index)
    {
        return index < argumentTypes.Count && type.Equals(argumentTypes[index]);
    }

    private readonly Dictionary<string, DefinedType> _customClasses = new();

    public List<DefinedType> GetDefinedClasses()
    {
        return _customClasses.Values.ToList();
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