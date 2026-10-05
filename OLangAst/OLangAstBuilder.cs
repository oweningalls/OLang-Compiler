using ErrorHelper;
using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangGrammar.ParseTree.AddExpression;
using OLangGrammar.ParseTree.AndExpression;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.ClassDeclarationList;
using OLangGrammar.ParseTree.ClassMember;
using OLangGrammar.ParseTree.ClassMemberList;
using OLangGrammar.ParseTree.EnumValueList;
using OLangGrammar.ParseTree.EnumVariant;
using OLangGrammar.ParseTree.EnumVariantList;
using OLangGrammar.ParseTree.EqualityExpression;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.FieldInitialization;
using OLangGrammar.ParseTree.FunctionInvocation;
using OLangGrammar.ParseTree.GreaterExpression;
using OLangGrammar.ParseTree.InitializationList;
using OLangGrammar.ParseTree.MethodInvocation;
using OLangGrammar.ParseTree.MultExpression;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Prog;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Stmt;
using OLangGrammar.ParseTree.StmtList;
using OLangGrammar.ParseTree.Term;
using OLangGrammar.ParseTree.Type;
using OLangGrammar.ParseTree.TypeDeclaration;
using OLangGrammar.ParseTree.UnaryExpression;
using OLangGrammar.ParseTree.UsingList;
using OLangGrammar.UsingStatement;
using OLangGrammar.UsingStatementIdentifier;
using OLangTokens.Tokens;
using ArrayType = OLangAst.Miscellaneous.ArrayType;
using BoolLiteral = OLangAst.Expressions.BoolLiteral;
using ClassDeclaration = OLangAst.TypeSystem.ClassDeclaration;
using EnumDeclaration = OLangAst.TypeSystem.EnumDeclaration;
using EnumVariant = OLangAst.EnumVariants.EnumVariant;
using FieldAccess = OLangAst.Expressions.FieldAccess;
using FloatLiteral = OLangAst.Expressions.FloatLiteral;
using FunctionDeclaration = OLangAst.Statements.FunctionDeclaration;
using MethodInvocation = OLangAst.Statements.MethodInvocation;
using GreaterOrEqual = OLangAst.Expressions.GreaterOrEqual;
using IExpression = OLangAst.Expressions.IExpression;
using IntLiteral = OLangAst.Expressions.IntLiteral;
using IStatement = OLangAst.Statements.IStatement;
using Not = OLangAst.Expressions.Not;
using NotEqual = OLangAst.Expressions.NotEqual;
using Return = OLangAst.Statements.Return;
using StringLiteral = OLangAst.Expressions.StringLiteral;
using FunctionInvocation = OLangAst.Statements.FunctionInvocation;
using IClassMember = OLangAst.ClassMembers.IClassMember;
using MethodDeclaration = OLangAst.ClassMembers.MethodDeclaration;
using FieldDeclaration = OLangAst.ClassMembers.FieldDeclaration;
using ITypeDeclaration = OLangAst.TypeSystem.ITypeDeclaration;

namespace OLangAst;

public class OLangAstBuilder(IErrorHelper errorHelper)
{
    public Program ParseProgram(ProgramNode programNode)
    {
        return new Program(ParseUsingStatements(programNode.UsingList), ParseTypeDeclarations(programNode.TypeDeclarationList)) { Span = programNode.Span};
    }

    private List<UsingStatement> ParseUsingStatements(IUsingList usingList)
    {
        return ParseList(usingList,
            x => x switch
            {
                EmptyUsingList _ => (null, null),
                UsingListWithStatement usingListWithStatement => (usingListWithStatement.UsingStatement, usingListWithStatement.UsingList),
                _ => throw new ArgumentOutOfRangeException(nameof(x)),
            },
            ParseUsingStatement
            );
    }

