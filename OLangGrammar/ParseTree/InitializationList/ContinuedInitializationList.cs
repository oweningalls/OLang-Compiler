using Lexing;
using OLangGrammar.ParseTree.FieldInitialization;

namespace OLangGrammar.ParseTree.InitializationList;

public class ContinuedInitializationList(IFieldInitialization fieldInitialization, IInitializationList initializationList) : IInitializationList
{
    public IFieldInitialization FieldInitialization = fieldInitialization;
    public IInitializationList InitializationList = initializationList;
    public SourceSpan Span { get; set; }
}