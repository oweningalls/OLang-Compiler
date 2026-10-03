using AstHelpers;
using ErrorHelper;
using OLangAst;
using OLangAst.ClassMembers;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangAst.TypeSystem;

namespace OLangTypeChecking.TypeChecking;

public class TypeRegisterer(IErrorHelper errorHelper, TypeHelper typeHelper) : BaseOLangAstVisitor(errorHelper)
{
    private DefinedType? _currentClass;
    protected override ITypeDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentClass = typeHelper.CreateCustomType(classDeclaration.Identifier, classDeclaration.IsStatic ? TypeVariant.StaticClass : TypeVariant.Class);
        classDeclaration.Type = _currentClass;
        
        return (ClassDeclaration)base.VisitClassDeclaration(classDeclaration);
    }
    
    protected override ITypeDeclaration VisitEnumDeclaration(EnumDeclaration enumDeclaration)
    {
        enumDeclaration.Type = typeHelper.CreateCustomType(enumDeclaration.Identifier, TypeVariant.Enum);
        
        return (EnumDeclaration)base.VisitEnumDeclaration(enumDeclaration);
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var method = typeHelper.CreateCustomMethod(_currentClass!.Value, methodDeclaration);
        methodDeclaration.ReturnType = method.ReturnType;
        SetParameterTypes(methodDeclaration.Parameters);
        
        return base.VisitMethodDeclaration(methodDeclaration);
    }
    
    protected override IClassMember VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        typeHelper.CreateCustomField(_currentClass!.Value, fieldDeclaration);

        return fieldDeclaration;
    }
    

    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        functionDeclaration.ReturnType = typeHelper.GetMethodType(functionDeclaration.DeclaredType);
        SetParameterTypes(functionDeclaration.Parameters);
        return base.VisitFunctionDeclaration(functionDeclaration);
    }

    private void SetParameterTypes(List<ParameterNode> parameters)
    {
        parameters.ForEach(x =>
        {
            x.Type = typeHelper.GetLocalType(x.DeclaredType);
        });
    }
}