    private UsingStatement ParseUsingStatement(IUsingStatement usingStatement)
    {
        return usingStatement switch
        {
            OLangGrammar.UsingStatement.UsingStatement usingStatement1 => new UsingStatement(ParseUsingStatementIdentifier(usingStatement1.UsingStatementIdentifier)),
            _ => throw errorHelper.UnknownVariant("class list", usingStatement.GetType())
        };
    }

    private string ParseUsingStatementIdentifier(IUsingStatementIdentifier usingStatementIdentifier)
    {
        var identList = ParseList(usingStatementIdentifier,
            x => x switch
            {
                ContinuedUsingStatementIdentifier continuedUsingStatementIdentifier => (continuedUsingStatementIdentifier.Identifier, continuedUsingStatementIdentifier.ContinuedIdentifier),
                SingleUsingStatementIdentifier singleUsingStatementIdentifier => (singleUsingStatementIdentifier.Identifier, null),
                _ => throw new ArgumentOutOfRangeException(nameof(x))
            },
            ParseIdentifier);

        return string.Join(".", identList);
    }

    private List<ITypeDeclaration> ParseTypeDeclarations(ITypeDeclarationListNode typeDeclarationList)
    {
        return ParseList(typeDeclarationList,
            x => x switch
            {
                TypeDeclarationListWithType classDeclarationListWithClass => (ClassDeclaration: classDeclarationListWithClass.TypeDeclaration, ClassDeclarationList: classDeclarationListWithClass.TypeDeclarationList),
                SingleTypeTypeList singleClassClassList => (ClassDeclaration: singleClassClassList.TypeDeclaration, null),
                _ => throw errorHelper.UnknownVariant("class list", x.GetType())
            },
            ParseTypeDeclaration
        );
    }
    
    private List<TTarget> ParseList<TList, TData, TTarget>(TList first, Func<TList, (TData?, TList?)> getValueAndNext, Func<TData, TTarget> parseValue)
    {
        var (value, next) = getValueAndNext(first);
        var list = new List<TTarget>();
        if (value is { } val)
        {
            list.Add(parseValue(val));
        }
        while (next != null)
        {
            (value, next) = getValueAndNext(next);
            if (value is { } nonNull)
            {
                list.Add(parseValue(nonNull));
            }
        }

        return list;
    }

    private ITypeDeclaration ParseTypeDeclaration(OLangGrammar.ParseTree.TypeDeclaration.ITypeDeclaration typeDeclaration)
    {
        return typeDeclaration switch
        {
            OLangGrammar.ParseTree.TypeDeclaration.ClassDeclaration concreteClassDeclaration => new ClassDeclaration(false, ParseIdentifier(concreteClassDeclaration.IdentifierToken), ParseClassMembers(concreteClassDeclaration.ClassMemberList)) { Span = typeDeclaration.Span },
            StaticClassDeclaration staticClassDeclaration => new ClassDeclaration(true, ParseIdentifier(staticClassDeclaration.IdentifierToken), ParseClassMembers(staticClassDeclaration.ClassMemberList)) { Span = typeDeclaration.Span },
            OLangGrammar.ParseTree.TypeDeclaration.EnumDeclaration enumDeclaration => new EnumDeclaration(ParseIdentifier(enumDeclaration.IdentifierToken), ParseEnumVariantList(enumDeclaration.EnumVariantList)) { Span = typeDeclaration.Span },
            _ => throw errorHelper.UnknownVariant("class declaration", typeDeclaration.GetType())
        };
    }
    
    private List<IClassMember> ParseClassMembers(IClassMemberListNode classMemberList)
    {
        var classMembers = ParseList(
            classMemberList,
            x => x switch
            {
                SingleMemberClassMemberList member => (member.ClassMember, null),
                ClassMemberListWithClassMember list => (list.ClassMember, list.ClassMemberList),
                _ => throw errorHelper.UnknownVariant("class member list", x.GetType())
            },
            ParseClassMember
        );
        return classMembers;
    }

