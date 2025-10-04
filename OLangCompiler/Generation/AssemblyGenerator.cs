using System.Text;
using OLangCompiler.Parser.Nodes;
using OLangCompiler.Parser.Nodes.NodeValues;

namespace OLangCompiler.Generation;

public class AssemblyGenerator
{
    public string GenerateProgram(ProgramNode program)
    {
        _output = new StringBuilder();
        _stackOffset = 0;
        _variableStackOffsets = new Dictionary<string, int>();

        _output.Append("""
                       global _start

                       section .text
                       _start:

                       """);

        foreach (var statement in program.Statements)
        {
            GenerateStatement(statement);
        }
        

        return _output.ToString();
    }

    private void GenerateStatement(StatementNode statement)
    {
        if (statement.Statement.Value is ExitStatement exitStatement)
        {
            GenerateExitStatement(exitStatement);
        }
        else if (statement.Statement.Value is VarDeclarationStatement declarationStatement)
        {
            GenerateVarDeclarationStatement(declarationStatement);
        }
        else
        {
            throw new Exception($"Unknown statement type: {statement.Statement.Value.GetType()}");
        }
    }

    private void GenerateExitStatement(ExitStatement exitStatement)
    {
        GenerateTerm(exitStatement.Term);
        _output!.Append("""
                             mov rax, 60
                             pop rdi
                             syscall
                         """);
        
    }

    private void GenerateVarDeclarationStatement(VarDeclarationStatement declarationStatement)
    {
        var identifier = declarationStatement.Identifier;
        GenerateTerm(declarationStatement.Term);
        _variableStackOffsets[identifier] = _stackOffset;
        _stackOffset++;
    }

    private void GenerateTerm(TermNode term)
    {
        if (term.Value.Value is int intLiteral)
        {
            _output!.Append($"""
                                 mov rax, {intLiteral}
                                 push rax

                             """);
        }
        else
        {
            throw new Exception($"Unknown term type: {term.Value.Value.GetType()}");
        }
    }

    private Dictionary<string, int> _variableStackOffsets = null!;
    private int _stackOffset;

    private StringBuilder? _output;
}