using System.Diagnostics;

namespace OLangTests;

public class Tests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var projectRoot = TestPathHelper.GetProjectRoot();
        Directory.SetCurrentDirectory($"{projectRoot}/../OLangCompiler");
    }

    public static readonly List<(string, int)> Programs =
    [
        ("""
         let a = 1;
         let b = 3;

         exit b;
         """, 3),

        ("""
         let a = 1;
         let b = 3;

         exit a;
         """, 1),

        ("""
         exit 1 + 3;
         """, 4),

        ("""
         let a = 1;
         let b = 3;

         exit a + b;
         """, 4),

        ("""
         let a = 1;
         let b = 3;
         let c = 5;
         b = a + c;

         exit b;
         """, 6),

        ("""
         let a = 1;
         let b = 3;
         let c = 5;

         exit b + a + c;
         """, 9),

        ("""
         let a = 1;
         let b = 3;
         let c = 5;

         exit b + (a + c);
         """, 9),

        ("""
         let a = 1;
         let b = 38;
         let c = 5;

         exit (b + a) + c;
         """, 44),
        ("let a = 0;", 0),
        ("let a = (1 + (3 + 2) + 5);\nexit a;", 11),
        ("let a = ((1));\nexit a;", 1),
        ("let a = 12 - (3 + 2);\nexit a;", 7),
        // ("let a = 12 - 3 - 2;\nexit a;", 7), // TODO: uncomment after implementing left associative minus
        ("int intLiteral = 1;\n exit intLiteral;", 1),
        ("""
         bool trueBool = true;
         bool falseBool = false;
         trueBool = falseBool;
         """, 0),
        ("""
         let a = true;
         bool b = a;
         """, 0)
    ];

    [TestCaseSource(nameof(Programs))]
    public void TestExitCodes((string, int) values)
    {
        var program = values.Item1;
        var exitCode = values.Item2;

        Assert.That(CompileAndExecuteProgram(program), Is.EqualTo(exitCode));
    }

    [TestCase("let a = a;")] // a hasn't been declared yet
    [TestCase("exit 4")] // missing semicolon
    [TestCase("exit (5 + 1;")] // missing semicolon
    [TestCase("let a = ((1;));\nexit a;")] // incorrect semicolon
    [TestCase("bool a = true;\nint b = a;")] // assigning a bool to an int
    [TestCase("int a = true;\nbool b = a;")] // assigning an int to a bool
    [TestCase("bool a = true;\nbool a = true;")] // declaring variable multiple times
    public void TestInvalidPrograms(string program)
    {
        Assert.That(() => CompileAndExecuteProgram(program), Throws.Exception);
    }

    private int CompileAndExecuteProgram(string program)
    {
        var assembly = OLangCompiler.OLangCompiler.GenerateAssembly(program);
        var outputFile = "test.asm";
        File.WriteAllText(outputFile, assembly);

        var psi = new ProcessStartInfo
        {
            FileName = "wsl.exe",
            Arguments = $"bash ./build_and_execute.sh {outputFile}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
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