using OLangCompiler.Parser.BottomUpParser.ParseActions;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser.Lr1;

public class Lr1ParseTable : ILr1ParseTable
{
    private IGrammar _grammar;
    private GrammarHelper _grammarHelper;
    private List<HashSet<Lr1Configuration>> _states = [];
    private List<Dictionary<Type, int>> _transitions = [];
    private IErrorHelper _errorHelper;

    public Lr1ParseTable(IGrammar grammar, IErrorHelper errorHelper)
    {
        _grammar = grammar;
        _errorHelper = errorHelper;
        _grammarHelper = new GrammarHelper(grammar);
        CreateTable();
    }

    private void CreateTable()
    {
        var augmentStart = new AugmentStartRule(_grammar.GetStartSymbol());
        var startState = GetClosure(new Lr1Configuration(augmentStart, typeof(EOI)));
        var firstState = CreateState(startState);
        var newStates = MakeTransitionableStates(firstState);

        while (newStates.Count != 0)
        {
            newStates = newStates.SelectMany(MakeTransitionableStates).ToList();
        }
    }

    private List<int> MakeTransitionableStates(int startState)
    {
        var connectedStates = GetConnectedStates(_states[startState]);
        var newStates = new List<int>();
        foreach (var transition in connectedStates)
        {
            if (AddStateAndTransition(transition.Item1, startState, transition.Item2) is { } state)
            {
                newStates.Add(state);
            }
        }

        return newStates;
    }
    
    private List<(Type, HashSet<Lr1Configuration>)> GetConnectedStates(HashSet<Lr1Configuration> state)
    {
        return state.Where(x => x.GetElementAfterBookmark() != null).Select(x =>
            {
                var lookahead = x.GetLookAhead();
                var bookmarkAdjusted = new Lr1Configuration(x, lookahead);
                
                return new { element = x.GetElementAfterBookmark(), configurations = GetClosure(bookmarkAdjusted) };
            })
            .GroupBy(x => x.element)
            .Select(x => (x.Key, x.SelectMany(y => y.configurations).ToHashSet()))
            .Cast<(Type, HashSet<Lr1Configuration>)>()
            .ToList();
    }

    // returns new state if created. returns null if transition to existing state was added
    private int? AddStateAndTransition(Type element, int fromState, HashSet<Lr1Configuration> toState)
    {
        var possibleIdenticalState = GetExistingIdenticalState(toState);
        if (possibleIdenticalState is { } existingState)
        {
            AddTransition(element, fromState, existingState);
            return null;
        }

        var newState = CreateState(toState);
        AddTransition(element, fromState, newState);

        return newState;
    }
    
    private int? GetExistingIdenticalState(HashSet<Lr1Configuration> state)
    {
        return _states
            .Select((x, i) => new { Index = i, Item = x })
            .SingleOrDefault(x => x.Item.SetEquals(state))?.Index;
    }


    private void AddTransition(Type element, int fromState, int toState)
    {
        if (_transitions[fromState].ContainsKey(element))
        {
            throw new Exception($"State {fromState} already has a transition on {element}");
        }
        _transitions[fromState][element] = toState;
    }

    private int CreateState(HashSet<Lr1Configuration> state)
    {
        var newState = _states.Count;
        Lr1ConflictHelper.CheckForConflicts(state);

        _states.Add(state);
        _transitions.Add(new Dictionary<Type, int>());

        return newState;
    }

    private HashSet<Lr1Configuration> GetClosure(Lr1Configuration configuration)
    {
        var closure = new HashSet<Lr1Configuration>();
        List<Lr1Configuration> previouslyAdded = [configuration];
        
        while (previouslyAdded.Count != 0)
        {
            closure.UnionWith(previouslyAdded);
            var newAdditions = previouslyAdded.SelectMany(ExpandNonTerminalForClosure);

            previouslyAdded = newAdditions.Distinct().Except(closure).ToList();
        }

        return closure;
    }

    private List<Lr1Configuration> ExpandNonTerminalForClosure(Lr1Configuration addedConfiguration)
    {
        var newAdditions = new List<Lr1Configuration>();
        var afterBookmark = addedConfiguration.GetElementsAfterBookmark().ToList();
        if (afterBookmark.Count == 0)
        {
            return [];
        }
            
        foreach (var grammarRule in _grammarHelper.GetProductionsFor(afterBookmark[0]))
        {
            var terminals = GetTerminals(addedConfiguration, afterBookmark);
                
            newAdditions.AddRange(terminals.Select(terminal => new Lr1Configuration(grammarRule, terminal)));
        }

        return newAdditions;
    }

    private List<Type> GetTerminals(Lr1Configuration addedConfiguration, List<Type> elementsAfterBookmark)
    {
        if (elementsAfterBookmark.Count < 2)
        {
            return [addedConfiguration.GetLookAhead()];
        }

        var first = _grammarHelper.First(elementsAfterBookmark[1]).ToList();
        if (first.Contains(typeof(Epsilon)))
        {
            first.Add(addedConfiguration.GetLookAhead());
        }

        return first;
    }

    private static Type GetInterface(Type type)
    {
        var interfaceType = type.GetInterfaces()
            .Except(type.GetInterfaces().SelectMany(i => i.GetInterfaces())).Single();
        return interfaceType;
    }

    public IParseAction GetActionAndState(IGrammarElement? next, Type? lookahead, int state)
    {
        if (_states[state].Count == 1 && _states[state].Single() is {} reduce && reduce.IsReduce() && reduce.GetLookAhead() == lookahead)
        {
            return new Reduce(reduce.GetRule());
        }

        if (next is null)
        {
            throw new Exception("Unexpected end of input");
        }
        
        if (next is INode node && _grammar.GetStartSymbol().IsInstanceOfType(node))
        {
            return new Accept(node);
        }
        
        if (_transitions[state].TryGetValue(next.GetType(), out var newState))
        {
            return new Shift(newState);
        }
        
        if (_transitions[state].TryGetValue(GetInterface(next.GetType()), out var newState2))
        {
            return new Shift(newState2);
        }

        throw _errorHelper.ShowErrorMessageAtElement("Parsing error: unexpected token", next);
    }

    private class AugmentStartSymbol(INode program) : INode
    {
        public INode Program = program;
    }

    private class AugmentStartRule : BaseGrammarRule
    {
        private readonly Func<List<IGrammarElement>, INode> _reduce;
        private readonly IReadOnlyList<Type> _rhsTypes;

        public AugmentStartRule(Type originalStartSymbol)
        {
            _reduce = x => new AugmentStartSymbol((INode)x.Single());
            _rhsTypes = [originalStartSymbol, typeof(EOI)];
        }

        public override Type GetLhsType()
        {
            return typeof(AugmentStartSymbol);
        }

        public override IReadOnlyList<Type> GetRhsTypes()
        {
            return _rhsTypes;
        }

        public override INode ReduceRule(List<IGrammarElement> rhs)
        {
            return _reduce.Invoke(rhs);
        }
    }
    
    public class EOI : BaseToken;
}