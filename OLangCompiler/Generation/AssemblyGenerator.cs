using System.Text;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Parser.ParseTree.ArgumentList;
using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Prog;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Parser.ParseTree.Stmt;
using OLangCompiler.Parser.ParseTree.StmtList;
using OLangCompiler.Parser.ParseTree.Term;
using OLangCompiler.Parser.ParseTree.Type;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;
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

        GenerateStmtList(program.StmtList);

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

    private void GenerateStmtList(IStmtListNode stmtList)
    {
        switch (stmtList)
        {
            case EmptyStmtList:
                break;
            case StmtListWithStatement stmtListWithStatement:
                GenerateStatement(stmtListWithStatement.Statement);
                GenerateStmtList(stmtListWithStatement.StmtList);
                break;
            default:
            {
                throw _errorHelper.UnknownVariant("statment list", stmtList.GetType());
            }
        }
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
                GenerateInvocation(invocationStatement.InvocationNode, false);
                break;
            case ReturnValueStatement returnValueStatement:
                GenerateReturnValueFunctionStatement(returnValueStatement);
                break;
            case ReturnStatement returnStatement:
                GenerateReturnFunctionStatement();
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
        if (assignmentStatement.Operator is not EqualsNode)
        {
            _errorHelper.UnknownVariant("operator", assignmentStatement.Operator.GetType());
        }
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
        var label = GetLabel("ifEnd");
        _output!.Append($"""
                             {GetPopStatement("rax")}
                             cmp rax, 0
                             je {label}

                         """);
        GenerateScope(ifStatement.Scope);
        if (ifStatement.ElseBlock is ElseNode elseNode)
        {
            var elseEndLabel = GetLabel("elseEnd");
            _output.Append($"    jmp {elseEndLabel}\n");

            _output.Append($"{label}:\n");
            GenerateScope(elseNode.Scope);

            _output.Append($"{elseEndLabel}:\n");
        }
        else
        {
            _output.Append($"{label}:\n");
        }
    }

    private void GenerateWhileStatement(WhileStatement whileStatement)
    {
        var whileBegin = GetLabel("whileBegin");
        var whileEnd = GetLabel("whileEnd");

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

        var declaration = new DeclarationStatement(new PrimitiveVariableType(new IntTypeNode()), forStatement.Identifier, forStatement.Start);
        GenerateVarDeclarationStatement(declaration);

        var scope = forStatement.Scope;
        var identifierExpression = new TermExpression(new IdentifierTerm(forStatement.Identifier.Identifier));
        var addExpression = new AddExpression(identifierExpression, new TermExpression(new IntLiteralTerm(1)));
        var assignment = new AssignmentStatement(forStatement.Identifier, new EqualsNode(), addExpression);
        var finalStmt = new StmtListWithStatement(assignment, new EmptyStmtList());
        if (scope.StmtList is EmptyStmtList)
        {
            scope.StmtList = finalStmt;
        }
        else if (scope.StmtList is StmtListWithStatement stmtList)
        {
            while (stmtList.StmtList is StmtListWithStatement innerStmtList)
            {
                stmtList = innerStmtList;
            }

            stmtList.StmtList = finalStmt;
        }

        var condition = new LessExpression(identifierExpression, forStatement.End);
        var whileStatement = new WhileStatement(condition, scope);

        GenerateWhileStatement(whileStatement);
        var toPop = EndScope();
        _output!.Append($"    add rsp, {toPop.Count * 8}\n");
        _stackOffset -= toPop.Count;
    }

    // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
    // return values in rax and rdx (if needed)
    // preserve rbx, rbp, r12, r13, r14, and r15
    private void StoreFunctionDeclaration(FunctionDeclarationStatement functionDeclaration)
    {
        var label = GetLabel($"{functionDeclaration.Identifier.Identifier}Start");
        var returnLabel = GetLabel($"{functionDeclaration.Identifier.Identifier}Return");

        var currentReturnLabel = _currentFunctionReturnLabel;
        _currentFunctionReturnLabel = returnLabel;

        _functionTracker.SetValue(functionDeclaration.Identifier.Identifier, label);
        var functionOutput = new StringBuilder();
        var currentOutput = _output;
        _output = functionOutput;
        _functions.Add(functionOutput);

        _output.Append($"{label}:\n");

        // save callee-saved registers
        var registersToSave = new[] { "rbx", "rbp", "r12", "r13", "r14", "r15" };
        var pushes = MakePushes(registersToSave);
        _output.Append($"{pushes}");

        var removeLocalVars = GenerateFunctionScope(functionDeclaration.Scope, functionDeclaration.Parameters);

        // restore callee-saved registers
        _output.Append($"""
                        {returnLabel}:
                            {removeLocalVars}
                        {MakeReversePops(registersToSave)}    ret
                        """);

        _output = currentOutput;
        _currentFunctionReturnLabel = currentReturnLabel;
    }

    // rsp must be 16-byte aligned before a call
    // call pushes an 8-byte return address, so increment stack tracker based on that and then make sure the stack is aligned
    // only rbx, rbp, r12, r13, r14, and r15 are preserved
    // return values in rax and rdx (if needed)
    private void GenerateInvocation(InvocationNode invocationNode, bool hasReturnValue)
    {
        var funcName = invocationNode.Identifier.Identifier;
        if (!_functionTracker.ContainsKey(funcName))
        {
            throw _errorHelper.ShowErrorMessageAtNode($"Unknown function name: {funcName}", invocationNode);
        }

        // Not preserving rax since that's where the return value goes
        _output!.Append(MakePushes("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11"));

        var argRegisters = new List<string> { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
        var i = 0;
        var argumentList = invocationNode.Arguments;
        while (argumentList is ExpressionArgumentList expArgList)
        {
            if (i >= argRegisters.Count)
            {
                throw new Exception("Too many parameters");
            }

            GenerateExpression(expArgList.Expression);

            _output!.Append($"    {GetPopStatement(argRegisters[i])}\n");

            if (expArgList is ContinuedArgumentList continued)
            {
                argumentList = continued.ArgumentList;
            }
            else
            {
                break;
            }
            i += 1;
        }

        var pops = MakeReversePops("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11");
        var returnPushes = "";
        if (hasReturnValue)
        {
            returnPushes = $"\n    {GetPushStatement("rax")}\n";
        }

        var label = _functionTracker.GetValue(funcName);
        _output.Append($"""
                            call {label}
                        {pops}
                        """);
        _output.Append(returnPushes);
    }

    private string MakePushes(params string[] registers)
    {
        var pushes = new StringBuilder();
        foreach (var reg in registers)
        {
            pushes.Append($"    {GetPushStatement(reg)}\n");
        }

        return pushes.ToString();
    }

    private string MakeReversePops(params string[] registers)
    {
        var pops = new StringBuilder();
        foreach (var reg in registers.Reverse())
        {
            pops.Append($"    {GetPopStatement(reg)}\n");
        }

        return pops.ToString();
    }

    private void GenerateScope(ScopeNode scopeNode)
    {
        BeginScope();
        GenerateStmtList(scopeNode.StmtList);

        var toPop = EndScope();
        _output!.Append($"    add rsp, {toPop.Count * 8}\n");
        _stackOffset -= toPop.Count;
    }

    // returns statement to remove local vars from the stack
    private string GenerateFunctionScope(ScopeNode functionScope, IParameterListNode parameterList)
    {
        BeginScope();
        // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
        var argRegisters = new List<string> { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };

        var i = 0;
        while (parameterList is Parameter param)
        {
            if (i >= argRegisters.Count)
            {
                throw new Exception("Too many parameters");
            }
            
            var identifier = param.Identifier;
            _output!.Append($"    {GetPushStatement(argRegisters[i])}\n");
            SaveVariableLocation(identifier.Identifier);

            if (parameterList is ContinuedParameterList continued)
            {
                parameterList = continued.ParameterList;
            }
            else
            {
                break;
            }
            i += 1;
        }

        GenerateStmtList(functionScope.StmtList);

        var toPop = EndScope();
        _stackOffset -= toPop.Count;
        return $"add rsp, {toPop.Count * 8}";
    }

    private void GenerateReturnFunctionStatement()
    {
        _output!.Append($"""
                         
                             jmp {_currentFunctionReturnLabel}

                         """);
    }

    private void GenerateReturnValueFunctionStatement(ReturnValueStatement returnValue)
    {
        GenerateExpression(returnValue.Expression);

        _output!.Append($"""
                             {GetPopStatement("rax")}
                             jmp {_currentFunctionReturnLabel}

                         """);
    }

    private void GenerateExpression(BaseExpressionNode expression)
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
                    case DivideExpression:
                        _output!.Append("    cqo\n"); // extends RAX into RDX
                        _output!.Append("    idiv rdi\n"); // does 128 signed division of RDX:RAX / RDI
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
                    case BooleanAndExpression:
                        _output!.Append("    and rax, rdi\n");
                        break;
                    case BooleanOrExpression:
                        _output!.Append("    or rax, rdi\n");
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

    private void GenerateTerm(BaseTermNode term)
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
            case InvocationTerm invocationTerm:
                GenerateInvocation(invocationTerm.InvocationNode, true);
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

    private string GetLabel(string type)
    {
        return $"{type}{_labelCount++}";
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

    private string? _currentFunctionReturnLabel;
    private StringBuilder? _output;
    private List<StringBuilder> _functions;
}