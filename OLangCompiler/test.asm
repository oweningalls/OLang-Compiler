global _start

section .text
_start:
    push 1
    push 6
    pop rdi
    mov rax, 60
    syscall
   add rsp, 0
   add rsp, 1
    push 1
    mov rdi, 0
    mov rax, 60
    syscall
