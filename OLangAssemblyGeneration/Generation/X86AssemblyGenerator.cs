using System.Text;
using ErrorHelper;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangCompiler.Utility;

namespace AssemblyGeneration.Generation;

public class X86AssemblyGenerator(IErrorHelper errorHelper) : BaseOLangAstVisitor(errorHelper), IGenerator
{
    public string GenerateProgram(Program program)
    {
        _output = new StringBuilder();
        _stackOffset = 0;
        _labelCount = 0;
        _variableStackOffsets = new ScopeTracker<string, int>();
        _functionTracker = new ScopeTracker<string, string>();
        _functions = new List<StringBuilder>();

        var start = "_start";

        _output.Append("""
                       global _start

                       section .text

                       """);
        WriteLabel(start);

        VisitStatements(program.Statements);

        WriteExit(0);

        foreach (var function in _functions)
        {
            _output.Append(function);
            _output.Append('\n');
        }

        return _output.ToString();
    }

    protected override ExitStatement VisitExitStatement(ExitStatement exitStatement)
    {
        exitStatement = base.VisitExitStatement(exitStatement);
        WriteExit();

        return exitStatement;
    }

    private void WriteInstruction(string instruction)
    {
        _output!.Append($"    {instruction}\n");
    }

    private void WritePop(string register)
    {
        WriteInstruction(GetPopStatement(register));
    }

    private void WritePush(string register)
    {
        WriteInstruction(GetPushStatement(register));
    }


    private void WriteLabel(string label)
    {
        _output!.Append($"{label}:\n");
    }

    private void WriteExit(int? value = null)
    {
        if (value != null)
        {
            WriteInstruction($"mov rdi, {value}");
        }
        else
        {
            WritePop("rdi");
        }

        WriteInstruction("mov rax, 60");
        WriteInstruction("syscall");
    }

    protected override VariableDeclarationStatement VisitDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        declarationStatement = base.VisitDeclarationStatement(declarationStatement);
        SaveVariableLocation(declarationStatement.Identifier);

