using ErrorHelper;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.FunctionInvocation;
using OLangGrammar.ParseTree.GreaterExpression;
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
using FunctionInvocation = OLangAst.Statements.FunctionInvocation;
using GreaterOrEqual = OLangAst.Expressions.GreaterOrEqual;
using IExpression = OLangAst.Expressions.IExpression;
using IntLiteral = OLangAst.Expressions.IntLiteral;
using IStatement = OLangAst.Statements.IStatement;
using Not = OLangAst.Expressions.Not;
using NotEqual = OLangAst.Expressions.NotEqual;
using Parameter = OLangAst.Statements.Parameter;
using Return = OLangAst.Statements.Return;

namespace OLangAst;

public class OLangAstBuilder(IErrorHelper errorHelper)
{
    public Program ParseProgram(ProgramNode programNode)
    {
        var statements = ParseStatementList(programNode.StmtList);
        return new Program(statements);
    }

    protected List<IStatement> ParseStatementList(IStmtListNode statementList)
    {
        switch (statementList)
        {
            case SingleStatementStmtList statement:
                return [ParseStatement(statement.Statement)];
            case StmtListWithStatement statement:
                var statements = new List<IStatement>();
                IStmtListNode currentStatement = statement;

                while (currentStatement is StmtListWithStatement nextStatement)
                {
                    statements.Add(ParseStatement(nextStatement.Statement));
                    currentStatement = nextStatement.StmtList;
                }
                
                statements.Add(ParseStatement(((SingleStatementStmtList)currentStatement).Statement));
                
                return statements;
            default:
                throw errorHelper.UnknownVariant("statement type", statementList.GetType());
        }
    }

