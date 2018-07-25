#include "stdafx.h"
#define SIMULSTATE_COUNT (5)

typedef struct {
	int us;
	int state;
} SIMULSTATE, * PSIMULSTATE;

int states_count = 0;
int states_alloc = 0;
PSIMULSTATE pstates = NULL;

PSIMULSTATE add(int us, int state) 
{
	PSIMULSTATE result;

	if (pstates == NULL) {
		pstates = (PSIMULSTATE)calloc(SIMULSTATE_COUNT, sizeof(SIMULSTATE));
		states_alloc = SIMULSTATE_COUNT;
	} else {
		if (states_count >= states_alloc) {
			states_alloc += SIMULSTATE_COUNT;
			pstates = (PSIMULSTATE)realloc(pstates, states_alloc * sizeof(SIMULSTATE));
		}
	}

	result = &pstates[states_count++];
	result->us = us;
	result->state = state;

	return result;
}

int current_state = -1;

unsigned get_IO() 
{
	return current_state;
}

void enque(int line_no, int received_char) 
{
	received_char = 0xFF & (received_char >> 1);
	printf("%d,%02x\n", line_no, received_char);
}

void invocation_error(char * msg)
{
	printf("Error %s\n", msg);
	printf("Synopsis simulator <simulatonfile>\n");
	exit(1);
}

void simulate()
{
	int usSimul = 0;
	for (int i = 0; i < states_count; i++) {
		int next_us = pstates[i].us;
		int next_state = pstates[i].state;
		while (usSimul < next_us) {
			usSimul += MICROSECONDS_PER_INTERRUPT;
			interrupt_service_routine();
		}
		current_state = next_state;
		usSimul += MICROSECONDS_PER_INTERRUPT;
		interrupt_service_routine();
	}

	for (int i = 0; i < 3; i++) {
		interrupt_service_routine();
	}
}

void load_test_pattern_from_file(FILE * fin)
{
	char line[256];
	while (fgets(line, sizeof(line), fin)) {
		int us;
		int state;
		sscanf(line, "%d %08x", &us, &state);
		add(us, state);
	}
}

void load_test_pattern(char * test_pattern_filename)
{
	FILE * fin = fopen(test_pattern_filename, "rt");
	load_test_pattern_from_file(fin);
	fclose(fin);
}

void foreach_test_pattern(void(*visitor)(int, int))
{
	for (int i = 0; i < states_count; i++) {
		if (visitor != NULL) {
			visitor(pstates[i].us, pstates[i].state);
		}
	}
}

void print_state(int us, int state)
{
	printf("%d | %x\n", us, state);
}

int main(int argc, char ** argv)
{
	if (argc < 2) {
		puts("Load stimulus from stdin");
		load_test_pattern_from_file(stdin);
	} else {
		load_test_pattern(argv[1]);
	}

	init();
	puts("Start simulation");
	simulate();
	puts("End simulation");
}
