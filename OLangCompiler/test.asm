global _start

section .text
_start:
    push 1
    push 38
    push 5
    push QWORD [rsp + 8]
    push QWORD [rsp + 24]
    pop rdi
    pop rax
    add rax, rdi
    push rax
        push QWORD [rsp + 8]
    pop rdi
    pop rax
    add rax, rdi
    push rax
        pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
