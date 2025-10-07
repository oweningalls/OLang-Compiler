global _start

section .text
_start:
    push 3
    push 0
    pop rax
    cmp rax, 0
    je label0
    push 4
    pop rax
    mov [rsp + 0], rax
    add rsp, 0
label0:
    push QWORD [rsp + 0]
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
