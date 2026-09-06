using System.Reflection;
using ErrorHelper;
using OLangAst.Miscellaneous;

namespace AstHelpers;

public class TypeHelper(IErrorHelper errorHelper)
{
    private IErrorHelper _errorHelper = errorHelper;
    
    public Type GetCsType(IVariableType type)
    {
        if (type is not PrimitiveVariableType primitiveVariableType)
        {
            throw _errorHelper.UnknownVariant("type", type.GetType());
        }

        return PrimitiveTypeMap[primitiveVariableType];
    }

    public IVariableType GetLocalType(Type type)
    {
        return ReversePrimitiveTypeMap.TryGetValue(type, out var value) ? value : throw _errorHelper.UnknownVariant("type", type);
    }

    public MethodInfo GetMethod(IVariableType type, string name, List<IVariableType> argumentTypes)
    {
        var csType = GetCsType(type);
        var matchingMethods = csType.GetMethods().Where(x => x.Name == name && x.GetParameters().Index().All(y => TypeMatches(y.Item.ParameterType, argumentTypes, y.Index))).ToList();

        return matchingMethods.Count switch
        {
            0 => throw _errorHelper.ShowErrorMessage($"Couldn't find method `{name}` with matching signature.", type.Span),
            > 1 => throw _errorHelper.ShowErrorMessage($"Couldn't uniquely identify method `{name}`.", type.Span),
            _ => matchingMethods.Single()
        };
    }

    private bool TypeMatches(Type type, List<IVariableType> argumentTypes, int index)
    {
        if (index >= argumentTypes.Count)
        {
            return false;
        }

        return GetCsType(argumentTypes[index]) == type;
    }

    private static readonly Dictionary<PrimitiveVariableType, Type> PrimitiveTypeMap = new()
    {
        { PrimitiveVariableType.IntType, typeof(int) },
        { PrimitiveVariableType.BoolType, typeof(bool) },
        { PrimitiveVariableType.FloatType, typeof(float) },
        { PrimitiveVariableType.StringType, typeof(string) },
    };

    private static readonly Dictionary<Type, PrimitiveVariableType> ReversePrimitiveTypeMap = PrimitiveTypeMap.ToDictionary(x => x.Value, x => x.Key);
}