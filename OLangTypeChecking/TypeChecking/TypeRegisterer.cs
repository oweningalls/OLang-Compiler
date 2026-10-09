using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.TypeSystem;

namespace OLangTypeChecking.TypeChecking;

public class TypeRegisterer(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private DefinedType? _currentType;
    protected override ITypeDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentType = typeHelper.CreateCustomType(classDeclaration.Identifier, TypeVariant.Class);
        classDeclaration.Type = _currentType;
        
        return (ClassDeclaration)base.VisitClassDeclaration(classDeclaration);
    }
    
    protected override ITypeDeclaration VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        _currentType = typeHelper.CreateCustomType(enumDeclaration.Identifier, TypeVariant.Enum);
        enumDeclaration.Type = _currentType;
        
        return (EnumDeclaration)base.VisitEnumDeclaration(enumDeclaration);
    }
}