using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace KEEK.TAS;

public static class HookUtils {
    private static readonly List<IDetour> Hooks = new();
    private static readonly List<UnityAction<Scene, Scene>> Actions = new();

    public static void Hook(Type type, string name, Delegate to) {
        if (type.GetMethod(name, (BindingFlags) (-1)) is { } method) {
            Hooks.Add(new Hook(method, to));
        } else {
            Plugin.Log.LogWarning($"Method {name} does not exist in {type.FullName}");
        }
    }

    public static void ILHook(Type type, string name, ILContext.Manipulator manipulator) {
        if (type.GetMethod(name, (BindingFlags) (-1)) is { } method) {
            Hooks.Add(new ILHook(method, manipulator));
        } else {
            Plugin.Log.LogWarning($"Method {name} does not exist in {type.FullName}");
        }
    }

    public static void ActiveSceneChanged(UnityAction<Scene, Scene> action) {
        Actions.Add(action);
        SceneManager.activeSceneChanged += action;
    }

    public static void Dispose() {
        foreach (IDetour detour in Hooks) {
            detour.Dispose();
        }

        foreach (UnityAction<Scene, Scene> action in Actions) {
            SceneManager.activeSceneChanged -= action;
        }

        Hooks.Clear();
        Actions.Clear();
    }
}