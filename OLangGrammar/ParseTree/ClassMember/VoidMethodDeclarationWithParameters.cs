using Lexing;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class VoidMethodDeclarationWithParameters(IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IClassMember
{
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}