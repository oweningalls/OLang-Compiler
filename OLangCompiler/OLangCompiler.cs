using AssemblyGeneration.Generation;
using OLangLexing;

namespace OLangCompiler;

public static class OLangCompiler
{
    public static void Main()
    {
        CompileFile(".", "test.ol", CompileTargets.X86);
    }
    
    public static void CompileFile(string filePath, string fileName, CompileTargets target, string? outputFile = null)
    {
        if (!fileName.EndsWith(".ol"))
        {
            throw new Exception($"Input file {fileName} must end with .ol");
        }

        var baseName = fileName.Substring(0, fileName.Length - ".ol".Length);
        var extension = target switch
        {
            CompileTargets.X86 => "asm",
            CompileTargets.Cil => "dll",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
        
        outputFile ??= $"{baseName}.{extension}";
        var contents = File.ReadAllText(fileName);

        GenerateAssembly(contents, target, filePath, outputFile);
    }

    public static void GenerateAssembly(string program, CompileTargets target, string filePath, string fileName)
    {
        var reader = new SourceReader(program);
        var errorHelper = new OLangHelpers.ErrorHelper(reader);

        IGenerator generator = target switch
        {
            CompileTargets.X86 => new X86AssemblyGenerator(errorHelper),
            CompileTargets.Cil => new CilGenerator(errorHelper),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };

        new OLangFrontEnd().Compile(program, generator, errorHelper, filePath, fileName);
    }

    public enum CompileTargets
    {
        X86,
        Cil
    }
}