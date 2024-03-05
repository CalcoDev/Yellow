using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    private readonly HashSet<AudioStreamPlayer> _freeNonSpatial = new();
	private readonly HashSet<AudioStreamPlayer3D> _freeSpatial = new();
	
	private readonly HashSet<AudioStreamPlayer> _inUseNonSpatial = new();
	private readonly HashSet<AudioStreamPlayer3D> _inUseSpatial= new();
    private readonly HashSet<Node> _forceStopped = new();
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
        bool overrideLoop = false,
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
			var player = GetSpatialAudioStreamPlayer(overrideLoop);
			player.GlobalPosition = position;
			player.VolumeDb = vol;

            var stream = Sound.Streams[_soundStreamIndex];
            if (player.Stream != stream) {
                player.Stream = stream;
            }
            if (!_inUseSpatial.Contains(player) || !overrideLoop) {
                player.Play();
            }
            if (!_freeSpatial.Contains(player) && !_inUseSpatial.Contains(player)) {
                player.Finished += () => {
                    if (Sound.Looping && !_forceStopped.Contains(player)) {
                        player.Play();
                        EmitSignal(SignalName.OnPlayEnd);
                        return;
                    }
                    _inUseSpatial.Remove(player);
                    _freeSpatial.Add(player);
                    EmitSignal(SignalName.OnPlayEnd);
                };
            }

            if (!_inUseSpatial.Contains(player)) {
                _inUseSpatial.Add(player);
            }
           
            retPlayer = player;
		} else {
			var player = GetNonSpatialAudioStreamPlayer(overrideLoop);
			player.VolumeDb = vol;
   
            var stream = Sound.Streams[_soundStreamIndex];
            if (player.Stream != stream) {
                player.Stream = stream;
            }
            if (!_inUseNonSpatial.Contains(player) || !overrideLoop) {
                player.Play();
            }
            if (!_freeNonSpatial.Contains(player) && !_inUseNonSpatial.Contains(player)) {
                player.Finished += () => {
                    if (Sound.Looping && !_forceStopped.Contains(player)) {
                        player.Play();
                        EmitSignal(SignalName.OnPlayEnd);
                        return;
                    }
                    _inUseNonSpatial.Remove(player);
                    _freeNonSpatial.Add(player);
                    EmitSignal(SignalName.OnPlayEnd);
                };
            }
            if (!_inUseNonSpatial.Contains(player)) {
                _inUseNonSpatial.Add(player);
            }
            
            retPlayer = player;
		}

        _forceStopped.Remove(retPlayer);
        EmitSignal(SignalName.OnPlayBegin);
        return retPlayer;
    }

    public void StopAll()
    {
        foreach (var player in _inUseSpatial) {
            player.Stop();
            player.EmitSignal(AudioStreamPlayer3D.SignalName.Finished);
            _forceStopped.Add(player);
        }
        
        foreach (var player in _inUseNonSpatial) {
            player.Stop();
            player.EmitSignal(AudioStreamPlayer.SignalName.Finished);
            _forceStopped.Add(player);
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

    private AudioStreamPlayer3D GetSpatialAudioStreamPlayer(bool overrideLoop)
	{
        if (overrideLoop && _inUseNonSpatial.Count > 0) {
            return _inUseSpatial.Last();
        }

		if (_freeSpatial.Count > 0) {
            var e = _freeSpatial.First();
            _freeSpatial.Remove(e);
			return e;
		}

        if (InstanceCount == MaxInstances) {
            GD.PushError($"ERROR: Tried playing spatial sound {Sound.Name} on component {Name}, but reached max instances! Wrapping around...");
            var e = _inUseSpatial.First();
            e.Stop();
            return e;
        }
		
        InstanceCount += 1;
        var asp = new AudioStreamPlayer3D();
		AddChild(asp);
		return asp;
	}

	private AudioStreamPlayer GetNonSpatialAudioStreamPlayer(bool overrideLoop)
	{
        if (overrideLoop && _inUseNonSpatial.Count > 0) {
            return _inUseNonSpatial.Last();
        }

		if (_freeNonSpatial.Count > 0) {
            var e = _freeNonSpatial.First();
            _freeNonSpatial.Remove(e);
			return e;
		}
		
        if (InstanceCount == MaxInstances) {
            GD.PushError($"ERROR: Tried playing non-spatial sound {Sound.Name} on component {Name}, but reached max instances! Wrapping around...");
            var e = _inUseNonSpatial.First();
            e.Stop();
            return e;
        }
		
        InstanceCount += 1;
		var asp = new AudioStreamPlayer();
		AddChild(asp);
		return asp;
	}
}