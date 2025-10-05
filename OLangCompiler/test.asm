global _start

section .text
_start:
    push 1
    push QWORD [rsp + 0]
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
