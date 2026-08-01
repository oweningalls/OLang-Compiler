using ErrorHelper;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangCompiler.Parser.ParseTree.AddExpression;
using OLangCompiler.Parser.ParseTree.AndExpression;
using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.EqualityExpression;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.FunctionInvocation;
using OLangCompiler.Parser.ParseTree.GreaterExpression;
using OLangCompiler.Parser.ParseTree.MultExpression;
using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Prog;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Parser.ParseTree.Stmt;
using OLangCompiler.Parser.ParseTree.StmtList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Parser.ParseTree.UnaryExpression;
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
    private IErrorHelper _errorHelper = errorHelper;

    public Program BuildAst(ProgramNode parsedProgram)
    {
        return ParseProgram(parsedProgram);
    }

    private Program ParseProgram(ProgramNode programNode)
    {
        return new Program { Statements = ParseStatementList(programNode.StmtList) };
    }

    private List<IStatement> ParseStatementList(IStmtListNode statementList)
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
                throw _errorHelper.UnknownVariant("statement type", statementList.GetType());
        }
    }

    private IStatement ParseStatement(OLangCompiler.Parser.ParseTree.Stmt.IStatement statement)
    {
        return statement switch
        {
            Assignment assignment => new VariableAssignment { Identifier = ParseIdentifier(assignment.Identifier), Value = ParseExpression(assignment.Expression) },
            Declaration declaration => new VariableDeclarationStatement { Identifier = ParseIdentifier(declaration.Identifier), Type = ParseType(declaration.Type) },
            LetDeclaration letDeclaration => new VariableDeclarationStatement { Identifier = ParseIdentifier(letDeclaration.Identifier), Type = null },
            Exit exit => new ExitStatement { Expression = ParseExpression(exit.Expression) },
            For forStatement => new ForLoop { Body = ParseScope(forStatement.Scope), Identifier = ParseIdentifier(forStatement.Identifier), RangeStart = ParseExpression(forStatement.Start), RangeEnd = ParseExpression(forStatement.End) },
            While whileStatement => new WhileLoop { Predicate = ParseExpression(whileStatement.Condition), Body = ParseScope(whileStatement.Scope) },
            OLangCompiler.Parser.ParseTree.Stmt.FunctionDeclaration functionDeclaration => new FunctionDeclaration { Identifier = ParseIdentifier(functionDeclaration.Identifier), Parameters = ParseParameterList(functionDeclaration.Parameters), Type = ParseType(functionDeclaration.Type) },
            VoidFunctionDeclaration voidFunctionDeclaration => new FunctionDeclaration { Identifier = ParseIdentifier(voidFunctionDeclaration.Identifier), Parameters = ParseParameterList(voidFunctionDeclaration.Parameters), },
            If ifStatement => new IfStatement { Predicate = ParseExpression(ifStatement.Condition), Body = ParseScope(ifStatement.Scope), Else = ParseElse(ifStatement.ElseBlock) },
            Invocation invocation => ParseFunctionInvocation(invocation.InvocationNode),
            OLangCompiler.Parser.ParseTree.Stmt.Return => new Return(),
            ReturnValue returnValueStatement => new Return { Value = ParseExpression(returnValueStatement.Expression) },
            ScopeStatement scopeStatement => ParseScope(scopeStatement.Scope),
            _ => throw _errorHelper.UnknownVariant("statement", statement.GetType())
        };
    }

    private Scope ParseScope(IScopeNode scope)
    {
        if (scope is not ScopeNode scopeNode)
        {
            throw _errorHelper.UnknownVariant("scope", scope.GetType());
        }

        return new Scope { Statements = ParseStatementList(scopeNode.StmtList) };
    }

    private IExpression ParseExpression(OLangCompiler.Parser.ParseTree.Expression.IExpression expression)
    {
        return expression switch
        {
            Expression orExpression => new Or { Left = ParseExpression(orExpression.Lhs), Right = ParseAndExpression(orExpression.Rhs) },
            NonExpression andExpression => ParseAndExpression(andExpression.Expression),
            _ => throw _errorHelper.UnknownVariant("expression", expression.GetType())
        };
    }
    
    private IExpression ParseAndExpression(IAndExpression expression)
    {
        return expression switch
        {
            AndExpression and => new And { Left = ParseAndExpression(and.Lhs), Right = ParseEqualityExpression(and.Rhs) },
            NonAnd nonAnd => ParseEqualityExpression(nonAnd.Expression),
            _ => throw _errorHelper.UnknownVariant("and expression", expression.GetType())
        };
    }

    private IExpression ParseEqualityExpression(IEqualityExpression expression)
    {
        return expression switch
        {
            DoubleEquals doubleEquals => new AreEqual { Left = ParseEqualityExpression(doubleEquals.Lhs), Right = ParseGreaterExpression(doubleEquals.Rhs) },
            OLangCompiler.Parser.ParseTree.EqualityExpression.NotEqual notEqual => new NotEqual { Left = ParseEqualityExpression(notEqual.Lhs), Right = ParseGreaterExpression(notEqual.Rhs) },
            NonEquality nonEquality => ParseGreaterExpression(nonEquality.Expression),
            _ => throw _errorHelper.UnknownVariant("equality expression", expression.GetType())
        };
    }

    private IExpression ParseGreaterExpression(IGreaterExpression expression)
    {
        return expression switch
        {
            Greater greater => new GreaterThan { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            OLangCompiler.Parser.ParseTree.GreaterExpression.GreaterOrEqual greater => new GreaterOrEqual { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            Less greater => new LessThan { Left = ParseGreaterExpression(greater.Lhs), Right = ParseAddExpression(greater.Rhs) },
            LessOrEqual lessOrEqual => new GreaterOrEqual { Left = ParseGreaterExpression(lessOrEqual.Lhs), Right = ParseAddExpression(lessOrEqual.Rhs) },
            NonGreaterExpression nonGreaterExpression => ParseAddExpression(nonGreaterExpression.Expression),
            _ => throw _errorHelper.UnknownVariant("greater expression", expression.GetType())
        };
    }

    private IExpression ParseAddExpression(IAddExpression expression)
    {
        return expression switch
        {
            Plus plus => new Add { Left = ParseAddExpression(plus.Lhs), Right = ParseMultExpression(plus.Rhs) },
            Minus minus => new Subtract { Left = ParseAddExpression(minus.Lhs), Right = ParseMultExpression(minus.Rhs) },
            NonAddExpression nonAdd => ParseMultExpression(nonAdd.Expression),
            _ => throw _errorHelper.UnknownVariant("add expression", expression.GetType())
        };
    }

    private IExpression ParseMultExpression(IMultExpression expression)
    {
        return expression switch
        {
            Mult mu => new Multiply { Left = ParseMultExpression(mu.Lhs), Right = ParseUnaryExpression(mu.Rhs) },
            Div div => new Multiply { Left = ParseMultExpression(div.Lhs), Right = ParseUnaryExpression(div.Rhs) },
            NonMultExpression nonAdd => ParseUnaryExpression(nonAdd.Expression),
            _ => throw _errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    private IExpression ParseUnaryExpression(IUnaryExpression expression)
    {
        return expression switch
        {
            OLangCompiler.Parser.ParseTree.UnaryExpression.Not not => new Not { Value = ParseTerm(not.Term)},
            Negation not => new Negate { Value = ParseTerm(not.Term)},
            NonUnaryExpression nonUnary => ParseTerm(nonUnary.Term),
            _ => throw _errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    private IExpression ParseTerm(ITerm term)
    {
        return term switch
        {
            OLangCompiler.Parser.ParseTree.Term.BoolLiteral boolLiteral => new BoolLiteral { Value = boolLiteral.Value.Value },
            OLangCompiler.Parser.ParseTree.Term.FloatLiteral floatLiteral => new FloatLiteral { Value = floatLiteral.Value.Value },
            FunctionInvocationTerm functionInvocationTerm => ParseFunctionInvocation(functionInvocationTerm.InvocationNode),
            IdentifierTerm identifierTerm => new VariableAccess { Identifier = ParseIdentifier(identifierTerm.Identifier) },

            _ => throw _errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    private IVariableType ParseType(IType type)
    {
        return type switch
        {
            BoolType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Bool },
            IntType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Int },
            FloatType => new PrimitiveVariableType { Type = PrimitiveVariableTypeEnum.Float },
            _ => throw _errorHelper.UnknownVariant("type", type.GetType())
        };
    }
    
    private string ParseIdentifier(IdentifierToken identifier)
    {
        return identifier.Identifier;
    }

    private List<Parameter> ParseParameterList(IParameterListNode parameterList)
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

        if (parameterList is OLangCompiler.Parser.ParseTree.ParameterList.Parameter singleParameter)
        {
            return [ParseParameter(singleParameter)];
        }

        return parameters;
    }

    private Parameter ParseParameter(OLangCompiler.Parser.ParseTree.ParameterList.Parameter parameter)
    {
        return new Parameter { Identifier = ParseIdentifier(parameter.Identifier), Type = ParseType(parameter.Type) };
    }

    private ElseBlock? ParseElse(IElse elseBlock)
    {
        if (elseBlock is EmptyElse)
        {
            return null;
        }

        if (elseBlock is Else actualElse)
        {
            return new ElseBlock { Body = ParseScope(actualElse.Scope) };
        }

        throw _errorHelper.UnknownVariant("else block", elseBlock.GetType());
    }

    private FunctionInvocation ParseFunctionInvocation(IFunctionInvocation functionInvocation)
    {
        if (functionInvocation is not OLangCompiler.Parser.ParseTree.FunctionInvocation.FunctionInvocation invocation)
        {
            throw _errorHelper.UnknownVariant("function invocation", functionInvocation.GetType());
        }

        return new FunctionInvocation { Identifier = ParseIdentifier(invocation.Identifier), Arguments = ParseArgumentList(invocation.Arguments) };
    }
    
    private List<IExpression> ParseArgumentList(IArgumentList argumentList)
    {
        if (argumentList is EmptyParameterList)
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