    private List<EnumVariant> ParseEnumVariantList(IEnumVariantList enumVariantList)
    {
        return ParseList(
            enumVariantList,
            x => x switch
            {
                EmptyEnumVariantList => (null, null),
                EnumVariantListWithStatement enumVariantListWithStatement => (enumVariantListWithStatement.EnumVariant, enumVariantListWithStatement.EnumVariantList),
                SingleVariantEnumVariantList singleVariantEnumVariantList => (singleVariantEnumVariantList.EnumVariant, null),
                _ => throw errorHelper.UnknownVariant("enum variant list", x.GetType())
            },
            ParseEnumVariant);
    }

    private EnumVariant ParseEnumVariant(IEnumVariant enumVariant)
    {
        return enumVariant switch
        {
            OLangGrammar.ParseTree.EnumVariant.EnumVariant enumVariant1 => new EnumVariant(ParseIdentifier(enumVariant1.Identifier), []),
            EnumVariantWithValues enumVariant1 => new EnumVariant(ParseIdentifier(enumVariant1.Identifier), ParseEnumVariantValueList(enumVariant1.ValueList)),
            _ => throw errorHelper.UnknownVariant("enum variant", enumVariant.GetType())
        };
    }

    private List<IVariableType> ParseEnumVariantValueList(IEnumValueList enumValueList)
    {
        return ParseList(
            enumValueList,
            x => x switch
            {
                ContinuedEnumValueList continuedEnumValueList => (continuedEnumValueList.Type, continuedEnumValueList.EnumValueList),
                SingleValueEnumValueList singleValueEnumValueList => (singleValueEnumValueList.Type, null),
                _ => throw errorHelper.UnknownVariant("enum variant value list", x.GetType())
            },
            ParseType);
    }

    private List<IStatement> ParseStatementList(IStmtListNode statementList)
    {
        return ParseList(
            statementList,
            x => x switch
            {
                SingleStatementStmtList value => (value.Statement, null),
                StmtListWithStatement list => (list.Statement, list.StmtList),
                _ => throw errorHelper.UnknownVariant("statement list", x.GetType())
            },
            ParseStatement
        );
    }

    private IClassMember ParseClassMember(OLangGrammar.ParseTree.ClassMember.IClassMember classMember)
    {
        return classMember switch
        {
            OLangGrammar.ParseTree.ClassMember.MethodDeclaration methodDeclaration => new MethodDeclaration(ParseType(methodDeclaration.Type), ParseIdentifier(methodDeclaration.Identifier), [], ParseScope(methodDeclaration.Scope)) { Span = methodDeclaration.Span },
            MethodDeclarationWithParameters methodDeclarationWithParameters => new MethodDeclaration(ParseType(methodDeclarationWithParameters.Type), ParseIdentifier(methodDeclarationWithParameters.Identifier), ParseParameterList(methodDeclarationWithParameters.Parameters), ParseScope(methodDeclarationWithParameters.Scope)) { Span = methodDeclarationWithParameters.Span },
            VoidMethodDeclaration voidMethodDeclaration => new MethodDeclaration(null, ParseIdentifier(voidMethodDeclaration.Identifier), [], ParseScope(voidMethodDeclaration.Scope)) { Span = voidMethodDeclaration.Span },
            VoidMethodDeclarationWithParameters voidMethodDeclarationWithParameters => new MethodDeclaration(null, ParseIdentifier(voidMethodDeclarationWithParameters.Identifier), ParseParameterList(voidMethodDeclarationWithParameters.Parameters), ParseScope(voidMethodDeclarationWithParameters.Scope)) { Span = voidMethodDeclarationWithParameters.Span },
            OLangGrammar.ParseTree.ClassMember.FieldDeclaration fieldDeclaration => new FieldDeclaration(ParseType(fieldDeclaration.Type), ParseIdentifier(fieldDeclaration.Identifier)) { Span = fieldDeclaration.Span },
            
            _ => throw errorHelper.UnknownVariant("class member", classMember.GetType())
        };
    }
    
