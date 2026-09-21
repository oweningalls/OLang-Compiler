using System.Text;
using ErrorHelper;
using Lexing;
using OLangTokens.Tokens;

namespace OLangLexing;

public class Tokenizer
{
    private IErrorHelper _errorHelper;

    public List<BaseToken> Tokenize(string input, IErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _input = input;
        _currentIndex = 0;
        var tokens = new List<BaseToken>();
        var absoluteCharacterNumber = _absoluteCharacterNumber;

        SkipWhitespaceAndComments();
        var token = GetNextToken();
        SetLineAndCharNumbers(token, absoluteCharacterNumber);

        while (token != null)
        {
            SkipWhitespaceAndComments();
            tokens.Add(token);
            absoluteCharacterNumber = _absoluteCharacterNumber;
            token = GetNextToken();
            SetLineAndCharNumbers(token, absoluteCharacterNumber);
        }

        return tokens;
    }

    private void SkipWhitespaceAndComments()
    {
        int inputPlace;
        do
        {
            inputPlace = _currentIndex;
            SkipComments();
            SkipWhitespace();
        } while (inputPlace != _currentIndex);
    }

    private void SkipWhitespace()
    {
        while (Peek() is { } c && char.IsWhiteSpace(c))
        {
            _ = Consume();
        }
    }

    private void SkipComments()
    {
        SkipBlockComment();
        SkipLineComment();
    }

    private void SkipBlockComment()
    {
        if (!(Peek() == '/' && Peek(1) == '*'))
        {
            return;
        }

        Consume();
        Consume();

        while (true)
        {
            if (Peek() == null)
            {
                throw _errorHelper.UnexpectedEndOfInputAfterChar(Peek(-1)!.Value);
            }

            if (Peek(1) == null)
            {
                _errorHelper.UnexpectedEndOfInputAfterChar(Peek()!.Value);
            }

            if (Peek() == '*' && Peek(1) == '/')
            {
                Consume();
                Consume();

                return;
            }

            Consume();
        }
    }

    private void SkipLineComment()
    {
        if (!(Peek() == '/' && Peek(1) == '/'))
        {
            return;
        }

        Consume();
        Consume();

        while (Peek() != null && Peek() != '\n')
        {
            _ = Consume();
        }
    }

    private void SetLineAndCharNumbers(BaseToken? token, int absoluteStartCharNumber)
    {
        if (token == null)
        {
            return;
        }

        var span = new SourceSpan { Start = absoluteStartCharNumber, Length = _absoluteCharacterNumber - absoluteStartCharNumber };
        token.Span = span;
    }

    private BaseToken? GetNextToken()
    {
        if (Peek() == null)
        {
            return null;
        }

        if (char.IsLetter(Peek()!.Value))
        {
            return TokenizeLetter();
        }

        if (char.IsDigit(Peek()!.Value) || Peek() == '-' && Peek(1) is {} c && char.IsDigit(c))
        {
            return MakeNumToken();
        }

        if (Peek()!.Value == '\"')
        {
            return MakeStringLiteralToken();
        }

        if (TryParseEqualsToken() is { } token)
        {
            return token;
        }

        if (TryParseOperator() is { } op)
        {
            return op;
        }

        throw new Exception($"Unexpected character: `{Peek()}`");
    }

    private BaseToken? MakeNumToken()
    {
        var buffer = "";
        buffer += Consume();
        var sawDecimalPoint = false;
        while (Peek() is { } c && (char.IsDigit(c) || c == '.'))
        {
            if (c == '.')
            {
                if (Peek(1) is {} c2 && !char.IsAsciiDigit(c2))
                {
                    break;
                }
                
                sawDecimalPoint = true;
            }
            buffer += Consume();
        }

        return MakeNumLiteralToken(buffer, sawDecimalPoint);
    }

    private BaseToken MakeNumLiteralToken(string buffer, bool sawDecimalPoint)
    {
        if (sawDecimalPoint)
        {
            return new FloatLiteralToken(float.Parse(buffer));
        }

        return new IntLiteralToken(int.Parse(buffer));
    }

    private StringLiteralToken MakeStringLiteralToken()
    {
        _ = Consume();
        var buffer = new StringBuilder();
        while (Consume() is var character && character != '\"')
        {
            buffer.Append(character == '\\' ? ValidateEscape(Consume()) : character);
        }

        return new StringLiteralToken(buffer.ToString());
    }

