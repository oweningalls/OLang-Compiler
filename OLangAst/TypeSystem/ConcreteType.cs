using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace OLangAst.TypeSystem;

public class ConcreteType(string name, TypeVariant typeVariant, bool isCs = false) : IDefinedType
{
    public string Name { get; set; } = name;
    public TypeVariant TypeVariant { get; set; } = typeVariant;
    public Dictionary<string, ConcreteEnumVariant> EnumVariants = [];
    public List<ConcreteFunctionDefinition> Methods = [];
    public readonly Dictionary<string, ConcreteField> Fields = [];
    public bool IsCs = isCs;
    public Dictionary<string, ConcreteType> ConcreteTypeMap = []; // for instantiations of generics

    private static readonly Lock LockObject = new();

    public Dictionary<string, IField> GetFields()
    {
        return new Dictionary<string, IField>(Fields.Select(x => new KeyValuePair<string, IField>(x.Key, x.Value)));
    }

    public Dictionary<string, IEnumVariant> GetEnumVariants()
    {
        return new Dictionary<string, IEnumVariant>(EnumVariants.Select(x => new KeyValuePair<string, IEnumVariant>(x.Key, x.Value)));
    }

    public static ConcreteType FromCsType(System.Type type)
    {
        ConcreteType concreteType;
        lock (LockObject)
        {
            if (_typeMap.TryGetValue(type, out var savedType))
            {
                return savedType;
            }

            concreteType = new ConcreteType(type.Name, GetCsTypeVariant(type), true);
            _typeMap[type] = concreteType;
        }

        // ignoring type arguments for C# types
        // foreach (var typeArgument in type.GenericTypeArguments.Zip(type.Generic))
        // {
        //     concreteType.ConcreteTypeMap.Add(null, FromCsType(typeArgument));
        // }

        foreach (var method in type.GetMethods())
        {
            concreteType.Methods.Add(new ConcreteFunctionDefinition(concreteType, FromCsType(method.ReturnType),
                method.Name,
                method.GetParameters().ToArray()
                    .Select(parameter => new ConcreteParameter(parameter.Name!,
                        FromCsType(parameter.ParameterType)))
                    .ToList(), !method.IsStatic));
        }

        return concreteType;
    }

    private static TypeVariant GetCsTypeVariant(System.Type type)
    {
        if (type.IsEnum)
        {
            return TypeVariant.Enum;
        }

        return TypeVariant.Class;
    }

    private static ConcurrentDictionary<System.Type, ConcreteType> _typeMap = new();

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConcreteType concreteType && concreteType.Name == Name && ConcreteTypeMap.Keys.All(x => ConcreteTypeMap[x] == concreteType.ConcreteTypeMap[x]);
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