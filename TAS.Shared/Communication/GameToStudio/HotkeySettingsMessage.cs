using System;
using System.Collections.Generic;

namespace TAS.Shared.Communication.GameToStudio; 

[Serializable]
public record struct HotkeySettingsMessage(Dictionary<HotkeyID, List<Keys>> Settings) : IServerToClientMessage {
    public Dictionary<HotkeyID, List<Keys>> Settings = Settings;
}