using System;
using System.Collections.Generic;

[Serializable]
public class Stat
{
	public float baseValue;
	protected bool IsDirty = true;
	protected float LastBaseValue;
	protected float _value;
	public virtual float Value
	{
		get
		{
			if (IsDirty || LastBaseValue != baseValue)
			{
				LastBaseValue = baseValue;
				_value = CalculateFinalValue();
				IsDirty = false;
			}
			return _value;
		}
	}
	public List<StatModifier> StatModifiers;
	public Stat()
	{
		StatModifiers = new List<StatModifier>();
	}
	public Stat(float baseValue) : this()
	{
		this.baseValue = baseValue;
	}

	public virtual void ModifyModifier(StatModifier mod)
	{
		IsDirty = true;
		var exitsMod = StatModifiers.Find(x => x.Source == mod.Source);
		if (exitsMod == null)
			StatModifiers.Add(mod);
		else
		{
			exitsMod.Value = mod.Value;
		}
	}
	
	public virtual void AddModifier(StatModifier mod)
	{
		IsDirty = true;
		StatModifiers.Add(mod);
	}
	public virtual bool RemoveModifier(StatModifier mod)
	{
		if (!StatModifiers.Remove(mod)) return false;
		IsDirty = true;
		return true;
	}
	public virtual void RemoveAllModifiers()
	{
		StatModifiers.Clear();
		IsDirty = true;
	}
	public virtual bool RemoveAllModifiersFromSource(object source)
	{
		var numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

		if (numRemovals <= 0) return false;
		IsDirty = true;
		return true;
	}

	protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
	{
		if (a.Order < b.Order) return -1;
		return a.Order > b.Order ? 1 : 0;
	}

	protected virtual float CalculateFinalValue()
	{
		var finalValue = baseValue;
		var sumPercentAdd = 0f;

		StatModifiers.Sort(CompareModifierOrder);

		StatModifier limitModify = null;
		for (var i = 0; i < StatModifiers.Count; i++)
		{
			var mod = StatModifiers[i];

			switch (mod.Type)
			{
				case StatModType.Flat:
					finalValue += mod.Value;
					break;
				case StatModType.PercentAdd:
				{
					sumPercentAdd += mod.Value;

					if (i + 1 >= StatModifiers.Count || StatModifiers[i + 1].Type != StatModType.PercentAdd)
					{
						finalValue *= 1 + sumPercentAdd;
						sumPercentAdd = 0;
					}

					break;
				}
				case StatModType.PercentMult:
					finalValue *= 1 + mod.Value;
					break;
				case StatModType.Limit:
					if (limitModify is null || limitModify.Value < mod.Value)
						limitModify = mod;
					break;
				default:
					break;
			}
		}
		if (limitModify is not null && finalValue > limitModify.Value)
			AddModifier(new(-(finalValue - limitModify.Value),
				StatModType.Flat, "limit"));

		// Workaround for float calculation errors, like displaying 12.00001 instead of 12
		return (float) Math.Round(finalValue, 4);
	}
	public static Stat Parse(string s)
	{
		return new Stat(float.Parse(s));
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
