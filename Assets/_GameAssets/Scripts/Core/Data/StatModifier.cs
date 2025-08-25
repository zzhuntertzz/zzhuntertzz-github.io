using System;

public enum StatModType
{
	Flat = 100,
	PercentAdd = 200,
	PercentMult = 300,
	
	Limit = 1000,
}
[Serializable]
public class StatModifier
{
	public float Value;
	public StatModType Type;
	public int Order;
	public string Source;
	public StatModifier(float value, StatModType type, int order, string source = "")
	{
		Value = value;
		Type = type;
		Order = order;
		Source = source;
	}
	public StatModifier(float value, StatModType type) : this(value, type, (int)type) { }

	public StatModifier(float value, StatModType type, string source) : this(value, type, (int)type, source) { }
}