namespace OLangCompiler.Parser.Nodes;

public class BoolLiteralTerm(bool value) : ITermNode
{
    public bool Value = value;
}