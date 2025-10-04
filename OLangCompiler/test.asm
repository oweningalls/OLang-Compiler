global _start

section .text
_start:
    mov rax, 69
    push rax
    mov rax, 72
    push rax
    mov rax, 60
    pop rdi
    syscall