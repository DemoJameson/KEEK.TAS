using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using KEEK.TAS.Commands;
using TAS;
using TAS.Core.Hotkey;
using UnityEngine;

namespace KEEK.TAS;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance { get; private set; }
    public static ManualLogSource Log => Instance.Logger;
    public static bool FixedUpdateFrame { get; private set; }
    private static bool? trainerInstalled;
    private static ConfigEntry<int> port;

    private void Awake() {
        Instance = this;
        Application.runInBackground = true;
        TimeCommand.Load();
        HookUtils.Hook(typeof(InputManager), "Update", InputManagerUpdate);
        Manager.Init(new KeekGame());

        port = Config.Bind("General", "Communication Server Port", 19982);
        port.SettingChanged += (sender, args) => {
            CommunicationServer.Start(port.Value);
        };
        CommunicationServer.Start(port.Value);
    }

    private void OnDestroy() {
        HookUtils.Dispose();
        CommunicationServer.Stop();
    }

    private void OnGUI() {
        trainerInstalled ??= Chainloader.PluginInfos.ContainsKey("KEEK.Trainer");

        float y = 20;
        if (trainerInstalled.Value) {
            y += 20;
        }
        GUI.Label(new Rect(Screen.width - 91, y, 105, 50), $"TAS v{MyPluginInfo.PLUGIN_VERSION}");
    }

    private void FixedUpdate() {
        FixedUpdateFrame = true;
        
        // make sure launching tas at fixed update frame
        if (!Manager.Running) {
            Hotkeys.Update();
        }
    }

    private void Update() {
        if (Manager.Running) {
            Hotkeys.Update();
        }
        
        TimeCommand.Finish();
    }

    private void LateUpdate() {
        UnlockCursor();
        Hotkeys.AllowKeyboard = Application.isFocused || !CommunicationServer.Connected;
        Manager.SendStateToStudio();
        FixedUpdateFrame = false;
    }

    private void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static void InputManagerUpdate(Action<InputManager> orig, InputManager self) {
        Manager.Update();

        if (!Manager.Running) {
            orig(self);
        }
    }
}