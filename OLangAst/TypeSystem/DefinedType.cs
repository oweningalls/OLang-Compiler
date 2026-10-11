namespace OLangAst.TypeSystem;

public class DefinedType(string name, TypeVariant typeVariant, List<string> typeParameters) : IDefinedType
{
    public string Name { get; } = name;
    public TypeVariant TypeVariant { get; } = typeVariant;
    public readonly List<string> TypeParameters = typeParameters;
    public Dictionary<string, DefinedEnumVariant> EnumVariants = [];
    public List<FunctionDefinition> Methods = [];
    public Dictionary<string, FieldDefinition> Fields = [];

    public Dictionary<string, IField> GetFields()
    {
        return new Dictionary<string, IField>(Fields.Select(x => new KeyValuePair<string, IField>(x.Key, x.Value)));
    }
    
    public Dictionary<string, IEnumVariant> GetEnumVariants()
    {
        return new Dictionary<string, IEnumVariant>(EnumVariants.Select(x => new KeyValuePair<string, IEnumVariant>(x.Key, x.Value)));
    }

    public ConcreteType MakeConcrete(List<ConcreteType> typeArguments)
    {
        var type = new ConcreteType(Name, TypeVariant);
        if (typeArguments.Count != TypeParameters.Count)
        {
            throw new Exception($"Got {typeArguments.Count} type arguments but expected {TypeParameters.Count}");
        }

        foreach (var (name, argType) in TypeParameters.Zip(typeArguments))
        {
            type.ConcreteTypeMap.Add(name, argType);
        }

        return type;
    }

    public override bool Equals(object? obj)
    {
        return obj is DefinedType definedType && definedType.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}