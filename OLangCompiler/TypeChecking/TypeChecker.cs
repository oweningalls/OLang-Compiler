using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.TypeChecking;

public class TypeChecker
{
    public void CheckTypes(ProgramNode program)
    {
        _variableTypes = new Dictionary<string, ExpressionType>();
        foreach (var statement in program.Statements)
        {
            CheckStatementTypes(statement);
        }
    }

    private void CheckStatementTypes(IStatementNode statementNode)
    {
        switch (statementNode)
        {
            case DeclarationStatement declarationStatement:
                var type = GetTypeFromExpression(declarationStatement.Expression);
                if (declarationStatement.ExpressionType != null && type != declarationStatement.ExpressionType)
                {
                    throw ErrorHelper.ShowErrorMessageAtNode($"Expression type {type} does not match variable type {declarationStatement.ExpressionType}", declarationStatement.Expression);
                }

                RecordExpressionType(declarationStatement.Identifier, type);
                break;
            case ExitStatement exitStatement:
                var exitType = GetTypeFromExpression(exitStatement.ExpressionNode);
                if (exitType != ExpressionType.Int)
                {
                    throw ErrorHelper.ShowErrorMessageAtNode($"Exit code has to be an integer", exitStatement.ExpressionNode);
                }

                break;
            case AssignmentStatement assignmentStatement:
                var expressionType = GetTypeFromExpression(assignmentStatement.Expression);
                var variableType = GetVariableType(assignmentStatement.Identifier);
                if (variableType != expressionType)
                {
                    throw ErrorHelper.ShowErrorMessageAtNode($"Cannot assign variable {assignmentStatement.Identifier.Identifier} of type {variableType} to expression of type {expressionType}", assignmentStatement.Expression);
                }

                break;
            default:
                throw ErrorHelper.UnknownVariant("statement", statementNode.GetType());
        }
    }

    private ExpressionType GetTypeFromExpression(IExpressionNode expression)
    {
        switch (expression)
        {
            case TermExpression term:
                return GetTypeFromTerm(term.Term);
            case BaseBinaryExpressionNode binaryExpression:
            {
                var lhsType = GetTypeFromTerm(binaryExpression.Lhs);
                var rhsType = GetTypeFromExpression(binaryExpression.Rhs);

                var type = GetTypeOfBinaryExpression(lhsType, rhsType);
                if (type == null)
                {
                    throw ErrorHelper.ShowErrorMessageAtNode($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}", binaryExpression.Lhs);
                }

                return type.Value;
            }
            default:
                throw ErrorHelper.UnknownVariant("binary expression", expression.GetType());
        }
    }

    private string GetNameOfOperator(BaseBinaryExpressionNode binaryExpressionNode)
    {
        return binaryExpressionNode switch
        {
            AddExpression => "add",
            SubtractExpression => "subtract",
            _ => throw ErrorHelper.UnknownVariant("binary expression", binaryExpressionNode.GetType())
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
            _ => throw ErrorHelper.UnknownVariant("term", term.GetType())
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

    private Dictionary<string, ExpressionType>? _variableTypes;

    private void RecordExpressionType(IdentifierToken identifierToken, ExpressionType expressionType)
    {
        if (_variableTypes!.ContainsKey(identifierToken.Identifier))
        {
            throw ErrorHelper.ShowErrorMessageAtToken($"Identifier '{identifierToken.Identifier}' already declared", identifierToken);
        }

        _variableTypes[identifierToken.Identifier] = expressionType;
    }
    
    private void RecordExpressionType(IdentifierTerm identifierTerm, ExpressionType expressionType)
    {
        if (_variableTypes!.ContainsKey(identifierTerm.Identifier))
        {
            throw ErrorHelper.ShowErrorMessageAtNode($"Identifier '{identifierTerm.Identifier}' already declared", identifierTerm);
        }

        _variableTypes[identifierTerm.Identifier] = expressionType;
    }

    private ExpressionType GetVariableType(IdentifierToken identifierToken)
    {
        if (!_variableTypes!.ContainsKey(identifierToken.Identifier))
        {
            throw ErrorHelper.ShowErrorMessageAtToken($"Unknown identifier '{identifierToken.Identifier}'", identifierToken);
        }

        return _variableTypes[identifierToken.Identifier];
    }
    private ExpressionType GetVariableType(IdentifierTerm identifierTerm)
    {
        if (!_variableTypes!.ContainsKey(identifierTerm.Identifier))
        {
            throw ErrorHelper.ShowErrorMessageAtNode($"Unknown identifier '{identifierTerm.Identifier}'", identifierTerm);
        }

        return _variableTypes[identifierTerm.Identifier];
    }
}