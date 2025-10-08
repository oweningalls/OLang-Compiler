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
        ("let a = (1 + (3 + 2) + 5); exit a;", 11),
        ("let a = ((1)); exit a;", 1),
        ("let a = 12 - (3 + 2); exit a;", 7),
        // ("let a = 12 - 3 - 2; exit a;", 7), // TODO: uncomment after implementing left associative minus
        ("int intLiteral = 1; exit intLiteral;", 1),
        ("""
         bool trueBool = true;
         bool falseBool = false;
         trueBool = falseBool;
         """, 0),
        ("""
         let a = true;
         bool b = a;
         """, 0),
        ("""
         bool a = true;
         let b = a;
         """, 0),
        ("{ let a = true; } let a = 1;", 0),
        ("{ let a = true; { exit 6; } } let a = 1;", 6),
        ("let a = true; if a {exit 1;} exit 5;", 1),
        ("let a = false; if a {let b = 23; exit 1;} exit 5;", 5),
        ("""
         if true {
             let code = 4;
             if true {
                 code = 6;
             }
             exit code;
         }
         """, 6),
        ("""
         let exitCode = 3;
         if false {
             exitCode = 4;
             if true {
                 exitCode = 6;
             }
         }
         exit exitCode;
         """, 3),
        ("""
         let exitCode = 3;
         if false {
             exitCode = 4;
         }
         exit exitCode;
         """, 3),
        ("""
         let a = 1;
         let b = 1;
         if (a == b) {
             a = a + 1;
         }
         
         exit a;
         """, 2),
        ("let test = 1 == 2;", 0),
        ("let test = 1 != 2;", 0),
        ("""
         let a1 = 1 != 2;
         if a1 {
             exit 1;
         }
         exit 123;
         """, 1),
        ("""
         let a1 = 2 != 2;
         if a1 {
             exit 1;
         }
         exit 123;
         """, 123)
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
    [TestCase("let a = ((1;)); exit a;")] // incorrect semicolon
    [TestCase("bool a = true; int b = a;")] // assigning a bool to an int
    [TestCase("int a = true; bool b = a;")] // assigning an int to a bool
    [TestCase("bool a = true; bool a = true;")] // declaring variable multiple times
    [TestCase("let a = true; a = 1;")] // assigning an int to a bool (using let)
    [TestCase("let a = 1; a = false;")] // assigning an int to a bool (using let)
    [TestCase("let a = 1; { let a = 2; } ")] // shadowed variable
    [TestCase("let a = 1; if true exit 5 ")] // no braces around if block
    [TestCase("let boolInt = 1 == true")] // comparing int to bool
    [TestCase("let a = 1; if a = 3 { exit 4; }")] // = instead of == in if condition
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