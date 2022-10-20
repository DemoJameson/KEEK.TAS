using TAS;
using TAS.Core.Input.Commands;

namespace KEEK.TAS.Commands; 

public class FrameRateCommand {
    [TasCommand("FrameRate")]
    private static void FrameRate(string[] args) {
        if (args.Length == 0) {
            return;
        }

        if (int.TryParse(args[0], out int newFrameRate)) {
            KeekGame.FrameRate = newFrameRate;
        }
    }
    
    [DisableRun]
    private static void DisableRun() {
        KeekGame.FrameRate = 100;
    }
}