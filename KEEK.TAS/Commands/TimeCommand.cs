using System;
using TAS;
using TAS.Core.Input.Commands;
using UnityEngine.SceneManagement;

namespace KEEK.TAS.Commands; 

public static class TimeCommand {
    private static float? startTime;
    
    [TasCommand("Time", AliasNames = new[] {"Time:", "Time："}, CalcChecksum = false)]
    private static void Time() {
        // dummy
    }

    public static void Load() {
        HookUtils.ActiveSceneChanged(StartTime);
        HookUtils.Hook(typeof(NextLevel), "GoToNextLevel", GoToNextLevel);
    }

    public static void Finish() {
        if (Manager.Running && startTime != null && SceneManager.GetActiveScene().name == "LevelFinal" && GhostBossFightManager.Instance.Paused) {
            MetadataCommand.UpdateAll("Time", _ => KeekGame.FormatTime(GlobalData.CurrentGameTime - startTime.Value));
        }
    }

    private static void StartTime(Scene _, Scene __) {
        startTime = Manager.Running ? GlobalData.CurrentGameTime : null;
    }

    private static void GoToNextLevel(Action<NextLevel, CharacterController2D> orig, NextLevel self, CharacterController2D character) {
        if (Manager.Running && startTime != null) {
            MetadataCommand.UpdateAll("Time", _ => KeekGame.FormatTime(GlobalData.CurrentGameTime - startTime.Value));
        }
        
        orig(self, character);
    }
}