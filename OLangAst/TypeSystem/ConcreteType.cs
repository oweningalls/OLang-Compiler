using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace OLangAst.TypeSystem;

public class ConcreteType : IDefinedType
{
    public DefinedType GenericType { get; }
    public string Name => GenericType.Name;
    public TypeVariant TypeVariant => GenericType.TypeVariant;
    public readonly Dictionary<string, IType> ConcreteTypeMap;

    public ConcreteType(DefinedType abstractType, Dictionary<string, IType> typeMap)
    {
        foreach (var typeParameter in abstractType.TypeParameters)
        {
            if (!typeMap.ContainsKey(typeParameter))
            {
                throw new Exception($"Type parameter {typeParameter} wasn't given a concrete type.");
            }
        }
        GenericType = abstractType;
        ConcreteTypeMap = typeMap;
    }

    public IType ResolveType(IType type)
    {
        if (type is GenericType generic)
        {
            return ConcreteTypeMap.GetValueOrDefault(generic.TypeParameterName) ?? type;
        }

        return type;
    }

    public IEnumerable<IType> GetOrderedTypeArguments()
    {
        return GenericType.TypeParameters.Select(x => ConcreteTypeMap[x]);
    }

    private static readonly Lock LockObject = new();

    public List<FunctionDefinition> GetMethods()
    {
        return GenericType.Methods;
    }
    
    public Dictionary<string, FieldDefinition> GetFields()
    {
        return GenericType.GetFields();
    }

    public Dictionary<string, DefinedEnumVariant> GetEnumVariants()
    {
        return GenericType.GetEnumVariants();
    }

    public static DefinedType FromCsType(Type type)
    {
        DefinedType concreteType;
        lock (LockObject)
        {
            if (_typeMap.TryGetValue(type, out var savedType))
            {
                return savedType;
            }

            concreteType = new DefinedType(type.Name, GetCsTypeVariant(type), [], true);
            _typeMap[type] = concreteType;
        }

        // foreach (var typeArgument in type.GenericTypeArguments.Zip(type.Generic))
        // {
        //     concreteType.ConcreteTypeMap.Add(null, FromCsType(typeArgument));
        // }

        foreach (var method in type.GetMethods())
        {
            concreteType.Methods.Add(new FunctionDefinition(concreteType, FromCsType(method.ReturnType),
                method.Name,
                method.GetParameters()
                    .Select(parameter => new DeclaredParameter(parameter.Name!,
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

    private static ConcurrentDictionary<Type, DefinedType> _typeMap = new();

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConcreteType concreteType && concreteType.Name == Name && ConcreteTypeMap.Keys.All(x => ConcreteTypeMap[x] == concreteType.ConcreteTypeMap[x]);
    }

    public override string ToString()
    {
        return $"{Name}<{string.Join(", ", GetOrderedTypeArguments().Select(x => x.ToString()))}>";
    }

    public override int GetHashCode()
    {
        var hash = Name.GetHashCode();

        return ConcreteTypeMap.OrderBy(x => x.Key).Select(x => HashCode.Combine(x.Key, x.Value)).Aggregate(hash, HashCode.Combine);
    }
}

public enum TypeVariant
{
    Class,
    Enum
}