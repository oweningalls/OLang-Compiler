using OLangCompiler.Parser.BottomUpParser.ParseActions;
using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser.Lr0;

public class Lr0ParseTable : ILr0ParseTable
{
    private IGrammar _grammar;
    private GrammarHelper _grammarHelper;
    private List<HashSet<Lr0Configuration>> _states;
    private List<Dictionary<Type, int>> _transitions;

    public Lr0ParseTable(IGrammar grammar)
    {
        _grammar = grammar;
        _grammarHelper = new GrammarHelper(grammar);
        _transitions = new List<Dictionary<Type, int>>();
        _states = [];
        InitializeTable();
    }

    private void InitializeTable()
    {
        var augmentStart = new AugmentStartRule(GetInterface(_grammar.GetStartSymbol()));
        var startState = GetClosure(new Lr0Configuration(augmentStart));
        CreateState(startState);
        var newStates = MakeTransitionableStates(0);

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
            var newState = AddStateAndTransition(transition.Item1, startState, transition.Item2);
            if (newState is { } state)
            {
                newStates.Add(state);
            }
        }

        return newStates;
    }
    
    private List<(Type, HashSet<Lr0Configuration>)> GetConnectedStates(HashSet<Lr0Configuration> state)
    {
        return state.Where(x => x.GetElementAfterBookmark() != null).Select(x =>
            {
                var bookmarkAdjusted = new Lr0Configuration(x);
                return new { element = x.GetElementAfterBookmark(), configurations = GetClosure(bookmarkAdjusted) };
            })
            .GroupBy(x => x.element)
            .Select(x => (x.Key, x.SelectMany(y => y.configurations).ToHashSet()))
            .Cast<(Type, HashSet<Lr0Configuration>)>()
            .ToList();
    }

    private int? GetExistingIdenticalState(HashSet<Lr0Configuration> state)
    {
        return _states
            .Select((x, i) => new { Index = i, Item = x })
            .SingleOrDefault(x => x.Item.SetEquals(state))?.Index;
    }

    // returns new state if created. returns null if transition to existing state was added
    private int? AddStateAndTransition(Type element, int fromState, HashSet<Lr0Configuration> state)
    {
        var possibleIdenticalState = GetExistingIdenticalState(state);
        if (possibleIdenticalState is { } existingState)
        {
            AddTransition(element, fromState, existingState);
            return null;
        }
        else
        {
            return AddTransition(element, fromState, state);
        }
    }

    private int AddTransition(Type element, int fromState, HashSet<Lr0Configuration> state)
    {
        var newState = CreateState(state);
        AddTransition(element, fromState, newState);

        return newState;
    }

    private void AddTransition(Type element, int fromState, int toState)
    {
        if (_transitions[fromState].ContainsKey(element))
        {
            throw new Exception($"State {fromState} already has a transition on {element}");
        }
        _transitions[fromState][element] = toState;
    }

    private int CreateState(HashSet<Lr0Configuration> state)
    {
        if (state.Any(x => x.IsReduce()))
        {
            if (state.Any(x => x.IsShift())) throw new Exception("Shift/reduce conflict");
            if (state.Count != 1) throw new Exception("Reduce/reduce conflict");
        }
        
        _states.Add(state);
        _transitions.Add(new Dictionary<Type, int>());
        var newState = _states.Count - 1;

        return newState;
    }

    private HashSet<Lr0Configuration> GetClosure(Lr0Configuration configuration)
    {
        var closure = new HashSet<Lr0Configuration>();
        Lr0Configuration[] previouslyAdded = [configuration];
        
        while (previouslyAdded.Length != 0)
        {
            closure.UnionWith(previouslyAdded);

            previouslyAdded = previouslyAdded
                .Select(x => x.GetElementAfterBookmark()).OfType<Type>()
                .Where(x => x.GetInterfaces().Contains(typeof(INode)))
                .SelectMany(x => _grammarHelper.GetProductionsFor(x).Select(y => new Lr0Configuration(y)))
                .Where(x => !closure.Contains(x)
                ).ToArray();
        }

        return closure;
    }
    
    private static Type GetInterface(Type type)
    {
        var interfaceType = type.GetInterfaces()
            .Except(type.GetInterfaces().SelectMany(i => i.GetInterfaces())).Single();
        return interfaceType;
    }

    public IParseAction GetActionAndTransition(IGrammarElement next)
    {
        throw new NotImplementedException();
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
            _reduce = x => new AugmentStartSymbol((INode)Convert.ChangeType(x.Single(), originalStartSymbol));
            _rhsTypes = [originalStartSymbol];
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
}