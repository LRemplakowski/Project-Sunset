using System;
using System.Collections.Generic;
using SunsetSystems.Dice;
using SunsetSystems.UI.Utils;

[Serializable]
public abstract class BaseStat : IUserInfertaceDataProvider<BaseStat>, IIntValue
{
    public event Action OnValueChange;

    public abstract string Name { get; }

    [field: NonSerialized]
    protected List<Modifier> Modifiers { get; private set; } = new();

    public BaseStat UIData => this;

    public BaseStat(BaseStat existing)
    {
        Modifiers = new();
        existing.Modifiers?.ForEach(m  => Modifiers.Add(new(m.Value, m.Type, m.Name)));
    }

    public BaseStat()
    {

    }

    public virtual void SetValue(int value)
    {
        SetValueImpl(value);
        OnValueChange?.Invoke();
    }

    protected abstract void SetValueImpl(int value);

    public int GetValue()
    {
        return GetValue(true);
    }

    public int GetValue(bool includeModifiers)
    {
        if (includeModifiers)
            return GetValue(ModifierType.ALL);
        else
            return GetValue(ModifierType.NONE);
    }

    public abstract int GetValue(ModifierType modifierTypesFlag);

    public virtual void AddModifier(int value, ModifierType type, string name)
    {
        Modifiers.Add(new Modifier(value, type, name));
        OnValueChange?.Invoke();
    }

    public virtual void AddModifier(Modifier modifier)
    {
        Modifiers.Add(modifier);
        OnValueChange?.Invoke();
    }

    public virtual void AddModifiers(List<Modifier> modifiers)
    {
        modifiers.ForEach(m => this.Modifiers.Add(m));
    }

    public virtual void RemoveModifiersOfType(ModifierType type)
    {
        Modifiers.RemoveAll(m => (m.Type & ModifierType.ALL) > 0);
        OnValueChange?.Invoke();
    }

    public virtual void RemoveModifier(Modifier modifier)
    {
        Modifiers.Remove(modifier);
        OnValueChange?.Invoke();
    }

    public virtual List<Modifier> GetModifiers()
    {
        return Modifiers;
    }
}
