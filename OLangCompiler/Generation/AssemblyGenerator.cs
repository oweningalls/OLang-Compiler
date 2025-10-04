using System.Text;
using OLangCompiler.Parser.Nodes;
using OLangCompiler.Parser.Nodes.NodeValues;
using OLangCompiler.Tokens;

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
        switch (statement.Statement.Value)
        {
            case ExitStatement exitStatement:
                GenerateExitStatement(exitStatement);
                break;
            case VarDeclarationStatement declarationStatement:
                GenerateVarDeclarationStatement(declarationStatement);
                break;
            case SetVarStatement setVarStatement:
                GenerateSetVarStatement(setVarStatement);
                break;
            default:
            {
                throw new Exception($"Unknown statement type: {statement.Statement.Value.GetType()}");
            }
        }
    }

    private void GenerateExitStatement(ExitStatement exitStatement)
    {
        GenerateTerm(exitStatement.Term);
        _output!.Append("""
                             pop rdi
                             mov rax, 60
                             syscall
                         
                         """);
        
    }

    private void GenerateVarDeclarationStatement(VarDeclarationStatement declarationStatement)
    {
        var identifier = declarationStatement.Identifier;
        GenerateTerm(declarationStatement.Term);
        _variableStackOffsets[identifier.Name] = _stackOffset;
        _stackOffset++;
    }

    private void GenerateSetVarStatement(SetVarStatement setVarStatement)
    {
        GenerateTerm(setVarStatement.Term);
        _output!.Append($"""
                             pop rax
                             mov {GetVariableLocation(setVarStatement.Identifier.Name)}, rax
                         
                         """);
    }

    private void GenerateTerm(TermNode term)
    {
        if (term.Value.Value is IntLiteralToken intLiteralToken)
        {
            _output!.Append($"""
                                 mov rax, {intLiteralToken.Value}
                                 push rax

                             """);
        }
        else if (term.Value.Value is IdentifierToken identifierToken)
        {
            _output!.Append($"""
                                mov rax, {GetVariableLocation(identifierToken.Name)}
                                push rax
                            
                            """);
        }
        else
        {
            throw new Exception($"Unknown term type: {term.Value.Value.GetType()}");
        }
    }

    private string GetVariableLocation(string variableName)
    {
        var relativeStackOffset = (_stackOffset - _variableStackOffsets[variableName] - 1) * 8;
        return $"[rsp + {relativeStackOffset}]";
    }

    private Dictionary<string, int> _variableStackOffsets = null!;
    private int _stackOffset;

    private StringBuilder? _output;
}