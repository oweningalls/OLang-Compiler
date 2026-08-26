using ErrorHelper;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;

namespace OLangAst;

public class BaseOLangAstVisitor(IErrorHelper errorHelper)
{
    protected readonly IErrorHelper ErrorHelper = errorHelper;

    public virtual Program VisitProgram(Program program)
    {
        program.Statements = VisitStatements(program.Statements);

        return program;
    }

    protected List<IStatement> VisitStatements(IEnumerable<IStatement> stmtList)
    {
        return stmtList.Select(VisitStatement).ToList();
    }

    protected IStatement VisitStatement(IStatement statement)
    {
        return statement switch
        {
            VariableDeclarationStatement declarationStatement => VisitDeclarationStatement(declarationStatement),
            ExitStatement exitStatement => VisitExitStatement(exitStatement),
            VariableAssignment assignmentStatement => VisitVariableAssignment(assignmentStatement),
            Scope scopeStatement => VisitScope(scopeStatement),
            IfStatement ifStatement => VisitIfStatement(ifStatement),
            WhileLoop whileStatement => VisitWhileLoop(whileStatement),
            ForLoop forStatement => VisitForLoop(forStatement),
            FunctionDeclaration functionDeclaration => VisitFunctionDeclaration(functionDeclaration),
            FunctionInvocation invocation => VisitFunctionInvocationStatement(invocation),
            Return returnStatement => VisitReturnStatement(returnStatement),
            _ => throw ErrorHelper.UnknownVariant("statement", statement.GetType())
        };
    }

    protected virtual Return VisitReturnStatement(Return returnStatement)
    {
        if (returnStatement.Value != null)
        {
            returnStatement.Value = VisitExpression(returnStatement.Value);
        }

        return returnStatement;
    }

    protected virtual ForLoop VisitForLoop(ForLoop forStatement)
    {
        forStatement.RangeStart = VisitExpression(forStatement.RangeStart);
        forStatement.RangeEnd = VisitExpression(forStatement.RangeEnd);
        forStatement.Body = VisitScope(forStatement.Body);

        return forStatement;
    }

    protected virtual IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement.Predicate = VisitExpression(ifStatement.Predicate);
        ifStatement.Body = VisitScope(ifStatement.Body);
        if (ifStatement.Else != null)
        {
            ifStatement.Else = VisitScope(ifStatement.Else);
        }

