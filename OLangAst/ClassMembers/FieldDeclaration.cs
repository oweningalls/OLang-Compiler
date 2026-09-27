using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.ClassMembers;

public class FieldDeclaration(IVariableType type, string identifier) : IClassMember
{
    public IVariableType Type = type;
    public string Identifier = identifier;
    public SourceSpan Span { get; set; }
}