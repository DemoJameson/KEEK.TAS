using System;

namespace TAS.Shared;

[Flags]
public enum States : byte {
    None = 0,
    Enable = 1 << 0,
    FrameStep = 1 << 1,
    Disable = 1 << 2
}