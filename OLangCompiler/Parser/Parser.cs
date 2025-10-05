using OLangCompiler.Parser.Nodes;
using OLangCompiler.Parser.Nodes.NodeValues;
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
            throw new Exception("Expected statement");
        }

        if (Peek() is ExitToken)
        {
            _ = Consume();
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new ExitStatement(expression);
        }

        if (Peek() is LetToken)
        {
            _ = Consume();
            var identifier = TryConsume<IdentifierToken>("Expected identifier");
            TryConsume<EqualsToken>("Expected `=`");
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new DeclarationStatement(identifier, expression);
        }

        if (Peek() is IdentifierToken ident)
        {
            _ = Consume();
            TryConsume<EqualsToken>("Expected `=`");
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new SetVarStatement(ident, expression);
        }

        throw new Exception("Expected statement");
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

        return new TermExpression(term);
    }

    private ITermNode ParseTerm()
    {
        if (Peek() == null)
        {
            throw new Exception("Expected term");
        }

        if (TryConsume<IntLiteralToken>() is {} ilt)
        {
            return new IntLiteralTerm(ilt.Value);
        }

        if (TryConsume<IdentifierToken>() is { } identifier)
        { 
            return new IdentifierTerm(identifier.Name);
        }

        if (TryConsume<LeftParenToken>() != null)
        {
            var expression = ParseExpression();
            TryConsume<RightParenToken>("Expected `)`");
            return new ParenTerm(expression);
        }
        
        throw new Exception("Expected term");
    }
    
    private List<IToken>? _tokens;
    private int _currentIndex;
    private IToken? Peek()
    {
        if (_currentIndex >= _tokens!.Count) return null;

        return _tokens[_currentIndex];
    }

    private IToken Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw new Exception("Unexpected end of token stream");
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

    private T TryConsume<T>(string errorMessage) where T : class, IToken
    {
        return Consume() as T ?? throw new Exception(errorMessage);
    }
}