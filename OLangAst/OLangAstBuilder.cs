using ErrorHelper;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.ElseBlock;
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
using IStatement = OLangAst.Statements.IStatement;
using Not = OLangAst.Expressions.Not;
using NotEqual = OLangAst.Expressions.NotEqual;
using Parameter = OLangAst.Statements.Parameter;
using Return = OLangAst.Statements.Return;

namespace OLangAst;

public class OLangAstBuilder(IErrorHelper errorHelper)
{
    public Program BuildAst(ProgramNode parsedProgram)
    {
        return ParseProgram(parsedProgram);
    }

    protected Program ParseProgram(ProgramNode programNode)
    {
        return new Program { Statements = ParseStatementList(programNode.StmtList) };
    }

    protected List<IStatement> ParseStatementList(IStmtListNode statementList)
    {
        switch (statementList)
        {
            case EmptyStmtList:
                return [];
            case StmtListWithStatement statement:
                var statements = new List<IStatement> { ParseStatement(statement.Statement) };
                IStmtListNode currentStatement = statement;

                while (currentStatement is StmtListWithStatement nextStatement)
                {
                    statements.Add(ParseStatement(nextStatement.Statement));
                    currentStatement = nextStatement.StmtList;
                }
                
                return statements;
            default:
                throw errorHelper.UnknownVariant("statement type", statementList.GetType());
        }
    }

    protected IStatement ParseStatement(OLangGrammar.ParseTree.Stmt.IStatement statement)
    {
        return statement switch
        {
            Assignment assignment => new VariableAssignment { Identifier = ParseIdentifier(assignment.Identifier), Value = ParseExpression(assignment.Expression) },
            Declaration declaration => new VariableDeclarationStatement { Identifier = ParseIdentifier(declaration.Identifier), Type = ParseType(declaration.Type) },
            LetDeclaration letDeclaration => new VariableDeclarationStatement { Identifier = ParseIdentifier(letDeclaration.Identifier), Type = null },
            Exit exit => new ExitStatement { Expression = ParseExpression(exit.Expression) },
            For forStatement => new ForLoop { Body = ParseScope(forStatement.Scope), Identifier = ParseIdentifier(forStatement.Identifier), RangeStart = ParseExpression(forStatement.Start), RangeEnd = ParseExpression(forStatement.End) },
            While whileStatement => new WhileLoop { Predicate = ParseExpression(whileStatement.Condition), Body = ParseScope(whileStatement.Scope) },
            OLangGrammar.ParseTree.Stmt.FunctionDeclaration functionDeclaration => new FunctionDeclaration { Identifier = ParseIdentifier(functionDeclaration.Identifier), Parameters = ParseParameterList(functionDeclaration.Parameters), Type = ParseType(functionDeclaration.Type) },
            VoidFunctionDeclaration voidFunctionDeclaration => new FunctionDeclaration { Identifier = ParseIdentifier(voidFunctionDeclaration.Identifier), Parameters = ParseParameterList(voidFunctionDeclaration.Parameters), },
            If ifStatement => new IfStatement { Predicate = ParseExpression(ifStatement.Condition), Body = ParseScope(ifStatement.Scope), Else = ParseElse(ifStatement.ElseBlock) },
            Invocation invocation => ParseFunctionInvocation(invocation.InvocationNode),
            OLangGrammar.ParseTree.Stmt.Return => new Return(),
            ReturnValue returnValueStatement => new Return { Value = ParseExpression(returnValueStatement.Expression) },
            ScopeStatement scopeStatement => ParseScope(scopeStatement.Scope),
            _ => throw errorHelper.UnknownVariant("statement", statement.GetType())
        };
    }

    protected Scope ParseScope(IScopeNode scope)
    {
        if (scope is not ScopeNode scopeNode)
        {
            throw errorHelper.UnknownVariant("scope", scope.GetType());
        }

        return new Scope { Statements = ParseStatementList(scopeNode.StmtList) };
    }

    protected IExpression ParseExpression(OLangGrammar.ParseTree.Expression.IExpression expression)
    {
        return expression switch
        {
            Expression orExpression => new Or { Left = ParseExpression(orExpression.Lhs), Right = ParseAndExpression(orExpression.Rhs) },
            NonExpression andExpression => ParseAndExpression(andExpression.Expression),
            _ => throw errorHelper.UnknownVariant("expression", expression.GetType())
        };
    }
    
    protected IExpression ParseAndExpression(IAndExpression expression)
    {
        return expression switch
        {
            AndExpression and => new And { Left = ParseAndExpression(and.Lhs), Right = ParseEqualityExpression(and.Rhs) },
            NonAnd nonAnd => ParseEqualityExpression(nonAnd.Expression),
            _ => throw errorHelper.UnknownVariant("and expression", expression.GetType())
        };
    }

