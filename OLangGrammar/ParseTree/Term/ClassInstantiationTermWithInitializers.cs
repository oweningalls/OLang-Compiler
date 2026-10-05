using Lexing;
using OLangGrammar.ParseTree.InitializationList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ClassInstantiationTermWithInitializers(IdentifierToken identifier, IInitializationList initializationList) : ITerm
{
    public IdentifierToken Identifier = identifier;
    public IInitializationList InitializationList = initializationList;
    public SourceSpan Span { get; set; }
}