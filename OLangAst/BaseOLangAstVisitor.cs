using ErrorHelper;
using OLangAst.ClassMembers;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;

namespace OLangAst;

public class BaseOLangAstVisitor(IErrorHelper errorHelper)
{
    protected readonly IErrorHelper ErrorHelper = errorHelper;

    public virtual Program VisitProgram(Program program)
    {
        program.ClassDeclarations = VisitClassDeclarations(program.ClassDeclarations);

        return program;
    }
    
    protected List<ClassDeclaration> VisitClassDeclarations(IEnumerable<ClassDeclaration> stmtList)
    {
        return stmtList.Select(VisitClassDeclaration).ToList();
    }
    
    protected virtual ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        classDeclaration.ClassMembers = classDeclaration.ClassMembers.Select(VisitClassMember).ToList();

        return classDeclaration;
    }

    protected List<IStatement> VisitStatements(IEnumerable<IStatement> stmtList)
    {
        return stmtList.Select(VisitStatement).ToList();
    }

    protected IClassMember VisitClassMember(IClassMember classMember)
    {
        return classMember switch
        {
            MethodDeclaration methodDeclaration => VisitMethodDeclaration(methodDeclaration),
            _ => throw ErrorHelper.UnknownVariant("class member", classMember.GetType())
        };
    }

    protected virtual IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        methodDeclaration.Scope = VisitScope(methodDeclaration.Scope);

        return methodDeclaration;
    }

    protected IStatement VisitStatement(IStatement statement)
    {
        return statement switch
        {
            VariableDeclarationStatement declarationStatement => VisitDeclarationStatement(declarationStatement),
            ExitStatement exitStatement => VisitExitStatement(exitStatement),
            PrintStatement printStatement => VisitPrintStatement(printStatement),
            VariableAssignment assignmentStatement => VisitVariableAssignment(assignmentStatement),
            Scope scopeStatement => VisitScope(scopeStatement),
            IfStatement ifStatement => VisitIfStatement(ifStatement),
            WhileLoop whileStatement => VisitWhileLoop(whileStatement),
            ForLoop forStatement => VisitForLoop(forStatement),
            FunctionDeclaration functionDeclaration => VisitFunctionDeclaration(functionDeclaration),
            FunctionInvocation invocation => VisitFunctionInvocationStatement(invocation),
            MethodInvocation invocation => VisitMethodInvocationStatement(invocation),
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
            InternalDefinedType type => type,
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
    
    protected virtual PrintStatement VisitPrintStatement(PrintStatement printStatement)
    {
        printStatement.Expression = VisitExpression(printStatement.Expression);

        return printStatement;
    }

    protected virtual VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        if (declarationStatement.DeclaredType != null)
        {
            declarationStatement.DeclaredType = VisitVariableType(declarationStatement.DeclaredType);
        }
        
        declarationStatement.Value = VisitExpression(declarationStatement.Value);

        return declarationStatement;
    }

    protected virtual FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        functionDeclaration.Scope = VisitScope(functionDeclaration.Scope);

        return functionDeclaration;
    }

    protected virtual IStatement VisitFunctionInvocationStatement(FunctionInvocation functionInvocation)
    {
        return (FunctionInvocation)VisitFunctionInvocation(functionInvocation);
    }
    
    protected virtual MethodInvocation VisitMethodInvocationStatement(MethodInvocation methodInvocation)
    {
        return VisitMethodInvocation(methodInvocation);
    }
    
    
    protected virtual IExpression VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        functionInvocation.Arguments = functionInvocation.Arguments.Select(VisitExpression).ToList();

        return functionInvocation;
    }
    
    protected virtual MethodInvocation VisitMethodInvocation(MethodInvocation methodInvocation)
    {
        if (methodInvocation.Expression != null)
        {
            methodInvocation.Expression = VisitExpression(methodInvocation.Expression);
        }
        
        methodInvocation.Arguments = methodInvocation.Arguments.Select(VisitExpression).ToList();
        
        return methodInvocation;
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
            Cast cast => VisitCast(cast),
            MethodInvocation methodInvocation => VisitMethodInvocation(methodInvocation),
            _ => throw ErrorHelper.UnknownVariant("expression", expression.GetType())
        };
    }

    protected virtual IExpression VisitNegate(Negate negate)
    {
        negate.Value = VisitExpression(negate.Value);

        return negate;
    }

    protected virtual IExpression VisitFloatLiteral(FloatLiteral floatLiteral)
    {
        return floatLiteral;
    }

    protected virtual IExpression VisitIntLiteral(IntLiteral intLiteral)
    {
        return intLiteral;
    }

    protected virtual IExpression VisitBoolLiteral(BoolLiteral boolLiteral)
    {
        return boolLiteral;
    }
    
    protected virtual IExpression VisitStringLiteral(StringLiteral stringLiteral)
    {
        return stringLiteral;
    }

    protected virtual IExpression VisitCast(Cast cast)
    {
        cast.TargetType = VisitVariableType(cast.TargetType);
        cast.Value = VisitExpression(cast.Value);

        return cast;
    }

    protected virtual IExpression VisitVariableAccess(VariableAccess variableAccess)
    {
        return variableAccess;
    }

    protected virtual IExpression VisitNotExpression(Not notExpression)
    {
        notExpression.Value = VisitExpression(notExpression.Value);

        return notExpression;
    }
    
    protected virtual IExpression VisitAddExpression(Add addExpression)
    {
        return VisitBinaryExpression(addExpression);
    }
    
    protected virtual IExpression VisitAndExpression(And andExpression)
    {
        return VisitBinaryExpression(andExpression);
    }
    
    protected virtual IExpression VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        return VisitBinaryExpression(areEqualExpression);
    }

    protected virtual IExpression VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        return VisitBinaryExpression(greaterOrEqualExpression);
    }

    protected virtual IExpression VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        return VisitBinaryExpression(greaterThanExpression);
    }

    protected virtual IExpression VisitLessThanExpression(LessThan lessThanExpression)
    {
        return VisitBinaryExpression(lessThanExpression);
    }

    protected virtual IExpression VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        return VisitBinaryExpression(lessThanOrEqualExpression);
    }

    protected virtual IExpression VisitDivideExpression(Divide divideExpression)
    {
        return VisitBinaryExpression(divideExpression);
    }
    
    protected virtual IExpression VisitMultiplyExpression(Multiply multiplyExpression)
    {
        return VisitBinaryExpression(multiplyExpression);
    }

    protected virtual IExpression VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        return VisitBinaryExpression(notEqualExpression);
    }

    protected virtual IExpression VisitOrExpression(Or orExpression)
    {
        return VisitBinaryExpression(orExpression);
    }

    protected virtual IExpression VisitSubtractExpression(Subtract subtractExpression)
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