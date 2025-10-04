using OLangCompiler.Parser.Nodes;
using OLangCompiler.Parser.Nodes.NodeValues;
using OLangCompiler.Tokens;
using OneOf;

namespace OLangCompiler.Parser;

public class Parser
{
    public ProgramNode ParseProgram(List<IToken> tokens)
    {
        _tokens = tokens;
        _currentIndex = 0;
        var statements = new List<StatementNode>();
        while (Peek() != null)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private StatementNode ParseStatement()
    {
        if (Peek() == null)
        {
            throw new Exception("Expected statement");
        }

        if (Peek() is ExitToken)
        {
            _ = Consume();
            var exitTerm = ParseTerm();
            TryConsume<SemicolonToken>("Expected `;`");
            return new StatementNode(new ExitStatement(exitTerm));
        }

        if (Peek() is LetToken)
        {
            _ = Consume();
            var identifier = TryConsume<IdentifierToken>("Expected identifier");
            TryConsume<EqualsToken>("Expected `=`");
            var term = ParseTerm();
            TryConsume<SemicolonToken>("Expected `;`");
            return new StatementNode(new VarDeclarationStatement(identifier, term));
        }

        if (Peek() is IdentifierToken ident)
        {
            _ = Consume();
            TryConsume<EqualsToken>("Expected `=`");
            var term = ParseTerm();
            TryConsume<SemicolonToken>("Expected `;`");
            return new StatementNode(new SetVarStatement(ident, term));
        }

        throw new Exception("Expected statement");
    }

    private TermNode ParseTerm()
    {
        if (Peek() == null)
        {
            throw new Exception("Expected term");
        }

        if (TryConsume<IntLiteralToken>() is {} ilt)
        {
            return new TermNode(ilt);
        }

        if (TryConsume<IdentifierToken>() is { } identifier)
        { 
            return new TermNode(identifier);
        }
        throw new Exception("Expected term");
    }
    
    private List<IToken>? _tokens = null!;
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
        if (Peek() is T token)
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