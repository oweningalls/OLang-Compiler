using System.Diagnostics;

namespace OLangTests;

public class TestCompileHelper
{
    private int _counter = 1;

    private readonly Lock _lockObject = new();

    private string GetName()
    {
        lock (_lockObject)
        {
            return $"test{_counter++.ToString()}";
        }
    }

    public int CompileAndExecuteProgram(string program, out string output, OLangCompiler.OLangCompiler.CompileTargets compileTarget, bool wrapWithMain = true)
    {
        var name = GetName();
        var outputFile = compileTarget switch
        {
            OLangCompiler.OLangCompiler.CompileTargets.X86 => $"{name}.asm",
            OLangCompiler.OLangCompiler.CompileTargets.Cil => $"{name}.dll",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (wrapWithMain)
        {
            program = $"class Program {{void Main() {{\n{program}\n}}}}";
        }
        OLangCompiler.OLangCompiler.GenerateAssembly(program, compileTarget, ".", outputFile);

        var psi = compileTarget switch
        {
            OLangCompiler.OLangCompiler.CompileTargets.X86 => new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"bash ./build_and_execute.sh {outputFile}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            OLangCompiler.OLangCompiler.CompileTargets.Cil => new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = outputFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.Start();

        output = process.StandardOutput.ReadToEnd();
        var errors = process.StandardError.ReadToEnd();

        process.WaitForExit();
        Console.WriteLine("Output:");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(errors))
        {
            throw new Exception(errors);
        }

        return process.ExitCode;
    }
}
