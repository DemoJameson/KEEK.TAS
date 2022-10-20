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
- F = Fire
- A = Fire Angle
- P = Pause 

## Playback / Recording of Input File
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
* [CelesteTAS](https://github.com/ShootMe/CelesteTAS)
* [CelesteTAS-EverestInterop](https://github.com/EverestAPI/CelesteTAS-EverestInterop)