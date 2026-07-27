using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser.Lr1;

public class Lr1Configuration
{
    private readonly BaseGrammarRule _rule;
    private readonly int _bookmark;
    private readonly Type _lookahead;

    private void ValidateLookaheadType(Type lookaheadType)
    {
        if (!lookaheadType.IsAssignableTo(typeof(BaseToken)))
        {
            throw new ArgumentException($"Invalid lookahead type {lookaheadType} doesn't implement {nameof(BaseToken)}");
        }
    }
    
    public Lr1Configuration(BaseGrammarRule rule, Type lookahead)
    {
        ValidateLookaheadType(lookahead);
        _rule = rule;
        _bookmark = 0;
        _lookahead = lookahead;
    }

    public Lr1Configuration(Lr1Configuration configuration, Type lookahead)
    {
        ValidateLookaheadType(lookahead);
        _rule = configuration._rule;
        _bookmark = configuration._bookmark + 1;
        _lookahead = lookahead;
        
        if (_bookmark > _rule.GetRhsTypes().Count)
        {
            throw new Exception("Bookmark cannot be greater than the number of items in the RHS of the production");
        }
    }

    public IEnumerable<Type> GetElementsAfterBookmark()
    {
        var rhsTypes = _rule.GetRhsTypes();
        return _bookmark == rhsTypes.Count ? new List<Type>() : rhsTypes.TakeLast(rhsTypes.Count - _bookmark);
    }

    public Type? GetElementAfterBookmark()
    {
        return _bookmark == _rule.GetRhsTypes().Count ? null : _rule.GetRhsTypes()[_bookmark];
    }

    public Type GetLookAhead()
    {
        return _lookahead;
    }

    public bool IsShift()
    {
        return GetElementAfterBookmark() != null;
    }

    public bool IsReduce()
    {
        return GetElementAfterBookmark() == null;
    }

    public BaseGrammarRule GetRule()
    {
        return _rule;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Lr1Configuration other) return false;
        
        return other._bookmark == _bookmark && other._rule.Equals(_rule) && other._lookahead == _lookahead;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_bookmark, _rule, _lookahead);
    }

    public override string ToString()
    {
        var typesWithBookmark = _rule.GetRhsTypes().Select(x => x.Name).ToList();
        typesWithBookmark.Insert(_bookmark, ".");
        return $"{_rule.GetLhsType().Name} -> {string.Join(" ", typesWithBookmark)}, {{{_lookahead.Name}}}";
    }
}