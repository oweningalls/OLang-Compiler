using System.Text;
using OLangCompiler.Parser.Nodes;
using OLangCompiler.TypeChecking.Types;
using OLangCompiler.Utility;

namespace OLangCompiler.Generation;

public class AssemblyGenerator
{
    private ErrorHelper _errorHelper;

    public string GenerateProgram(ProgramNode program, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _output = new StringBuilder();
        _stackOffset = 0;
        _labelCount = 0;
        _variableStackOffsets = new ScopeTracker<string, int>();
        _functionTracker = new ScopeTracker<string, string>();
        _functions = new List<StringBuilder>();

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
        foreach (var function in _functions)
        {
            _output.Append(function);
            _output.Append('\n');
        }

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
            case ForStatement forStatement:
                GenerateForStatement(forStatement);
                break;
            case FunctionDeclarationStatement functionDeclarationStatement:
                StoreFunctionDeclaration(functionDeclarationStatement);
                break;
            case InvocationStatement invocationStatement:
                GenerateInvocationStatement(invocationStatement);
                break;
            default:
            {
                throw _errorHelper.UnknownVariant("statement", statement.GetType());
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
        _output!.Append($"""
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

        _output!.Append($"{whileBegin}:\n");

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

    private void GenerateForStatement(ForStatement forStatement)
    {
        BeginScope();

        var declaration = new DeclarationStatement(forStatement.Identifier, forStatement.Start, ExpressionType.Int);
        GenerateVarDeclarationStatement(declaration);

        var scope = forStatement.Scope;
        var identifierExpression = new TermExpression(new IdentifierTerm(forStatement.Identifier.Identifier));
        var addExpression = new AddExpression(identifierExpression, new TermExpression(new IntLiteralTerm(1)));
        scope.Statements.Add(new AssignmentStatement(forStatement.Identifier, addExpression));

        var condition = new LessExpression(identifierExpression, forStatement.End);
        var whileStatement = new WhileStatement(condition, scope);

        GenerateWhileStatement(whileStatement);
        EndScope();
    }

    // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
    // return values in rax and rdx (if needed)
    // preserve rbx, rbp, r12, r13, r14, and r15
    private void StoreFunctionDeclaration(FunctionDeclarationStatement functionDeclaration)
    {
        var label = GetLabel();
        _functionTracker.SetValue(functionDeclaration.Identifier.Identifier, label);
        var functionOutput = new StringBuilder();
        var currentOutput = _output;
        _output = functionOutput;
        _functions.Add(functionOutput);

        _output.Append($"    {label}:\n");

        var (pushes, pops) = MakeMatchingPushAndPops("rbx", "rbp", "r12", "r13", "r14", "r15");

        // save callee-saved registers
        _output.Append($"{pushes}");

        GenerateScope(functionDeclaration.Scope);

        // restore callee-saved registers
        _output.Append($"{pops}");
        _output.Append("    ret");

        _output = currentOutput;
    }

    // rsp must be 16-byte aligned before a call
    // call pushes an 8-byte return address, so increment stack tracker based on that and then make sure the stack is aligned
    // only rbx, rbp, r12, r13, r14, and r15 are preserved
    // return values in rax and rdx (if needed)
    private void GenerateInvocationStatement(InvocationStatement invocationStatement)
    {
        var funcName = invocationStatement.Identifier.Identifier;
        if (!_functionTracker.ContainsKey(funcName))
        {
            throw _errorHelper.ShowErrorMessageAtNode($"Unknown function name: {funcName}", invocationStatement);
        }

        var (pushes, pops) = MakeMatchingPushAndPops("rax", "rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11");
        var label = _functionTracker.GetValue(funcName);
        _output.Append($"""
                        {pushes}
                            
                            call {label}
                            
                        {pops}
                        """);
    }

    private (string, string) MakeMatchingPushAndPops(params string[] registers)
    {
        var pushes = new StringBuilder();
        foreach (var reg in registers)
        {
            pushes.Append($"    {GetPushStatement(reg)}\n");
        }

        var pops = new StringBuilder();
        foreach (var reg in registers.Reverse())
        {
            pops.Append($"    {GetPopStatement(reg)}\n");
        }

        return (pushes.ToString(), pops.ToString());
    }

    private void GenerateScope(ScopeNode scopeNode)
    {
        BeginScope();
        foreach (var statement in scopeNode.Statements)
        {
            GenerateStatement(statement);
        }

        var toPop = EndScope();
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
            case BaseBinaryExpressionNode binaryExpressionNode:
                GenerateExpression(binaryExpressionNode.Lhs);
                GenerateExpression(binaryExpressionNode.Rhs);

                _output!.Append($"""
                                     {GetPopStatement("rdi")}
                                     {GetPopStatement("rax")}

                                 """);
                switch (binaryExpressionNode)
                {
                    case AddExpression:
                        _output!.Append("    add rax, rdi\n");
                        break;
                    case SubtractExpression:
                        _output!.Append("    sub rax, rdi\n");
                        break;
                    case TimesExpression:
                        _output!.Append("    mul rdi\n");
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
                    case GreaterExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           setg al
                                           movzx rax, al

                                       """);
                        break;
                    case GreaterOrEqualExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           setge al
                                           movzx rax, al

                                       """);
                        break;
                    case LessExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           setl al
                                           movzx rax, al

                                       """);
                        break;
                    case LessOrEqualExpression:
                        _output.Append("""
                                           cmp rax, rdi
                                           setle al
                                           movzx rax, al

                                       """);
                        break;
                    default:
                        throw _errorHelper.UnknownVariant("expression", expression.GetType());
                }

                _output.Append($"    {GetPushStatement("rax")}\n");

                break;
            default:
                throw _errorHelper.UnknownVariant("expression", expression.GetType());
        }
    }

    private void GenerateTerm(ITermNode term)
    {
        switch (term)
        {
            case IntLiteralTerm intLiteralTerm:
                _output!.Append($"""
                                     {GetPushStatement(intLiteralTerm.Value.ToString())}

                                 """);
                break;
            case BoolLiteralTerm boolLiteralTerm:
                _output!.Append($"""
                                     {GetPushStatement((boolLiteralTerm.Value ? 1 : 0).ToString())}

                                 """);
                break;
            case IdentifierTerm identifierTerm:
                _output!.Append($"""
                                     {GetPushStatement($"QWORD {GetVariableLocation(identifierTerm.Identifier)}")}

                                 """);
                break;
            case ParenTerm parenTerm:
                GenerateExpression(parenTerm.Expression);
                break;
            default:
                throw _errorHelper.UnknownVariant("term", term.GetType());
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
        var relativeStackOffset = (_stackOffset - _variableStackOffsets.GetValue(variableName)) * 8;
        return $"[rsp + {relativeStackOffset}]";
    }

    private string GetLabel()
    {
        return $"label{_labelCount++}";
    }

    private void BeginScope()
    {
        _variableStackOffsets.BeginScope();
        _functionTracker.BeginScope();
    }

    private Dictionary<string, int> EndScope()
    {
        _functionTracker.EndScope();
        return _variableStackOffsets.EndScope();
    }

    private ScopeTracker<string, int> _variableStackOffsets = null!;
    private ScopeTracker<string, string> _functionTracker;
    private int _stackOffset;
    private int _labelCount;

    private StringBuilder? _output;
    private List<StringBuilder> _functions;
}