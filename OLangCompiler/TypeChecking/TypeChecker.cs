using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;
using OLangCompiler.Utility;

namespace OLangCompiler.TypeChecking;

public class TypeChecker
{
    private ErrorHelper _errorHelper;
    public void CheckTypes(ProgramNode program, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _variableTypeStack = new ScopeTracker<string, ExpressionType>();
        foreach (var statement in program.Statements)
        {
            CheckStatementType(statement);
        }
    }

    private void CheckStatementType(IStatementNode statementNode)
    {
        switch (statementNode)
        {
            case DeclarationStatement declarationStatement:
                var type = GetTypeFromExpression(declarationStatement.Expression);
                if (declarationStatement.ExpressionType != null && type != declarationStatement.ExpressionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Expression type {type} does not match variable type {declarationStatement.ExpressionType}", declarationStatement.Expression);
                }

                RecordExpressionType(declarationStatement.Identifier, type);
                break;
            case ExitStatement exitStatement:
                var exitType = GetTypeFromExpression(exitStatement.ExpressionNode);
                if (exitType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Exit code has to be an integer", exitStatement.ExpressionNode);
                }

                break;
            case AssignmentStatement assignmentStatement:
                var expressionType = GetTypeFromExpression(assignmentStatement.Expression);
                var variableType = GetVariableType(assignmentStatement.Identifier);
                if (variableType != expressionType)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot assign variable {assignmentStatement.Identifier.Identifier} of type {variableType} to expression of type {expressionType}", assignmentStatement.Expression);
                }

                break;
            case ScopeStatement scopeStatement:
                CheckScopeTypes(scopeStatement.Scope);
                break;
            case IfStatement ifStatement:
                var conditionType = GetTypeFromExpression(ifStatement.Condition);
                if (conditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", ifStatement.Condition);
                }

                CheckScopeTypes(ifStatement.Scope);
                break;
            case WhileStatement whileStatement:
                var whileConditionType = GetTypeFromExpression(whileStatement.Condition);
                if (whileConditionType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("If predicate must be a boolean", whileStatement.Condition);
                }

                CheckScopeTypes(whileStatement.Scope);
                break;
            case ForStatement forStatement:
                _variableTypeStack.BeginScope();
                var startType = GetTypeFromExpression(forStatement.Start);
                if (startType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Bounds of range must be integers", forStatement.Start);
                }
                var endType = GetTypeFromExpression(forStatement.End);
                if (endType != ExpressionType.Int)
                {
                    throw _errorHelper.ShowErrorMessageAtNode("Bounds of range must be integers", forStatement.End);
                }
                
                RecordExpressionType(forStatement.Identifier, ExpressionType.Int);
                CheckScopeTypes(forStatement.Scope);
                _variableTypeStack.EndScope();
                break;
            default:
                throw _errorHelper.UnknownVariant("statement", statementNode.GetType());
        }
    }

    private void CheckScopeTypes(ScopeNode scopeNode)
    {
        _variableTypeStack.BeginScope();
        foreach (var statement in scopeNode.Statements)
        {
            CheckStatementType(statement);
        }

        _variableTypeStack.EndScope();
    }

    private ExpressionType GetTypeFromExpression(IExpressionNode expression)
    {
        switch (expression)
        {
            case TermExpression term:
                return GetTypeFromTerm(term.Term);
            case NotExpression notExpression:
                var notTermType = GetTypeFromExpression(notExpression.Expression);
                if (notTermType != ExpressionType.Bool)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot negate a non-boolean value of type {notTermType}", notExpression.Expression);
                }

                return ExpressionType.Bool;
            case BaseBinaryExpressionNode binaryExpression:
            {
                var lhsType = GetTypeFromExpression(binaryExpression.Lhs);
                var rhsType = GetTypeFromExpression(binaryExpression.Rhs);

                if (binaryExpression is BaseComparisonExpressionNode)
                {
                    if (lhsType != rhsType)
                    {
                        throw _errorHelper.ShowErrorMessageAtNode($"Cannot compare expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                    }

                    return ExpressionType.Bool;
                }

                var type = GetTypeOfBinaryExpression(lhsType, rhsType);
                if (type == null)
                {
                    throw _errorHelper.ShowErrorMessageAtNode($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                }

                return type.Value;
            }
            default:
                throw _errorHelper.UnknownVariant("binary expression", expression.GetType());
        }
    }

    private string GetNameOfOperator(BaseBinaryExpressionNode binaryExpressionNode)
    {
        return binaryExpressionNode switch
        {
            AddExpression => "add",
            SubtractExpression => "subtract",
            TimesExpression => "multiply",
            BaseComparisonExpressionNode => "compare",
            _ => throw _errorHelper.UnknownVariant("binary expression", binaryExpressionNode.GetType())
        };
    }

    private ExpressionType GetTypeFromTerm(ITermNode term)
    {
        return term switch
        {
            IdentifierTerm identifierTerm => GetVariableType(identifierTerm),
            ParenTerm parenTerm => GetTypeFromExpression(parenTerm.Expression),
            BoolLiteralTerm => ExpressionType.Bool,
            IntLiteralTerm => ExpressionType.Int,
            _ => throw _errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    private ExpressionType? GetTypeOfBinaryExpression(ExpressionType type1, ExpressionType type2)
    {
        if (type1 == type2)
        {
            return type1;
        }

        return null;
    }

    private ScopeTracker<string, ExpressionType> _variableTypeStack = null!;


    private void RecordExpressionType(IdentifierToken identifierToken, ExpressionType expressionType)
    {
        if (_variableTypeStack.TryGetValue(identifierToken.Identifier) != null)
        {
            throw _errorHelper.ShowErrorMessageAtToken($"Identifier '{identifierToken.Identifier}' already declared", identifierToken);
        }

        _variableTypeStack.SetValue(identifierToken.Identifier, expressionType);
    }

    private ExpressionType GetVariableType(IdentifierToken identifierToken)
    {
        return _variableTypeStack.TryGetValue(identifierToken.Identifier) ??
               throw _errorHelper.ShowErrorMessageAtToken($"Unknown identifier '{identifierToken.Identifier}'", identifierToken);
    }

    private ExpressionType GetVariableType(IdentifierTerm identifierTerm)
    {
        return _variableTypeStack.TryGetValue(identifierTerm.Identifier) ??
               throw _errorHelper.ShowErrorMessageAtNode($"Unknown identifier '{identifierTerm.Identifier}'", identifierTerm);
    }
}