        return declarationStatement;
    }

    protected override VariableAssignment VisitVariableAssignment(VariableAssignment assignment)
    {
        assignment = base.VisitVariableAssignment(assignment);
        WritePop("rax");
        WriteInstruction($"mov {GetVariableLocation(assignment.Identifier)}, rax");

        return assignment;
    }

    protected override IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        ifStatement.Predicate = VisitExpression(ifStatement.Predicate);
        var label = GetLabel("ifEnd");

        WritePop("rax");
        WriteInstruction("cmp rax, 0");
        WriteInstruction($"je {label}");

        ifStatement.Body = VisitScope(ifStatement.Body);
        if (ifStatement.Else is { } elseScope)
        {
            var elseEndLabel = GetLabel("elseEnd");
            WriteInstruction($"jmp {elseEndLabel}");

            WriteLabel(label);
            ifStatement.Else = VisitScope(elseScope);

            WriteLabel(elseEndLabel);
        }
        else
        {
            WriteLabel(label);
        }

        return ifStatement;
    }

    protected override WhileLoop VisitWhileLoop(WhileLoop @while)
    {
        var whileBegin = GetLabel("whileBegin");
        var whileEnd = GetLabel("whileEnd");

        WriteLabel(whileBegin);

        @while.Predicate = VisitExpression(@while.Predicate);
        WritePop("rax");
        WriteInstruction("cmp rax, 0");
        WriteInstruction($"je {whileEnd}");

        @while.Body = VisitScope(@while.Body);

        WriteInstruction($"jmp {whileBegin}");
        WriteLabel(whileEnd);

        return @while;
    }

    protected override ForLoop VisitForLoop(ForLoop @for)
    {
        BeginScope();

        var declaration = new VariableDeclarationStatement(new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int), @for.Identifier, @for.RangeStart);
        VisitDeclarationStatement(declaration);
        var scope = @for.Body;
        var identifierExpression = new VariableAccess(@for.Identifier);
        var addExpression = new Add(identifierExpression, new IntLiteral(1));
        var finalStmt = new VariableAssignment(@for.Identifier, addExpression);

        scope.Statements.Add(finalStmt);

        var condition = new LessThan(identifierExpression, @for.RangeEnd);
        var whileStatement = new WhileLoop(condition, scope);

        VisitWhileLoop(whileStatement);
        var toPop = EndScope();
        WriteInstruction($"add rsp, {toPop.Count * 8}");
        _stackOffset -= toPop.Count;

        return @for;
    }

    // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
    // return values in rax and rdx (if needed)
    // preserve rbx, rbp, r12, r13, r14, and r15
    protected override FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var label = GetLabel($"{functionDeclaration.Identifier}Start");
        var returnLabel = GetLabel($"{functionDeclaration.Identifier}Return");

        var currentReturnLabel = _currentFunctionReturnLabel;
        _currentFunctionReturnLabel = returnLabel;

        _functionTracker.SetValue(functionDeclaration.Identifier, label);
        var functionOutput = new StringBuilder();
        var currentOutput = _output;
        _output = functionOutput;
        _functions.Add(functionOutput);

        WriteLabel(label);

        // save callee-saved registers
        var registersToSave = new[] { "rbx", "rbp", "r12", "r13", "r14", "r15" };
        var pushes = MakePushes(registersToSave);
        _output.Append($"{pushes}");

        var removeLocalVars = GenerateFunctionScope(functionDeclaration.Scope, functionDeclaration.Parameters);

        WriteLabel(returnLabel);
        // restore callee-saved registers
        _output.Append($"""
                            {removeLocalVars}
                        {MakeReversePops(registersToSave)}    ret
                        """);

        _output = currentOutput;
        _currentFunctionReturnLabel = currentReturnLabel;

        return functionDeclaration;
    }

    // rsp must be 16-byte aligned before a call
    // call pushes an 8-byte return address, so increment stack tracker based on that and then make sure the stack is aligned
    // only rbx, rbp, r12, r13, r14, and r15 are preserved
    // return values in rax and rdx (if needed)
    protected override FunctionInvocation VisitFunctionInvocation(FunctionInvocation functionInvocation)
    {
        var funcName = functionInvocation.Identifier;
        if (!_functionTracker.ContainsKey(funcName))
        {
            throw ErrorHelper.ShowErrorMessage($"Unknown function name: {funcName}", functionInvocation.Span);
        }

        // Not preserving rax since that's where the return value goes
        _output!.Append(MakePushes("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11"));

        var argRegisters = new List<string> { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
        var argumentList = functionInvocation.Arguments;
        if (argumentList.Count > argRegisters.Count)
        {
            throw new Exception("Too many parameters");
        }

        for (var i = 0; i < argumentList.Count; i++)
        {
            var argument = argumentList[i];

            VisitExpression(argument);

            WritePop(argRegisters[i]);
        }

        var pops = MakeReversePops("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11");
        var returnPushes = "";
        if (functionInvocation.Type != null)
        {
            returnPushes = $"\n    {GetPushStatement("rax")}\n";
        }

        var label = _functionTracker.GetValue(funcName);
        WriteInstruction($"call {label}");
        _output.Append($"{pops}");
        _output.Append(returnPushes);

        return functionInvocation;
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

    protected override Scope VisitScope(Scope scopeNode)
    {
        BeginScope();
        VisitStatements(scopeNode.Statements);

        var toPop = EndScope();
        WriteInstruction($"add rsp, {toPop.Count * 8}");
        _stackOffset -= toPop.Count;

        return scopeNode;
    }

    // returns statement to remove local vars from the stack
    private string GenerateFunctionScope(Scope functionScope, List<Parameter> parameterList)
    {
        BeginScope();
        // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
        var argRegisters = new List<string> { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };

        if (parameterList.Count > argRegisters.Count)
        {
            throw new Exception("Too many parameters");
        }

        for (var i = 0; i < parameterList.Count; i++)
        {
            var identifier = parameterList[i].Identifier;
            WritePush(argRegisters[i]);
            SaveVariableLocation(identifier);
        }

        VisitStatements(functionScope.Statements);

        var toPop = EndScope();
        _stackOffset -= toPop.Count;
        return $"add rsp, {toPop.Count * 8}";
    }

    protected override Return VisitReturnStatement(Return returnStatement)
    {
        base.VisitReturnStatement(returnStatement);
        if (returnStatement.Value != null)
        {
            WritePop("rax");
        }
        WriteInstruction($"jmp {_currentFunctionReturnLabel}");

        return returnStatement;
    }

    protected override Not VisitNotExpression(Not notExpression)
    {
        notExpression = base.VisitNotExpression(notExpression);
        WritePop("rdi");
        WriteInstruction("cmp rdi, 1");
        WriteInstruction("setne al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return notExpression;
    }

    protected override IntLiteral VisitIntLiteral(IntLiteral intLiteral)
    {
        WritePush(intLiteral.Value.ToString());

        return intLiteral;
    }
    
    protected override BoolLiteral VisitBoolLiteral(BoolLiteral boolLiteral)
    {
        WritePush((boolLiteral.Value ? 1 : 0).ToString());

        return boolLiteral;
    }

    protected override VariableAccess VisitVariableAccess(VariableAccess variableAccess)
    {
        variableAccess = base.VisitVariableAccess(variableAccess);
        WritePush($"QWORD {GetVariableLocation(variableAccess.Identifier)}");

        return variableAccess;
    }
    
    protected override Add VisitAddExpression(Add addExpression)
    {
        addExpression = base.VisitAddExpression(addExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("add rax, rdi");
        WritePush("rax");

        return addExpression;
    }
    
    protected override And VisitAndExpression(And andExpression)
    {
        andExpression = base.VisitAndExpression(andExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("and rax, rdi");
        WritePush("rax");

        return andExpression;
    }

    protected override AreEqual VisitAreEqualExpression(AreEqual areEqualExpression)
    {
        areEqualExpression = base.VisitAreEqualExpression(areEqualExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("sete al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return areEqualExpression;
    }

    protected override GreaterOrEqual VisitGreaterOrEqualExpression(GreaterOrEqual greaterOrEqualExpression)
    {
        greaterOrEqualExpression = base.VisitGreaterOrEqualExpression(greaterOrEqualExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("setge al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return greaterOrEqualExpression;
    }

    protected override GreaterThan VisitGreaterThanExpression(GreaterThan greaterThanExpression)
    {
        greaterThanExpression = base.VisitGreaterThanExpression(greaterThanExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("setg al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return greaterThanExpression;
    }

    protected override LessThan VisitLessThanExpression(LessThan lessThanExpression)
    {
        lessThanExpression = base.VisitLessThanExpression(lessThanExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("setl al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return lessThanExpression;
    }

    protected override LessThanOrEqual VisitLessThanOrEqualExpression(LessThanOrEqual lessThanOrEqualExpression)
    {
        lessThanOrEqualExpression = base.VisitLessThanOrEqualExpression(lessThanOrEqualExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("setle al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return lessThanOrEqualExpression;
    }

    protected override Multiply VisitMultiplyExpression(Multiply multiplyExpression)
    {
        multiplyExpression = base.VisitMultiplyExpression(multiplyExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("mul rdi");
        WritePush("rax");

        return multiplyExpression;
    }

    protected override Divide VisitDivideExpression(Divide divideExpression)
    {
        divideExpression = base.VisitDivideExpression(divideExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cqo"); // extends RAX into RDX
        WriteInstruction("idiv rdi"); // does 128 signed division of RDX:RAX / RDI
        WritePush("rax");
        
        return divideExpression;
    }

    protected override NotEqual VisitNotEqualExpression(NotEqual notEqualExpression)
    {
        notEqualExpression = base.VisitNotEqualExpression(notEqualExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("cmp rax, rdi");
        WriteInstruction("setne al");
        WriteInstruction("movzx rax, al");
        WritePush("rax");

        return notEqualExpression;
    }

    protected override Or VisitOrExpression(Or orExpression)
    {
        orExpression = base.VisitOrExpression(orExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("or rax, rdi");
        WritePush("rax");

        return orExpression;
    }

    protected override Subtract VisitSubtractExpression(Subtract subtractExpression)
    {
        subtractExpression = base.VisitSubtractExpression(subtractExpression);
        WritePop("rdi");
        WritePop("rax");
        WriteInstruction("sub rax, rdi");
        WritePush("rax");

        return subtractExpression;
    }

    protected override Negate VisitNegate(Negate negate)
    {
        negate = base.VisitNegate(negate);
        var reg = "rax";
        WritePop(reg);
        WriteInstruction($"neg {reg}");
        WritePush(reg);

        return negate;
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