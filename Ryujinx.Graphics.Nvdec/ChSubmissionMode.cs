namespace Ryujinx.Graphics
{
    enum ChSubmissionMode
    {
        SetClass        = 0,
        Incrementing    = 1,
        NonIncrementing = 2,
        Mask            = 3,
        Immediate       = 4,
        Restart         = 5,
        Gather          = 6
    }
}