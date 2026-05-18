---
name: monogame-audio
description: MonoGame audio implementation guide covering SoundEffect vs Song, SoundEffectInstance pooling, volume/pitch control, 3D audio, DynamicSoundEffectInstance, and memory management rules. Use this skill whenever the user asks about playing sounds, background music, audio effects, looping audio, 3D positional sound, sound pools, or any audio-related implementation in MonoGame — even if they just say "how do I play X" or "my sounds aren't working".
---

# MonoGame Audio Implementation Guide

This skill guides implementation of audio systems in MonoGame. For detailed API signatures, see `references/audio.md`.

## Choosing the Right Audio Class

| Class | Use case |
|-------|----------|
| `SoundEffect` | Short clips (SFX, UI sounds). Loaded entirely in memory. Call `Play()` for a fire-and-forget shot. |
| `SoundEffectInstance` | When you need control: loop, pause, resume, adjust volume/pitch at runtime. One instance = one concurrent voice. |
| `Song` + `MediaPlayer` | Background music. Streams from disk. Only one `Song` plays at a time. |
| `DynamicSoundEffectInstance` | Procedural audio, WAV streaming, microphone echo. You push PCM buffers manually. |

Never use `Song` for short SFX — it streams from disk and has latency. Never use `SoundEffect.Play()` when you need to stop or loop — use a `SoundEffectInstance` instead.

## Loading Audio

Always load in `LoadContent()`, never in `Update()` or `Draw()`:

```csharp
_jumpSound  = Content.Load<SoundEffect>("Audio/Jump");
_bgMusic    = Content.Load<Song>("Audio/Theme");
_footstep   = Content.Load<SoundEffect>("Audio/Footstep");
```

## Playing Sound Effects

### Fire-and-forget (no control needed)
```csharp
_jumpSound.Play(); // volume=1, pitch=0, pan=0
_jumpSound.Play(volume: 0.8f, pitch: 0.1f, pan: 0f);
```

### With control (loop, stop, pitch changes)
```csharp
// Create the instance once in LoadContent:
_footstepInstance = _footstep.CreateInstance();
_footstepInstance.IsLooped = true;

// Start/stop in Update:
_footstepInstance.Play();
_footstepInstance.Stop();
_footstepInstance.Pause();

// Real-time adjustments:
_footstepInstance.Volume = 0.6f;  // 0.0 - 1.0
_footstepInstance.Pitch  = 0.2f;  // -1.0 - 1.0 (semitone shift × 12)
_footstepInstance.Pan    = -0.5f; // -1.0 (left) - 1.0 (right)
```

## Background Music

```csharp
// In LoadContent:
MediaPlayer.Volume = 0.7f;
MediaPlayer.IsRepeating = true;
MediaPlayer.Play(_bgMusic);

// Pause/resume:
MediaPlayer.Pause();
MediaPlayer.Resume();

// State check:
if (MediaPlayer.State == MediaState.Stopped) { ... }
```

Only one `Song` can play at a time. Calling `MediaPlayer.Play(newSong)` while another plays stops the current one automatically.

## Sound Instance Pooling

Concurrent voice limit: **~256 on desktop, ~32 on mobile**. For high-frequency sounds (gunshots, particles), maintain a pool:

```csharp
// Create pool once in LoadContent:
private const int PoolSize = 8;
private SoundEffectInstance[] _gunPool;
private int _gunPoolIndex;

_gunPool = new SoundEffectInstance[PoolSize];
for (int i = 0; i < PoolSize; i++)
    _gunPool[i] = _gunSound.CreateInstance();

// Play from pool (round-robin):
private void PlayGunshot()
{
    var instance = _gunPool[_gunPoolIndex];
    _gunPoolIndex = (_gunPoolIndex + 1) % PoolSize;
    instance.Stop();  // stop if already playing
    instance.Play();
}
```

## 3D Positional Audio

Use `AudioListener` (camera/player) and `AudioEmitter` (sound source). Both are class fields — do not instantiate per frame.

```csharp
// Fields:
private AudioListener _listener = new AudioListener();
private AudioEmitter  _emitter  = new AudioEmitter();

// Update each frame:
_listener.Position = new Vector3(playerPos, 0f);
_emitter.Position  = new Vector3(enemyPos, 0f);

// Apply to instance (call before or after Play):
_instance.Apply3D(_listener, _emitter);
```

`Apply3D` sets volume and pan automatically based on distance and direction. It overrides manual `Volume` and `Pan` settings.

## Dispose and Cleanup

`SoundEffect` and `SoundEffectInstance` hold unmanaged audio resources. Dispose them explicitly when unloading a scene:

```csharp
protected override void UnloadContent()
{
    _footstepInstance?.Dispose();
    // SoundEffect assets are disposed by ContentManager.Unload()
}
```

Never dispose a `SoundEffectInstance` while it is playing — call `Stop()` first.

## Rules

- Load all audio in `LoadContent()` — no exceptions.
- Never create `SoundEffectInstance` inside `Update()` or `Draw()` — create in `LoadContent()` and reuse.
- Never play more than ~255 concurrent voices — pool high-frequency sounds.
- Do not allocate `AudioListener` or `AudioEmitter` per frame — they are class fields.
- On mobile, test with the 32-voice limit in mind — desktop limits do not apply.

## Reference

For `DynamicSoundEffectInstance` (WAV streaming, microphone), see `references/audio.md`.
