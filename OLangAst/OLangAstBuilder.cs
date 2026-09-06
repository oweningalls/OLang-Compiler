using ErrorHelper;
using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.ClassMemberList;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.FunctionInvocation;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MethodInvocation;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;
using BoolLiteral = OLangAst.Expressions.BoolLiteral;
using FloatLiteral = OLangAst.Expressions.FloatLiteral;
using FunctionDeclaration = OLangAst.Statements.FunctionDeclaration;
using MethodInvocation = OLangAst.Statements.MethodInvocation;
using GreaterOrEqual = OLangAst.Expressions.GreaterOrEqual;
using IExpression = OLangAst.Expressions.IExpression;
using IntLiteral = OLangAst.Expressions.IntLiteral;
using IStatement = OLangAst.Statements.IStatement;
using Not = OLangAst.Expressions.Not;
using NotEqual = OLangAst.Expressions.NotEqual;
using Parameter = OLangAst.Miscellaneous.Parameter;
using Return = OLangAst.Statements.Return;
using StringLiteral = OLangAst.Expressions.StringLiteral;
using FunctionInvocation = OLangAst.Statements.FunctionInvocation;
using IClassMember = OLangAst.ClassMembers.IClassMember;
using MethodDeclaration = OLangAst.ClassMembers.MethodDeclaration;

namespace OLangAst;

public class OLangAstBuilder(IErrorHelper errorHelper)
{
    public Program ParseProgram(ProgramNode programNode)
    {
        var statements = ParseList(
            programNode.ClassMemberList,
            x => x switch
            {
                SingleMemberClassMemberList member => (member.ClassMember, null),
                ClassMemberListWithClassMember list => (list.ClassMember, list.ClassMemberList),
                _ => throw errorHelper.UnknownVariant("class member list", x.GetType())
            },
            ParseClassMember
        );
        return new Program(statements) { Span = SourceSpan.CombineSpans(statements.Select(x => x.Span).ToArray()) };
    }

    private List<TTarget> ParseList<TList, TData, TTarget>(TList first, Func<TList, (TData, TList?)> getValueAndNext, Func<TData, TTarget> parseValue)
    {
        var (value, next) = getValueAndNext(first);
        var list = new List<TTarget> { parseValue(value) };
        while (next != null)
        {
            (value, next) = getValueAndNext(next);
            list.Add(parseValue(value));
        }

        return list;
    }

    protected List<IStatement> ParseStatementList(IStmtListNode statementList)
    {
        return ParseList(
            statementList,
            x => x switch
            {
                SingleStatementStmtList value => (value.Statement, null),
                StmtListWithStatement list => (list.Statement, list.StmtList),
                _ => throw errorHelper.UnknownVariant("statement list", x.GetType())
            },
            ParseStatement
        );
    }

    protected IClassMember ParseClassMember(OLangGrammar.ParseTree.ClassMember.IClassMember classMember)
    {
        return classMember switch
        {
            OLangGrammar.ParseTree.ClassMember.MethodDeclaration methodDeclaration => new MethodDeclaration(ParseType(methodDeclaration.Type), ParseIdentifier(methodDeclaration.Identifier), [], ParseScope(methodDeclaration.Scope)),
            MethodDeclarationWithParameters methodDeclarationWithParameters => new MethodDeclaration(ParseType(methodDeclarationWithParameters.Type), ParseIdentifier(methodDeclarationWithParameters.Identifier), ParseParameterList(methodDeclarationWithParameters.Parameters), ParseScope(methodDeclarationWithParameters.Scope)),
            VoidMethodDeclaration voidMethodDeclaration => new MethodDeclaration(null, ParseIdentifier(voidMethodDeclaration.Identifier), [], ParseScope(voidMethodDeclaration.Scope)),
            VoidMethodDeclarationWithParameters voidMethodDeclarationWithParameters => new MethodDeclaration(null, ParseIdentifier(voidMethodDeclarationWithParameters.Identifier), ParseParameterList(voidMethodDeclarationWithParameters.Parameters), ParseScope(voidMethodDeclarationWithParameters.Scope)),
            _ => throw errorHelper.UnknownVariant("class member", classMember.GetType())
        };
    }
    
