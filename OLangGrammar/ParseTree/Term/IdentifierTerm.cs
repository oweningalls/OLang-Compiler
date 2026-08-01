using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class IdentifierTerm(IdentifierToken identifier) : ITerm
{
    public IdentifierToken Identifier = identifier;
}