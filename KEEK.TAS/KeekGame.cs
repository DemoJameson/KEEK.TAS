using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TAS;
using TAS.Core;
using TAS.Core.Input;
using TAS.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KEEK.TAS;

public class KeekGame : IGame {
    public static int FrameRate = FixedFrameRate * 2;
    public static int FixedFrameRate => 50;
    private static bool pressedMenu;

    static KeekGame() {
        HookUtils.ILHook(typeof(PauseMenu), "Update", PauseMenuUpdate);
    }

    private static void PauseMenuUpdate(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        ilCursor.GotoNext(MoveType.After, i => i.MatchLdstr("Menu"),
            i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("::GetButtonDown"));
        ilCursor.EmitDelegate<Func<bool, bool>>(buttonDown => Manager.Running ? pressedMenu : buttonDown);
    }

    public string CurrentTime => FormatTime(GlobalData.CurrentGameTime);
    public float FastForwardSpeed => 100f;
    public float SlowForwardSpeed => 0.1f;
    public string LevelName => SceneManager.GetActiveScene().buildIndex.ToString();
    public ulong FrameCount => (ulong) Time.frameCount;
    public bool IsLoading => false;

    private Vector3? lastPlayerPosition;
    private string lastTime;
    private string lastInfo;
    private readonly List<string> infos = new();
    private readonly List<string> statuses = new();

    public string StudioInfo {
        get {
            if (!SavingUtil.FileLoaded || lastTime == CurrentTime && GlobalData.CurrentGameTime != 0f) {
                return lastInfo;
            }

            CharacterController2D player = GameManager.Instance.PlayerCharacter;
            if (player) {
                string position = $"Pos:   {player.transform.position.ToSimpleString()}";
                string speed =
                    $"Speed: {(player._horizontalSpeed / FixedFrameRate).ToFormattedString()}, {(player._verticalSpeed / FixedFrameRate).ToFormattedString()}";
                string velocity = "";
                if (lastPlayerPosition.HasValue) {
                    velocity = $"Vel:   {(player.transform.position - lastPlayerPosition.Value).ToSimpleString()}";
                }

                lastPlayerPosition = player.transform.position;

                statuses.Clear();

                if (!player.AcceptMoveAxis && !player.AcceptSkillInput) {
                    statuses.Add("NoControl");
                }

                if (player.AcceptMoveAxis) {
                    statuses.Add("Move");
                }

                if (player.AcceptSkillInput) {
                    if (player.gameObject.GetComponent<CharacterJump>() is {_jumping: false} cJump) {
                        if (player.Grounded || cJump._airTimer < cJump.CoyoteJumpTime) {
                            statuses.Add("Jump");
                        }
                    }

                    if (player.gameObject.GetComponent<CharacterDash>() is {_canDash: true, _dashing: false}) {
                        statuses.Add("Dash");
                    }

                    statuses.Add("Fire");
                }

                if (player.gameObject.GetComponent<CharacterRay>() is {StuckInPlatform: true}) {
                    statuses.Add("Stuck");
                }

                string levelAndTime = $"[{LevelName}] {CurrentTime}";
                if (Plugin.FixedUpdateFrame) {
                    levelAndTime += " F";
                }

                infos.Clear();
                infos.Add(position);
                infos.Add(speed);
                infos.Add(velocity);
                infos.Add(string.Join(" ", statuses));
                infos.Add(levelAndTime);

                lastTime = CurrentTime;
                return lastInfo = string.Join("\n", infos);
            }

            return "";
        }
    }

    public void SetFrameRate(float multiple) {
        int newFrameRate = (int) (FrameRate * multiple);
        Time.timeScale = Time.timeScale == 0 ? 0 : (float) newFrameRate / FrameRate;
        Time.captureFramerate = newFrameRate;
        Application.targetFrameRate = newFrameRate;
        Time.maximumDeltaTime = Time.fixedDeltaTime;
        QualitySettings.vSyncCount = 0;
    }

