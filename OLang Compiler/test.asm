global _start

section .text
_start:
    mov rax, 60     ; syscall number (exit)
    mov rdi, 123    ; exit code 0
    syscall
