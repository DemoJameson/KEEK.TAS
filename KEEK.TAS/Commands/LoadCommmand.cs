using TAS.Core.Input.Commands;
using UnityEngine.SceneManagement;

namespace KEEK.TAS.Commands; 

public static class LoadCommmand {
    [TasCommand("Load", LegalInMainGame = false)]
    private static void LoadLevel(string[] args) {
        if (args.Length == 0) {
            return;
        }

        if (int.TryParse(args[0], out int buildIndex)) {
            SceneManager.LoadScene(buildIndex);
            GlobalData.CurrentGameTime = 0;
            GlobalData.LastSavePointName = null;
            GlobalData.ManualSaved = false;
        }
    }
}