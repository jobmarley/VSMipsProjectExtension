
extern "C" int start();

__attribute__((naked))
extern "C" int main()
{
	asm("li $v0, 0x2000");
	asm("move $sp, $v0");
	asm("j start");
}
unsigned int* leds = (unsigned int*)0x80000004;

__attribute__((naked))
unsigned int get_cycle_count()
{
	asm("mfc0 $v0, $9, 0b000");
	asm("jr $ra");
}
__attribute__((naked))
void debugbreak()
{
	asm("sdbbp");
}

extern "C" int start()
{
	unsigned int last_count = 0;
	last_count = get_cycle_count();

	unsigned int toggle = 0;
	*leds = 0;
	while (true)
	{
		unsigned int now_count = get_cycle_count();
		unsigned int elapsed = now_count - last_count;
		if (now_count < last_count)
		{
			last_count = now_count;
		}
		else if (elapsed > 50000000)
		{
			if (toggle == 0)
			{
				toggle = 1;
				*leds = 1;
				debugbreak();
			}
			else
			{
				toggle = 0;
				*leds = 0;
			}
			last_count = now_count;
		}
	}
	return 55;
}


