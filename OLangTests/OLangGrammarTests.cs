using Lexing;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.ClassMemberList;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangTokens.Tokens;

namespace OLangTests;

public class OLangGrammarTests
{
    [Test]
    public void ReduceMethodDeclaration()
    {
        var rule = new OLangGrammar.OLangGrammar().GetRules().First(x => x.GetLhsType() == typeof(IClassMemberListNode) && x.GetRhsTypes().Count == 1);
        var classMember = new MethodDeclaration(null, null, null);
        var parsedClassMemberList = rule.ReduceRule([classMember]);

        var classMemberList = parsedClassMemberList as SingleMemberClassMemberList;
        Assert.That(classMemberList, Is.Not.Null);
        Assert.That(classMemberList.ClassMember, Is.EqualTo(classMember));
    }
    
    [Test]
    public void ReduceDeclarationStatement()
    {
        var rule = GrammarRule.Create((IType type, IdentifierToken identifier, EqualsToken _, IExpression expression, SemicolonToken _) => new Declaration(type, identifier, expression));
        var varType = new BoolType();
        var identifier = new IdentifierToken("test");
        var expression = new NonExpression(new NonAnd(new NonEquality(new NonGreaterExpression(new NonAddExpression(new NonMultExpression(new NonUnaryExpression(new IdentifierTerm(new IdentifierToken("ident2")))))))));
        
        var decl = rule.ReduceRule([varType, identifier, new EqualsToken(), expression, new SemicolonToken()]);

        var declaration = decl as Declaration;
        Assert.That(declaration, Is.Not.Null);
        Assert.That(declaration.Type, Is.EqualTo(varType));
        Assert.That(declaration.Identifier, Is.EqualTo(identifier));
        Assert.That(declaration.Expression, Is.EqualTo(expression));
    }
}