#!/bin/bash
# Usage: ./runasm.sh program.asm

if [ -z "$1" ]; then
    echo "Usage: $0 file.asm"
    exit 1
fi

SRC="$1"
BASE="${SRC%.*}"

# Assemble
nasm -f elf64 "$SRC" -o "my_obj/$BASE.o"
if [ $? -ne 0 ]; then
    echo "Assembly failed"
    exit 1
fi

# Link
ld "my_obj/$BASE.o" -o "my_obj/$BASE"
if [ $? -ne 0 ]; then
    echo "Linking failed"
    exit 1
fi

# Run
echo "Running ./$BASE"
my_obj/"$BASE"
echo "Program exited with code $?"
