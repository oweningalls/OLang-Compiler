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
        
        _output!.Append("""
                             mov rdi, 0
                             mov rax, 60
                             syscall
                         
                         """);
        return _output.ToString();
    }

    private void GenerateStatement(IStatementNode statement)
    {
        switch (statement)
        {
            case ExitStatement exitStatement:
                GenerateExitStatement(exitStatement);
                break;
            case DeclarationStatement declarationStatement:
                GenerateVarDeclarationStatement(declarationStatement);
                break;
            case SetVarStatement setVarStatement:
                GenerateSetVarStatement(setVarStatement);
                break;
            default:
            {
                throw new Exception($"Unknown statement type: {statement.GetType()}");
            }
        }
    }

    private void GenerateExitStatement(ExitStatement exitStatement)
    {
        GenerateExpression(exitStatement.ExpressionNode);
        _output!.Append($"""
                             {GetPopStatement("rdi")}
                             mov rax, 60
                             syscall
                         
                         """);
        
    }

    private void GenerateVarDeclarationStatement(DeclarationStatement declarationStatement)
    {
        var identifier = declarationStatement.Identifier;
        GenerateExpression(declarationStatement.Expression);
        SaveVariableLocation(identifier.Name);
    }

    private void GenerateSetVarStatement(SetVarStatement setVarStatement)
    {
        GenerateExpression(setVarStatement.Expression);
        _output!.Append($"""
                             {GetPopStatement("rax")}
                             mov {GetVariableLocation(setVarStatement.Identifier.Name)}, rax
                         
                         """);
    }

    private void GenerateExpression(IExpressionNode expression)
    {
        if (expression is TermExpression term)
        {
            GenerateTerm(term.Term);
        }
        else if (expression is AddExpression addExpression)
        {
            GenerateTerm(addExpression.Lhs);
            GenerateExpression(addExpression.Rhs);
            _output.Append($"""
                                {GetPopStatement("rdi")}
                                {GetPopStatement("rax")}
                                add rax, rdi
                                {GetPushStatement("rax")}
                                
                            """);
        }
        else if (expression is SubtractExpression subtractExpression)
        {
            GenerateTerm(subtractExpression.Lhs);
            GenerateExpression(subtractExpression.Rhs);
            _output.Append($"""
                                {GetPopStatement("rdi")}
                                {GetPopStatement("rax")}
                                sub rax, rdi
                                {GetPushStatement("rax")}
                                
                            """);
        }
        else
        {
            throw new Exception("Expected expression");
        }
    }
    private void GenerateTerm(ITermNode term)
    {
        if (term is IntLiteralTerm intLiteralTerm)
        {
            _output!.Append($"""
                                 {GetPushStatement(intLiteralTerm.Value.ToString())}

                             """);
        }
        else if (term is IdentifierTerm identifierTerm)
        {
            _output!.Append($"""
                                {GetPushStatement($"QWORD {GetVariableLocation(identifierTerm.Identifier)}")}
                            
                            """);
        }
        else if (term is ParenTerm parenTerm)
        {
            GenerateExpression(parenTerm.Expression);
        }
        else
        {
            throw new Exception($"Unknown term type: {term.GetType()}");
        }
    }

    private string GetPushStatement(string value)
    {
        _stackOffset++;
        return $"push {value}";
    }

    private string GetPopStatement(string register)
    {
        _stackOffset--;
        return $"pop {register}";
    }

    private void SaveVariableLocation(string variableName)
    {
        _variableStackOffsets[variableName] = _stackOffset;
    }

    private string GetVariableLocation(string variableName)
    {
        var relativeStackOffset = (_stackOffset - _variableStackOffsets[variableName]) * 8;
        return $"[rsp + {relativeStackOffset}]";
    }

    private Dictionary<string, int> _variableStackOffsets = null!;
    private int _stackOffset;

    private StringBuilder? _output;
}