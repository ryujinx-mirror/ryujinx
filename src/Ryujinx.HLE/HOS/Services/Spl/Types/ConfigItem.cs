namespace Ryujinx.HLE.HOS.Services.Spl.Types
{
    enum ConfigItem
    {
        // Standard config items.
        DisableProgramVerification = 1,
        DramId = 2,
        SecurityEngineInterruptNumber = 3,
        FuseVersion = 4,
        HardwareType = 5,
        HardwareState = 6,
        IsRecoveryBoot = 7,
        DeviceId = 8,
        BootReason = 9,
        MemoryMode = 10,
        IsDevelopmentFunctionEnabled = 11,
        KernelConfiguration = 12,
        IsChargerHiZModeEnabled = 13,
        QuestState = 14,
        RegulatorType = 15,
        DeviceUniqueKeyGeneration = 16,
        Package2Hash = 17,
    }
}