    public void SetInputs(InputFrame currentInput) {
        UpdateMoveAxis(currentInput);
        UpdateFireAxis(currentInput);
        UpdateButton(currentInput, Actions.Dash, nameof(Actions.Dash));
        UpdateButton(currentInput, Actions.Fire, nameof(Actions.Fire));
        UpdateTwoButtons(currentInput, Actions.Jump, Actions.Jump2, "Jump");
        UpdateTwoButtons(currentInput, Actions.Dash, Actions.Dash2, "Dash");

        // press every frame
        pressedMenu = currentInput.HasActions(Actions.Pause);
    }

    private void UpdateFireAxis(InputFrame currentInput) {
        if (currentInput.Angle.HasValue) {
            InputManager.Instance.FireAxis = new Vector2(currentInput.GetX(), currentInput.GetY()).normalized;
        }
    }

    private static void UpdateMoveAxis(InputFrame currentInput) {
        InputManager input = InputManager.Instance;
        Vector2 move = Vector2.zero;
        if (currentInput.HasActions(Actions.Left)) {
            move.x = -1f;
        }

        if (currentInput.HasActions(Actions.Right)) {
            move.x = 1f;
        }

        input.MoveAxis = move;
    }

    private static void UpdateButton(InputFrame currentInput, Actions action, string buttonName) {
        InputManager input = InputManager.Instance;
        Dictionary<string, ButtonState> buttons = input._buttons;

        if (!buttons.TryGetValue(buttonName, out ButtonState lastState)) {
            return;
        }

        if (currentInput.HasActions(action)) {
            if (lastState is ButtonState.Down or ButtonState.Holding) {
                buttons[buttonName] = ButtonState.Holding;
            } else {
                buttons[buttonName] = ButtonState.Down;
            }
        } else {
            if (lastState is ButtonState.Down or ButtonState.Holding) {
                buttons[buttonName] = ButtonState.Up;
            } else {
                buttons[buttonName] = ButtonState.Released;
            }
        }
    }

    private static void UpdateTwoButtons(InputFrame currentInput, Actions actions1, Actions actions2, string buttonName) {
        InputManager input = InputManager.Instance;
        Dictionary<string, ButtonState> buttons = input._buttons;

        if (!buttons.ContainsKey(buttonName)) {
            return;
        }

        string buttonName1 = $"{buttonName}1";
        string buttonName2 = $"{buttonName}2";
        if (!buttons.ContainsKey(buttonName1)) {
            buttons[buttonName1] = ButtonState.Released;
        }

        if (!buttons.ContainsKey(buttonName2)) {
            buttons[buttonName2] = ButtonState.Released;
        }

        UpdateButton(currentInput, actions1, buttonName1);
        UpdateButton(currentInput, actions2, buttonName2);

        ButtonState jump1 = buttons[buttonName1];
        ButtonState jump2 = buttons[buttonName2];

        if (jump1 is ButtonState.Down || jump2 is ButtonState.Down) {
            buttons[buttonName] = ButtonState.Down;
        } else if (jump1 is ButtonState.Holding || jump2 is ButtonState.Holding) {
            buttons[buttonName] = ButtonState.Holding;
        } else if (jump1 is ButtonState.Up || jump2 is ButtonState.Up) {
            buttons[buttonName] = ButtonState.Up;
        } else {
            buttons[buttonName] = ButtonState.Released;
        }
    }

    [EnableRun]
    [DisableRun]
    private static void ResetButtonStates() {
        if (InputManager.Instance is { } inputManager) {
            foreach (string buttonsKey in inputManager._buttons.Keys.ToList()) {
                inputManager._buttons[buttonsKey] = ButtonState.Released;
            }
        }
    }

    // FIXME: 目前暂停状态下无法启动 TAS，因为暂停时 FixedUpdate 不会执行
    [DisableRun]
    private static void Unpause() {
        PauseMenu.Instance.Hide();
    }

    public static string FormatTime(float time) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        return $"{timeSpan:m\\:ss\\.ff}({Math.Ceiling(time * FrameRate)})";
    }
}