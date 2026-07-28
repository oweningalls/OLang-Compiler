using Lexing;

namespace OLangCompiler.Parser.BottomUpParser;

public abstract class BaseGrammarRule
{
    public abstract Type GetLhsType();
    public abstract IReadOnlyList<Type> GetRhsTypes();
    public abstract INode ReduceRule(List<IGrammarElement> rhs);

    public override string ToString()
    {
        return $"{GetLhsType().Name} -> {string.Join(" ", GetRhsTypes().Select(x => x.Name).ToList())}";
    }

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(GetLhsType());
        foreach (var item in GetRhsTypes())
        {
            hc.Add(item);
        }
        return hc.ToHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseGrammarRule other) return false;

        return GetLhsType() == other.GetLhsType() && GetRhsTypes().SequenceEqual(other.GetRhsTypes());
    }
}