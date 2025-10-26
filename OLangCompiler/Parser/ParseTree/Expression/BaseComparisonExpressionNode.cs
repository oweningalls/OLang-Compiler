namespace OLangCompiler.Parser.ParseTree.Expression;

public abstract class BaseComparisonExpressionNode(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);