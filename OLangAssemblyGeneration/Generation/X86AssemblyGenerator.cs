using System.Text;
using ErrorHelper;
using OLangAst;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangCompiler.Utility;

namespace AssemblyGeneration.Generation;

public class X86AssemblyGenerator
{
    private IErrorHelper _errorHelper;

    public string GenerateProgram(Program program, IErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
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

        GenerateStatements(program.Statements);

        WriteExit(0);
        
        foreach (var function in _functions)
        {
            _output.Append(function);
            _output.Append('\n');
        }

        return _output.ToString();
    }

    private void GenerateStatements(List<IStatement> stmtList)
    {
        foreach (var statement in stmtList)
        {
            GenerateStatement(statement);
        }
    }

     private void GenerateStatement(IStatement statement)
     {
         switch (statement)
         {
             case ExitStatement exitStatement:
                 GenerateExitStatement(exitStatement);
                 break;
             case VariableDeclarationStatement declarationStatement:
                 GenerateVarDeclarationStatement(declarationStatement);
                 break;
             case VariableAssignment assignmentStatement:
                 GenerateAssignmentStatement(assignmentStatement);
                 break;
             case Scope scopeStatement:
                 GenerateScopeStatement(scopeStatement);
                 break;
             case IfStatement ifStatement:
                 GenerateIfStatement(ifStatement);
                 break;
             case WhileLoop whileStatement:
                 GenerateWhileStatement(whileStatement);
                 break;
             case ForLoop forStatement:
                 GenerateForStatement(forStatement);
                 break;
             case FunctionDeclaration functionDeclarationStatement:
                 StoreFunctionDeclaration(functionDeclarationStatement);
                 break;
             case FunctionInvocation invocationStatement:
                 GenerateInvocation(invocationStatement, false);
                 break;
             case Return returnValueStatement:
                 if (returnValueStatement.Value != null)
                 {
                     GenerateReturnValueFunctionStatement(returnValueStatement.Value);
                 }
                 else
                 {
                     GenerateReturnFunctionStatement();
                 }
                 break;
             default:
             {
                 throw _errorHelper.UnknownVariant("statement", statement.GetType());
             }
         }
     }

     private void GenerateExitStatement(ExitStatement exit)
     {
         GenerateExpression(exit.Expression);
         WriteExit();
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

     private void GenerateVarDeclarationStatement(VariableDeclarationStatement declaration)
     {
         GenerateExpression(declaration.Value);
         SaveVariableLocation(declaration.Identifier);
     }

     private void GenerateAssignmentStatement(VariableAssignment assignment)
     {
         GenerateExpression(assignment.Value);
         WritePop("rax");
         WriteInstruction($"mov {GetVariableLocation(assignment.Identifier)}, rax");
     }

     private void GenerateScopeStatement(Scope scopeStatement)
     {
         GenerateScope(scopeStatement);
     }

     private void GenerateIfStatement(IfStatement ifStatement)
     {
         GenerateExpression(ifStatement.Predicate);
         var label = GetLabel("ifEnd");
         
         WritePop("rax");
         WriteInstruction("cmp rax, 0");
         WriteInstruction($"je {label}");
         
         GenerateScope(ifStatement.Body);
         if (ifStatement.Else is {} elseScope)
         {
             var elseEndLabel = GetLabel("elseEnd");
             WriteInstruction($"jmp {elseEndLabel}");

             WriteLabel(label);
             GenerateScope(elseScope);

             WriteLabel(elseEndLabel);
         }
         else
         {
             WriteLabel(label);
         }
     }

     private void GenerateWhileStatement(WhileLoop @while)
     {
         var whileBegin = GetLabel("whileBegin");
         var whileEnd = GetLabel("whileEnd");

         WriteLabel(whileBegin);

         GenerateExpression(@while.Predicate);
         WritePop("rax");
         WriteInstruction("cmp rax, 0");
         WriteInstruction($"je {whileEnd}");
         
         GenerateScope(@while.Body);
         
         WriteInstruction($"jmp {whileBegin}");
         WriteLabel(whileEnd);
     }

     private void GenerateForStatement(ForLoop @for)
     {
         BeginScope();

         var declaration = new VariableDeclarationStatement(new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int), @for.Identifier, @for.RangeStart);
         GenerateVarDeclarationStatement(declaration);

         var scope = @for.Body;
         var identifierExpression = new VariableAccess(@for.Identifier);
         var addExpression = new Add(identifierExpression, new IntLiteral(1));
         var finalStmt = new VariableAssignment(@for.Identifier, addExpression);
         
         scope.Statements.Add(finalStmt);
         
         var condition = new LessThan(identifierExpression, @for.RangeEnd);
         var whileStatement = new WhileLoop(condition, scope);

         GenerateWhileStatement(whileStatement);
         var toPop = EndScope();
         WriteInstruction($"add rsp, {toPop.Count * 8}");
         _stackOffset -= toPop.Count;
     }

