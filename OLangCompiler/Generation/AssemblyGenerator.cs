using System.Text;
using OLangCompiler.Parser.Nodes;

namespace OLangCompiler.Generation;

public class AssemblyGenerator
{
    public string GenerateProgram(ProgramNode program)
    {
        _output = new StringBuilder();

        _output.Append("""
                       global _start

                       section .text
                       _start:

                       """);
        
        _output.Append($@"mov rax, 60
    mov rdi, {program.Value.Value.Value}
    syscall");

        return _output.ToString();
    }

    private StringBuilder? _output;
}