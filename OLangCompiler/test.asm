global _start

section .text
_start:
    push 1
    push 3
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