    protected IStatement ParseStatement(OLangGrammar.ParseTree.Stmt.IStatement statement)
    {
        return statement switch
        {
            Assignment assignment => ParseAssignment(assignment),
            ArrayAssignmentStatement assignment => ParseArrayAssignment(assignment),
            FieldAssignmentStatement assignment => ParseFieldAssignment(assignment),
            Declaration declaration => new VariableDeclarationStatement(ParseType(declaration.Type), ParseIdentifier(declaration.Identifier), ParseExpression(declaration.Expression)) { Span = declaration.Span },
            LetDeclaration letDeclaration => new VariableDeclarationStatement(null, ParseIdentifier(letDeclaration.Identifier), ParseExpression(letDeclaration.Expression)) { Span = letDeclaration.Span },
            Exit exit => new ExitStatement(ParseExpression(exit.Expression)) { Span = exit.Span },
            Print exit => new PrintStatement(ParseExpression(exit.Expression)) { Span = exit.Span },
            For forStatement => new ForLoop(ParseIdentifier(forStatement.Identifier), ParseExpression(forStatement.Start), ParseExpression(forStatement.End), ParseScope(forStatement.Scope)) { Span = forStatement.Span },
            While whileStatement => new WhileLoop(ParseExpression(whileStatement.Condition), ParseScope(whileStatement.Scope)) { Span = whileStatement.Span },
            FunctionDeclarationWithParameters functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), ParseParameterList(functionDeclaration.Parameters), ParseScope(functionDeclaration.Scope)) { Span = functionDeclaration.Span },
            VoidFunctionDeclarationWithParameters voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), ParseParameterList(voidFunctionDeclaration.Parameters), ParseScope(voidFunctionDeclaration.Scope)) { Span = voidFunctionDeclaration.Span },
            OLangGrammar.ParseTree.Stmt.FunctionDeclaration functionDeclaration => new FunctionDeclaration(ParseType(functionDeclaration.Type), ParseIdentifier(functionDeclaration.Identifier), [], ParseScope(functionDeclaration.Scope)) { Span = functionDeclaration.Span },
            VoidFunctionDeclaration voidFunctionDeclaration => new FunctionDeclaration(null, ParseIdentifier(voidFunctionDeclaration.Identifier), [], ParseScope(voidFunctionDeclaration.Scope)) { Span = voidFunctionDeclaration.Span },
            If ifStatement => new IfStatement(ParseExpression(ifStatement.Condition), ParseScope(ifStatement.Scope), null) { Span = ifStatement.Span },
            IfWithElse ifStatementWithElse => new IfStatement(ParseExpression(ifStatementWithElse.Condition), ParseScope(ifStatementWithElse.Scope), ParseScope(ifStatementWithElse.ElseBlock)) { Span = ifStatementWithElse.Span },
            Invocation invocation => ParseFunctionInvocation(invocation.InvocationNode),
            MethodInvocationStatement methodInvocation => ParseMethodInvocation(methodInvocation.InvocationNode),
            OLangGrammar.ParseTree.Stmt.Return returnStatement => new Return(null) { Span = returnStatement.Span },
            ReturnValue returnValueStatement => new Return(ParseExpression(returnValueStatement.Expression)) { Span = returnValueStatement.Span },
            ScopeStatement scopeStatement => ParseScope(scopeStatement.Scope),
            _ => throw errorHelper.UnknownVariant("statement", statement.GetType())
        };
    }

    protected VariableAssignment ParseAssignment(Assignment assignment)
    {
        var identifier = ParseIdentifier(assignment.Identifier);
        var parsedExpression = ParseExpression(assignment.Expression);
        var value = assignment.Operator switch
        {
            Equals _ => parsedExpression,
            PlusEquals => new Add(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            MinusEquals => new Subtract(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            TimesEquals => new Multiply(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            DivideEquals => new Divide(new VariableAccess(identifier) { Span = assignment.Identifier.Span }, parsedExpression),
            _ => throw errorHelper.UnknownVariant("assignment operator", assignment.Operator.GetType())
        };

        value.Span = assignment.Expression.Span;
        return new VariableAssignment(ParseIdentifier(assignment.Identifier), value) { Span = assignment.Span };
    }

    private ArrayAssignment ParseArrayAssignment(ArrayAssignmentStatement assignmentStatement)
    {
        var array = ParseTerm(assignmentStatement.Array);
        var index = ParseExpression(assignmentStatement.Index);
        var declaredValue = ParseExpression(assignmentStatement.Value);
        var value = assignmentStatement.AssignmentOperator switch
        {
            Equals _ => declaredValue,
            PlusEquals => new Add(new ArrayAccess(array, index) { Span = assignmentStatement.Span }, declaredValue),
            MinusEquals => new Subtract(new ArrayAccess(array, index) { Span = assignmentStatement.Span }, declaredValue),
            TimesEquals => new Multiply(new ArrayAccess(array, index) { Span = assignmentStatement.Span }, declaredValue),
            DivideEquals => new Divide(new ArrayAccess(array, index) { Span = assignmentStatement.Span }, declaredValue),
            _ => throw errorHelper.UnknownVariant("assignment operator", assignmentStatement.AssignmentOperator.GetType())
        };
        return new ArrayAssignment(array, index, value) { Span = assignmentStatement.Span};
    }
    
    private FieldAssignment ParseFieldAssignment(FieldAssignmentStatement assignmentStatement)
    {
        var target = ParseTerm(assignmentStatement.Target);
        var identifier = ParseIdentifier(assignmentStatement.Identifier);
        var declaredValue = ParseExpression(assignmentStatement.Value);
        var value = assignmentStatement.AssignmentOperator switch
        {
            Equals _ => declaredValue,
            PlusEquals => new Add(new FieldAccess(target, identifier) { Span = assignmentStatement.Span }, declaredValue),
            MinusEquals => new Subtract(new FieldAccess(target, identifier) { Span = assignmentStatement.Span }, declaredValue),
            TimesEquals => new Multiply(new FieldAccess(target, identifier) { Span = assignmentStatement.Span }, declaredValue),
            DivideEquals => new Divide(new FieldAccess(target, identifier) { Span = assignmentStatement.Span }, declaredValue),
            _ => throw errorHelper.UnknownVariant("assignment operator", assignmentStatement.AssignmentOperator.GetType())
        };
        return new FieldAssignment(target, ParseIdentifier(assignmentStatement.Identifier), value) { Span = assignmentStatement.Span};
    }

    protected Scope ParseScope(IScopeNode scope)
    {
        return scope switch
        {
            EmptyScope emptyScope => new Scope([]) { Span = emptyScope.Span },
            ScopeNode scopeNode => new Scope(ParseStatementList(scopeNode.StmtList)) { Span = scopeNode.Span },
            _ => throw errorHelper.UnknownVariant("scope", scope.GetType())
        };
    }

    private IExpression ParseExpression(OLangGrammar.ParseTree.Expression.IExpression expression)
    {
        return expression switch
        {
            Expression orExpression => new Or(ParseExpression(orExpression.Lhs), ParseAndExpression(orExpression.Rhs)) { Span = orExpression.Span },
            NonExpression andExpression => ParseAndExpression(andExpression.Expression),
            _ => throw errorHelper.UnknownVariant("expression", expression.GetType())
        };
    }
    
    private IExpression ParseAndExpression(IAndExpression expression)
    {
        return expression switch
        {
            AndExpression and => new And(ParseAndExpression(and.Lhs), ParseEqualityExpression(and.Rhs)) { Span = and.Span },
            NonAnd nonAnd => ParseEqualityExpression(nonAnd.Expression),
            _ => throw errorHelper.UnknownVariant("and expression", expression.GetType())
        };
    }

    private IExpression ParseEqualityExpression(IEqualityExpression expression)
    {
        return expression switch
        {
            DoubleEquals doubleEquals => new AreEqual(ParseEqualityExpression(doubleEquals.Lhs), ParseGreaterExpression(doubleEquals.Rhs)) { Span = doubleEquals.Span },
            OLangGrammar.ParseTree.EqualityExpression.NotEqual notEqual => new NotEqual(ParseEqualityExpression(notEqual.Lhs), ParseGreaterExpression(notEqual.Rhs)) { Span = notEqual.Span },
            NonEquality nonEquality => ParseGreaterExpression(nonEquality.Expression),
            _ => throw errorHelper.UnknownVariant("equality expression", expression.GetType())
        };
    }

    private IExpression ParseGreaterExpression(IGreaterExpression expression)
    {
        return expression switch
        {
            Greater greater => new GreaterThan(ParseGreaterExpression(greater.Lhs), ParseAddExpression(greater.Rhs)) { Span = greater.Span },
            OLangGrammar.ParseTree.GreaterExpression.GreaterOrEqual greaterOrEqual => new GreaterOrEqual(ParseGreaterExpression(greaterOrEqual.Lhs), ParseAddExpression(greaterOrEqual.Rhs)) { Span = greaterOrEqual.Span },
            Less less => new LessThan(ParseGreaterExpression(less.Lhs), ParseAddExpression(less.Rhs)) { Span = less.Span },
            LessOrEqual lessOrEqual => new LessThanOrEqual(ParseGreaterExpression(lessOrEqual.Lhs), ParseAddExpression(lessOrEqual.Rhs)) { Span = lessOrEqual.Span },
            NonGreaterExpression nonGreaterExpression => ParseAddExpression(nonGreaterExpression.Expression),
            _ => throw errorHelper.UnknownVariant("greater expression", expression.GetType())
        };
    }

    private IExpression ParseAddExpression(IAddExpression expression)
    {
        return expression switch
        {
            Plus plus => new Add(ParseAddExpression(plus.Lhs), ParseMultExpression(plus.Rhs)) { Span = plus.Span },
            Minus minus => new Subtract(ParseAddExpression(minus.Lhs), ParseMultExpression(minus.Rhs)) { Span = minus.Span },
            NonAddExpression nonAdd => ParseMultExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("add expression", expression.GetType())
        };
    }

    private IExpression ParseMultExpression(IMultExpression expression)
    {
        return expression switch
        {
            Mult mu => new Multiply(ParseMultExpression(mu.Lhs), ParseUnaryExpression(mu.Rhs)) { Span = mu.Span },
            Div div => new Divide(ParseMultExpression(div.Lhs), ParseUnaryExpression(div.Rhs)) { Span = div.Span },
            NonMultExpression nonAdd => ParseUnaryExpression(nonAdd.Expression),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    private IExpression ParseUnaryExpression(IUnaryExpression expression)
    {
        return expression switch
        {
            OLangGrammar.ParseTree.UnaryExpression.Not not => new Not(ParseTerm(not.Term)) { Span = not.Span },
            Negation negate => new Negate(ParseTerm(negate.Term)) { Span = negate.Span },
            NonUnaryExpression nonUnary => ParseTerm(nonUnary.Term),
            _ => throw errorHelper.UnknownVariant("mult expression", expression.GetType())
        };
    }

    protected IExpression ParseTerm(ITerm term)
    {
        return term switch
        {
            OLangGrammar.ParseTree.Term.BoolLiteral boolLiteral => new BoolLiteral(boolLiteral.Value.Value) { Span = boolLiteral.Span },
            OLangGrammar.ParseTree.Term.IntLiteral intLiteral => new IntLiteral(intLiteral.Value.Value) { Span = intLiteral.Span },
            OLangGrammar.ParseTree.Term.FloatLiteral floatLiteral => new FloatLiteral(floatLiteral.Value.Value) { Span = floatLiteral.Span },
            OLangGrammar.ParseTree.Term.StringLiteral stringLiteral => new StringLiteral(stringLiteral.Value.Value) { Span = stringLiteral.Span },
            FunctionInvocationTerm functionInvocationTerm => ParseFunctionInvocation(functionInvocationTerm.InvocationNode),
            MethodInvocationTerm methodInvocationTerm => ParseMethodInvocation(methodInvocationTerm.InvocationNode),
            IdentifierTerm identifierTerm => new VariableAccess(ParseIdentifier(identifierTerm.Identifier)) { Span = identifierTerm.Span },
            Paren paren => ParseExpression(paren.Expression),
            CastTerm castTerm => new Cast(ParseType(castTerm.Type), ParseExpression(castTerm.Expression)),
            ClassInstantiationTerm instantiationTerm => new Instantiation(ParseIdentifier(instantiationTerm.Identifier), []) { Span = instantiationTerm.Span },
            ClassInstantiationTermWithInitializers instantiationTerm => new Instantiation(ParseIdentifier(instantiationTerm.Identifier), ParseInitializerList(instantiationTerm.InitializationList)) { Span = instantiationTerm.Span },
            ArrayInstantiationTerm instantiationTerm => new ArrayInstantiation(new ArrayType(ParseType(instantiationTerm.Type)), ParseExpression(instantiationTerm.Size)),
            CustomTypeArrayInstantiationTerm instantiationTerm => new ArrayInstantiation(new ArrayType(new CustomType(ParseIdentifier(instantiationTerm.Type))), ParseExpression(instantiationTerm.Size)),
            ArrayAccessTerm arrayAccessTerm => new ArrayAccess(ParseTerm(arrayAccessTerm.ArrayExpression), ParseExpression(arrayAccessTerm.Index)),
            OLangGrammar.ParseTree.Term.FieldAccess fieldAccess => new FieldAccess(ParseTerm(fieldAccess.Term), ParseIdentifier(fieldAccess.Identifier)),
            EnumVariantInstantiation enumVariantInstantiation => new EnumInstantiation(ParseIdentifier(enumVariantInstantiation.EnumName), ParseIdentifier(enumVariantInstantiation.VariantName), []),
            EnumVariantInstantiationWithArguments enumVariantInstantiation => new EnumInstantiation(ParseIdentifier(enumVariantInstantiation.EnumName), ParseIdentifier(enumVariantInstantiation.VariantName), ParseArgumentList(enumVariantInstantiation.ArgumentList)),
            _ => throw errorHelper.UnknownVariant("term", term.GetType())
        };
    }

    private Dictionary<string, IExpression> ParseInitializerList(IInitializationList initializationList)
    {
        var list = ParseList(
            initializationList,
            x => x switch
            {
                ContinuedInitializationList continuedInitializationList => (continuedInitializationList.FieldInitialization, continuedInitializationList.InitializationList),
                SingleFieldInitializationList singleFieldInitializationList => (singleFieldInitializationList.fieldInitialization, null),
                _ => throw errorHelper.UnknownVariant("initializer list", x.GetType())
            },
            ParseFieldInitialization
        );

        var dict = new Dictionary<string, IExpression>();

        foreach (var initialization in list)
        {
            if (dict.ContainsKey(initialization.name))
            {
                throw errorHelper.ShowErrorMessage($"Member `{initialization.name}` was initialized twice. Each member must be initialized exactly once.", initialization.span);
            }

            dict[initialization.name] = initialization.value;
        }

        return dict;
    }

    private (string name, IExpression value, SourceSpan span) ParseFieldInitialization(IFieldInitialization fieldInitialization)
    {
        return fieldInitialization switch
        {
            FieldInitialization fieldInitialization1 => (ParseIdentifier(fieldInitialization1.FieldName), ParseExpression(fieldInitialization1.Value), fieldInitialization1.Span),
            _ => throw errorHelper.UnknownVariant("field initialization", fieldInitialization.GetType())
        };
    }

    private IVariableType ParseType(IType type)
    {
        return type switch
        {
            BoolType boolType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool) { Span = boolType.Span },
            IntType intType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int) { Span = intType.Span },
            FloatType floatType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.Float) { Span = floatType.Span },
            StringType stringType => new PrimitiveVariableType(PrimitiveVariableTypeEnum.String) { Span = stringType.Span },
            NonPrimitiveType customType => new CustomType(ParseIdentifier(customType.Identifier)) { Span = customType.Span },
            OLangGrammar.ParseTree.Type.ArrayType arrayType => new ArrayType(ParseType(arrayType.InnerType)),
            CustomArrayType arrayType => new ArrayType(new CustomType(ParseIdentifier(arrayType.InnerType))),
            _ => throw errorHelper.UnknownVariant("type", type.GetType())
        };
    }
    
    private string ParseIdentifier(IdentifierToken identifier)
    {
        return identifier.Identifier;
    }

    private List<ParameterNode> ParseParameterList(IParameterListNode parameterList)
    {
        return ParseList<IParameterListNode, Parameter, ParameterNode>(
            parameterList,
            x => x switch
            {
                ContinuedParameterList value => (value, value.ParameterList),
                Parameter parameter => (parameter, null),
                _ => throw errorHelper.UnknownVariant("parameter list", x.GetType())
            },
            ParseParameter
        );
    }

    private ParameterNode ParseParameter(Parameter parameter)
    {
        return new ParameterNode(ParseIdentifier(parameter.Identifier), ParseType(parameter.Type)) { Span = parameter.Span };
    }

    private FunctionInvocation ParseFunctionInvocation(IFunctionInvocation functionInvocation)
    {
        List<IExpression> arguments;
        IdentifierToken identifier;
        switch (functionInvocation)
        {
            case OLangGrammar.ParseTree.FunctionInvocation.FunctionInvocation invocation:
                arguments = new List<IExpression>();
                identifier = invocation.Identifier;
                break;
            case FunctionInvocationWithArguments invocationWithArguments:
                arguments = ParseArgumentList(invocationWithArguments.Arguments);
                identifier = invocationWithArguments.Identifier;
                break;
            default:
                throw errorHelper.UnknownVariant("function invocation", functionInvocation.GetType());
        }

        return new FunctionInvocation(ParseIdentifier(identifier), arguments) { Span = functionInvocation.Span };
    }
    
    private MethodInvocation ParseMethodInvocation(IMethodInvocation methodInvocation)
    {
        IExpression obj;
        List<IExpression> arguments;
        IdentifierToken identifier;
        switch (methodInvocation)
        {
            case OLangGrammar.ParseTree.MethodInvocation.MethodInvocation invocation:
                obj = ParseTerm(invocation.Term);
                arguments = new List<IExpression>();
                identifier = invocation.Identifier;
                break;
            case MethodInvocationWithArguments invocationWithArguments:
                obj = ParseTerm(invocationWithArguments.Term);
                arguments = ParseArgumentList(invocationWithArguments.Arguments);
                identifier = invocationWithArguments.Identifier;
                break;
            default:
                throw errorHelper.UnknownVariant("function invocation", methodInvocation.GetType());
        }

        return new MethodInvocation(obj, ParseIdentifier(identifier), arguments) { Span = methodInvocation.Span };
    }
    
    private List<IExpression> ParseArgumentList(IArgumentList argumentList)
    {
        return ParseList(
            argumentList,
            x => x switch
            {
                ContinuedArgumentList value => (value.Expression, value.ArgumentList),
                ExpressionArgumentList parameter => (parameter.Expression, null),
                _ => throw errorHelper.UnknownVariant("parameter list", x.GetType())
            },
            ParseExpression
        );
    }
}