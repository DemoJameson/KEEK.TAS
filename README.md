# KEEK.TAS
Simple TAS Tools for the game [KEEK](https://store.steampowered.com/app/2088080/KEEK/).

## How to use
1. [Download](https://github.com/DemoJameson/KEEK.TAS/releases) the zip then unzip all files into the game folder.
2. Run `KEEK.exe` will start the game with tas plugin now.
3. Run `TAS.Stuido.exe` write tas.

## Input File
The input file is a text file with tas as an extension

Format for the input file is (Frames),(Actions)

e.g. 123,R,J (For 123 frames, hold Right and Jump)

## Actions Available
- R = Right
- L = Left
- X = Dash
- C = Dash2
- J = Jump
- K = Jump2
- F = Fire, Angle
- P = Pause

## Special Input

### Breakpoints
- You can create a breakpoint in the input file by typing `***` by itself on a single line
- The program when played back from the start will fast forward until it reaches that line and then go into frame stepping mode
- You can specify the speed with `***X`, where `X` is the speedup factor. e.g. `***10` will go at 10x speed, `***0.5` will go at half speed.

## Playback of Input File
### Controller
While in game
- Playback/Stop: Right Stick
- Restart: Left Stick
- When Playing Back
    - Faster/Slower Playback: Right Stick X+/X-
    - Frame Step: DPad Up
    - While Frame Stepping:
        - One more frame: DPad Up
        - Continue at normal speed: DPad Down
        - Frame step continuously: Right Stick X+

### Keyboard
While in game
- Playback/Stop: RightControl
- Restart: =
- Faster/Slower Playback: RightShift / Alt+LeftShift
- Frame Step: [
- While Frame Stepping:
    - One more frame: [
    - Continue at normal speed: ]
    - Frame step continuously: RightShift

## Credits
* [TinertiaTAS](https://github.com/ShootMe/TinertiaTAS)
* [CelesteTAS-EverestInterop](https://github.com/EverestAPI/CelesteTAS-EverestInterop)
