using System;
using System.Collections.Generic;

namespace TAS.Shared.Communication.GameToStudio; 

[Serializable]
public record struct HotkeySettingsMessage(Dictionary<HotkeyID, List<Keys>> Settings) : IGameToStudioMessage {
    public Dictionary<HotkeyID, List<Keys>> Settings = Settings;
}