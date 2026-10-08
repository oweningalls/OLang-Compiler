using Lexing;
using OLangGrammar.ParseTree.EnumInstantiation;

namespace OLangGrammar.ParseTree.Term;

public class EnumInstantiationTerm(IEnumInstantiation enumInstantiation) : ITerm
{
    public IEnumInstantiation EnumInstantiation = enumInstantiation;
    public SourceSpan Span { get; set; }
}