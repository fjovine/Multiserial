// <copyright file="multiserial.c" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
//
// This module is part of Multiserial, a piece of software aimed at receiving a high, up to 16, number of serial lines on which serial characters are sent with one start and one stop bit at
// the selected baud rate. Each line is decoded as follows. This diagram shows the levels of the start bit (A), of the eight bits forming the transmitted character (7-0 7 being the most significant bit)
// and of the stop bit (S)

//        A    7    6    5    4    3    2    1    0    S     
// -----  .  -----  .    .   ---------  .  -----  .    .  -----------------
//     -----  .  ----------- .    .  -----  .  ----------  
//     <-------------------active-----------------> 

// DEFINITIONS
// A line is inactive when it is not receiving any serial character. In this condition the serial line is highvideo
// A line is active when it is receiving a character so
//   1. if a starting high to low transition took place and
//   2. the time passed from this transition is less than the duration of 7.5 bits at the curent baud.

#include "multiserial.h"

typedef struct {
    unsigned received_so_far;
    unsigned current_mask;
    unsigned interrupts_to_go;
} LINE_DESCRIPTOR, *PLINE_DESCRIPTOR;

unsigned old_state;
unsigned mask;

// If line[i] is inactive, then counters[i] == 0
LINE_DESCRIPTOR line_status[LINES_COUNT];

// state of the lines sampled during the last interrupt_service_routine
unsigned last_state;

// A bit is set if the corresponding serial line is active
unsigned receiving_mask;

void init()
{
    receiving_mask = ~0;
    last_state = -1;
    for (int i=0; i<LINES_COUNT; i++) {
        PLINE_DESCRIPTOR current = & line_status[i];
        current->received_so_far = 0;
        current->current_mask = 0;
        current->interrupts_to_go = 0;
    }
}

void interrupt_service_routine()
{
    unsigned current_state = get_IO();
    unsigned masked_transition = DOWN_TRANSITION(last_state, current_state) & receiving_mask;
    int bit;
    if (masked_transition != 0) {
        // Something changed
        for (unsigned mask = INITIAL_MASK, bit = LINES_COUNT; mask > 0; mask >>= 1, bit--) {
            if (mask & masked_transition) {
                // Start receving a character;
                receiving_mask &= ~mask;
                line_status[bit].received_so_far = 0;
                line_status[bit].current_mask = 0x200;
                line_status[bit].interrupts_to_go = INTERRUPTS_PER_HALFBIT;
            }
        }
    }
    last_state = current_state;

    unsigned bit_mask = 0x1;
    for (bit =0; bit<LINES_COUNT; bit++, bit_mask <<= 1) {
        PLINE_DESCRIPTOR current_line = &line_status[bit];
        if ((current_line -> current_mask == 0) || (--current_line->interrupts_to_go > 0)) {
            continue;
        }

        if (current_state & bit_mask) {
            current_line->received_so_far |= current_line->current_mask;
        }
        current_line->current_mask >>= 1;
        if (current_line->current_mask) {
            current_line->interrupts_to_go = INTERRUPTS_PER_BIT;
        } else {
            enque(bit, current_line->received_so_far);
            receiving_mask |= (1u << bit);
        }
    }
}
