global _start

section .text
_start:
    push 1
    push 1
    mov rdi, 0
    mov rax, 60
    syscall
