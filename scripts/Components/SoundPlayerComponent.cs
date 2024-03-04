using System.Collections.Generic;
using Godot;
using Yellow.Resources.Sounds;

namespace Yellow.Components;

[GlobalClass]
public partial class SoundPlayerComponent : Node3D
{
    [Export] public SoundSO Sound;
    [Export] public int MaxInstances = 16;
    public int InstanceCount { get; private set; }

    [Signal]
    public delegate void OnPlayBeginEventHandler();

    [Signal]
    public delegate void OnPlayEndEventHandler();

    private AudioStreamPlayer _nonPlayer;
    private AudioStreamPlayer3D _player;

    private readonly List<AudioStreamPlayer> _freeNonSpatial = new();
	private readonly List<AudioStreamPlayer3D> _freeSpatial = new();
	
	private readonly List<AudioStreamPlayer> _inUseNonSpatial = new();
	private readonly List<AudioStreamPlayer3D> _inUseSpatial= new();
    private bool _init = false;

    private int _soundStreamIndex = 0;

    public override void _Ready()
    {
        InstanceCount = 0;
    }

    public void SetSound(SoundSO sound)
    {
        if (sound == null || sound.IsSpatial == Sound?.IsSpatial) {
            return;
        }
        _init = true;

        Sound = sound;
        _soundStreamIndex = 0;
        if (sound.IsSpatial) {
            if (_nonPlayer != null) {
                RemoveChild(_nonPlayer);
                _nonPlayer = null;
            }

            _player = new AudioStreamPlayer3D();
            AddChild(_player);
        } else {
            if (_player != null) {
                RemoveChild(_player);
                _player = null;
            }

            _nonPlayer = new AudioStreamPlayer();
            AddChild(_nonPlayer);
        }
    }

    public Node Play(
        Vector3 position = new Vector3(),
        float volume = -1f,
        float volumeMult = -1f
    ) {
        if (!_init) {
            SetSound(Sound);
        }

        var vol = volume >= 0f ? volume : Sound.Volume * volumeMult;
        Node retPlayer = null;

        switch (Sound.PoolType) {
            case SoundPoolType.Sequential:
                _soundStreamIndex = (_soundStreamIndex + 1) % Sound.Streams.Count;
                break;
            case SoundPoolType.Random:
                _soundStreamIndex = GD.RandRange(0, Sound.Streams.Count - 1);
                break;
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS0164 // This label has not been referenced
            _:
                GD.PushError($"ERROR: Unsupported SoundPoolType ({Sound.PoolType})!");
#pragma warning restore CS0164 // This label has not been referenced
#pragma warning restore CS0162 // Unreachable code detected
                break;
        }
        
        if (Sound.IsSpatial) {
			var player = GetSpatialAudioStreamPlayer();
			player.GlobalPosition = position;
			player.VolumeDb = vol;
            player.Stream = Sound.Streams[_soundStreamIndex];
			player.Play();
			player.Finished += () => {
				_inUseSpatial.Remove(player);
                _freeSpatial.Add(player);
                EmitSignal(SignalName.OnPlayEnd);
			};
            _inUseSpatial.Add(player);
           
            retPlayer = player;
		} else {
			var player = GetNonSpatialAudioStreamPlayer();
			player.VolumeDb = vol;
            player.Stream = Sound.Streams[_soundStreamIndex];
			player.Play();
			player.Finished += () => {
				_inUseNonSpatial.Remove(player);
                _freeNonSpatial.Add(player);
                EmitSignal(SignalName.OnPlayEnd);
			};
            _inUseNonSpatial.Add(player);
            
            retPlayer = player;
		}

        EmitSignal(SignalName.OnPlayBegin);
        return retPlayer;
    }

    public void StopAll()
    {
        foreach (var player in _inUseSpatial) {
            player.Stop();
        }
        
        foreach (var player in _inUseNonSpatial) {
            player.Stop();
        }
    }

    public void SetPauseAll(bool paused)
    {
        foreach (var player in _inUseSpatial) {
            player.StreamPaused = paused;
        }
        
        foreach (var player in _inUseNonSpatial) {
            player.StreamPaused = paused;
        }
    }

    private AudioStreamPlayer3D GetSpatialAudioStreamPlayer()
	{
		if (_freeSpatial.Count > 0) {
            var e = _freeSpatial[0];
            _freeSpatial.RemoveAt(0);
			return e;
		}

        if (InstanceCount == MaxInstances) {
            GD.PushError($"ERROR: Tried playing spatial sound {Sound.Name} on component {Name}, but reached max instances! Wrapping around...");
            _inUseSpatial[0].Stop();
            return _inUseSpatial[0];
        }
		
        InstanceCount += 1;
		var asp = new AudioStreamPlayer3D();
		AddChild(asp);
		return asp;
	}

	private AudioStreamPlayer GetNonSpatialAudioStreamPlayer()
	{
		if (_freeNonSpatial.Count > 0) {
            var e = _freeNonSpatial[0];
            _freeNonSpatial.RemoveAt(0);
			return e;
		}
		
        if (InstanceCount == MaxInstances) {
            GD.PushError($"ERROR: Tried playing non-spatial sound {Sound.Name} on component {Name}, but reached max instances! Wrapping around...");
            _inUseNonSpatial[0].Stop();
            return _inUseNonSpatial[0];
        }
		
        GD.Print("CREATED NEW SOUND");
        InstanceCount += 1;
		var asp = new AudioStreamPlayer();
		AddChild(asp);
		return asp;
	}
}