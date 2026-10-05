using Lexing;
using OLangGrammar.ParseTree.FieldInitialization;

namespace OLangGrammar.ParseTree.InitializationList;

public class SingleFieldInitializationList(IFieldInitialization fieldInitialization) : IInitializationList
{
    public IFieldInitialization fieldInitialization { get; } = fieldInitialization;
    public SourceSpan Span { get; set; }
}