     // arguments in order should be in rdi, rsi, rdx, rcx, r8, r9, stack (earlier arguments first)
     // return values in rax and rdx (if needed)
     // preserve rbx, rbp, r12, r13, r14, and r15
     private void StoreFunctionDeclaration(FunctionDeclaration functionDeclaration)
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
     }

     // rsp must be 16-byte aligned before a call
     // call pushes an 8-byte return address, so increment stack tracker based on that and then make sure the stack is aligned
     // only rbx, rbp, r12, r13, r14, and r15 are preserved
     // return values in rax and rdx (if needed)
     private void GenerateInvocation(FunctionInvocation invocationNode, bool hasReturnValue)
     {
         var funcName = invocationNode.Identifier;
         if (!_functionTracker.ContainsKey(funcName))
         {
             throw _errorHelper.ShowErrorMessage($"Unknown function name: {funcName}", invocationNode.Span);
         }

         // Not preserving rax since that's where the return value goes
         _output!.Append(MakePushes("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11"));

         var argRegisters = new List<string> { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
         var argumentList = invocationNode.Arguments;
         if (argumentList.Count > argRegisters.Count)
         {
             throw new Exception("Too many parameters");
         }
         
         for (var i = 0; i < argumentList.Count; i++)
         {
             var argument = argumentList[i];

             GenerateExpression(argument);

             WritePop(argRegisters[i]);
         }

         var pops = MakeReversePops("rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11");
         var returnPushes = "";
         if (hasReturnValue)
         {
             returnPushes = $"\n    {GetPushStatement("rax")}\n";
         }

         var label = _functionTracker.GetValue(funcName);
         WriteInstruction($"call {label}");
         _output.Append($"{pops}");
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

     private void GenerateScope(Scope scopeNode)
     {
         BeginScope();
         GenerateStatements(scopeNode.Statements);

         var toPop = EndScope();
         WriteInstruction($"add rsp, {toPop.Count * 8}");
         _stackOffset -= toPop.Count;
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

         GenerateStatements(functionScope.Statements);

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

     private void GenerateReturnValueFunctionStatement(IExpression returnValue)
     {
         GenerateExpression(returnValue);

         WritePop("rax");
         WriteInstruction($"jmp {_currentFunctionReturnLabel}");
     }

     private void GenerateExpression(IExpression expression)
     {
         switch (expression)
         {
             case Not notExpression:
                 GenerateExpression(notExpression.Value);
                 WritePop("rdi");
                 WriteInstruction("cmp rdi, 1");
                 WriteInstruction("setne al");
                 WriteInstruction("movzx rax, al");
                 WritePush("rax");
                 break;
             case BaseBinaryExpression binaryExpressionNode:
                 GenerateExpression(binaryExpressionNode.Lhs);
                 GenerateExpression(binaryExpressionNode.Rhs);

                 WritePop("rdi");
                 WritePop("rax");
                 switch (binaryExpressionNode)
                 {
                     case Add:
                         WriteInstruction("add rax, rdi");
                         break;
                     case Subtract:
                         WriteInstruction("sub rax, rdi");
                         break;
                     case Multiply:
                         WriteInstruction("mul rdi");
                         break;
                     case Divide:
                         WriteInstruction("cqo"); // extends RAX into RDX
                         WriteInstruction("idiv rdi"); // does 128 signed division of RDX:RAX / RDI
                         break;
                     case AreEqual:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("sete al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case NotEqual:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("setne al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case GreaterThan:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("setg al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case GreaterOrEqual:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("setge al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case LessThan:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("setl al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case LessThanOrEqual:
                         WriteInstruction("cmp rax, rdi");
                         WriteInstruction("setle al");
                         WriteInstruction("movzx rax, al");
                         break;
                     case And:
                         WriteInstruction("and rax, rdi");
                         break;
                     case Or:
                         WriteInstruction("or rax, rdi");
                         break;
                     default:
                         throw _errorHelper.UnknownVariant("expression", expression.GetType());
                 }

                 WritePush("rax");

                 break;
             case IntLiteral intLiteralTerm:
                 WritePush(intLiteralTerm.Value.ToString());
                 break;
             case BoolLiteral boolLiteralTerm:
                 WritePush((boolLiteralTerm.Value ? 1 : 0).ToString());
                 break;
             case VariableAccess identifierTerm:
                 WritePush($"QWORD {GetVariableLocation(identifierTerm.Identifier)}");
                 break;
             case FunctionInvocation invocationTerm:
                 GenerateInvocation(invocationTerm, true);
                 break;
             case Negate negate:
                 GenerateNegate(negate);
                 break;
             default:
                 throw _errorHelper.UnknownVariant("expression", expression.GetType());
         }
     }

     private void GenerateNegate(Negate negate)
     {
         GenerateExpression(negate.Value);
         var reg = "rax";
         WritePop(reg);
         WriteInstruction($"neg {reg}");
         WritePush(reg);
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