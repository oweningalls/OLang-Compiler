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
    protected override ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        _currentClass = typeHelper.CreateCustomClass(classDeclaration.Identifier, classDeclaration.IsStatic);
        classDeclaration.Type = _currentClass;
        
        return base.VisitClassDeclaration(classDeclaration);
    }

    protected override IClassMember VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var method = typeHelper.CreateCustomMethod(_currentClass!.Value, methodDeclaration);
        methodDeclaration.ReturnType = method.ReturnType;
        SetParameterTypes(methodDeclaration.Parameters);
        
        return base.VisitMethodDeclaration(methodDeclaration);
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