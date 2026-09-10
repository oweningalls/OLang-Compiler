using System.Reflection;
using System.Reflection.Emit;
using ErrorHelper;
using OLangAst.ClassMembers;
using OLangAst.Miscellaneous;

namespace AstHelpers;

public class TypeHelper
{
    private IErrorHelper _errorHelper;
    public PersistedAssemblyBuilder AssemblyBuilder;
    public ModuleBuilder ModuleBuilder;
    
    public Type GetCsType(IVariableType type)
    {
        return type switch
        {
            CustomClass userDefinedClass => userDefinedClass.DefinedType,
            PrimitiveVariableType primitiveVariableType => PrimitiveTypeMap[primitiveVariableType],
            _ => throw _errorHelper.UnknownVariant("type", type.GetType())
        };
    }

    public IVariableType GetLocalType(Type type)
    {
        return ReversePrimitiveTypeMap.TryGetValue(type, out var value) ? value : throw _errorHelper.UnknownVariant("type", type);
    }

    public MethodInfo GetMethod(IVariableType type, string name, List<IVariableType> argumentTypes)
    {
        if (type is CustomClass customClass)
        {
            return GetCustomMethod(customClass, name);
        }
        
        var csType = GetCsType(type);
        var matchingMethods = csType.GetMethods().Where(x => x.Name == name && x.GetParameters().Index().All(y => TypeMatches(y.Item.ParameterType, argumentTypes, y.Index))).ToList();

        return matchingMethods.Count switch
        {
            0 => throw _errorHelper.ShowErrorMessage($"Couldn't find method `{name}` with matching signature.", type.Span),
            > 1 => throw _errorHelper.ShowErrorMessage($"Couldn't uniquely identify method `{name}`.", type.Span),
            _ => matchingMethods.Single()
        };
    }

    public void CreateCustomMethod(CustomClass customClass, MethodDeclaration methodDeclaration)
    {
        var attributes = MethodAttributes.Private | MethodAttributes.Static;
        // arguments for instance method
        // new List<IVariableType> { customClass }.Concat(methodDeclaration.Parameters.Select(x => x.Type)).Select(GetCsType).ToArray()
        var method = customClass.DefinedType.DefineMethod(methodDeclaration.Identifier, attributes, methodDeclaration.Type == null ? typeof(void) : GetCsType(methodDeclaration.Type), methodDeclaration.Parameters.Select(x => x.Type).Select(GetCsType).ToArray());
        customClass.Methods.Add(method);
    }

    private MethodInfo? GetCustomMethod(CustomClass customClass, string name)
    {
        return customClass.Methods.SingleOrDefault(x => x.Name == name);
    }

    public CustomClass CreateCustomClass(string name, bool isStatic)
    {
        var classAttributes = TypeAttributes.Public;
        if (isStatic)
        {
            classAttributes = classAttributes | TypeAttributes.Abstract | TypeAttributes.Sealed;
        }
        
        var classDef = ModuleBuilder.DefineType(name, classAttributes);
        var userDefined = new CustomClass(classDef);
        CsCustomClasses.Add(name, userDefined);
        
        return userDefined;
    }

    public CustomClass? GetCustomClass(string name)
    {
        return CsCustomClasses.GetValueOrDefault(name);
    }

    private bool TypeMatches(Type type, List<IVariableType> argumentTypes, int index)
    {
        if (index >= argumentTypes.Count)
        {
            return false;
        }

        return GetCsType(argumentTypes[index]) == type;
    }

    private Dictionary<string, CustomClass> CsCustomClasses = new();

    public TypeHelper(IErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        AssemblyBuilder = new(new AssemblyName("AssemblyName"), typeof(object).Assembly);
        ModuleBuilder = AssemblyBuilder.DefineDynamicModule("OLangProgram");
    }

    private static readonly Dictionary<PrimitiveVariableType, Type> PrimitiveTypeMap = new()
    {
        { PrimitiveVariableType.IntType, typeof(int) },
        { PrimitiveVariableType.BoolType, typeof(bool) },
        { PrimitiveVariableType.FloatType, typeof(float) },
        { PrimitiveVariableType.StringType, typeof(string) },
    };

    private static readonly Dictionary<Type, PrimitiveVariableType> ReversePrimitiveTypeMap = PrimitiveTypeMap.ToDictionary(x => x.Value, x => x.Key);
}