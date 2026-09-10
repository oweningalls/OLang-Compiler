using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.Miscellaneous;

namespace OLangTypeChecking.TypeChecking;

public class TypeRegisterer(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private CustomClass? _currentClass;
    protected override ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentClass = typeHelper.CreateCustomClass(classDeclaration.Identifier, classDeclaration.IsStatic);
        
        return base.VisitClassDeclaration(classDeclaration);
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        typeHelper.CreateCustomMethod(_currentClass!, methodDeclaration);
        
        return base.VisitMethodDeclaration(methodDeclaration);
    }
}