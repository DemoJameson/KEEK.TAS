using System;
using System.Collections.Generic;

namespace TAS.Shared.Communication.GameToStudio; 

[Serializable]
public record struct UpdateTextsMessage(Dictionary<int, string> Texts) : IServerToClientMessage {
    public Dictionary<int, string> Texts = Texts;
}