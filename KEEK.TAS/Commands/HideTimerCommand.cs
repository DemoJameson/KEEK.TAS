using System;
using TAS;
using TAS.Core.Input.Commands;

namespace KEEK.TAS.Commands; 

public class HideTimerCommand {
    private static bool hide;

    static HideTimerCommand () {
       HookUtils.Hook(typeof(Timer), "Update", TimerUpdate); 
    }

    private static void TimerUpdate(Action<Timer> orig, Timer self) {
        if (hide) {
            self.gameObject.SetActive(false);
            return;
        }

        orig(self);
    }

    [TasCommand("HideTimer")]
    private static void HideTimer(string[] args) {
        hide = true;
    }

    [DisableRun]
    public static void DisableRun() {
        hide = false;
    }
}