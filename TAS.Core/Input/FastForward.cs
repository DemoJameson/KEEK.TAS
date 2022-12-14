using System;

namespace TAS.Core.Input;

public record FastForward {
    public const float DefaultSpeed = 1000f;
    public const float MinSpeed = 1f / 60f;
    public readonly int Frame;
    public readonly int Line;
    public readonly float Speed;

    public FastForward(int frame, string modifiers, int line) {
        Frame = frame;
        Line = line;
        Speed = float.TryParse(modifiers, out float speed) ? speed : DefaultSpeed;
        if (Speed < MinSpeed) {
            Speed = MinSpeed;
        } else if (Speed > 1f) {
            Speed = (int) Math.Round(Speed);
        }
    }

    public override string ToString() {
        return "***" + Speed;
    }
}