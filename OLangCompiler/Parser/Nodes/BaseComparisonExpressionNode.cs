namespace OLangCompiler.Parser.Nodes;

public abstract class BaseComparisonExpressionNode(BaseExpressionNode lhs, BaseExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);