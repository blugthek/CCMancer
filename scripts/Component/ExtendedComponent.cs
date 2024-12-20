using System;
using System.Collections.Generic;
using Godot;

public class ExtendedComponent
{
    private readonly Dictionary<string, object> _data = new();

    public void Set<T>(string key, T value)
    {
        _data[key] = value;
    }

    public void Get<T>(string key, Action<T> callback)
    {
        if (_data.TryGetValue(key, out var value) && value is T typedValue)
        {
            callback(typedValue);
        }
        else
        {
            throw new KeyNotFoundException($"Key '{key}' not found or incompatible type.");
        }
    }
}

public struct HealthData
{
    public int CurrentHealth;
    public int MaxHealth;

    public HealthData(int current, int max)
    {
        CurrentHealth = current;
        MaxHealth = max;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(CurrentHealth - damage, 0);
    }

    public void TakeDamage(DamagePayload[] allDamage)
    {
        foreach (var damage in allDamage)
        {
            CurrentHealth = Math.Max(CurrentHealth - damage.DamageAmount, 0);
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }
}

public struct UnitStat
{
    public int HorizontalSpeed;
    public int JumpSpeed;
    public int AttackSpeed;
    public int ActionSpeed;

    public DamagePayload[] UnitDamage;

    public ArmourPayload[] ArmourPayloads;

    // Constructor initializing all fields
    public UnitStat(int movementSpeed, int jumpSpeed, int attackSpeed, DamagePayload[] damagePayloads)
    {
        HorizontalSpeed = movementSpeed > 0 ? movementSpeed : 0;
        JumpSpeed = jumpSpeed > 0 ? jumpSpeed : 0; // Use jumpSpeed parameter
        AttackSpeed = attackSpeed > 0 ? attackSpeed : 0; // Use attackSpeed parameter
        ActionSpeed = 0; // Default or calculated value (update if needed)
        UnitDamage = damagePayloads ?? new DamagePayload[0]; // Assign parameter or empty array
        ArmourPayloads = new ArmourPayload[0];

        GD.Print(HorizontalSpeed);
    }
}

public enum DamageType
{
    Hyle,
    Psyche,
    Chaos,
}

public struct DamagePayload
{
    public DamageType DamageType;
    public int DamageAmount;
}

public struct ArmourPayload
{
    public DamageType ArmourType;
    public int SubtractAmount;
}