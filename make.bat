@echo off
CommentAssembly CLANGUAGE .
C:\SysGCC\raspberry\bin\arm-linux-gnueabihf-gcc -Wall -Wextra -static -static-libgcc -static-libstdc++ -o multiserial multiserial.c -lm -lpthread -lrt
rem C:\SysGCC\raspberry\bin\arm-linux-gnueabihf-gcc -Wall -Wextra -static -static-libgcc -static-libstdc++ -o testexe test.c 

