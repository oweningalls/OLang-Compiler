global _start

section .text
_start:
    push 60
    push 20
    push 10
    push QWORD [rsp + 16]
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
