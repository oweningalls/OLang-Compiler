global _start

section .text
_start:
    push 1
    push QWORD [rsp + 0]
    mov rdi, 0
    mov rax, 60
    syscall
