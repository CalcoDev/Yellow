using System.Collections.Generic;
using Godot;
using Yellow.Components;
using Yellow.Resources.Sounds;

namespace Yellow.Managers;

[GlobalClass]
public partial class SoundManager : Node
{
	public readonly struct SoundIdentifier
	{
		public readonly SoundPlayerComponent Comp;
		public readonly Node Player;

		public static readonly SoundIdentifier Invalid = new(null, null);

		public SoundIdentifier(SoundPlayerComponent comp, Node player)
		{
			Comp = comp;
			Player = player;
		}
	}

	public static SoundManager Instance { get; private set; }

	// TODO(calco): Add other audio busses later
	public static StringName SFXAudioBus => "SFX";
	public static StringName MusicAudioBus => "Music";

	// NOTE(calco): Redefinition of private stuff, for easier access.
	// Bad, but whatever.

	[ExportGroup("Settings")]
	[Export] private bool LoadOnStartup = true;
	[Export] public int DefaultMaxInstances = 16;

	// NOTE(calco): Separate, as we might want to use _sounds to identify stuff
	// later down the line
	private readonly Dictionary<string, SoundSO> _sounds = new();
	// NOTE(calco): Still associating by string, as it allows better convenience
	// and maybe I'll keep only this in the long term.
	private readonly Dictionary<string, SoundPlayerComponent> _comps = new();

    public override void _EnterTree()
    {
		if (Instance != null) {
			GD.PushWarning("WARN: Sound Manager already exists!");
			QueueFree();
			return;
		}
		Instance = this;

		if (LoadOnStartup) {
			LoadSoundsFromDir();
		}
	}

	// Interface
	public bool IsAudioPlaying(string name)
	{
        if (!_comps.TryGetValue(name, out SoundPlayerComponent comp)) {
			return false;
		}
		return comp.IsPlaying;
	}

	public SoundIdentifier Play(
		string name,
		bool overrideLoop = false,
		Vector3 position = new Vector3(),
		float volume = -1,
		float volumeMult = -1
	) {
		// Get SO
		var sound = GetSoundSO(name);
		if (sound == null) {
			return SoundIdentifier.Invalid;
		}

        if (!_comps.TryGetValue(name, out SoundPlayerComponent comp)) {
            comp = new SoundPlayerComponent();
			AddChild(comp);
			comp.SetSound(sound);
			comp.MaxInstances = DefaultMaxInstances;
			_comps.Add(sound.Name, comp);
        }

        return new SoundIdentifier(comp, comp.Play(overrideLoop, position, volume, volumeMult));
	}
	
	// TODO(calco): This is absolutely horrendous lmfao
	public void SetPaused(SoundIdentifier sid, bool paused)
	{
		if (sid.Player is AudioStreamPlayer3D player3D) {
			player3D.StreamPaused = paused;
		} else if (sid.Player is AudioStreamPlayer player) {
			player.StreamPaused = paused;
		}
	}
	
	public void SetPauseAllName(string name, bool paused, bool logError = false)
	{
		var player = GetSoundPlayer(name, logError);
		if (player == null) {
			return;
		}
		player.SetPauseAll(paused);
	}

	public void Stop(SoundIdentifier sid)
	{
		if (sid.Player is AudioStreamPlayer3D player3D) {
			player3D.Stop();
		} else if (sid.Player is AudioStreamPlayer player) {
			player.Stop();
		}
	}

	public void StopAllName(string name, bool logError = false)
	{
		var player = GetSoundPlayer(name, logError);
		if (player == null) {
			return;
		}
		player.StopAll();
	}

	public void LoadSoundsFromDir()
	{
		var dirQueue = new Queue<(DirAccess d, string p)>();
		var soundsDir = DirAccess.Open("res://resources/sounds/");
		dirQueue.Enqueue((soundsDir, "res://resources/sounds/"));

		while (dirQueue.Count > 0) {
			var (dir, path) = dirQueue.Dequeue();
			dir.ListDirBegin();
			var filePath = dir.GetNext();
			while (filePath != "") {
				var fullPath = $"{path}/{filePath}";

				if (dir.CurrentIsDir()) {
					dirQueue.Enqueue((DirAccess.Open(fullPath), fullPath));
					filePath = dir.GetNext();
					continue;
				}

				var res = ResourceLoader.Load<Resource>(fullPath);
				if (res is SoundSO sound) {
					_sounds.Add(sound.Name, sound);
				}

				filePath = dir.GetNext();
			}
		}
	}

	private SoundSO GetSoundSO(string name, bool logError = true)
	{
		if (!_sounds.TryGetValue(name, out SoundSO sound)) {
			if (logError) {
				GD.PushWarning($"WARN: Could not find sound effect ({name})!");
			}
			return null;
		}
		return sound;
	}

	private SoundPlayerComponent GetSoundPlayer(string name, bool logError = true)
	{
		if (!_comps.TryGetValue(name, out SoundPlayerComponent soundPlayer)) {
			if (logError) {
				GD.PushWarning($"WARN: Could not find sound player for ({name})!");
			}
			return null;
		}
		return soundPlayer;
	}

	private static long EncodeInts(int a, int b)
	{
		return (((long) a) << 32) | (b & 0xFFFFFFFFL);
	}

	private static (int a, int b) DecodeInts(long l)
	{
        return ((int)(l >> 32), (int)l);
    }
}
