#include "multiserial.h"
#include "QueueArray.h"
IntervalTimer timer5;

class Multiserial {
private:
    byte hi;
    byte lo;
public:
    Multiserial(int ser_no, int c);
    void print(void);
};

Multiserial::Multiserial(int ser_no, int c)
{
    // HI 1SSS SCCC
    // LO 000C CCCC
    hi = 0x80 | (ser_no << 3) | (c >>5);
    lo = c & 0x1F;
}

void Multiserial::print(void)
{
	Serial.write(hi);
	Serial.write(lo);
}

QueueArray<Multiserial> queue;

void setup() {
  // Per agire in forma GPIO ocorre definire pin per pin alcuni parametri
  PORTC_PCR0 = (1<<8);
  PORTC_PCR1 = (1<<8);
  PORTC_PCR2 = (1<<8);
  PORTC_PCR3 = (1<<8);
  PORTC_PCR4 = (1<<8);
  PORTC_PCR5 = (1<<8);
  PORTC_PCR6 = (1<<8);
  PORTC_PCR7 = (1<<8);
  PORTC_PCR8 = (1<<8);
  PORTC_PCR9 = (1<<8);
  PORTC_PCR10 = (1<<8);
  
  GPIOC_PDDR = 0x3FF; // porta C in output
  timer5.begin(interrupt_service_routine, MICROSECONDS_PER_INTERRUPT);  // blinkLED to run every 0.15 seconds
  init();
  Serial.begin(115200);
  Serial.write("Multiserial ver 0.1\n");
}

void enque(int line_no, int received_char)
{
	Multiserial m = Multiserial(line_no, received_char);
	queue.enqueue(m);
}

void loop() {
  for (int i=0; i<10; i++) {
    enque(1,'A');
  }
	while (!queue.isEmpty()) {
		queue.dequeue().print();
	}
  delay(2000);
}
