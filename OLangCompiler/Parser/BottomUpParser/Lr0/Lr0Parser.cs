using OLangCompiler.Parser.BottomUpParser.ParseActions;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser.Lr0;

public class Lr0Parser : IParser
{
    private Stack<IGrammarElement> _input;
    private Stack<ParseState> _stateStack;
    
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper)
    {
        var parseTable = new Lr0ParseTable(grammar, errorHelper);
        _stateStack = new Stack<ParseState>();
        _stateStack.Push(new ParseState(null, 0));
        SetInput(tokens);

        IParseAction action;
        do
        {
            var input = PeekNextInput();
            action = parseTable.GetActionAndState(input, GetCurrentState());
            switch (action)
            {
                case Accept accept:
                    return accept.Node;
                case Shift shift:
                    AppendState(input, shift.NewState);
                    _input.Pop();
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
        var reverseTokens = tokens.AsEnumerable().ToList();
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

    private IGrammarElement? PeekNextInput()
    {
        return _input.TryPeek(out var value) ? value : null;
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