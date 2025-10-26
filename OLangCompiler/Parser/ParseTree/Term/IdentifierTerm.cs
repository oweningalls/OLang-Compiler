using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Term;

public class IdentifierTerm(IdentifierToken identifier) : ITerm
{
    public IdentifierToken Identifier = identifier;
}