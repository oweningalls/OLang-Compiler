using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

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
            throw EndOfInputException();
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
            var assignmentOperatorToken = TryConsume<IAssignmentOperatorToken>();
            if (assignmentOperatorToken == null)
            {
                throw ErrorHelper.ExpectedToken<IAssignmentOperatorToken>(Peek());
            }

            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();

            if (assignmentOperatorToken is EqualsToken)
            {
                return new AssignmentStatement(ident, expression);
            }

            var lhs = new TermExpression(new IdentifierTerm(ident.Identifier));

            BaseBinaryExpressionNode reformedExpression;
            switch (assignmentOperatorToken)
            {
                case PlusEqualsToken:
                    reformedExpression = new AddExpression(lhs, expression);
                    break;
                case MinusEqualsToken:
                    reformedExpression = new SubtractExpression(lhs, expression);
                    break;
                default:
                    throw ErrorHelper.UnknownVariant("assignment operator", assignmentOperatorToken!.GetType());
            }

            return new AssignmentStatement(ident, reformedExpression);
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

        if (TryConsume<WhileToken>() != null)
        {
            var condition = ParseExpression();
            var scope = ParseScope();

            return new WhileStatement(condition, scope);
        }

        if (TryConsume<ForToken>() != null)
        {
            var identifier = ConsumeType<IdentifierToken>();
            _ = ConsumeType<InToken>();
            var start = ParseExpression();
            _ = ConsumeType<RangeToken>();
            var end = ParseExpression();
            var scope = ParseScope();
            
            return new ForStatement(identifier, start, end, scope);
        }

        throw ErrorHelper.ExpectedValue("statement", Peek()!);
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

    private IExpressionNode ParseExpression(int minPrecedence = 0)
    {
        if (TryConsume<NotToken>() != null)
        {
            var innerExpression = ParseExpression();

            return new NotExpression(innerExpression);
        }

        IExpressionNode expression = new TermExpression(ParseTerm());

        while (Peek() is IBinaryOperatorToken binaryOperatorToken && binaryOperatorToken.Precedence >= minPrecedence)
        {
            Consume();
            // add one for left associative operators, don't for right associative
            var nextMinPrecedence = binaryOperatorToken.Precedence + 1;
            var rhs = ParseExpression(nextMinPrecedence);

            expression = binaryOperatorToken switch
            {
                DoubleEqualsToken => new DoubleEqualsExpression(expression, rhs),
                PlusToken => new AddExpression(expression, rhs),
                MinusToken => new SubtractExpression(expression, rhs),
                NotEqualToken => new NotEqualExpression(expression, rhs),
                GreaterToken => new GreaterExpression(expression, rhs),
                GreaterOrEqualToken => new GreaterOrEqualExpression(expression, rhs),
                LessToken => new LessExpression(expression, rhs),
                LessOrEqualToken => new LessOrEqualExpression(expression, rhs),
                _ => throw ErrorHelper.UnknownVariant("binary operator", binaryOperatorToken.GetType())
            };
        }

        return expression;
    }

    private ITermNode ParseTerm()
    {
        if (Peek() == null)
        {
            throw EndOfInputException();
        }

        if (TryConsume<IntLiteralToken>() is { } ilt)
        {
            return new IntLiteralTerm(ilt.Value);
        }

        if (TryConsume<BoolLiteralToken>() is { } blt)
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

        if (TryConsume<MinusToken>() != null)
        {
            if (Peek() == null)
            {
                throw EndOfInputException();
            }
        
            var intLiteral = TryConsume<IntLiteralToken>() ?? throw ErrorHelper.ExpectedValue("int literal", Peek()!);
        
            return new IntLiteralTerm(-intLiteral.Value);
        }

        throw ErrorHelper.ExpectedValue("term", Peek()!);
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
            throw EndOfInputException();
        }

        return ret;
    }

    private Exception EndOfInputException()
    {
        throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
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
            throw EndOfInputException();
        }

        var token = Peek() as T ?? throw ErrorHelper.ExpectedToken<T>(Peek()!);
        Consume();
        return token;
    }
}