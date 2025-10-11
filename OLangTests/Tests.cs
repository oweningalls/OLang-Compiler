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

    public static readonly List<(string, int)> AddPrograms =
    [
        ("exit 1 + 3;", 4),
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
    ];

    [TestCaseSource(nameof(AddPrograms))]
    public void AddTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> SubtractionPrograms =
    [
        ("let a = 12 - (3 + 2); exit a;", 7),
        ("let a = 12 - 3 - 2; exit a;", 7),
        ("let a = 12 - 3 + 2; exit a;", 11),
    ];

    [TestCaseSource(nameof(SubtractionPrograms))]
    public void SubtractionTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> VariablePrograms =
    [
        ("let a = 0;", 0),
        ("int intLiteral = 1; exit intLiteral;", 1),
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
    ];

    [TestCaseSource(nameof(VariablePrograms))]
    public void VariableTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> ParenthesesPrograms =
    [
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
        ("let a = (1 + (3 + 2) + 5); exit a;", 11),
        ("let a = ((1)); exit a;", 1),
    ];

    [TestCaseSource(nameof(ParenthesesPrograms))]
    public void ParenthesesTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> ScopePrograms =
    [
        ("{ let a = true; } let a = 1;", 0),
        ("{ let a = true; { exit 6; } } let a = 1;", 6),
    ];

    [TestCaseSource(nameof(ScopePrograms))]
    public void ScopeTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> IfPrograms =
    [
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
    ];

    [TestCaseSource(nameof(IfPrograms))]
    public void IfTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> EqualityPrograms =
    [
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
         """, 123),
        ("""
         let a = 4;
         let i = 0;
         while i != 5 {
             i = i + 1;
             a = a + 1;
         }

         exit a;
         """, 9),
        ("if !true == false { exit 39; }", 39),
        ("if !true == true { exit 39; }", 0),
        ("if 1 + 3 == 2 + 2 { exit 39; }", 39),
        ("if 1 + 1 == 2 + 2 { exit 39; }", 0),
    ];

    [TestCaseSource(nameof(EqualityPrograms))]
    public void EqualityTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> NegationPrograms =
    [
        ("""
         let boolValue = !false;
         if boolValue {
             exit 41;
         }

         exit 12;
         """, 41),
        ("""
         let boolValue = !true;
         if boolValue {
             exit 41;
         }

         exit 12;
         """, 12)
    ];

    [TestCaseSource(nameof(NegationPrograms))]
    public void NegationTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> OpEqualsPrograms =
    [
        ("let value1 = 3; value1 += 1; exit value1;", 4),
        ("let value1 = 3; let value2 = 6; value1 += value2; value1 += 1; exit value1;", 10),
        ("let value1 = 3; let value2 = 6; value1 -= value2; value1 += 4; exit value1;", 1)
    ];

    [TestCaseSource(nameof(OpEqualsPrograms))]
    public void OpEqualsTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> ForLoopPrograms =
    [
        ("""
         let a = 3;
         for i in 0..4 {
             a += i;
         }

         exit a;
         """, 9),
        ("""
         let a = 3;
         for i in 2..6 {
             a += i;
         }

         exit a;
         """, 17),
        // ("""
        //  for i in 10..6 {
        //      exit 20;
        //  }
        //  """, 0), // TODO: uncomment after implementing less than
        ("""
         let start = 3;
         let end = 6;
         for i in start..end {
             if i == 4 {
                 exit i;
             }
         }
         """, 4),
//         ("""
//          let start = 8;
//          let end = 1;
//          for i in start..end {
//              exit i;
//          }
//
//          exit 2;
//          """, 2) // TODO: uncomment after implementing less than
        ("for i in 1..2{} for i in 1..2{}", 0),
        ("for i in 0..0 {exit 1;}", 0)
    ];

    [TestCaseSource(nameof(ForLoopPrograms))]
    public void ForLoopTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> NegativeIntLiteralPrograms =
    [
        ("let a = -3; exit a + 7;", 4),
        ("let a = 3; exit a + -1;", 2),
        ("let a = -3; a += -4; exit a + 9;", 2)
    ];
    
    [TestCaseSource(nameof(NegativeIntLiteralPrograms))]
    public void NegativeIntLiteralTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    public static readonly List<(string, int)> GreaterOrEqualPrograms =
    [
        ("""
         let a = 3;
         if a > 3 {
             exit 2;
         }
         
         exit 4;
         """, 4),
        ("""
         if 3 >= 3 {
             exit 2;
         }
         
         exit 4;
         """, 2),
    ];
    
    [TestCaseSource(nameof(GreaterOrEqualPrograms))]
    public void GreaterOrEqualTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    private void TestProgramExitCode((string, int) values)
    {
        var program = values.Item1;
        var exitCode = values.Item2;

        Assert.That(CompileAndExecuteProgram(program), Is.EqualTo(exitCode));
    }

    [TestCase("let a = a;")] // a hasn't been declared yet
    [TestCase("exit 4")] // missing semicolon
    [TestCase("exit (5 + 1;")] // missing right parenthesis
    [TestCase("let a = ((1;)); exit a;")] // incorrect semicolon
    [TestCase("bool a = true; int b = a;")] // assigning a bool to an int
    [TestCase("int a = true; bool b = a;")] // assigning an int to a bool
    [TestCase("bool a = true; bool a = true;")] // declaring variable multiple times
    [TestCase("let a = true; a = 1;")] // assigning an int to a bool (using let)
    [TestCase("let a = 1; a = false;")] // assigning an int to a bool (using let)
    [TestCase("let a = 1; { let a = 2; } ")] // shadowed variable
    [TestCase("let a = 1; if true exit 5 ")] // no braces around if block
    [TestCase("let boolInt = 1 == true;")] // comparing int to bool
    [TestCase("let a = 1; if a = 3 { exit 4; }")] // = instead of == in if condition
    [TestCase("for i in 0..5 {} exit i;")] // i is in for loop scope
    [TestCase("for i in i..5 {}")] // i not yet declared
    [TestCase("let a = 0; a = a > 2;")] // greater returns bool
    [TestCase("let a = 0; a = a >= 2;")] // greater returns bool
    public void TestInvalidPrograms(string program)
    {
        Assert.That(() => OLangCompiler.OLangCompiler.GenerateAssembly(program), Throws.Exception);
    }

    [Test]
    public void TestFibonacci()
    {
        Assert.Multiple(() =>
        {
            for (var i = 1; i <= 13; i++) // 13 is the greatest fib number less than 255
            {
                var program = $$"""
                                let fn = 0;
                                let fn1 = 1;

                                for i in 1..{{i}} {
                                    let temp = fn1;
                                    fn1 += fn;
                                    fn = temp;
                                }
                                exit fn1;
                                """;
                var olFibNum = CompileAndExecuteProgram(program);
                var trueFibNum = Fib(i);

                Assert.That(olFibNum, Is.EqualTo(trueFibNum));
            }
        });
    }

    private int Fib(int n)
    {
        if (n == 0) return 0;
        if (n == 1) return 1;

        return Fib(n - 1) + Fib(n - 2);
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