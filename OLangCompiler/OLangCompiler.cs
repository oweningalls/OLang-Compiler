using AssemblyGeneration.Generation;
using OLangLexing;

namespace OLangCompiler;

public static class OLangCompiler
{
    public static void Main()
    {
        CompileFile("test.ol", CompileTargets.X86);
    }
    
    public static void CompileFile(string fileName, CompileTargets target, string? outputFile = null)
    {
        if (!fileName.EndsWith(".ol"))
        {
            throw new Exception($"Input file {fileName} must end with .ol");
        }

        var baseName = fileName.Substring(0, fileName.Length - ".ol".Length);
        var extension = target switch
        {
            CompileTargets.X86 => "asm",
            CompileTargets.Cil => "exe",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
        
        outputFile ??= $"{baseName}.{extension}";
        var contents = File.ReadAllText(fileName);

        var assembly = GenerateAssembly(contents, target);
        File.WriteAllText(outputFile, assembly);
    }

    public static string GenerateAssembly(string program, CompileTargets target)
    {
        var reader = new SourceReader(program);
        var errorHelper = new OLangHelpers.ErrorHelper(reader);

        var generator = target switch
        {
            CompileTargets.X86 => new X86AssemblyGenerator(),
            // CompileTargets.Cil => new CilGenerator(),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };

        return new OLangFrontEnd().Compile(program, generator, errorHelper);
    }

    public enum CompileTargets
    {
        X86,
        Cil
    }
}