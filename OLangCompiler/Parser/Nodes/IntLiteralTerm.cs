namespace OLangCompiler.Parser.Nodes;

public class IntLiteralTerm(int value) : ITermNode
{
    public int Value = value;
}