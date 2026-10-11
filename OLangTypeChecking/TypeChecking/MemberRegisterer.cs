using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.EnumVariants;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangAst.TypeSystem;

namespace OLangTypeChecking.TypeChecking;

public class MemberRegisterer(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private DefinedType? _currentType;

    protected override ITypeDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentType = classDeclaration.Type;

        return (ClassDeclaration)base.VisitClassDeclaration(classDeclaration);
    }

    protected override ITypeDeclaration VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        _currentType = enumDeclaration.Type;

        return (EnumDeclaration)base.VisitEnumDeclaration(enumDeclaration);
    }

    protected override EnumVariant VisitEnumVariant(EnumVariant enumVariant)
    {
        typeHelper.CreateEnumVariant(_currentType!, enumVariant);

        return enumVariant;
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var method = typeHelper.CreateCustomMethod(_currentType!, methodDeclaration);
        methodDeclaration.ReturnType = method.ReturnType;
        SetParameterTypes(methodDeclaration.Parameters);

        return base.VisitMethodDeclaration(methodDeclaration);
    }

    protected override IClassMember VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        typeHelper.CreateCustomField(_currentType!, fieldDeclaration);

        return fieldDeclaration;
    }


    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        functionDeclaration.ReturnType = typeHelper.GetMethodType(functionDeclaration.DeclaredType, _currentType);
        SetParameterTypes(functionDeclaration.Parameters);
        return base.VisitFunctionDeclaration(functionDeclaration);
    }

    private void SetParameterTypes(List<ParameterNode> parameters)
    {
        parameters.ForEach(x => { x.Type = typeHelper.GetLocalType(x.DeclaredType, _currentType); });
    }
}