    protected IExpression ParseEqualityExpression(IEqualityExpression expression)
    {
        return expression switch
        {
            DoubleEquals doubleEquals => new AreEqual { Left = ParseEqualityExpression(doubleEquals.Lhs), Right = ParseGreaterExpression(doubleEquals.Rhs) },
            OLangGrammar.ParseTree.EqualityExpression.NotEqual notEqual => new NotEqual { Left = ParseEqualityExpression(notEqual.Lhs), Right = ParseGreaterExpression(notEqual.Rhs) },
            NonEquality nonEquality => ParseGreaterExpression(nonEquality.Expression),
            _ => throw errorHelper.UnknownVariant("equality expression", expression.GetType())
        };
    }

    protected IExpression ParseGreaterExpression(IGreaterExpression expression)
    {
        return expression switch
        {
            Greater greater => new GreaterThan { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            OLangGrammar.ParseTree.GreaterExpression.GreaterOrEqual greater => new GreaterOrEqual { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            Less greater => new LessThan { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            LessOrEqual lessOrEqual => new GreaterOrEqual { Left = ParseGreaterExpression(lessOrEqual.Lhs), Right = ParseAddExpression(lessOrEqual.Rhs) },
            NonGreaterExpression nonGreaterExpression => ParseAddExpression(nonGreaterExpression.Expression),
            _ => throw errorHelper.UnknownVariant("greater expression", expression.GetType())
        };
    }

    protected IExpression ParseAddExpression(IAddExpression expression)
    {
        return expression switch
        {
            Plus plus => new Add { Left = ParseAddExpression(plus.Lhs), Right = ParseMultExpression(plus.Rhs) },
            Minus minus => new Subtract { Left = ParseAddExpression(minus.Lhs), Right = ParseMultExpression(minus.Rhs) },
            NonAddExpression nonAdd => ParseMultExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("add expression", expression.GetType())
        };
    }

    protected IExpression ParseMultExpression(IMultExpression expression)
    {
        return expression switch
        {
            Mult mu => new Multiply { Left = ParseMultExpression(mu.Lhs), Right = ParseUnaryExpression(mu.Rhs) },
            Div div => new Multiply { Left = ParseMultExpression(div.Lhs), Right = ParseUnaryExpression(div.Rhs) },
            NonMultExpression nonAdd => ParseUnaryExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseUnaryExpression(IUnaryExpression expression)
    {
        return expression switch
        {
            OLangGrammar.ParseTree.UnaryExpression.Not not => new Not { Value = ParseTerm(not.Term)},
            Negation not => new Negate { Value = ParseTerm(not.Term)},
            NonUnaryExpression nonUnary => ParseTerm(nonUnary.Term),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseTerm(ITerm term)
    {
        return term switch
        {
            OLangGrammar.ParseTree.Term.BoolLiteral boolLiteral => new BoolLiteral { Value = boolLiteral.Value.Value },
            OLangGrammar.ParseTree.Term.FloatLiteral floatLiteral => new FloatLiteral { Value = floatLiteral.Value.Value },
            FunctionInvocationTerm functionInvocationTerm => ParseFunctionInvocation(functionInvocationTerm.InvocationNode),
            IdentifierTerm identifierTerm => new VariableAccess { Identifier = ParseIdentifier(identifierTerm.Identifier) },

            _ => throw errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    protected IVariableType ParseType(IType type)
    {
        return type switch
        {
            BoolType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Bool },
            IntType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Int },
            FloatType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Float },
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
        return new Parameter { Identifier = ParseIdentifier(parameter.Identifier), Type = ParseType(parameter.Type) };
    }

    protected ElseBlock? ParseElse(IElse elseBlock)
    {
        if (elseBlock is EmptyElse)
        {
            return null;
        }

        if (elseBlock is Else actualElse)
        {
            return new ElseBlock { Body = ParseScope(actualElse.Scope) };
        }

        throw errorHelper.UnknownVariant("else block", elseBlock.GetType());
    }

    protected FunctionInvocation ParseFunctionInvocation(IFunctionInvocation functionInvocation)
    {
        if (functionInvocation is not OLangGrammar.ParseTree.FunctionInvocation.FunctionInvocation invocation)
        {
            throw errorHelper.UnknownVariant("function invocation", functionInvocation.GetType());
        }

        return new FunctionInvocation { Identifier = ParseIdentifier(invocation.Identifier), Arguments = ParseArgumentList(invocation.Arguments) };
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