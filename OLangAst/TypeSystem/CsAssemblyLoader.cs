using System.Reflection;

namespace OLangAst.TypeSystem;

public static class CsAssemblyLoader
{
    public static IEnumerable<Type> LoadCsTypesFromAssembly(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        return assembly.ExportedTypes;
    }
    
    public static List<(Type, DefinedType)> LoadDefinedTypesFromAssembly(string assemblyName)
    {
        return LoadCsTypesFromAssembly(assemblyName).Select(x => (x, DefinedType.FromCsType(x))).ToList();
    }
}