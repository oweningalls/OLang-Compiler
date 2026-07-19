

$$
\begin{align}
    [\text{Prog}] &\to \text{[StmtList]} \\
	[\text{StmtList}] &\to
    \begin{cases}
		\epsilon \\
		\text{[Stmt] [StmtList]} \\
    \end{cases} \\
    [\text{Stmt}] &\to
    \begin{cases}
	    \text{exit [Expression];} \\
		\text{[VariableType] ident = [Expression];} \\
		\text{ident [AssignmentOperator] [Expression];} \\
		\text{[Scope]} \\
		\text{if [Expression] [Scope] [ElseBlock]} \\
		\text{while [Expression] [Scope]} \\
		\text{for ident in [Expression]..[Expression] [Scope]} \\
		\text{[FunctionType] ident([ParameterList]) [Scope]} \\
        \text{[FunctionInvocation];} \\
	    \text{return;} \\
	    \text{return [Expression];} \\
    \end{cases} \\
	[\text{ElseBlock}] &\to
    \begin{cases}
		\epsilon \\
		\text{else [Scope]} \\
    \end{cases} \\
	[\text{ParameterList}] &\to
    \begin{cases}
		\epsilon \\
		\text{[Type] ident, [ParameterList]} \\
		\text{[Type] ident} \\
    \end{cases} \\
	[\text{ArgumentList}] &\to
    \begin{cases}
		\epsilon \\
		\text{[Expression], [ArgumentList]} \\
		\text{[Expression]} \\
    \end{cases} \\
    [\text{AssignmentOperator}] &\to
    \begin{cases}
	    \text{=} \\
		\text{+=} \\
		\text{-=} \\
		\text{*=} \\
		\text{/=} \\
    \end{cases} \\
    [\text{Scope}] &\to \text{\{[StmtList]\}} \\
    [\text{FunctionType}] &\to
    \begin{cases}
	    \text{void} \\
	    \text{[Type]} \\
    \end{cases} \\
    [\text{VariableType}] &\to
    \begin{cases}
	    \text{let} \\
	    \text{[Type]} \\
    \end{cases} \\
    [\text{Type}] &\to
    \begin{cases}
		\text{int} \\
		\text{bool} \\
		\text{float} \\
    \end{cases} \\
    [\text{Expression}] &\to
    \begin{cases}
        \text{[Expression] || [AndExpression]} \\
        \text{[AndExpression]} \\
    \end{cases} \\
    [\text{AndExpression}] &\to
    \begin{cases}
        \text{[AndExpression] \&\& [EqualityExpression]} \\
        \text{[EqualityExpression]} \\
    \end{cases} \\
    [\text{EqualityExpression}] &\to
    \begin{cases}
        \text{[EqualityExpression] == [GreaterExpression]} \\
        \text{[EqualityExpression] != [GreaterExpression]} \\
        \text{[GreaterExpression]} \\
    \end{cases} \\
    [\text{GreaterExpression}] &\to
    \begin{cases}
        \text{[GreaterExpression] > [AddExpression]} \\
        \text{[GreaterExpression] >= [AddExpression]} \\
        \text{[GreaterExpression] < [AddExpression]} \\
        \text{[GreaterExpression] <= [AddExpression]} \\
        \text{[AddExpression]} \\
    \end{cases} \\
    [\text{AddExpression}] &\to
    \begin{cases}
        \text{[AddExpression] + [MultExpression]} \\
        \text{[AndExpression] - [MultExpression]} \\
        \text{[MultExpression]} \\
    \end{cases} \\
    [\text{MultExpression}] &\to
    \begin{cases}
        \text{[MultExpression] * [UnaryExpression]} \\
        \text{[MultExpression] / [UnaryExpression]} \\
        \text{[UnaryExpression]} \\
    \end{cases} \\
    [\text{UnaryExpression}] &\to
    \begin{cases}
        \text{![Term]} \\
        \text{[Term]} \\
    \end{cases} \\
    [\text{Term}] &\to
    \begin{cases}
        \text{int\_lit} \\
        \text{float\_lit} \\
        \text{bool\_lit} \\
        \text{ident} \\
        \text{([Expression])} \\
        \text{[FunctionInvocation]} \\
    \end{cases} \\
	[\text{FunctionInvocation}] &\to \text{ident([ArgumentList])} \\
\end{align}
$$