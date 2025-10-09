using System.Text;
using OLangCompiler.Parser.Nodes;
using OLangCompiler.Utility;

namespace OLangCompiler.Generation;

public class AssemblyGenerator
{
    public string GenerateProgram(ProgramNode program)
    {
        _output = new StringBuilder();
        _stackOffset = 0;
        _labelCount = 0;
        _variableStackOffsets = new ScopeTracker<string, int>();

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
            case AssignmentStatement assignmentStatement:
                GenerateAssignmentStatement(assignmentStatement);
                break;
            case ScopeStatement scopeStatement:
                GenerateScopeStatement(scopeStatement);
                break;
            case IfStatement ifStatement:
                GenerateIfStatement(ifStatement);
                break;
            case WhileStatement whileStatement:
                GenerateWhileStatement(whileStatement);
                break;
            default:
            {
                throw ErrorHelper.UnknownVariant("statement", statement.GetType());
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
        SaveVariableLocation(identifier.Identifier);
    }

    private void GenerateAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        GenerateExpression(assignmentStatement.Expression);
        _output!.Append($"""
                             {GetPopStatement("rax")}
                             mov {GetVariableLocation(assignmentStatement.Identifier.Identifier)}, rax

                         """);
    }

    private void GenerateScopeStatement(ScopeStatement scopeStatement)
    {
        GenerateScope(scopeStatement.Scope);
    }

    private void GenerateIfStatement(IfStatement ifStatement)
    {
        GenerateExpression(ifStatement.Condition);
        var label = GetLabel();
        _output.Append($"""
                            {GetPopStatement("rax")}
                            cmp rax, 0
                            je {label}

                        """);
        GenerateScope(ifStatement.Scope);
        _output.Append($"{label}:\n");
    }

    private void GenerateWhileStatement(WhileStatement whileStatement)
    {
        var whileBegin = GetLabel();
        var whileEnd = GetLabel();

        _output!.Append($"{whileBegin}:");
        
        GenerateExpression(whileStatement.Condition);
        _output.Append($"""
                            {GetPopStatement("rax")}
                            cmp rax, 0
                            je {whileEnd}

                        """);
        GenerateScope(whileStatement.Scope);
        _output.Append($"""
                            jmp {whileBegin}
                        {whileEnd}:
                        
                        """);
    }

    private void GenerateScope(ScopeNode scopeNode)
    {
        _variableStackOffsets.BeginScope();
        foreach (var statement in scopeNode.Statements)
        {
            GenerateStatement(statement);
        }

        var toPop = _variableStackOffsets.EndScope();
        _output!.Append($"    add rsp, {toPop.Count * 8}\n");
        _stackOffset -= toPop.Count;
    }

    private void GenerateExpression(IExpressionNode expression)
    {
        switch (expression)
        {
            case TermExpression term:
                GenerateTerm(term.Term);
                break;
            case NotExpression notExpression:
                GenerateExpression(notExpression.Expression);
                _output!.Append($"""
                                     {GetPopStatement("rdi")}
                                     cmp rdi, 1
                                     setne al
                                     movzx rax, al
                                     {GetPushStatement("rax")}
                                 
                                 """);
                break;
            case BaseBinaryExpressionNode addExpression:
                GenerateTerm(addExpression.Lhs);
                GenerateExpression(addExpression.Rhs);

                _output!.Append($"""
                                     {GetPopStatement("rdi")}
                                     {GetPopStatement("rax")}
                                     
                                 """);
                switch (addExpression)
                {
                    case AddExpression:
                        _output!.Append("    add rax, rdi\n");
                        break;
                    case SubtractExpression:
                        _output!.Append("    sub rax, rdi\n");
                        break;
                    case DoubleEqualsExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           sete al
                                           movzx rax, al
                                           
                                       """);
                        break;
                    case NotEqualExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           setne al
                                           movzx rax, al
                                           
                                       """);
                        break;
                }

                _output.Append($"    {GetPushStatement("rax")}\n");

                break;
            default:
                throw ErrorHelper.UnknownVariant("expression", expression.GetType());
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
        else if (term is BoolLiteralTerm boolLiteralTerm)
        {
            _output!.Append($"""
                                 {GetPushStatement((boolLiteralTerm.Value ? 1 : 0).ToString())}

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
            throw ErrorHelper.UnknownVariant("term", term.GetType());
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
        _variableStackOffsets.SetValue(variableName, _stackOffset);
    }

    private string GetVariableLocation(string variableName)
    {
        var relativeStackOffset = (_stackOffset - _variableStackOffsets.TryGetValue(variableName)) * 8;
        return $"[rsp + {relativeStackOffset}]";
    }

    private string GetLabel()
    {
        return $"label{_labelCount++}";
    }

    private ScopeTracker<string, int> _variableStackOffsets = null!;
    private int _stackOffset;
    private int _labelCount;

    private StringBuilder? _output;
}