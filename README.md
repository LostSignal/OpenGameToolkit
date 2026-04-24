# Open Game Toolkit (OGT)

OGT is an open-source Unity package that bundles the foundational systems most
games need — bootstrapping, managers, input, UI, audio, networking, optimizers,
cloud upload, and more — behind a single `OGT` namespace. 

OGT is going though a very large cleanup phase, so I wouldn't take any dependencies on OGT yet since a LOT will be changing.  Once the cleanup phase is done, it will be distributed on [OpenUPM](https://openupm.com/) under the package (`com.lostsignal.ogt`).

> **Current version:** `0.1.0`
> **Minimum Unity:** `6000.4` (Unity 6)

---

## Installing via the OGT Wizard

OGT has several dependencies on packages that live on
[OpenUPM](https://openupm.com/) rather than the Unity registry. To make sure
every scoped registry, package version, and dependency is wired up correctly,
**do not install OGT by hand** — use the **OGT Wizard**.

The wizard walks you through adding the OpenUPM scoped registry, installing the
required upstream packages, and importing OGT itself in the right order. Once
the wizard finishes, your `manifest.json` will be set up so that Unity's
Package Manager can resolve everything without manual intervention.

**Steps:**

1. Open the OGT Wizard from the Unity menu.
2. Let it register the OpenUPM scoped registry (if not already present).
3. Let it pull the required dependencies (listed below).
4. Confirm the install of `com.lostsignal.ogt`.

### Package dependencies

OGT depends on the following packages (see [`package.json`](package.json)):

| Package | Version |
|---|---|
| `com.unity.addressables` | `2.9.1` |
| `com.unity.editorcoroutines` | `1.0.1` |
| `com.unity.nuget.newtonsoft-json` | `3.2.2` |
| `com.unity.inputsystem` | `1.19.0` |
| `com.unity.coding` | `0.1.0-preview.25` |
| `com.revenantx.litenetlib` | `2.1.3` |
| `net.bunnycdn.storage` | `1.0.5` |

---

## Project layout

OGT is organized as a set of assemblies that can be pulled in piecemeal. The
high-level dependency graph is documented in
[`Dependences.md`](Dependences.md):

```
_Common     → LiteNetLib
_Core       → _Common, BunnyCDN.Net.Storage
Audio       → _Core, _Common
Misc        → (none)
Networking  → _Core, _Common
Optimizer   → _Core, _Common
UI          → _Core, _Common
```

### `_Common/` — pure C#, no Unity required

**Goal:** `_Common` is **pure C#**. It intentionally avoids any hard reference
to `UnityEngine` so that it can be dropped straight into a **standard console
app, a web server, a dedicated game server, or any other .NET project** — not
just Unity.

To support this, `_Common` ships a small shim at
[`UnityEngine.cs`](_Common/Runtime/UnityEngine.cs) that stubs out attributes
like `SerializeField`, `HideInInspector`, and `Header` when Unity is not
present, and uses `UNITY_6000_0_OR_NEWER` guards to light up Unity-specific
behavior only when compiled inside a Unity project (see for example
[`GameBehavior.cs`](_Common/Runtime/GameBehavior.cs)).

What lives in `_Common`:

- **Activation** — `IAwake`, `IStart`, `IUpdate`, `IFixedUpdate`, `ILateUpdate`
  interfaces for pure-C# lifecycle hooks.
- **Attributes** — `CalledByUI`, `InspectorButton`.
- **Backend** — `IBackend`, request/result/message contracts.
- **Bootloader** — the app/game boot entry point.
- **Cloud Upload** — Azure, S3, and FTP upload helpers.
- **Collections** — `BitArray`, `ConcurrentList/Queue/Stack`, `ObjectTracker`,
  `ProcessList`, `Queue`.
- **Extensions** — for `Action`, `byte[]`, `char`, `DateTime`, `Dictionary`,
  `HashSet`, `ICollection`, numeric types, `Random`, `string`, `Task`.
- **Managers** — Activation, Camera, Delay Execute, Fade, Level, Localization,
  Work, plus the base `Manager`.
- **Networking** — client/server layers, transports (LiteNetLib, WebSocket),
  Unity + UNET bridges, and a `MessageCollection` for typed messages.
- **Properties** — `Enum`, `EnumValue`, and the generic `Properties` system.
- **Providers** — Analytics, DeviceData, Logging, Platform.
- **Serialization** — `JsonUtil`, `PositionConverter`, `RGBAConverter`.
- **Settings File** — `ISettingsFile`, `SettingsFileCollection`.
- **Text** — `BetterStringBuilder`.
- **Third Party** — `LZString` (see `_Common/Licenses/`).
- **Types** — `Position2D`, `Position3D`, `RGBA`, `Rotation`.
- **Utilities** — `ColorUtil`, `CosUtil`, `HttpUtil`, `IniSerializer`,
  `TimingLogger`, `TypeUtil`.
- **Validation** — `IValidate`, `ValidationError`, `ValidationReport`.

### `_Core/` — Unity runtime & editor foundation

`_Core` is the Unity-facing glue layer. It builds on `_Common` and provides:

- **Bootloader runtime** — `BootloaderRunner`, `SceneRef`.
- **Managers** — Input (+ Keyboard / Trigger), Player Input, Screen, Spawning
  (pools, spawnables), UserInfo.
- **Providers** — Unity logging and Unity platform providers (with a
  `UnityDispatcher`).
- **Singleton** — `ISingleton`, `SingletonMonoBehaviour`, `SingletonUtil`.
- **Unity Extensions** — Addressables, AnimationCurve, Animator, Awaitable,
  BetterStringBuilder, GameObject, Input, Matrix4x4, MonoBehaviour, Object,
  Rect, RectTransform, Transform, UnityEvent.
- **Utility** — `Caching`, `DirectoryUtil`, `EditorUtil`, `FileUtil`,
  `PackageManagerUtil`, `ProjectDefinesHelper`, `TextObject`, `WaitForUtil`.
- **Editor** — Project Settings framework, Build Steps, Editor Tools
  (APK install, Fast Platform Switcher, Folder Finder, Mesh tools, Package
  Mapper, Reference Finder, Rename Sub-Object window, Timeline editor), and
  GUI scopes (`BoxAreaScope`, `FoldoutScope`, `IndentLevelScope`,
  `LabelWidthScope`).

### `Audio/`

`AudioManager`, `AudioBlock` / `AudioBlockInstance`, `AudioChannel`, and ready
-made components like `PlayAudioBlockOnButtonClick` and
`PlayAudioBlockOnCollision`.

### `UI/`

- **Button** — `OGTButton`, `OGTToggle`, and a `UIActions` folder.
- **Widget** — `Widget`, `ButtonWidget`, `InputFieldWidget`, `ModalWidget`,
  `ToggleWidget`.
- **Panels** — `Panel`, `PanelLogic`, `PanelManager`, `NewMessageBox`.
- **Property Bindings** — bind `bool` / `enum` / `int` / `float` / `string`
  properties to toggles, sliders, and text.
- **ScrollView**, **Localization**, **Text**, **Extensions**,
  `InputBlocker`, `RectTransformSpinner`.

### `Networking/`

High-level `NetworkingManager`, `UnityGameClientSubsystem`, behaviours, and
transports that plug into `_Common`'s networking layer.

### `Optimizer/`

- `StreamingLODGroup` component.
- Object and Scene optimizers (with settings ScriptableObjects), plus
  `VolumeOptimizer`.

### `Misc/`

Standalone helpers with no dependencies — notably the `WordDictionary`
ScriptableObject.

---

## Assemblies

| Assembly | Location |
|---|---|
| `OGT.Common` | [`_Common/Runtime`](_Common/Runtime/OGT.Common.asmdef) |
| `OGT.Common.Tests` | [`_Common/Tests`](_Common/Tests/OGT.Common.Tests.asmdef) |
| `OGT.Unity.Core` | [`_Core/Runtime`](_Core/Runtime/OGT.Core.asmdef) |
| `OGT.Core.Editor` | [`_Core/Editor`](_Core/Editor/OGT.Core.Editor.asmdef) |
| `OGT.Unity.Audio` | [`Audio/Runtime`](Audio/Runtime/OGT.Unity.Audio.asmdef) |
| `OGT.Unity.UI` | [`UI/Runtime`](UI/Runtime/OGT.Unity.UI.asmdef) |
| `OGT.Unity.UI.Editor` | [`UI/Editor`](UI/Editor/OGT.Unity.UI.Editor.asmdef) |
| `OGT.Networking` | [`Networking/Runtime`](Networking/Runtime/OGT.Networking.asmdef) |
| `OGT.Networking.Editor` | [`Networking/Editor`](Networking/Editor/OGT.Networking.Editor.asmdef) |
| `OGT.Optimizer` | [`Optimizer/Runtime`](Optimizer/Runtime/OGT.Optimizer.asmdef) |
| `OGT.Optimizer.Editor` | [`Optimizer/Editor`](Optimizer/Editor/OGT.Optimizer.Editor.asmdef) |
| `OGT.Misc` | [`Misc/Runtime`](Misc/Runtime/OGT.Misc.asmdef) |
| `OGT.Misc.Editor` | [`Misc/Editor`](Misc/Editor/OGT.Misc.Editor.asmdef) |
| `OGT.Misc.Tests` | [`Misc/Tests`](Misc/Tests/OGT.Misc.Tests.asmdef) |

---

## Credits & licensing

See [`Special Thanks.md`](Special%20Thanks.md) for the full list of acknowledgements.
Third-party licenses shipped with OGT live under [`_Common/Licenses`](_Common/Licenses/)
(for example, `lz-string-csharp`).

---

## Status

OGT is at version `0.1.0` and under active development. The
[`_WIP/`](_WIP/) folder contains experimental modules that are not yet part of
the public surface and are excluded from this README.
