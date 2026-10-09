using Lexing;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class MethodDeclarationWithParameters(IType type, IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope, bool isInstance) : IClassMember
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
    public bool IsInstance = isInstance;
    public SourceSpan Span { get; set; }
}