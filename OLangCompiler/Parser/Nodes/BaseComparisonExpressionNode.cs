namespace OLangCompiler.Parser.Nodes;

public abstract class BaseComparisonExpressionNode(IExpressionNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);