    protected IStatement ParseStatement(OLangGrammar.ParseTree.Stmt.IStatement statement)
    {
        return statement switch
        {
            Assignment assignment => ParseAssignment(assignment),
            Declaration declaration => new VariableDeclarationStatement(ParseType(declaration.Type), ParseIdentifier(declaration.Identifier), ParseExpression(declaration.Expression)) { Span = declaration.Span },
            LetDeclaration letDeclaration => new VariableDeclarationStatement(null, ParseIdentifier(letDeclaration.Identifier), ParseExpression(letDeclaration.Expression)) { Span = letDeclaration.Span },
            Exit exit => new ExitStatement(ParseExpression(exit.Expression)) { Span = exit.Span },
            Print exit => new PrintStatement(ParseExpression(exit.Expression)) { Span = exit.Span },
            For forStatement => new ForLoop(ParseIdentifier(forStatement.Identifier), ParseExpression(forStatement.Start), ParseExpression(forStatement.End), ParseScope(forStatement.Scope)) { Span = forStatement.Span },
            While whileStatement => new WhileLoop(ParseExpression(whileStatement.Condition), ParseScope(whileStatement.Scope)) { Span = whileStatement.Span },
            FunctionDeclarationWithParameters functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), ParseParameterList(functionDeclaration.Parameters), ParseScope(functionDeclaration.Scope)) { Span = functionDeclaration.Span },
            VoidFunctionDeclarationWithParameters voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), ParseParameterList(voidFunctionDeclaration.Parameters), ParseScope(voidFunctionDeclaration.Scope)) { Span = voidFunctionDeclaration.Span },
            OLangGrammar.ParseTree.Stmt.FunctionDeclaration functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), [], ParseScope(functionDeclaration.Scope)) { Span = functionDeclaration.Span },
            VoidFunctionDeclaration voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), [], ParseScope(voidFunctionDeclaration.Scope)) { Span = voidFunctionDeclaration.Span },
            If ifStatement => new IfStatement(ParseExpression(ifStatement.Condition), ParseScope(ifStatement.Scope), null) { Span = ifStatement.Span },
            IfWithElse ifStatementWithElse => new IfStatement(ParseExpression(ifStatementWithElse.Condition), ParseScope(ifStatementWithElse.Scope), ParseScope(ifStatementWithElse.ElseBlock)) { Span = ifStatementWithElse.Span },
            Invocation invocation => ParseFunctionInvocation(invocation.InvocationNode),
            MethodInvocationStatement methodInvocation => ParseMethodInvocation(methodInvocation.InvocationNode),
            OLangGrammar.ParseTree.Stmt.Return returnStatement => new Return(null) { Span = returnStatement.Span },
            ReturnValue returnValueStatement => new Return(ParseExpression(returnValueStatement.Expression)) { Span = returnValueStatement.Span },
            ScopeStatement scopeStatement => ParseScope(scopeStatement.Scope),
            _ => throw errorHelper.UnknownVariant("statement", statement.GetType())
        };
    }

    protected VariableAssignment ParseAssignment(Assignment assignment)
    {
        var identifier = ParseIdentifier(assignment.Identifier);
        var parsedExpression = ParseExpression(assignment.Expression);
        var value = assignment.Operator switch
        {
            Equals _ => parsedExpression,
            PlusEquals => new Add(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            MinusEquals => new Subtract(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            TimesEquals => new Multiply(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            DivideEquals => new Divide(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            _ => throw errorHelper.UnknownVariant("assignment operator", assignment.Operator.GetType())
        };

        value.Span = assignment.Expression.Span;
        return new VariableAssignment(ParseIdentifier(assignment.Identifier), value) { Span = assignment.Span };
    }

    protected Scope ParseScope(IScopeNode scope)
    {
        return scope switch
        {
            EmptyScope emptyScope => new Scope([]) { Span = emptyScope.Span },
            ScopeNode scopeNode => new Scope(ParseStatementList(scopeNode.StmtList)) { Span = scopeNode.Span },
            _ => throw errorHelper.UnknownVariant("scope", scope.GetType())
        };
    }

    protected IExpression ParseExpression(OLangGrammar.ParseTree.Expression.IExpression expression)
    {
        return expression switch
        {
            Expression orExpression => new Or(ParseExpression(orExpression.Lhs), ParseAndExpression(orExpression.Rhs)) { Span = orExpression.Span },
            NonExpression andExpression => ParseAndExpression(andExpression.Expression),
            _ => throw errorHelper.UnknownVariant("expression", expression.GetType())
        };
    }
    
    protected IExpression ParseAndExpression(IAndExpression expression)
    {
        return expression switch
        {
            AndExpression and => new And(ParseAndExpression(and.Lhs), ParseEqualityExpression(and.Rhs)) { Span = and.Span },
            NonAnd nonAnd => ParseEqualityExpression(nonAnd.Expression),
            _ => throw errorHelper.UnknownVariant("and expression", expression.GetType())
        };
    }

    protected IExpression ParseEqualityExpression(IEqualityExpression expression)
    {
        return expression switch
        {
            DoubleEquals doubleEquals => new AreEqual(ParseEqualityExpression(doubleEquals.Lhs), ParseGreaterExpression(doubleEquals.Rhs)) { Span = doubleEquals.Span },
            OLangGrammar.ParseTree.EqualityExpression.NotEqual notEqual => new NotEqual(ParseEqualityExpression(notEqual.Lhs), ParseGreaterExpression(notEqual.Rhs)) { Span = notEqual.Span },
            NonEquality nonEquality => ParseGreaterExpression(nonEquality.Expression),
            _ => throw errorHelper.UnknownVariant("equality expression", expression.GetType())
        };
    }

    protected IExpression ParseGreaterExpression(IGreaterExpression expression)
    {
        return expression switch
        {
            Greater greater => new GreaterThan(ParseGreaterExpression(greater.Lhs), ParseAddExpression(greater.Rhs)) { Span = greater.Span },
            OLangGrammar.ParseTree.GreaterExpression.GreaterOrEqual greaterOrEqual => new GreaterOrEqual(ParseGreaterExpression(greaterOrEqual.Lhs), ParseAddExpression(greaterOrEqual.Rhs)) { Span = greaterOrEqual.Span },
            Less less => new LessThan(ParseGreaterExpression(less.Lhs), ParseAddExpression(less.Rhs)) { Span = less.Span },
            LessOrEqual lessOrEqual => new LessThanOrEqual(ParseGreaterExpression(lessOrEqual.Lhs), ParseAddExpression(lessOrEqual.Rhs)) { Span = lessOrEqual.Span },
            NonGreaterExpression nonGreaterExpression => ParseAddExpression(nonGreaterExpression.Expression),
            _ => throw errorHelper.UnknownVariant("greater expression", expression.GetType())
        };
    }

    protected IExpression ParseAddExpression(IAddExpression expression)
    {
        return expression switch
        {
            Plus plus => new Add(ParseAddExpression(plus.Lhs), ParseMultExpression(plus.Rhs)) { Span = plus.Span },
            Minus minus => new Subtract(ParseAddExpression(minus.Lhs), ParseMultExpression(minus.Rhs)) { Span = minus.Span },
            NonAddExpression nonAdd => ParseMultExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("add expression", expression.GetType())
        };
    }

    protected IExpression ParseMultExpression(IMultExpression expression)
    {
        return expression switch
        {
            Mult mu => new Multiply(ParseMultExpression(mu.Lhs), ParseUnaryExpression(mu.Rhs)) { Span = mu.Span },
            Div div => new Divide(ParseMultExpression(div.Lhs), ParseUnaryExpression(div.Rhs)) { Span = div.Span },
            NonMultExpression nonAdd => ParseUnaryExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseUnaryExpression(IUnaryExpression expression)
    {
        return expression switch
        {
            OLangGrammar.ParseTree.UnaryExpression.Not not => new Not(ParseTerm(not.Term)) { Span = not.Span },
            Negation negate => new Negate(ParseTerm(negate.Term)) { Span = negate.Span },
            NonUnaryExpression nonUnary => ParseTerm(nonUnary.Term),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseTerm(ITerm term)
    {
        return term switch
        {
            OLangGrammar.ParseTree.Term.BoolLiteral boolLiteral => new BoolLiteral(boolLiteral.Value.Value) { Span = boolLiteral.Span },
            OLangGrammar.ParseTree.Term.IntLiteral intLiteral => new IntLiteral(intLiteral.Value.Value) { Span = intLiteral.Span },
            OLangGrammar.ParseTree.Term.FloatLiteral floatLiteral => new FloatLiteral(floatLiteral.Value.Value) { Span = floatLiteral.Span },
            OLangGrammar.ParseTree.Term.StringLiteral stringLiteral => new StringLiteral(stringLiteral.Value.Value) { Span = stringLiteral.Span },
            FunctionInvocationTerm functionInvocationTerm => ParseFunctionInvocation(functionInvocationTerm.InvocationNode),
            MethodInvocationTerm methodInvocationTerm => ParseMethodInvocation(methodInvocationTerm.InvocationNode),
            IdentifierTerm identifierTerm => new VariableAccess(ParseIdentifier(identifierTerm.Identifier)) { Span = identifierTerm.Span },
            Paren paren => ParseExpression(paren.Expression),
            CastTerm castTerm => new Cast(ParseType(castTerm.Type), ParseExpression(castTerm.Expression)),
            _ => throw errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    protected IVariableType ParseType(IType type)
    {
        return type switch
        {
            BoolType boolType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool) { Span = boolType.Span },
            IntType intType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int) { Span = intType.Span },
            FloatType floatType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Float) { Span = floatType.Span },
            StringType stringType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.String) { Span = stringType.Span },
            _ => throw errorHelper.UnknownVariant("type", type.GetType())
        };
    }
    
    protected string ParseIdentifier(IdentifierToken identifier)
    {
        return identifier.Identifier;
    }

    protected List<Parameter> ParseParameterList(IParameterListNode parameterList)
    {
        return ParseList<IParameterListNode, OLangGrammar.ParseTree.ParameterList.Parameter, Parameter>(
            parameterList,
            x => x switch
            {
                ContinuedParameterList value => (value, value.ParameterList),
                OLangGrammar.ParseTree.ParameterList.Parameter parameter => (parameter, null),
                _ => throw errorHelper.UnknownVariant("parameter list", x.GetType())
            },
            ParseParameter
        );
    }

    protected Parameter ParseParameter(OLangGrammar.ParseTree.ParameterList.Parameter parameter)
    {
        return new Parameter(ParseIdentifier(parameter.Identifier), ParseType(parameter.Type)) { Span = parameter.Span };
    }

    protected FunctionInvocation ParseFunctionInvocation(IFunctionInvocation functionInvocation)
    {
        List<IExpression> arguments;
        IdentifierToken identifier;
        switch (functionInvocation)
        {
            case OLangGrammar.ParseTree.FunctionInvocation.FunctionInvocation invocation:
                arguments = new List<IExpression>();
                identifier = invocation.Identifier;
                break;
            case FunctionInvocationWithArguments invocationWithArguments:
                arguments = ParseArgumentList(invocationWithArguments.Arguments);
                identifier = invocationWithArguments.Identifier;
                break;
            default:
                throw errorHelper.UnknownVariant("function invocation", functionInvocation.GetType());
        }

        return new FunctionInvocation(ParseIdentifier(identifier), arguments) { Span = functionInvocation.Span };
    }
    
    protected MethodInvocation ParseMethodInvocation(IMethodInvocation methodInvocation)
    {
        IExpression obj;
        List<IExpression> arguments;
        IdentifierToken identifier;
        switch (methodInvocation)
        {
            case OLangGrammar.ParseTree.MethodInvocation.MethodInvocation invocation:
                obj = ParseTerm(invocation.Term);
                arguments = new List<IExpression>();
                identifier = invocation.Identifier;
                break;
            case MethodInvocationWithArguments invocationWithArguments:
                obj = ParseTerm(invocationWithArguments.Term);
                arguments = ParseArgumentList(invocationWithArguments.Arguments);
                identifier = invocationWithArguments.Identifier;
                break;
            default:
                throw errorHelper.UnknownVariant("function invocation", methodInvocation.GetType());
        }

        return new MethodInvocation(obj, ParseIdentifier(identifier), arguments) { Span = methodInvocation.Span };
    }
    
    protected List<IExpression> ParseArgumentList(IArgumentList argumentList)
    {
        return ParseList(
            argumentList,
            x => x switch
            {
                ContinuedArgumentList value => (value.Expression, value.ArgumentList),
                ExpressionArgumentList parameter => (parameter.Expression, null),
                _ => throw errorHelper.UnknownVariant("parameter list", x.GetType())
            },
            ParseExpression
        );
    }
}