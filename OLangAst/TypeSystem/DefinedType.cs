namespace OLangAst.TypeSystem;

public class DefinedType(string name, TypeVariant typeVariant, List<string> typeParameters, bool isCs) : IDefinedType
{
    public string Name { get; } = name;
    public TypeVariant TypeVariant { get; } = typeVariant;
    public readonly List<string> TypeParameters = typeParameters;
    public readonly bool IsCs = isCs;
    public Dictionary<string, DefinedEnumVariant> EnumVariants = [];
    public List<FunctionDefinition> Methods = [];
    public Dictionary<string, FieldDefinition> Fields = [];
    public bool IsGeneric => TypeParameters.Count != 0;

    public List<FunctionDefinition> GetMethods()
    {
        return Methods;
    }
    
    public Dictionary<string, FieldDefinition> GetFields()
    {
        return new Dictionary<string, FieldDefinition>(Fields.Select(x => new KeyValuePair<string, FieldDefinition>(x.Key, x.Value)));
    }
    
    public Dictionary<string, DefinedEnumVariant> GetEnumVariants()
    {
        return EnumVariants;
    }

    public ConcreteType MakeConcrete(List<IType> typeArguments)
    {
        var typeMap = new Dictionary<string, IType>();
        foreach (var (name, argType) in TypeParameters.Zip(typeArguments))
        {
            typeMap.Add(name, argType);
        }
        var type = new ConcreteType(this, typeMap);

        return type;
    }

    public override bool Equals(object? obj)
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