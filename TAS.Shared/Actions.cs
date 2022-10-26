using System;
using System.Collections.Generic;

namespace TAS.Shared;

[Flags]
public enum Actions {
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Jump = 1 << 4,
    Jump2 = 1 << 5,
    Dash = 1 << 6,
    Dash2 = 1 << 7,
    Fire = 1 << 8,
    Pause = 1 << 9,
}

public static class ActionsUtils {
    public static readonly Dictionary<char, Actions> Chars = new() {
        {'L', Actions.Left},
        {'R', Actions.Right},
        // {'U', Actions.Up},
        // {'D', Actions.Down},
        {'J', Actions.Jump},
        {'K', Actions.Jump2},
        {'X', Actions.Dash},
        {'C', Actions.Dash2},
        {'F', Actions.Fire},
        {'P', Actions.Pause}
    };
}