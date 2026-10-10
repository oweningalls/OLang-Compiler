using System.Reflection;

namespace OLangAst.TypeSystem;

public static class CsAssemblyLoader
{
    public static IEnumerable<Type> LoadCsTypesFromAssembly(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        return assembly.ExportedTypes;
    }
    
    public static List<(Type, ConcreteType)> LoadDefinedTypesFromAssembly(string assemblyName)
    {
        return LoadCsTypesFromAssembly(assemblyName).Select(x => (x, ConcreteType.FromCsType(x))).ToList();
    }
}