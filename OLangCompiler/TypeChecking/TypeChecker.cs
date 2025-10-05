using OLangCompiler.Parser.Nodes;
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
                    throw new Exception($"Expression type {type} does not match variable type {declarationStatement.ExpressionType}");
                }
                _variableTypes[declarationStatement.Identifier.Name] = type;
                break;
            case ExitStatement exitStatement:
                var exitType = GetTypeFromExpression(exitStatement.ExpressionNode);
                if (exitType != ExpressionType.Int)
                {
                    throw new Exception("Exit code has to be an integer");
                }

                break;
            case SetVarStatement setVarStatement:
                var expressionType = GetTypeFromExpression(setVarStatement.Expression);
                var variableType = _variableTypes[setVarStatement.Identifier.Name];
                if (variableType != expressionType)
                {
                    throw new Exception($"Cannot assign variable {setVarStatement.Identifier.Name} of type {variableType} to expression of type {expressionType}");
                }
                break;
            default:
                throw new Exception($"Unknown statement type: {statementNode.GetType()}");
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
                    throw new Exception($"Cannot {GetNameOfOperator(binaryExpression)} expressions of types {lhsType} and {rhsType}");
                }

                return type.Value;
            }
            default:
                throw new Exception($"Unknown binary expression type: {expression.GetType()}");
        }
    }

    private string GetNameOfOperator(BaseBinaryExpressionNode binaryExpressionNode)
    {
        return binaryExpressionNode switch
        {
            AddExpression => "add",
            SubtractExpression => "subtract",
            _ => throw new Exception($"Unknown binary expression type: {binaryExpressionNode.GetType()}")
        };
    }

    private ExpressionType GetTypeFromTerm(ITermNode term)
    {
        return term switch
        {
            IdentifierTerm identifierTerm => _variableTypes[identifierTerm.Identifier],
            ParenTerm parenTerm => GetTypeFromExpression(parenTerm.Expression),
            BoolLiteralTerm => ExpressionType.Bool,
            IntLiteralTerm => ExpressionType.Int,
            _ => throw new Exception($"Unknown term type: {term.GetType()}")
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

    private Dictionary<string, ExpressionType> _variableTypes;
}