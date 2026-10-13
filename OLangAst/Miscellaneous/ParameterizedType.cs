using Lexing;

namespace OLangAst.Miscellaneous;

public class ParameterizedType(IVariableType genericType, List<IVariableType> typeArguments) : IVariableType
{
    public IVariableType GenericType = genericType;
    public List<IVariableType> TypeArguments = typeArguments;
    public SourceSpan Span { get; set; }
    public string Name => $"{GenericType.Name}<{string.Join(", ",TypeArguments.Select(x => x.Name))}>";
}