    protected IStatement ParseStatement(OLangGrammar.ParseTree.Stmt.IStatement statement)
    {
        return statement switch
        {
            Assignment assignment => ParseAssignment(assignment),
            Declaration declaration => new VariableDeclarationStatement(ParseType(declaration.Type), ParseIdentifier(declaration.Identifier), ParseExpression(declaration.Expression)),
            LetDeclaration letDeclaration => new VariableDeclarationStatement(null, ParseIdentifier(letDeclaration.Identifier), ParseExpression(letDeclaration.Expression)),
            Exit exit => new ExitStatement(ParseExpression(exit.Expression)),
            For forStatement => new ForLoop(ParseIdentifier(forStatement.Identifier), ParseExpression(forStatement.Start), ParseExpression(forStatement.End), ParseScope(forStatement.Scope)),
            While whileStatement => new WhileLoop(ParseExpression(whileStatement.Condition), ParseScope(whileStatement.Scope)),
            FunctionDeclarationWithParameters functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), ParseParameterList(functionDeclaration.Parameters)),
            VoidFunctionDeclarationWithParameters voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), ParseParameterList(voidFunctionDeclaration.Parameters)),
            OLangGrammar.ParseTree.Stmt.FunctionDeclaration functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), []),
            VoidFunctionDeclaration voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), []),
            If ifStatement => new IfStatement(ParseExpression(ifStatement.Condition), ParseScope(ifStatement.Scope), null),
            IfWithElse ifStatementWithElse => new IfStatement(ParseExpression(ifStatementWithElse.Condition), ParseScope(ifStatementWithElse.Scope), ParseScope(ifStatementWithElse.ElseBlock)),
            Invocation invocation => ParseFunctionInvocation(invocation.InvocationNode),
            OLangGrammar.ParseTree.Stmt.Return => new Return(null),
            ReturnValue returnValueStatement => new Return(ParseExpression(returnValueStatement.Expression)),
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
            PlusEquals => new Add(new VariableAccess(identifier), parsedExpression),
            MinusEquals => new Subtract(new VariableAccess(identifier), parsedExpression),
            TimesEquals => new Multiply(new VariableAccess(identifier), parsedExpression),
            DivideEquals => new Divide(new VariableAccess(identifier), parsedExpression),
            _ => throw errorHelper.UnknownVariant("assignment operator", assignment.Operator.GetType())
        };
        return new VariableAssignment(ParseIdentifier(assignment.Identifier), value);
    }

    protected Scope ParseScope(IScopeNode scope)
    {
        return scope switch
        {
            EmptyScope _ => new Scope([]),
            ScopeNode scopeNode => new Scope(ParseStatementList(scopeNode.StmtList)),
            _ => throw errorHelper.UnknownVariant("scope", scope.GetType())
        };
    }

    protected IExpression ParseExpression(OLangGrammar.ParseTree.Expression.IExpression expression)
    {
        return expression switch
        {
            Expression orExpression => new Or(ParseExpression(orExpression.Lhs), ParseAndExpression(orExpression.Rhs)),
            NonExpression andExpression => ParseAndExpression(andExpression.Expression),
            _ => throw errorHelper.UnknownVariant("expression", expression.GetType())
        };
    }
    
    protected IExpression ParseAndExpression(IAndExpression expression)
    {
        return expression switch
        {
            AndExpression and => new And(ParseAndExpression(and.Lhs), ParseEqualityExpression(and.Rhs)),
            NonAnd nonAnd => ParseEqualityExpression(nonAnd.Expression),
            _ => throw errorHelper.UnknownVariant("and expression", expression.GetType())
        };
    }

    protected IExpression ParseEqualityExpression(IEqualityExpression expression)
    {
        return expression switch
        {
            DoubleEquals doubleEquals => new AreEqual(ParseEqualityExpression(doubleEquals.Lhs), ParseGreaterExpression(doubleEquals.Rhs)),
            OLangGrammar.ParseTree.EqualityExpression.NotEqual notEqual => new NotEqual(ParseEqualityExpression(notEqual.Lhs), ParseGreaterExpression(notEqual.Rhs)),
            NonEquality nonEquality => ParseGreaterExpression(nonEquality.Expression),
            _ => throw errorHelper.UnknownVariant("equality expression", expression.GetType())
        };
    }

    protected IExpression ParseGreaterExpression(IGreaterExpression expression)
    {
        return expression switch
        {
            Greater greater => new GreaterThan(ParseGreaterExpression(greater.Lhs), ParseAddExpression(greater.Rhs)),
            OLangGrammar.ParseTree.GreaterExpression.GreaterOrEqual greater => new GreaterOrEqual(ParseGreaterExpression(greater.Lhs), ParseAddExpression(greater.Rhs)),
            Less greater => new LessThan(ParseGreaterExpression(greater.Lhs), ParseAddExpression(greater.Rhs)),
            LessOrEqual lessOrEqual => new GreaterOrEqual(ParseGreaterExpression(lessOrEqual.Lhs), ParseAddExpression(lessOrEqual.Rhs)),
            NonGreaterExpression nonGreaterExpression => ParseAddExpression(nonGreaterExpression.Expression),
            _ => throw errorHelper.UnknownVariant("greater expression", expression.GetType())
        };
    }

    protected IExpression ParseAddExpression(IAddExpression expression)
    {
        return expression switch
        {
            Plus plus => new Add(ParseAddExpression(plus.Lhs), ParseMultExpression(plus.Rhs)),
            Minus minus => new Subtract(ParseAddExpression(minus.Lhs), ParseMultExpression(minus.Rhs)),
            NonAddExpression nonAdd => ParseMultExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("add expression", expression.GetType())
        };
    }

    protected IExpression ParseMultExpression(IMultExpression expression)
    {
        return expression switch
        {
            Mult mu => new Multiply(ParseMultExpression(mu.Lhs), ParseUnaryExpression(mu.Rhs)),
            Div div => new Multiply(ParseMultExpression(div.Lhs), ParseUnaryExpression(div.Rhs)),
            NonMultExpression nonAdd => ParseUnaryExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseUnaryExpression(IUnaryExpression expression)
    {
        return expression switch
        {
            OLangGrammar.ParseTree.UnaryExpression.Not not => new Not(ParseTerm(not.Term)),
            Negation not => new Negate(ParseTerm(not.Term)),
            NonUnaryExpression nonUnary => ParseTerm(nonUnary.Term),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseTerm(ITerm term)
    {
        return term switch
        {
            OLangGrammar.ParseTree.Term.BoolLiteral boolLiteral => new BoolLiteral(boolLiteral.Value.Value),
            OLangGrammar.ParseTree.Term.IntLiteral intLiteral => new IntLiteral(intLiteral.Value.Value),
            OLangGrammar.ParseTree.Term.FloatLiteral floatLiteral => new FloatLiteral(floatLiteral.Value.Value),
            FunctionInvocationTerm functionInvocationTerm => ParseFunctionInvocation(functionInvocationTerm.InvocationNode),
            IdentifierTerm identifierTerm => new VariableAccess(ParseIdentifier(identifierTerm.Identifier)),

            _ => throw errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    protected IVariableType ParseType(IType type)
    {
        return type switch
        {
            BoolType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool),
            IntType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int),
            FloatType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Float),
            _ => throw errorHelper.UnknownVariant("type", type.GetType())
        };
    }
    
    protected string ParseIdentifier(IdentifierToken identifier)
    {
        return identifier.Identifier;
    }

    protected List<Parameter> ParseParameterList(IParameterListNode parameterList)
    {
        if (parameterList is EmptyParameterList)
        {
            return [];
        }

        var parameters = new List<Parameter>();
        while (parameterList is ContinuedParameterList continued)
        {
            parameters.Add(ParseParameter(continued));
            parameterList = continued.ParameterList;
        }

        if (parameterList is OLangGrammar.ParseTree.ParameterList.Parameter singleParameter)
        {
            return [ParseParameter(singleParameter)];
        }

        return parameters;
    }

    protected Parameter ParseParameter(OLangGrammar.ParseTree.ParameterList.Parameter parameter)
    {
        return new Parameter(ParseIdentifier(parameter.Identifier), ParseType(parameter.Type));
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

        return new FunctionInvocation(ParseIdentifier(identifier), arguments);
    }
    
    protected List<IExpression> ParseArgumentList(IArgumentList argumentList)
    {
        if (argumentList is EmptyArgumentList)
        {
            return [];
        }

        var parameters = new List<IExpression>();
        while (argumentList is ContinuedArgumentList continued)
        {
            parameters.Add(ParseExpression(continued.Expression));
            argumentList = continued.ArgumentList;
        }

        if (argumentList is ExpressionArgumentList singleParameter)
        {
            return [ParseExpression(singleParameter.Expression)];
        }

        return parameters;
    }
}