global _start

section .text
_start:
    push 0
    push 1
    push 1
label0:    push QWORD [rsp + 0]
    push 13
    pop rdi
    pop rax
        cmp rax, rdi
    setne al
    movzx rax, al
        push rax
    pop rax
    cmp rax, 0
    je label1
    push QWORD [rsp + 0]
    push 1
    pop rdi
    pop rax
        add rax, rdi
    push rax
    pop rax
    mov [rsp + 0], rax
    push QWORD [rsp + 8]
    push QWORD [rsp + 16]
    push QWORD [rsp + 32]
    pop rdi
    pop rax
        add rax, rdi
    push rax
    pop rax
    mov [rsp + 16], rax
    push QWORD [rsp + 0]
    pop rax
    mov [rsp + 24], rax
    add rsp, 8
    jmp label0
label1:
    push QWORD [rsp + 8]
    pop rdi
    mov rax, 60
    syscall
    mov rdi, 0
    mov rax, 60
    syscall