    private char ValidateEscape(char escapedChar)
    {
        return escapedChar switch
        {
            'n' => '\n',
            '\\' => '\\',
            '"' => '"',
            _ => throw new Exception($"Invalid escape sequence: \\{escapedChar}")
        };
    }

    private BaseToken? TryParseEqualsToken()
    {
        if (!EqualsOperatorMap.ContainsKey(Peek()!.Value)) return null;
        var funcs = EqualsOperatorMap[Consume()];
        if (Peek() is '=')
        {
            _ = Consume();
            return funcs.Item2.Invoke();
        }

        return funcs.Item1.Invoke();
    }

    // operators that could be x, or could be x= (e.g. ! and !=)
    // first function makes x token, second makes the x= token
    private static readonly Dictionary<char, (Func<BaseToken>, Func<BaseToken>)> EqualsOperatorMap = new()
    {
        { '=', (() => new EqualsToken(), () => new DoubleEqualsToken()) },
        { '+', (() => new PlusToken(), () => new PlusEqualsToken()) },
        { '-', (() => new MinusToken(), () => new MinusEqualsToken()) },
        { '*', (() => new TimesToken(), () => new TimesEqualsToken()) },
        { '/', (() => new DivideToken(), () => new DivideEqualsToken()) },
        { '!', (() => new NotToken(), () => new NotEqualToken()) },
        { '>', (() => new GreaterToken(), () => new GreaterOrEqualToken()) },
        { '<', (() => new LessToken(), () => new LessOrEqualToken()) }
    };


    private BaseToken TokenizeLetter()
    {
        var buffer = "";
        while (Peek() is { } c && (char.IsLetterOrDigit(c) || c == '_'))
        {
            buffer += Consume();
        }

        if (KeywordMap.TryGetValue(buffer, out var tokenFunc))
        {
            return tokenFunc.Invoke();
        }

        return new IdentifierToken(buffer);
    }

    private static readonly Dictionary<string, Func<BaseToken>> KeywordMap = new()
    {
        { "class", () => new ClassToken() },
        { "static", () => new StaticToken() },
        { "exit", () => new ExitToken() },
        { "let", () => new LetToken() },
        { "int", () => new IntTypeToken() },
        { "float", () => new FloatTypeToken() },
        { "bool", () => new BoolTypeToken() },
        { "string", () => new StringTypeToken() },
        { "true", () => new BoolLiteralToken(true) },
        { "false", () => new BoolLiteralToken(false) },
        { "if", () => new IfToken() },
        { "else", () => new ElseToken() },
        { "while", () => new WhileToken() },
        { "for", () => new ForToken() },
        { "in", () => new InToken() },
        { "void", () => new VoidToken() },
        { "return", () => new ReturnToken() },
        { "print", () => new PrintToken() },
        { "new", () => new NewToken() },
        { "using", () => new UsingToken() },
    };

    private BaseToken? TryParseOperator()
    {
        if (Peek(1) != null)
        {
            var nextTwoChars = Peek()!.Value.ToString() + Peek(1)!.Value;
            if (OperatorMap.TryGetValue(nextTwoChars, out var value))
            {
                Consume();
                Consume();
                return value.Invoke();
            }
        }
        
        if (OperatorMap.ContainsKey(Peek()!.Value.ToString()))
        {
            return OperatorMap[Consume().ToString()].Invoke();
        }

        return null;
    }

    private static readonly Dictionary<string, Func<BaseToken>> OperatorMap = new()
    {
        { ";", () => new SemicolonToken() },
        { "(", () => new LeftParenToken() },
        { ")", () => new RightParenToken() },
        { "{", () => new LeftCurlyToken() },
        { "}", () => new RightCurlyToken() },
        { ",", () => new CommaToken() },
        { ".", () => new DotToken() },
        { "..", () => new RangeToken() },
        { "&&", () => new BooleanAndToken() },
        { "||", () => new BooleanOrToken() },
    };

    private string? _input;
    private int _currentIndex;

    private int _absoluteCharacterNumber;

    private char? Peek(int offset = 0)
    {
        var index = _currentIndex + offset;
        if (index >= _input!.Length) return null;

        return _input[index];
    }

    private char Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw new Exception("Unexpected end of input");
        }

        _absoluteCharacterNumber++;

        return (char)ret;
    }
}