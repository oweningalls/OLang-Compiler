using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public class OLangParser : IParser
{
    private IErrorHelper _errorHelper;

    public INode ParseProgram(IGrammar _, List<BaseToken> tokens, IErrorHelper errorHelper)
    {
        // _errorHelper = errorHelper;
        // _tokens = tokens;
        // _currentIndex = 0;
        // var stmtList = ParseStmtList();
        //
        // return new ProgramNode(stmtList);
        throw new NotImplementedException();
    }

    // private IStmtListNode ParseStmtList()
    // {
    //     if (Peek() == null)
    //     {
    //         return new EmptyStmtList();
    //     }
    //
    //     var statement = TryParseStatement();
    //     if (statement == null)
    //     {
    //         return new EmptyStmtList();
    //     }
    //     var stmtList = ParseStmtList();
    //
    //     return new StmtListWithStatement(statement, stmtList);
    // }
    //
    // private IStatement? TryParseStatement()
    // {
    //     if (TryConsume<ExitToken>() != null)
    //     {
    //         var expression = ParseExpression();
    //         ConsumeType<SemicolonToken>();
    //         return new Exit(expression);
    //     }
    //
    //     if (TryParseDeclaration() is { } declaration)
    //     {
    //         return declaration;
    //     }
    //
    //     if (TryParseInvocation() is { } invocation)
    //     {
    //         ConsumeType<SemicolonToken>();
    //         return new Invocation(invocation);
    //     }
    //
    //     if (TryConsume<IdentifierToken>() is { } ident)
    //     {
    //         CheckEndOfInput();
    //         var assignmentOperatorToken = ConsumeType<BaseToken>();
    //         
    //         var expression = ParseExpression();
    //         ConsumeType<SemicolonToken>();
    //
    //         if (assignmentOperatorToken is EqualsToken)
    //         {
    //             return new Assignment(ident, new Equals(), expression);
    //         }
    //
    //         var lhs = new TermExpression(new IdentifierTerm(ident.Identifier));
    //
    //         BaseBinaryExpression reformedExpression = assignmentOperatorToken switch
    //         {
    //             PlusEqualsToken => new Add(lhs, expression),
    //             MinusEqualsToken => new Subtract(lhs, expression),
    //             TimesEqualsToken => new Times(lhs, expression),
    //             DivideEqualsToken => new Divide(lhs, expression),
    //             _ => throw _errorHelper.UnknownVariant("assignment operator", assignmentOperatorToken.GetType())
    //         };
    //
    //         return new Assignment(ident, new Equals(), reformedExpression);
    //     }
    //
    //     if (Peek() is LeftCurlyToken)
    //     {
    //         return new ScopeStatement(ParseScope());
    //     }
    //
    //     if (TryConsume<IfToken>() != null)
    //     {
    //         var condition = ParseExpression();
    //         var scope = ParseScope();
    //         Else? elseNode = null;
    //         if (Peek() != null && TryConsume<ElseToken>() != null)
    //         {
    //             var elseScope = ParseScope();
    //             elseNode = new Else(elseScope);
    //         }
    //
    //         return new If(condition, scope, elseNode);
    //     }
    //
    //     if (TryConsume<WhileToken>() != null)
    //     {
    //         var condition = ParseExpression();
    //         var scope = ParseScope();
    //
    //         return new While(condition, scope);
    //     }
    //
    //     if (TryConsume<ForToken>() != null)
    //     {
    //         var identifier = ConsumeType<IdentifierToken>();
    //         _ = ConsumeType<InToken>();
    //         var start = ParseExpression();
    //         _ = ConsumeType<RangeToken>();
    //         var end = ParseExpression();
    //         var scope = ParseScope();
    //
    //         return new For(identifier, start, end, scope);
    //     }
    //     
    //     if (TryConsume<ReturnToken>() != null)
    //     {
    //         if (TryConsume<SemicolonToken>() != null)
    //         {
    //             return new Return();
    //         }
    //
    //         var expression = ParseExpression();
    //         TryConsume<SemicolonToken>();
    //         return new ReturnValue(expression);
    //     }
    //
    //     return null;
    // }
    //
    // private IStatement? TryParseDeclaration()
    // {
    //     if (Peek() is not (BaseToken or LetToken or VoidToken)) return null;
    //
    //     var token = Consume();
    //     if (!IsVariableType(token) && !IsFunctionType(token))
    //     {
    //         throw _errorHelper.ShowErrorMessageAtToken("Expected a variable or function type", token);
    //     }
    //
    //     var identifier = ConsumeType<IdentifierToken>();
    //     switch (token)
    //     {
    //         case LetToken:
    //             ConsumeType<EqualsToken>();
    //             var expression = ParseExpression();
    //             ConsumeType<SemicolonToken>();
    //             return new Declaration(new Let(), identifier, expression);
    //         case VoidToken:
    //             return ParseFunctionDeclarationAfterTypeAndIdentifier(new Void(), identifier);
    //         case BaseToken typeToken:
    //             if (TryConsume<EqualsToken>() != null)
    //             {
    //                 expression = ParseExpression();
    //                 ConsumeType<SemicolonToken>();
    //                 return new Declaration(new PrimitiveVariableType(GetType(typeToken)), identifier, expression);
    //             }
    //
    //             return ParseFunctionDeclarationAfterTypeAndIdentifier(new PrimitiveFunctionType(GetType(typeToken)), identifier);
    //         default:
    //             throw new Exception($"You shouldn't be able to get here. token type: {token.GetType()}");
    //     }
    // }
    //
    // private IType GetType(BaseToken typeToken)
    // {
    //     switch (typeToken)
    //     {
    //         case IntTypeToken:
    //             return new IntType();
    //         case BoolTypeToken:
    //             return new BoolType();
    //         case FloatTypeToken:
    //             return new FloatType();
    //         default:
    //             throw _errorHelper.UnknownVariant("type", typeToken.GetType());
    //     }
    // }
    //
    // private FunctionDeclaration ParseFunctionDeclarationAfterTypeAndIdentifier(IFunctionType type, IdentifierToken identifier)
    // {
    //     ConsumeType<LeftParenToken>();
    //     var parameters = ParseParameterList();
    //     ConsumeType<RightParenToken>();
    //     var scope = ParseScope();
    //
    //     return new FunctionDeclaration(type, identifier, parameters, scope);
    // }
    //
    // private IParameterListNode ParseParameterList()
    // {
    //     if (TryConsume<BaseToken>() is not { } type)
    //     {
    //         return new EmptyParameterList();
    //     }
    //     
    //     var identifierToken = ConsumeType<IdentifierToken>();
    //     if (TryConsume<CommaToken>() == null)
    //     {
    //         return new Parameter(type.ExpType!.Value, identifierToken);
    //     }
    //     
    //     var paramList = ParseParameterList();
    //     return new ContinuedParameterList(type.ExpType!.Value, identifierToken, paramList);
    //
    // }
    //
    // private FunctionInvocation? TryParseInvocation()
    // {
    //     if (Peek() is not IdentifierToken || Peek(1) is not LeftParenToken)
    //     {
    //         return null;
    //     }
    //
    //     var identifier = ConsumeType<IdentifierToken>();
    //     ConsumeType<LeftParenToken>();
    //     var arguments = ParseArgumentList();
    //     ConsumeType<RightParenToken>();
    //
    //     return new FunctionInvocation(identifier, arguments);
    // }
    //
    // private IArgumentList ParseArgumentList()
    // {
    //     if (Peek() is RightParenToken)
    //     {
    //         return new EmptyArgumentList();
    //     }
    //
    //     var expression = ParseExpression();
    //     if (TryConsume<CommaToken>() == null)
    //     {
    //         return new ExpressionArgumentList(expression);
    //     }
    //
    //     return new ContinuedArgumentList(expression, ParseArgumentList());
    // }
    //
    // private bool IsVariableType(BaseToken token)
    // {
    //     return token is BaseToken or LetToken;
    // }
    //
    // private bool IsFunctionType(BaseToken token)
    // {
    //     return token is BaseToken or VoidToken;
    // }
    //
    // private ScopeNode ParseScope()
    // {
    //     ConsumeType<LeftCurlyToken>();
    //
    //     var statements = ParseStmtList();
    //
    //     ConsumeType<RightCurlyToken>();
    //
    //     return new ScopeNode(statements);
    // }
    //
    // private IExpression ParseExpression(int minPrecedence = 0)
    // {
    //     if (TryConsume<NotToken>() != null)
    //     {
    //         var innerExpression = ParseExpression();
    //
    //         return new NotExpression(innerExpression);
    //     }
    //
    //     IExpression expression = new TermExpression(ParseTerm());
    //
    //     while (Peek() is BaseToken binaryOperatorToken && binaryOperatorToken.Precedence >= minPrecedence)
    //     {
    //         Consume();
    //         // add one for left associative operators, don't for right associative
    //         var nextMinPrecedence = binaryOperatorToken.Precedence + 1;
    //         var rhs = ParseExpression(nextMinPrecedence);
    //
    //         expression = binaryOperatorToken switch
    //         {
    //             DoubleEqualsToken => new DoubleEquals(expression, rhs),
    //             PlusToken => new Add(expression, rhs),
    //             MinusToken => new Subtract(expression, rhs),
    //             TimesToken => new Times(expression, rhs),
    //             DivideToken => new Divide(expression, rhs),
    //             NotEqualToken => new NotEqual(expression, rhs),
    //             GreaterToken => new Greater(expression, rhs),
    //             GreaterOrEqualToken => new GreaterOrEqual(expression, rhs),
    //             LessToken => new Less(expression, rhs),
    //             LessOrEqualToken => new LessOrEqual(expression, rhs),
    //             BooleanAndToken => new BooleanAnd(expression, rhs),
    //             BooleanOrToken => new BooleanOr(expression, rhs),
    //             _ => throw _errorHelper.UnknownVariant("binary operator", binaryOperatorToken.GetType())
    //         };
    //     }
    //
    //     return expression;
    // }
    //
    // private ITerm ParseTerm()
    // {
    //     var invocation = TryParseInvocation();
    //     if (invocation != null)
    //     {
    //         return new ParseTree.Term.FunctionInvocation(invocation);
    //     }
    //
    //     if (TryConsume<IntLiteralToken>() is { } ilt)
    //     {
    //         return new IntLiteral(ilt.Value);
    //     }
    //     
    //     if (TryConsume<FloatLiteralToken>() is { } flt)
    //     {
    //         return new FloatLiteral(flt.Value);
    //     }
    //
    //     if (TryConsume<BoolLiteralToken>() is { } blt)
    //     {
    //         return new BoolLiteral(blt.Value);
    //     }
    //
    //     if (TryConsume<IdentifierToken>() is { } identifier)
    //     {
    //         return new IdentifierTerm(identifier.Identifier);
    //     }
    //
    //     if (TryConsume<LeftParenToken>() != null)
    //     {
    //         var expression = ParseExpression();
    //         ConsumeType<RightParenToken>();
    //         return new Paren(expression);
    //     }
    //
    //     if (TryConsume<MinusToken>() != null)
    //     {
    //         var intLiteral = TryConsume<IntLiteralToken>();
    //         if (intLiteral != null)
    //         {
    //             return new IntLiteral(-intLiteral.Value);
    //         }
    //
    //         var floatLiteral = TryConsume<FloatLiteralToken>();
    //         if (floatLiteral != null)
    //         {
    //             return new FloatLiteral(-floatLiteral.Value);
    //         }
    //         throw _errorHelper.ExpectedValue("number literal", Peek()!);
    //     }
    //
    //     throw _errorHelper.ExpectedValue("term", Peek()!);
    // }
    //
    // private List<BaseToken>? _tokens;
    // private int _currentIndex;
    //
    // private BaseToken? Peek(int offset = 0)
    // {
    //     var index = _currentIndex + offset;
    //     if (index >= _tokens!.Count) return null;
    //
    //     return _tokens[index];
    // }
    //
    // private BaseToken Consume()
    // {
    //     CheckEndOfInput();
    //     var ret = Peek();
    //     _currentIndex++;
    //
    //     return ret;
    // }
    //
    // private void CheckEndOfInput()
    // {
    //     if (Peek() == null)
    //     {
    //         throw EndOfInputException();
    //     }
    // }
    //
    // private Exception EndOfInputException()
    // {
    //     throw _errorHelper.UnexpectedEndOfInput(Peek(-1)!);
    // }
    //
    // private T? TryConsume<T>() where T : BaseToken
    // {
    //     CheckEndOfInput();
    //     if (Peek() is T)
    //     {
    //         return Consume() as T;
    //     }
    //
    //     return null;
    // }
    //
    // private T ConsumeType<T>() where T : BaseToken
    // {
    //     var token = TryConsume<T>() ?? throw _errorHelper.ExpectedToken<T>(Peek()!);
    //     return token;
    // }
}