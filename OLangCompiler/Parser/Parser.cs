using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser;

public class Parser : IParser
{
    private ErrorHelper _errorHelper;

    public ProgramNode ParseProgram(List<BaseToken> tokens, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _tokens = tokens;
        _currentIndex = 0;
        var stmtList = ParseStmtList();

        return new ProgramNode(stmtList);
    }

    private IStmtListNode ParseStmtList()
    {
        if (Peek() == null)
        {
            return new EmptyStmtList();
        }

        var statement = TryParseStatement();
        if (statement == null)
        {
            return new EmptyStmtList();
        }
        var stmtList = ParseStmtList();

        return new StmtListWithStatement(statement, stmtList);
    }

    private IStatementNode? TryParseStatement()
    {
        if (TryConsume<ExitToken>() != null)
        {
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();
            return new ExitStatement(expression);
        }

        if (TryParseDeclaration() is { } declaration)
        {
            return declaration;
        }

        if (TryParseInvocation() is { } invocation)
        {
            ConsumeType<SemicolonToken>();
            return new InvocationStatement(invocation);
        }

        if (TryConsume<IdentifierToken>() is { } ident)
        {
            CheckEndOfInput();
            var assignmentOperatorToken = ConsumeType<BaseAssignmentOperatorToken>();
            
            var expression = ParseExpression();
            ConsumeType<SemicolonToken>();

            if (assignmentOperatorToken is EqualsToken)
            {
                return new AssignmentStatement(ident, new EqualsNode(), expression);
            }

            var lhs = new TermExpression(new IdentifierTerm(ident.Identifier));

            BaseBinaryExpressionNode reformedExpression = assignmentOperatorToken switch
            {
                PlusEqualsToken => new AddExpression(lhs, expression),
                MinusEqualsToken => new SubtractExpression(lhs, expression),
                TimesEqualsToken => new TimesExpression(lhs, expression),
                DivideEqualsToken => new DivideExpression(lhs, expression),
                _ => throw _errorHelper.UnknownVariant("assignment operator", assignmentOperatorToken.GetType())
            };

            return new AssignmentStatement(ident, new EqualsNode(), reformedExpression);
        }

        if (Peek() is LeftCurlyToken)
        {
            return new ScopeStatement(ParseScope());
        }

        if (TryConsume<IfToken>() != null)
        {
            var condition = ParseExpression();
            var scope = ParseScope();
            ElseNode? elseNode = null;
            if (Peek() != null && TryConsume<ElseToken>() != null)
            {
                var elseScope = ParseScope();
                elseNode = new ElseNode(elseScope);
            }

            return new IfStatement(condition, scope, elseNode);
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
        
        if (TryConsume<ReturnToken>() != null)
        {
            if (TryConsume<SemicolonToken>() != null)
            {
                return new ReturnStatement();
            }

            var expression = ParseExpression();
            TryConsume<SemicolonToken>();
            return new ReturnValueStatement(expression);
        }

        return null;
    }

    private IStatementNode? TryParseDeclaration()
    {
        if (Peek() is not (BaseTypeToken or LetToken or VoidToken)) return null;

        var token = Consume();
        if (!IsVariableType(token) && !IsFunctionType(token))
        {
            throw _errorHelper.ShowErrorMessageAtToken("Expected a variable or function type", token);
        }

        var identifier = ConsumeType<IdentifierToken>();
        switch (token)
        {
            case LetToken:
                ConsumeType<EqualsToken>();
                var expression = ParseExpression();
                ConsumeType<SemicolonToken>();
                return new DeclarationStatement(new LetVariableType(), identifier, expression);
            case VoidToken:
                return ParseFunctionDeclarationAfterTypeAndIdentifier(new VoidFunctionType(), identifier);
            case BaseTypeToken typeToken:
                if (TryConsume<EqualsToken>() != null)
                {
                    expression = ParseExpression();
                    ConsumeType<SemicolonToken>();
                    return new DeclarationStatement(new PrimitiveVariableType(typeToken), identifier, expression);
                }

                return ParseFunctionDeclarationAfterTypeAndIdentifier(new PrimitiveFunctionType(typeToken), identifier);
            default:
                throw new Exception($"You shouldn't be able to get here. token type: {token.GetType()}");
        }
    }

    private FunctionDeclarationStatement ParseFunctionDeclarationAfterTypeAndIdentifier(IFunctionType type, IdentifierToken identifier)
    {
        ConsumeType<LeftParenToken>();
        var parameters = ParseParameterList();
        ConsumeType<RightParenToken>();
        var scope = ParseScope();

        return new FunctionDeclarationStatement(type, identifier, parameters, scope);
    }

    private IParameterListNode ParseParameterList()
    {
        if (TryConsume<BaseTypeToken>() is not { } type)
        {
            return new EmptyParameterList();
        }
        
        var identifierToken = ConsumeType<IdentifierToken>();
        if (TryConsume<CommaToken>() == null)
        {
            return new Parameter(type.ExpType!.Value, identifierToken);
        }
        
        var paramList = ParseParameterList();
        return new ContinuedParameterList(type.ExpType!.Value, identifierToken, paramList);

    }

    private InvocationNode? TryParseInvocation()
    {
        if (Peek() is not IdentifierToken || Peek(1) is not LeftParenToken)
        {
            return null;
        }

        var identifier = ConsumeType<IdentifierToken>();
        ConsumeType<LeftParenToken>();
        var arguments = ParseArgumentList();
        ConsumeType<RightParenToken>();

        return new InvocationNode(identifier, arguments);
    }

    private ArgumentList ParseArgumentList()
    {
        var expressions = new List<BaseExpressionNode>();
        while (Peek() is not RightParenToken)
        {
            expressions.Add(ParseExpression());
            if (TryConsume<CommaToken>() is null)
            {
                break;
            }
        }

        return new ArgumentList(expressions);
    }

    private bool IsVariableType(BaseToken token)
    {
        return token is BaseTypeToken or LetToken;
    }

    private bool IsFunctionType(BaseToken token)
    {
        return token is BaseTypeToken or VoidToken;
    }

    private ScopeNode ParseScope()
    {
        ConsumeType<LeftCurlyToken>();

        var statements = ParseStmtList();

        ConsumeType<RightCurlyToken>();

        return new ScopeNode(statements);
    }

    private BaseExpressionNode ParseExpression(int minPrecedence = 0)
    {
        if (TryConsume<NotToken>() != null)
        {
            var innerExpression = ParseExpression();

            return new NotExpression(innerExpression);
        }

        BaseExpressionNode expression = new TermExpression(ParseTerm());

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
                DivideToken => new DivideExpression(expression, rhs),
                NotEqualToken => new NotEqualExpression(expression, rhs),
                GreaterToken => new GreaterExpression(expression, rhs),
                GreaterOrEqualToken => new GreaterOrEqualExpression(expression, rhs),
                LessToken => new LessExpression(expression, rhs),
                LessOrEqualToken => new LessOrEqualExpression(expression, rhs),
                BooleanAndToken => new BooleanAndExpression(expression, rhs),
                BooleanOrToken => new BooleanOrExpression(expression, rhs),
                _ => throw _errorHelper.UnknownVariant("binary operator", binaryOperatorToken.GetType())
            };
        }

