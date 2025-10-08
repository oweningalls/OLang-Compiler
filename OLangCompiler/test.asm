global _start

section .text
_start:
    push 2
    push 2
    pop rdi
    pop rax
        cmp rax, rdi
    setne al
    movzx rax, al
        push rax
    push QWORD [rsp + 0]
    pop rax
    cmp rax, 0
    je label0
    push 1
    pop rdi
    mov rax, 60
    syscall
    add rsp, 0
label0:
    push 123
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
