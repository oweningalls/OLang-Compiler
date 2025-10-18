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
        ("if !true == false { exit 39; }", 39),
        ("if !true == true { exit 39; }", 0),
        ("if 1 + 3 == 2 + 2 { exit 39; }", 39),
        ("if 1 + 1 == 2 + 2 { exit 39; }", 0),
    ];

    public static readonly List<(string, int)> WhilePrograms =
    [
        ("""
         let a = 4;
         let i = 0;
         while i != 5 {
             i = i + 1;
             a = a + 1;
         }

         exit a;
         """, 9),
        ("while 1 != 1 { exit 35; } ", 0)
    ];

    [TestCaseSource(nameof(WhilePrograms))]
    public void WhileTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

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
        ("""
         for i in 10..6 {
             exit 20;
         }
         """, 0),
        ("""
         let start = 3;
         let end = 6;
         for i in start..end {
             if i == 4 {
                 exit i;
             }
         }
         """, 4),
         ("""
          let start = 8;
          let end = 1;
          for i in start..end {
              exit i;
          }

          exit 2;
          """, 2),
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
        ("""
         if 2 >= 3 {
             exit 2;
         }
         
         exit 4;
         """, 4),
        ("""
         if 5 >= 3 {
             exit 2;
         }
         
         exit 4;
         """, 2)
    ];
    
    [TestCaseSource(nameof(GreaterOrEqualPrograms))]
    public void GreaterOrEqualTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    public static readonly List<(string, int)> LessOrEqualPrograms =
    [
        ("""
         let a = 3;
         if a < 3 {
             exit 2;
         }
         
         exit 4;
         """, 4),
        ("""
         if 3 <= 3 {
             exit 2;
         }
         
         exit 4;
         """, 2),
        ("""
         if 2 <= 3 {
             exit 2;
         }
         
         exit 4;
         """, 2),
        ("""
         if 5 <= 3 {
             exit 2;
         }
         
         exit 4;
         """, 4),
    ];
    
    [TestCaseSource(nameof(LessOrEqualPrograms))]
    public void LessOrEqualTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> MultiplicationPrograms =
    [
        ("exit 2 * 3;", 6),
        ("let a = 4; exit 2 * a;", 8),
        ("exit 2 + 3 * 4;", 14),
        ("exit 2 * 3 + 4;", 10),
        ("exit 2 * 3 * 4;", 24),
        ("exit (2 + 3) * 4;", 20),
        ("int a = (2 + 3) * 4;", 0),
        ("int a = 0 * 40; exit a;", 0),
        ("int a = -2 * 40; exit a + 100;", 20),
        ("exit -3 * -5;", 15),
    ];
    
    [TestCaseSource(nameof(MultiplicationPrograms))]
    public void MultiplicationTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    public static readonly List<(string, int)> CommentPrograms =
    [
        ("// exit 1;\nexit 3;", 3),
        ("""
         // comment
         // another comment
         exit 6;
         // third comment
         """, 6),
        ("""
         /*
         exit 6;
         */
         """, 0),
        ("/**//**/ exit 6;", 6),
        ("""
         // /*
         exit 3;
         // */
         """, 3),
    ];
    
    [TestCaseSource(nameof(CommentPrograms))]
    public void CommentTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    public static readonly List<(string, int)> FunctionPrograms =
    [
        ("""
         void function() {
         }
         
         exit 6;
         """, 6),
        ("""
         void function() {
             exit 31;
         }
         
         function();
         """, 31),
        ("""
         let a = 1;
         void function() {
             bool a = false;
             if (a) {
                 exit 13;
             }
             exit 81;
         }
         
         function();
         """, 81),
        ("""
         let a = 1;
         {
             void function() { exit 81; }
         }
         
         {
             void function() { exit 9; }
             function();
         }
         
         """, 9),
        ("""
         let a = 5;
         let b = 3;
         void func() { }
         
         func();
         func();
         exit a;
         """, 5),
        ("""
         let a = 5;
         let b = 3;
         void func() { }
         
         exit b;
         """, 3),
        ("""
         void fun() {
             return;
             exit 35;
         }
         
         exit 19;
         """, 19),
        ("""
         void other() {
             for i in 0..1 {
             }
         }
         
         other();
         """, 0)
    ];
    
    [TestCaseSource(nameof(FunctionPrograms))]
    public void FunctionTests((string, int) values)
    {
        TestProgramExitCode(values);
    }

    public static readonly List<(string, int)> ReturnValueFunctionPrograms =
    [
        ("int func() { return 1; } func();", 0),
        ("""
         int function() {
             return 6;
         }
         
         function();
         """, 0),
        ("""
         int function() {
             return 6;
         }
         
         exit function();
         """, 6),
        ("""
         int innerFunction() {
             return 4;
         }
         
         int outerFunction() {
             return innerFunction() + 3;
         }
         
         exit outerFunction();
         """, 7),
        ("""
         bool trueFunc() { return true; }
         if !trueFunc() { exit 1; }
         exit 3;
         """, 3),
        ("""
         bool func() {
             if (true) { return false; }
             return true;
         }
         
         if func() { exit 10; }
         
         exit 4;
         """, 4)
    ];
    
    [TestCaseSource(nameof(ReturnValueFunctionPrograms))]
    public void ReturnValueFunctionTests((string, int) values)
    {
        TestProgramExitCode(values);
    }
    
    public static readonly List<(string, int)> ParameterPrograms =
    [
        ("""
         void func(int exit_value) {
             exit exit_value;
         }
         
         func(5);
         """, 5),
        ("""
         int add(int a, int b) {
             return a + b;
         }
         
         exit add(2, 5);
         """, 7),
        ("""
         int triangleNumber(int n) {
             let total = 0;
             for i in 1..n+1 {
                 total += i;
             }
             
             return total;
         }
         
         exit triangleNumber(4);
         """, 10),
        ("""
         bool or(bool a, bool b) {
             if a { return true; }
             if b { return true; }
             
             return false;
         }
         
         let a = true;
         let b = false;
         if or(a, b) { exit 3; }
         """, 3),
        ("""
         bool or(bool a, bool b) {
             if a { return true; }
             if b { return true; }
             
             return false;
         }
         
         let a = false;
         let b = false;
         if or(a, b) { exit 3; }
         """, 0)
    ];

    [TestCaseSource(nameof(ParameterPrograms))]
    public void ParameterTests((string, int) values)
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
    [TestCase("let a = 0; a = a >= 2;")] // setting int to bool
    [TestCase("let a = 4 * true;")] // setting int to bool
    [TestCase("void function() { return 1;}")] // void cannot have return value
    [TestCase("int function() { return; }")] // void cannot have return value
    [TestCase("""
              int f() {
                  if (true) {
                      return 3;
                  }
              }
              """)] // not all code paths return value
    [TestCase("""
                     let variable = 1;
                     void a() {
                         variable += 1;
                         if variable > 8 {
                             exit variable;
                         }
                     }
                     """)]
    [TestCase("""
              int a() { return true; }
              """)]
    public void TestInvalidPrograms(string program)
    {
        // OLangCompiler.OLangCompiler.GenerateAssembly(program);
        var ex = Assert.Throws<Exception>(() => OLangCompiler.OLangCompiler.GenerateAssembly(program));
        
        Assert.That(ex.Message.ToLower().Contains("expression type") && ex.Message.ToLower().Contains("unknown"), Is.False);
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