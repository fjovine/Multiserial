#ifndef MULTISERIAL_H
#define MULTISERIAL_H
#include <stdio.h>
#include <stdlib.h>

#define BITS_PER_CHARACTER (8)
#define MICROSECONDS_PER_INTERRUPT (5)
#define MICROSECONDS_PER_SECOND (1000000)
#define BAUD (9600)
#define INTERRUPTS_PER_HALFBIT (MICROSECONDS_PER_SECOND / (BAUD * MICROSECONDS_PER_INTERRUPT * 2))
#define INTERRUPTS_PER_BIT (2 * INTERRUPTS_PER_HALFBIT)

#define LINES_COUNT (8)
#define INITIAL_MASK (1 << LINES_COUNT)

#define DOWN_TRANSITION(x,y) ((x) & (~y))

unsigned get_IO();
void enque(int line_no, int received_char);

void init();
void interrupt_service_routine();
#endif