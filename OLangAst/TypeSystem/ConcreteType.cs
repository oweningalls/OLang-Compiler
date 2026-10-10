using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace OLangAst.TypeSystem;

public struct ConcreteType(string name, TypeVariant typeVariant, bool isCs = false, bool isArray = false)
{
    public string Name = name;
    public TypeVariant TypeVariant = typeVariant;
    public Dictionary<string, DefinedEnumVariant> EnumVariants = [];
    public List<FunctionDefinition> Methods = [];
    public Dictionary<string, FieldDefinition> Fields = [];
    public bool IsCs = isCs;
    public bool IsArray = isArray;
    public List<ConcreteType> ConcreteTypeParameters = []; // for instantiations of generics

    private static readonly Lock _lockObject = new();

    public static ConcreteType FromCsType(Type type)
    {
        ConcreteType concreteType;
        lock (_lockObject)
        {
            if (_typeMap.TryGetValue(type, out var savedType))
            {
                return savedType;
            }

            concreteType = new ConcreteType(type.Name, GetCsTypeVariant(type), true);
            _typeMap[type] = concreteType;
        }

        foreach (var typeArgument in type.GenericTypeArguments)
        {
            concreteType.ConcreteTypeParameters.Add(FromCsType(typeArgument));
        }

        foreach (var method in type.GetMethods())
        {
            concreteType.Methods.Add(new FunctionDefinition(concreteType, FromCsType(method.ReturnType),
                method.Name,
                method.GetParameters().ToArray()
                    .Select(parameter => new Parameter(parameter.Name,
                        FromCsType(parameter.ParameterType)))
                    .ToList(), !method.IsStatic));
        }

        return concreteType;
    }

    private static TypeVariant GetCsTypeVariant(Type type)
    {
        if (type.IsEnum)
        {
            return TypeVariant.Enum;
        }

        return TypeVariant.Class;
    }

    private static ConcurrentDictionary<Type, ConcreteType> _typeMap = new();

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConcreteType definedType && definedType.Name == Name && IsArray == definedType.IsArray && ConcreteTypeParameters.SequenceEqual(definedType.ConcreteTypeParameters);
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

public enum TypeVariant
{
    Class,
    Enum
}