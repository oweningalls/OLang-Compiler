using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace OLangAst.TypeSystem;

public struct DefinedType(string name, bool isStatic, bool isCs = false)
{
    public string Name = name;
    public bool IsStatic = isStatic;
    public List<FunctionDefinition> Methods = [];
    public bool IsCs = isCs;

    private static readonly Lock _lockObject = new();

    public static DefinedType FromCsType(Type type)
    {
        DefinedType definedType;
        lock (_lockObject)
        {
            if (_typeMap.TryGetValue(type, out var savedType))
            {
                return savedType;
            }
        
            definedType = new DefinedType(type.Name, type.IsSealed && type.IsAbstract, true);
            _typeMap[type] = definedType;
        }

        foreach (var method in type.GetMethods())
        {
            definedType.Methods.Add(new FunctionDefinition(definedType, FromCsType(method.ReturnType),
                method.Name,
                method.GetParameters().ToArray()
                    .Select(parameter => new Parameter(parameter.Name,
                        FromCsType(parameter.ParameterType)))
                    .ToList()));
        }

        return definedType;
    }
    
    private static ConcurrentDictionary<Type, DefinedType> _typeMap = new();

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