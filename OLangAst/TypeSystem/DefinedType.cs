using System.Diagnostics.CodeAnalysis;

namespace OLangAst.TypeSystem;

public struct DefinedType(string name, bool isStatic, bool isCs = false)
{
    public string Name = name;
    public bool IsStatic = isStatic;
    public List<FunctionDefinition> Methods = [];
    public bool IsCs = isCs;

    public static DefinedType FromCsType(Type type)
    {
        if (_typeMap.TryGetValue(type, out var savedType))
        {
            return savedType;
        }
        
        var definedType = new DefinedType(type.Name, type.IsSealed && type.IsAbstract, true);
        _typeMap[type] = definedType;

        foreach (var method in type.GetMethods())
        {
            definedType.Methods.Add(new FunctionDefinition(definedType, FromCsType(method.ReturnType),
                method.Name,
                method.GetParameters()
                    .Select(parameter => new Parameter(parameter.Name,
                        FromCsType(parameter.ParameterType)))
                    .ToList()));
        }

        return definedType;
    }
    
    private static Dictionary<Type, DefinedType> _typeMap = new();

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DefinedType definedType && definedType.Name == Name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}