        return ifStatement;
    }

    protected virtual WhileLoop VisitWhileLoop(WhileLoop whileStatement)
    {
        whileStatement.Predicate = VisitExpression(whileStatement.Predicate);
        whileStatement.Body = VisitScope(whileStatement.Body);

        return whileStatement;
    }

    protected virtual VariableAssignment VisitVariableAssignment(VariableAssignment assignmentStatement)
    {
        assignmentStatement.Value = VisitExpression(assignmentStatement.Value);
        return assignmentStatement;
    }

    protected IVariableType VisitVariableType(IVariableType variableType)
    {
        return variableType switch
        {
            PrimitiveVariableType primitiveVariableType => VisitPrimitiveVariableType(primitiveVariableType),
            _ => throw ErrorHelper.UnknownVariant("variable type", variableType.GetType())
        };
    }
    
    protected virtual PrimitiveVariableType VisitPrimitiveVariableType(PrimitiveVariableType variableType)
    {
        return variableType;
    }

    protected virtual ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement.Expression = VisitExpression(exitStatement.Expression);

        return exitStatement;
    }

    protected virtual VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        if (declarationStatement.Type != null)
        {
            declarationStatement.Type = VisitVariableType(declarationStatement.Type);
        }
        
        declarationStatement.Value = VisitExpression(declarationStatement.Value);

        return declarationStatement;
    }

    protected virtual FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        functionDeclaration.Scope = VisitScope(functionDeclaration.Scope);

        return functionDeclaration;
    }

    protected virtual FunctionInvocation VisitFunctionInvocationStatement(FunctionInvocation functionInvocation)
    {
        return VisitFunctionInvocation(functionInvocation);
    }
    
    protected virtual FunctionInvocation VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation.Arguments = functionInvocation.Arguments.Select(VisitExpression).ToList();

        return functionInvocation;
    }

    protected virtual Scope VisitScope(Scope scopeNode)
    {
        scopeNode.Statements = VisitStatements(scopeNode.Statements);

        return scopeNode;
    }

    protected IExpression VisitExpression(IExpression expression)
    {
        return expression switch
        {
            And and => VisitAndExpression(and),
            AreEqual areEqual => VisitAreEqualExpression(areEqual),
            Not notExpression => VisitNotExpression(notExpression),
            Add addExpression => VisitAddExpression(addExpression),
            Divide divide => VisitDivideExpression(divide),
            GreaterOrEqual greaterOrEqual => VisitGreaterOrEqualExpression(greaterOrEqual),
            GreaterThan greaterThan => VisitGreaterThanExpression(greaterThan),
            LessThan lessThan => VisitLessThanExpression(lessThan),
            LessThanOrEqual lessThanOrEqual => VisitLessThanOrEqualExpression(lessThanOrEqual),
            Multiply multiply => VisitMultiplyExpression(multiply),
            NotEqual notEqual => VisitNotEqualExpression(notEqual),
            Or or => VisitOrExpression(or),
            Subtract subtract => VisitSubtractExpression(subtract),
            VariableAccess variableAccess => VisitVariableAccess(variableAccess),
            BoolLiteral boolLiteral => VisitBoolLiteral(boolLiteral),
            IntLiteral intLiteral => VisitIntLiteral(intLiteral),
            FloatLiteral floatLiteral => VisitFloatLiteral(floatLiteral),
            StringLiteral stringLiteral => VisitStringLiteral(stringLiteral),
            FunctionInvocation invocation => VisitFunctionInvocation(invocation),
            Negate negate => VisitNegate(negate),
            _ => throw ErrorHelper.UnknownVariant("binary expression", expression.GetType())
        };
    }

    protected virtual Negate VisitNegate(Negate negate)
    {
        negate.Value = VisitExpression(negate.Value);

        return negate;
    }

    protected FloatLiteral VisitFloatLiteral(FloatLiteral floatLiteral)
    {
        return floatLiteral;
    }

    protected virtual IntLiteral VisitIntLiteral(IntLiteral intLiteral)
    {
        return intLiteral;
    }

    protected virtual BoolLiteral VisitBoolLiteral(BoolLiteral boolLiteral)
    {
        return boolLiteral;
    }
    
    protected virtual StringLiteral VisitStringLiteral(StringLiteral stringLiteral)
    {
        return stringLiteral;
    }


    protected virtual VariableAccess VisitVariableAccess(VariableAccess variableAccess)
    {
        return variableAccess;
    }

    protected virtual Not VisitNotExpression(Not notExpression)
    {
        notExpression.Value = VisitExpression(notExpression.Value);

        return notExpression;
    }
    
    protected virtual Add VisitAddExpression(Add addExpression)
    {
        return VisitBinaryExpression(addExpression);
    }
    
    protected virtual And VisitAndExpression(And andExpression)
    {
        return VisitBinaryExpression(andExpression);
    }
    
    protected virtual AreEqual VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        return VisitBinaryExpression(areEqualExpression);
    }

    protected virtual GreaterOrEqual VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        return VisitBinaryExpression(greaterOrEqualExpression);
    }

    protected virtual GreaterThan VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        return VisitBinaryExpression(greaterThanExpression);
    }

    protected virtual LessThan VisitLessThanExpression(LessThan lessThanExpression)
    {
        return VisitBinaryExpression(lessThanExpression);
    }

    protected virtual LessThanOrEqual VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        return VisitBinaryExpression(lessThanOrEqualExpression);
    }

    protected virtual Divide VisitDivideExpression(Divide divideExpression)
    {
        return VisitBinaryExpression(divideExpression);
    }
    
    protected virtual Multiply VisitMultiplyExpression(Multiply multiplyExpression)
    {
        return VisitBinaryExpression(multiplyExpression);
    }

    protected virtual NotEqual VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        return VisitBinaryExpression(notEqualExpression);
    }

    protected virtual Or VisitOrExpression(Or orExpression)
    {
        return VisitBinaryExpression(orExpression);
    }

    protected virtual Subtract VisitSubtractExpression(Subtract subtractExpression)
    {
        return VisitBinaryExpression(subtractExpression);
    }

    protected virtual T VisitBinaryExpression<T>(T binaryExpression) where T : BaseBinaryExpression
    {
        binaryExpression.Lhs = VisitExpression(binaryExpression.Lhs);
        binaryExpression.Rhs = VisitExpression(binaryExpression.Rhs);

        return binaryExpression;
    }
}