using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser;

public class GrammarRule : BaseGrammarRule
{
    private GrammarRule(Func<List<IGrammarElement>, INode> reduce, Type lhsType, params Type[] types)
    {
        _reduce = reduce;
        _rhsTypes = types.ToList();
        _lhsType = lhsType;
    }

    public static GrammarRule Create<TNode>(Func<TNode> reduce)
        where TNode : INode
    {
        return new GrammarRule(_ => reduce(), typeof(TNode));
    }

    public static GrammarRule Create<TNode, T1>(Func<T1, TNode> reduce)
        where T1 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0]), typeof(TNode), typeof(T1));
    }

    public static GrammarRule Create<TNode, T1, T2>(Func<T1, T2, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0], (T2)x[1]), typeof(TNode), typeof(T1), typeof(T2));
    }

    public static GrammarRule Create<TNode, T1, T2, T3>(Func<T1, T2, T3, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where T3 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0], (T2)x[1], (T3)x[2]),
            typeof(TNode), typeof(T1), typeof(T2), typeof(T3));
    }

    public static GrammarRule Create<TNode, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where T3 : IGrammarElement
        where T4 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3]),
            typeof(TNode), typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public static GrammarRule Create<TNode, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where T3 : IGrammarElement
        where T4 : IGrammarElement
        where T5 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4]),
            typeof(TNode), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    public static GrammarRule Create<TNode, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where T3 : IGrammarElement
        where T4 : IGrammarElement
        where T5 : IGrammarElement
        where T6 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(x => reduce((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4], (T6)x[5]),
            typeof(TNode), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    public static GrammarRule Create<TNode, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TNode> reduce)
        where T1 : IGrammarElement
        where T2 : IGrammarElement
        where T3 : IGrammarElement
        where T4 : IGrammarElement
        where T5 : IGrammarElement
        where T6 : IGrammarElement
        where T7 : IGrammarElement
        where TNode : INode
    {
        return new GrammarRule(
            x => reduce((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4], (T6)x[5], (T7)x[6]),
            typeof(TNode), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)
        );
    }

    private readonly Func<List<IGrammarElement>, INode> _reduce;
    private readonly IReadOnlyList<Type> _rhsTypes;
    private readonly Type _lhsType;

    public override Type GetLhsType()
    {
        return _lhsType;
    }

    public override IReadOnlyList<Type> GetRhsTypes()
    {
        return _rhsTypes;
    }

    public override INode ReduceRule(List<IGrammarElement> rhs)
    {
        if (rhs.Count != _rhsTypes.Count)
        {
            throw new Exception($"Incorrect number of grammar elements to reduce. Expected {_rhsTypes.Count} but was {rhs.Count}");
        }
        
        var theseRhsTypes = rhs.Select(x => x.GetType()).ToList();
        for (var i = 0; i < _rhsTypes.Count; i++)
        {
            var expected = _rhsTypes[i];
            var actual = theseRhsTypes[i];

            if (!expected.IsAssignableFrom(actual))
            {
                throw new Exception($"Incorrect rhs type at index {i}. Expected {expected.Name} but was {actual.Name}");
            }
        }
        
        return _reduce.Invoke(rhs);
    }
}