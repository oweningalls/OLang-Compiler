global _start

section .text
_start:
whileBegin0:
    push 1
    push 1
    pop rdi
    pop rax
    cmp rax, rdi
    setne al
    movzx rax, al
    push rax
    pop rax
    cmp rax, 0
    je whileEnd1
    push 35
    pop rdi
    mov rax, 60
    syscall
    add rsp, 0
    jmp whileBegin0
whileEnd1:
    mov rdi, 0
    mov rax, 60
    syscall

