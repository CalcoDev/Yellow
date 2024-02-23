using Godot;

namespace Yellow.Components;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float Health { 
        get => _health;
        set {
            float diff = _health - value;
            TakeDamage(diff);
        }
    }
    private float _health = 100f;

    [Signal]
    public delegate void OnHealthChangedEventHandler(float previous, float current, float maximum);

    [Signal]
    public delegate void OnDiedEventHandler();

    public bool IsDead => _health <= 0;

    public override void _Ready()
    {
        _health = MaxHealth;
    }

    public float GetHealthPercentage()
    {
        if (MaxHealth <= 0)
            return 0;

        return Mathf.Min(_health / MaxHealth, 1f);
    }

    public void TakeDamage(float damage)
    {
        var oldHealth = _health;
        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            EmitSignal(SignalName.OnDied);
        }

        EmitSignal(SignalName.OnHealthChanged, oldHealth, _health, MaxHealth);
    }

    public void Heal(float amount)
    {
        var oldHealth = _health;

        _health += amount;
        if (_health > MaxHealth)
            _health = MaxHealth;

        EmitSignal(SignalName.OnHealthChanged, oldHealth, _health, MaxHealth);
    }

    public void ResetHealth()
    {
        var oldHealth = _health;
        _health = MaxHealth;
        EmitSignal(SignalName.OnHealthChanged, oldHealth, _health, MaxHealth);
    }
}