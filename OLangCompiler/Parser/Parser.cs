using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public class Parser
{
    public ProgramNode ParseProgram(List<IToken> tokens)
    {
        _tokens = tokens;
        _currentIndex = 0;
        var statements = new List<IStatementNode>();
        while (Peek() != null)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private IStatementNode ParseStatement()
    {
        if (Peek() == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        if (TryConsume<ExitToken>() != null)
        {
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new ExitStatement(expression);
        }

        if (TryConsume<ITypeToken>() is { } type)
        {
            var identifier = ConsumeType<IdentifierToken>();
            ConsumeType<EqualsToken>();
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new DeclarationStatement(identifier, expression, type.ExpType);
        }

        if (TryConsume<IdentifierToken>() is { } ident)
        {
            ConsumeType<EqualsToken>();
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new AssignmentStatement(ident, expression);
        }

        if (Peek() is LeftCurlyToken)
        {
            return new ScopeStatement(ParseScope());
        }

        if (TryConsume<IfToken>() != null)
        {
            var condition = ParseExpression();
            var scope = ParseScope();

            return new IfStatement(condition, scope);
        }

        throw ErrorHelper.ExpectedValue("statement", Peek(-1)!);
    }

    private ScopeNode ParseScope()
    {
        var statements = new List<IStatementNode>();
        ConsumeType<LeftCurlyToken>();
        while (Peek() != null && Peek() is not RightCurlyToken)
        {
            statements.Add(ParseStatement());
        }

        ConsumeType<RightCurlyToken>();

        return new ScopeNode(statements);
    }

    private IExpressionNode ParseExpression()
    {
        var term = ParseTerm();
        if (TryConsume<PlusToken>() != null)
        {
            var expression = ParseExpression();

            return new AddExpression(term, expression);
        }

        if (TryConsume<MinusToken>() != null)
        {
            // TODO: this needs to be left associative
            var expression = ParseExpression();

            return new SubtractExpression(term, expression);
        }

        if (TryConsume<DoubleEqualsToken>() != null)
        {
            var expression = ParseExpression();

            return new DoubleEqualsExpression(term, expression);
        }

        return new TermExpression(term);
    }

    private ITermNode ParseTerm()
    {
        if (Peek() == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        if (TryConsume<IntLiteralToken>() is {} ilt)
        {
            return new IntLiteralTerm(ilt.Value);
        }
        
        if (TryConsume<BoolLiteralToken>() is {} blt)
        {
            return new BoolLiteralTerm(blt.Value);
        }

        if (TryConsume<IdentifierToken>() is { } identifier)
        { 
            return new IdentifierTerm(identifier.Identifier);
        }

        if (TryConsume<LeftParenToken>() != null)
        {
            var expression = ParseExpression();
            ConsumeType<RightParenToken>();
            return new ParenTerm(expression);
        }
        
        throw ErrorHelper.ExpectedValue("term", Peek(-1)!);
    }
    
    private List<IToken>? _tokens;
    private int _currentIndex;
    private IToken? Peek(int offset = 0)
    {
        var index = _currentIndex + offset;
        if (index >= _tokens!.Count) return null;

        return _tokens[index];
    }

    private IToken Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        return ret;
    }

    private T? TryConsume<T>() where T : class, IToken
    {
        if (Peek() is T)
        {
            return Consume() as T;
        }

        return null;
    }

    private T ConsumeType<T>() where T : class, IToken
    {
        if (Peek() == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        var token = Peek() as T ?? throw ErrorHelper.ExpectedToken<T>(Peek()!);
        Consume();
        return token;
    }
}