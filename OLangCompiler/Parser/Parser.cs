using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public class Parser
{
    private ErrorHelper _errorHelper;
    public ProgramNode ParseProgram(List<BaseToken> tokens, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
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
        CheckEndOfInput();

        if (TryConsume<ExitToken>() != null)
        {
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new ExitStatement(expression);
        }

        if (TryConsume<BaseTypeToken>() is { } type)
        {
            var identifier = ConsumeType<IdentifierToken>();
            ConsumeType<EqualsToken>();
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new DeclarationStatement(identifier, expression, type.ExpType);
        }

        if (TryConsume<IdentifierToken>() is { } ident)
        {
            CheckEndOfInput();
            var assignmentOperatorToken = ConsumeType<BaseAssignmentOperatorToken>();

            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();

            if (assignmentOperatorToken is EqualsToken)
            {
                return new AssignmentStatement(ident, expression);
            }

            var lhs = new TermExpression(new IdentifierTerm(ident.Identifier));

            BaseBinaryExpressionNode reformedExpression = assignmentOperatorToken switch
            {
                PlusEqualsToken => new AddExpression(lhs, expression),
                MinusEqualsToken => new SubtractExpression(lhs, expression),
                TimesEqualsToken => new TimesExpression(lhs, expression),
                _ => throw _errorHelper.UnknownVariant("assignment operator", assignmentOperatorToken.GetType())
            };

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

        throw _errorHelper.ExpectedValue("statement", Peek()!);
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

        while (Peek() is BaseBinaryOperatorToken binaryOperatorToken && binaryOperatorToken.Precedence >= minPrecedence)
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
                TimesToken => new TimesExpression(expression, rhs),
                NotEqualToken => new NotEqualExpression(expression, rhs),
                GreaterToken => new GreaterExpression(expression, rhs),
                GreaterOrEqualToken => new GreaterOrEqualExpression(expression, rhs),
                LessToken => new LessExpression(expression, rhs),
                LessOrEqualToken => new LessOrEqualExpression(expression, rhs),
                _ => throw _errorHelper.UnknownVariant("binary operator", binaryOperatorToken.GetType())
            };
        }

        return expression;
    }

    private ITermNode ParseTerm()
    {
        CheckEndOfInput();

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
            CheckEndOfInput();
        
            var intLiteral = TryConsume<IntLiteralToken>() ?? throw _errorHelper.ExpectedValue("int literal", Peek()!);
        
            return new IntLiteralTerm(-intLiteral.Value);
        }

        throw _errorHelper.ExpectedValue("term", Peek()!);
    }

    private List<BaseToken>? _tokens;
    private int _currentIndex;

    private BaseToken? Peek(int offset = 0)
    {
        var index = _currentIndex + offset;
        if (index >= _tokens!.Count) return null;

        return _tokens[index];
    }

    private BaseToken Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw EndOfInputException();
        }

        return ret;
    }

    private void CheckEndOfInput()
    {
        if (Peek() == null)
        {
            throw EndOfInputException();
        }
    }

    private Exception EndOfInputException()
    {
        throw _errorHelper.UnexpectedEndOfInput(Peek(-1)!);
    }

    private T? TryConsume<T>() where T : BaseToken
    {
        if (Peek() is T)
        {
            return Consume() as T;
        }

        return null;
    }

    private T ConsumeType<T>() where T : BaseToken
    {
        CheckEndOfInput();
        var token = Peek() as T ?? throw _errorHelper.ExpectedToken<T>(Peek()!);
        Consume();
        return token;
    }
}