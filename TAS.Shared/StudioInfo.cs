namespace TAS.Shared;

// ReSharper disable once StructCanBeMadeReadOnly
public record struct StudioInfo {
    public readonly int CurrentLine;
    public readonly string CurrentLineSuffix;
    public readonly int CurrentFrameInTas;
    public readonly int TotalFrames;
    public readonly int tasStates;
    public readonly string GameInfo;
    public readonly string LevelName;
    public readonly string CurrentTime;

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once ConvertToPrimaryConstructor
    public StudioInfo(
        int currentLine, string currentLineSuffix, int currentFrameInTas, int totalFrames, int tasStates,
        string gameInfo, string levelName, string currentTime) {
        CurrentLine = currentLine;
        CurrentLineSuffix = currentLineSuffix;
        CurrentFrameInTas = currentFrameInTas;
        TotalFrames = totalFrames;
        this.tasStates = tasStates;
        GameInfo = gameInfo;
        LevelName = levelName;
        CurrentTime = currentTime;
    }

    // ReSharper disable once UnusedMember.Global
    public byte[] ToByteArray() {
        return BinaryFormatterHelper.ToByteArray(new object[] {
            CurrentLine,
            CurrentLineSuffix,
            CurrentFrameInTas,
            TotalFrames,
            tasStates,
            GameInfo,
            LevelName,
            CurrentTime,
        });
    }

    // ReSharper disable once UnusedMember.Global
    public static StudioInfo FromByteArray(byte[] data) {
        object[] values = BinaryFormatterHelper.FromByteArray<object[]>(data);
        return new StudioInfo(
            (int) values[0],
            values[1] as string,
            (int) values[2],
            (int) values[3],
            (int) values[4],
            values[5] as string,
            values[6] as string,
            values[7] as string
        );
    }
}