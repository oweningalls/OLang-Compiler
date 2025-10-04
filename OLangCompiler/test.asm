global _start

section .text
_start:
    mov rax, 30
    push rax
    mov rax, 20
    push rax
    mov rax, 10
    push rax
    mov rax, 23
    push rax
    pop rax
    mov [rsp + 16], rax
    mov rdi, 0
    mov rax, 60
    syscall
