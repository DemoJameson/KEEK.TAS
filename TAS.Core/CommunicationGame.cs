using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using TAS.Core.Hotkey;
using TAS.Core.Input;
using TAS.Shared;

namespace TAS.Core;

public sealed class CommunicationGame : CommunicationBase {
    public static CommunicationGame Instance { get; private set; }

    private byte[] lastBindingsData = new byte[0];
    private Thread thread;
    private CommunicationGame(string gameName) : base(gameName) { }

    public static void OnApplicationExit() {
        Manager.Game.OnClientDestroy();
        Destroy();
    }

    public static void Run(string gameName) {
        if (Instance != null) {
            return;
        }

        Manager.Game.OnClientCreate();
        Instance = new CommunicationGame(gameName);
        RunThread("TAS.Studio Client");
    }

    public static void Destroy() {
        Instance?.WriteReset();
        Instance?.thread?.Abort();
        Instance = null;
    }

    private static void RunThread(string threadName) {
        Thread thread = new(() => {
            try {
                Instance.UpdateLoop();
            } catch (Exception e) when (e is not ThreadAbortException) {
                Console.WriteLine($"Studio Communication Thread Name: {threadName}");
            }
        }) {
            Name = threadName,
            IsBackground = true
        };
        thread.Start();
        Instance.thread = thread;
    }

    #region Read

    protected override MemoryMappedFile writeMMF => gameToStudioMMF;
    protected override MemoryMappedFile readMMF => studioToGameMMF;

    protected override void ReadData(Message message) {
        switch (message.Id) {
            case MessageID.EstablishConnection:
                throw new NeedsResetException("Initialization data recieved in main loop");
            case MessageID.Wait:
                ProcessWait();
                break;
            case MessageID.GetData:
                ProcessGetData(message.Data);
                break;
            case MessageID.SendPath:
                ProcessSendPath(message.Data);
                break;
            case MessageID.SendHotkeyPressed:
                ProcessHotkeyPressed(message.Data);
                break;
            case MessageID.ToggleGameSetting:
                ProcessToggleGameSetting(message.Data);
                break;
        }
    }

    private void ProcessGetData(byte[] data) {
        object[] objects = BinaryFormatterHelper.FromByteArray<object[]>(data);
        GameDataType gameDataType = (GameDataType) objects[0];
        string gameData = gameDataType switch {
            GameDataType.ConsoleCommand => GetConsoleCommand((bool) objects[1]),
            // todo ExactGameInfo
            GameDataType.ExactGameInfo => "GameInfo.ExactStudioInfo",
            _ => string.Empty
        };

        ReturnData(gameData);
    }

    private void ReturnData(string gameData) {
        byte[] gameDataBytes = Encoding.Default.GetBytes(gameData ?? string.Empty);
        WriteMessageGuaranteed(new Message(MessageID.ReturnData, gameDataBytes));
    }

    private string GetConsoleCommand(bool simple) {
        // todo GetConsoleCommand
        return "load 1";
        // return ConsoleCommand.CreateConsoleCommand(simple);
    }

    private void ProcessSendPath(byte[] data) {
        string path = Encoding.Default.GetString(data);
        if (PlatformUtils.NonWindows && path.StartsWith("Z:\\", StringComparison.InvariantCultureIgnoreCase)) {
            path = path.Substring(2, path.Length - 2).Replace("\\", "/");
        }

        InputController.StudioTasFilePath = path;
    }

    private void ProcessHotkeyPressed(byte[] data) {
        HotkeyID hotkeyId = (HotkeyID) data[0];
        bool released = Convert.ToBoolean(data[1]);
        Hotkeys.KeysDict[hotkeyId].OverrideCheck = !released;
        // $"{hotkeyId.ToString()} {(released ? "released" : "pressed")}".DebugLog();
    }

    private void ProcessToggleGameSetting(byte[] data) {
        
    }

    #endregion

    #region Write

    protected override void EstablishConnection() {
        // var studio = this;
        var celeste = this;

        Message lastMessage;

        // Stall until input initialized to avoid sending invalid hotkey data
        while (Hotkeys.KeysDict == null) {
            Thread.Sleep(Timeout);
        }

        // studio.WriteMessageGuaranteed(new Message(MessageID.EstablishConnection, new byte[0]));
        lastMessage = celeste.ReadMessageGuaranteed();
        if (lastMessage.Id != MessageID.EstablishConnection) {
            throw new NeedsResetException("Invalid data recieved while establishing connection");
        }

        // studio.SendPath(null);
        lastMessage = celeste.ReadMessageGuaranteed();
        if (lastMessage.Id != MessageID.SendPath) {
            throw new NeedsResetException("Invalid data recieved while establishing connection");
        }

        celeste.ProcessSendPath(lastMessage.Data);

        celeste.SendCurrentBindings(true);
        // lastMessage = studio.ReadMessageGuaranteed();
        // if (lastMessage.Id != MessageID.SendCurrentBindings) {
        // throw new NeedsResetException();
        // }
        // studio.ProcessSendCurrentBindings(lastMessage?.Data);

        Initialized = true;
    }

    private void SendStateNow(StudioInfo studioInfo, bool canFail) {
        if (Initialized) {
            byte[] data = studioInfo.ToByteArray();
            Message message = new(MessageID.SendState, data);
            if (canFail) {
                WriteMessage(message);
            } else {
                WriteMessageGuaranteed(message);
            }
        }
    }

    public void SendState(StudioInfo studioInfo, bool canFail) {
        PendingWrite = () => SendStateNow(studioInfo, canFail);
    }

    public void SendCurrentBindings(bool forceSend = false) {
        Dictionary<int, List<int>> nativeBindings =
            Hotkeys.KeysInteractWithStudio.ToDictionary(pair => (int) pair.Key, pair => pair.Value.Cast<int>().ToList());
        byte[] data = BinaryFormatterHelper.ToByteArray(nativeBindings);
        if (!forceSend && string.Join("", data) == string.Join("", lastBindingsData)) {
            return;
        }

        WriteMessageGuaranteed(new Message(MessageID.SendCurrentBindings, data));
        lastBindingsData = data;
    }

    public void UpdateLines(Dictionary<int, string> lines) {
        byte[] data = BinaryFormatterHelper.ToByteArray(lines);
        try {
            WriteMessageGuaranteed(new Message(MessageID.UpdateLines, data));
        } catch {
            // ignored
        }
    }

    #endregion
}