        return expression;
    }

    private BaseTermNode ParseTerm()
    {
        var invocation = TryParseInvocation();
        if (invocation != null)
        {
            return new InvocationTerm(invocation);
        }

        if (TryConsume<IntLiteralToken>() is { } ilt)
        {
            return new IntLiteralTerm(ilt.Value);
        }
        
        if (TryConsume<FloatLiteralToken>() is { } flt)
        {
            return new FloatLiteralTerm(flt.Value);
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
            var intLiteral = TryConsume<IntLiteralToken>();
            if (intLiteral != null)
            {
                return new IntLiteralTerm(-intLiteral.Value);
            }

            var floatLiteral = TryConsume<FloatLiteralToken>();
            if (floatLiteral != null)
            {
                return new FloatLiteralTerm(-floatLiteral.Value);
            }
            throw _errorHelper.ExpectedValue("number literal", Peek()!);
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
        CheckEndOfInput();
        var ret = Peek();
        _currentIndex++;

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
        CheckEndOfInput();
        if (Peek() is T)
        {
            return Consume() as T;
        }

        return null;
    }

    private T ConsumeType<T>() where T : BaseToken
    {
        var token = TryConsume<T>() ?? throw _errorHelper.ExpectedToken<T>(Peek()!);
        return token;
    }
}