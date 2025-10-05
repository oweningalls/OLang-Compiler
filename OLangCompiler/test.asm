global _start

section .text
_start:
    push 12
    push 3
    push 2
    pop rdi
    pop rax
    sub rax, rdi
    push rax
        pop rdi
    pop rax
    sub rax, rdi
    push rax
        push QWORD [rsp + 0]
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
