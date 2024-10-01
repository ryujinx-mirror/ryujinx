namespace ARMeilleure.State
{
    enum Aarch32Mode
    {
        User = 0b10000,
        Fiq = 0b10001,
        Irq = 0b10010,
        Supervisor = 0b10011,
        Monitor = 0b10110,
        Abort = 0b10111,
        Hypervisor = 0b11010,
        Undefined = 0b11011,
        System = 0b11111,
    }
}
