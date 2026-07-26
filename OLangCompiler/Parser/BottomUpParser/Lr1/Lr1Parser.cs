using OLangCompiler.Parser.BottomUpParser.ParseActions;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser.Lr1;

public class Lr1Parser : IParser
{
    private Stack<IGrammarElement> _input;
    private Stack<ParseState> _stateStack;
    
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, ErrorHelper errorHelper)
    {
        var parseTable = new Lr1ParseTable(grammar, errorHelper);
        _stateStack = new Stack<ParseState>();
        _stateStack.Push(new ParseState(null, 0));
        SetInput(tokens);

        do
        {
            var input = GetNextInput();
            var action = parseTable.GetActionAndState(input, GetLookahead(), GetCurrentState());
            switch (action)
            {
                case Accept accept:
                    return accept.Node;
                case Shift shift:
                    AppendState(input, shift.NewState);
                    break;
                case Reduce reduce:
                    var lhs = DoReduce(reduce.Rule);
                    PrependToInput(lhs);
                    break;
                default:
                    throw errorHelper.UnknownVariant("parse action", action.GetType());
            }
        } while (true);
    }
    
    private void SetInput(List<BaseToken> tokens)
    {
        var reverseTokens = tokens.Append(new Lr1ParseTable.EOI()).AsEnumerable().ToList();
        reverseTokens.Reverse();
        _input = new Stack<IGrammarElement>(reverseTokens);
    }

    private void AppendState(IGrammarElement element, int state)
    {
        _stateStack.Push(new ParseState(element, state));
    }

    private int GetCurrentState()
    {
        return _stateStack.Peek().State;
    }

    private IGrammarElement? GetNextInput()
    {
        return _input.Count != 0 ? _input.Pop() : null;
    }

    private Type? GetLookahead()
    {
        return _input.Count > 1 ? _input.ElementAt(1).GetType() : typeof(Lr1ParseTable.EOI);
    }

    private void PrependToInput(IGrammarElement element)
    {
        _input.Push(element);
    }

    private INode DoReduce(BaseGrammarRule rule)
    {
        var count = rule.GetRhsTypes().Count;

        var elements = PopStates(count);
        return rule.ReduceRule(elements);
    }

    private List<IGrammarElement> PopStates(int count)
    {
        var elements = new List<IGrammarElement>();
        for (var i = 0; i < count; i++)
        {
            var parseState = _stateStack.Pop();
            elements.Add(parseState.Element!);
        }

        elements.Reverse();
        return elements;
    }
    
    private class ParseState(IGrammarElement? element, int state)
    {
        public IGrammarElement? Element = element;
        public int State = state;
    }
}