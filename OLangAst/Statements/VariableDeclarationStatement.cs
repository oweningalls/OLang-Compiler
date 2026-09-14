using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Statements;

public class VariableDeclarationStatement(IVariableType? declaredType, string identifier, IExpression value) : IStatement
{
    public IVariableType? DeclaredType = declaredType;
    public string Identifier = identifier;
    public IExpression Value = value;
    public DefinedType? VariableType;
    public SourceSpan Span { get; set; }
}