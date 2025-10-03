namespace OLangCompiler.Tokens;

public class Tokenizer
{
    public List<IToken> Tokenize(string input)
    {
        _input = input;
        _currentIndex = 0;
        var tokens = new List<IToken>();
        while (Peek() != null)
        {
            if (LookForString("exit"))
            {
                tokens.Add(new ExitToken());
                continue;
            }

            if (char.IsDigit(Peek()!.Value))
            {
                var buffer = "";
                buffer += Consume();
                while (Peek() is { } c && char.IsDigit(c))
                {
                    buffer += Consume();
                }
                
                tokens.Add(new IntLiteralToken(int.Parse(buffer)));
                continue;
            }

            if (Peek() == ';')
            {
                Consume();
                tokens.Add(new SemicolonToken());
                continue;
            }

            if (char.IsWhiteSpace(Peek()!.Value))
            {
                Consume();
                continue;
            }

            throw new Exception($"Unexpected character: `{Peek()}`");
        }

        return tokens;
    }

    private string? _input;
    private int _currentIndex;

    private char? Peek()
    {
        if (_currentIndex >= _input!.Length) return null;

        return _input[_currentIndex];
    }

    private char? Consume()
    {
        var ret = Peek();
        _currentIndex++;

        return ret;
    }

    private bool LookForString(string expected)
    {
        var startIndex = _currentIndex;
        var buffer = "";
        while (Consume() is { } c)
        {
            buffer += c;
            if (buffer == expected)
            {
                return true;
            }

            if (!expected.StartsWith(buffer))
            {
                break;
            }
        }

        _currentIndex = startIndex;
        return false;
    }
}