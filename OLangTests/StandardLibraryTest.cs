namespace OLangTests;

public class StandardLibraryTest
{
    private readonly TestCompileHelper _testCompileHelper = new();

    [Test]
    public void TestStandardLibrary()
    {
        var program = File.ReadAllText("stdlib.ol");
        _testCompileHelper.CompileAndExecuteProgram(program, out var output, OLangCompiler.OLangCompiler.CompileTargets.Cil, false);
        
        TestContext.Out.Write(output);
    }
}