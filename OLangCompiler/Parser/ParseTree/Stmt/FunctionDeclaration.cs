using OLangCompiler.Parser.ParseTree.FunctionType;
using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class FunctionDeclaration(IFunctionType type, IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IStatement
{
    public IFunctionType Type = type;
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
}