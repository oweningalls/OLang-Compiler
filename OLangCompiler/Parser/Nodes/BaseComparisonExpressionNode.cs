namespace OLangCompiler.Parser.Nodes;

public abstract class BaseComparisonExpressionNode(ITermNode lhs, IExpressionNode rhs) : BaseBinaryExpressionNode(lhs, rhs);