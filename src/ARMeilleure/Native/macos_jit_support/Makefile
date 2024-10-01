NAME = libarmeilleure-jitsupport.dylib

all: ${NAME}

${NAME}:
	clang -O3 -dynamiclib support.c -o ${NAME}
clean:
	rm -f ${NAME}
