using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TAS.Shared;

public abstract record InputFrameBase {
    public Actions Actions { get; set; }
    public int Frames { get; set; }
    public float? Angle { get; set; }
    public string AngleStr { get; set; } = "";
    public bool HasActions(Actions actions) => (Actions & actions) != 0;
    public float GetX() => (float) Math.Sin(Angle.Value * Math.PI / 180.0);
    public float GetY() => (float) Math.Cos(Angle.Value * Math.PI / 180.0);
    
    public string ToActionsString() {
        StringBuilder sb = new();

        foreach (KeyValuePair<char, Actions> pair in ActionsUtils.Chars) {
            if (HasActions(pair.Value)) {
                sb.Append($",{pair.Key}");
                if (pair.Value == Actions.Fire) {
                    sb.Append($",{AngleStr.ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }

        return sb.ToString();
    }
}