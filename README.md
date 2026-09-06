$$
\begin{aligned} \\
[\mathrm{Prog}]
&\to \text{class} \{ [\mathrm{ClassMemberList}] \} \\
\\
[\mathrm{ClassMemberList}]
&\to [\mathrm{ClassMember}] \\
&\mid [\mathrm{ClassMember}] [\mathrm{ClassMemberList}] \\
[\mathrm{ClassMember}]
&\to [\mathrm{Type}]\ \mathrm{ident}\text{(}[\mathrm{ParameterList}]\text{)}\ [\mathrm{Scope}] \\
&\mid \text{void ident}\text{(}[\mathrm{ParameterList}]\text{)}\ [\mathrm{Scope}] \\
&\mid [\mathrm{Type}]\ \mathrm{ident}\text{()}\ [\mathrm{Scope}] \\
&\mid \text{void ident}\text{()}\ [\mathrm{Scope}] \\
[\mathrm{StmtList}]
&\to [\mathrm{Stmt}] \\
&\mid [\mathrm{Stmt}] [\mathrm{StmtList}] \\
[\mathrm{Stmt}]
&\to \text{exit} [\mathrm{Expression}]; \\
&\to \text{print} [\mathrm{Expression}]; \\
&\mid \text{let ident} = [\mathrm{Expression}]; \\
&\mid [\mathrm{Type}] \mathrm{ident} = [\mathrm{Expression}]; \\
&\mid \mathrm{ident} [\mathrm{AssignmentOperator}] [\mathrm{Expression}]; \\
&\mid [\mathrm{Scope}] \\
&\mid \text{if}\ [\mathrm{Expression}]\ [\mathrm{Scope}]\ \text{else}\ [\mathrm{Scope}] \\
&\mid \text{if}\ [\mathrm{Expression}]\ [\mathrm{Scope}] \\
&\mid \text{while}\ [\mathrm{Expression}]\ [\mathrm{Scope}] \\
&\mid \text{for}\ \mathrm{ident}\ \text{in}\ [\mathrm{Expression}]..[\mathrm{Expression}]\ [\mathrm{Scope}] \\
&\mid [\mathrm{Type}]\ \mathrm{ident}\text{(}[\mathrm{ParameterList}]\text{)}\ [\mathrm{Scope}] \\
&\mid \text{void ident}\text{(}[\mathrm{ParameterList}]\text{)}\ [\mathrm{Scope}] \\
&\mid [\mathrm{Type}]\ \mathrm{ident}\text{()}\ [\mathrm{Scope}] \\
&\mid \text{void ident}\text{()}\ [\mathrm{Scope}] \\
&\mid [\mathrm{Expression}].\mathrm{ident}\text{(}[\mathrm{ArgumentList}]\text{)} \\
&\mid [\mathrm{Expression}].\mathrm{ident}\text{(}\text{)} \\
&\mid [\mathrm{FunctionInvocation}]; \\
&\mid [\mathrm{MethodInvocation}]; \\
&\mid \text{return}; \\
&\mid \text{return}\ [\mathrm{Expression}]; \\
[\mathrm{ParameterList}]
&\to [\mathrm{Type}]\ \mathrm{ident},\ [\mathrm{ParameterList}] \\
&\mid [\mathrm{Type}]\ \mathrm{ident} \\
[\mathrm{ArgumentList}]
&\to [\mathrm{Expression}], \ [\mathrm{ArgumentList}] \\
&\mid [\mathrm{Expression}] \\
[\mathrm{AssignmentOperator}]
&\to \text{=} \\
&\mid \text{+=} \\
&\mid \text{-=} \\
&\mid \text{*=} \\
&\mid \text{/=} \\
[\mathrm{Scope}]
&\to \{ [\mathrm{StmtList}] \} \\
[\mathrm{Type}]
&\to \text{int} \\
&\mid \text{bool} \\
&\mid \text{float} \\
&\mid \text{string} \\
[\mathrm{Expression}]
&\to [\mathrm{Expression}] \text{||} [\mathrm{AndExpression}] \\
&\mid [\mathrm{AndExpression}] \\
[\mathrm{AndExpression}]
&\to [\mathrm{AndExpression}] \\&\\& [\mathrm{EqualityExpression}] \\
&\mid [\mathrm{EqualityExpression}] \\
[\mathrm{EqualityExpression}]
&\to [\mathrm{EqualityExpression}] == [\mathrm{GreaterExpression}] \\
&\mid [\mathrm{EqualityExpression}] != [\mathrm{GreaterExpression}] \\
&\mid [\mathrm{GreaterExpression}] \\
[\mathrm{GreaterExpression}]
&\to [\mathrm{GreaterExpression}] > [\mathrm{AddExpression}] \\
&\mid [\mathrm{GreaterExpression}] >= [\mathrm{AddExpression}] \\
&\mid [\mathrm{GreaterExpression}] < [\mathrm{AddExpression}] \\
&\mid [\mathrm{GreaterExpression}] <= [\mathrm{AddExpression}] \\
&\mid [\mathrm{AddExpression}] \\
[\mathrm{AddExpression}]
&\to [\mathrm{AddExpression}] + [\mathrm{MultExpression}] \\
&\mid [\mathrm{AndExpression}] - [\mathrm{MultExpression}] \\
&\mid [\mathrm{MultExpression}] \\
[\mathrm{MultExpression}]
&\to [\mathrm{MultExpression}] * [\mathrm{UnaryExpression}] \\
&\mid [\mathrm{MultExpression}] / [\mathrm{UnaryExpression}] \\
&\mid [\mathrm{UnaryExpression}] \\
[\mathrm{UnaryExpression}]
&\to ![\mathrm{Term}] \\
&\to -[\mathrm{Term}] \\
&\mid [\mathrm{Term}] \\
[\mathrm{Term}]
&\to \text{int\\_lit} \\
&\mid \text{float\\_lit} \\
&\mid \text{bool\\_lit} \\
&\mid \text{string\\_lit} \\
&\mid \text{ident} \\
&\mid \text{(}[\mathrm{Expression}]\text{)} \\
&\mid [\mathrm{FunctionInvocation}] \\
&\mid [\mathrm{MethodInvocation}] \\
[\mathrm{FunctionInvocation}]
&\to \mathrm{ident}\text{(}[\mathrm{ArgumentList}]\text{)} \\
&\mid \mathrm{ident}\text{()} \\
[\mathrm{MethodInvocation}]
&\to [\mathrm{Term}].\mathrm{ident}\text{(}[\mathrm{ArgumentList}]\text{)} \\
&\mid [\mathrm{Term}].\mathrm{ident}\text{(}\text{)} \\
\end{aligned}
$$
