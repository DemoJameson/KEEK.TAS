using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using UnityEngine;

namespace KEEK.TAS.Commands;

public static class PauseCommand {
    private static bool pause;
    private static float lastTimeScale;

    [TasCommand("Pause")]
    private static void Pause(string[] args) {
        pause = true;
        lastTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public static void Resume() {
        if (pause) {
            Time.timeScale = lastTimeScale;
            pause = false;
        }
    }
}