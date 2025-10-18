global _start

section .text
_start:
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push 13
    pop rdi
    call fibStart0
    pop r11
    pop r10
    pop r9
    pop r8
    pop rdi
    pop rsi
    pop rdx
    pop rcx

    push rax
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall

fibStart0:
    push rbx
    push rbp
    push r12
    push r13
    push r14
    push r15
    push rdi
    push QWORD [rsp + 0]
    push 0
    pop rdi
    pop rax
    cmp rax, rdi
    sete al
    movzx rax, al
    push rax
    pop rax
    cmp rax, 0
    je ifEnd2
    push 0
    pop rax
    jmp fibReturn1
    add rsp, 0
ifEnd2:
    push QWORD [rsp + 0]
    push 1
    pop rdi
    pop rax
    cmp rax, rdi
    sete al
    movzx rax, al
    push rax
    pop rax
    cmp rax, 0
    je ifEnd3
    push 1
    pop rax
    jmp fibReturn1
    add rsp, 0
ifEnd3:
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push QWORD [rsp + 64]
    push 1
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    pop rdi
    call fibStart0
    pop r11
    pop r10
    pop r9
    pop r8
    pop rdi
    pop rsi
    pop rdx
    pop rcx

    push rax
    push rcx
    push rdx
    push rsi
    push rdi
    push r8
    push r9
    push r10
    push r11
    push QWORD [rsp + 72]
    push 2
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    pop rdi
    call fibStart0
    pop r11
    pop r10
    pop r9
    pop r8
    pop rdi
    pop rsi
    pop rdx
    pop rcx

    push rax
    pop rdi
    pop rax
    add rax, rdi
    push rax
    pop rax
    jmp fibReturn1
fibReturn1:
    add rsp, 8
    pop r15
    pop r14
    pop r13
    pop r12
    pop rbp
    pop rbx
    ret
