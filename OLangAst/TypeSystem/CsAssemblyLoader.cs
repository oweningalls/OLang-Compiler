using System.Reflection;

namespace OLangAst.TypeSystem;

public static class CsAssemblyLoader
{
    public static IEnumerable<System.Type> LoadCsTypesFromAssembly(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        return assembly.ExportedTypes;
    }
    
    public static List<(System.Type, ConcreteType)> LoadDefinedTypesFromAssembly(string assemblyName)
    {
        return LoadCsTypesFromAssembly(assemblyName).Select(x => (x, ConcreteType.FromCsType(x))).ToList();
    }
}