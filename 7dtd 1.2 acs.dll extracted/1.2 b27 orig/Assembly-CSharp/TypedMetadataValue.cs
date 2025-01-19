using System;
using System.IO;

public class TypedMetadataValue
{
	public static TypedMetadataValue.TypeTag StringToTag(string str)
	{
		if (str == "float")
		{
			return TypedMetadataValue.TypeTag.Float;
		}
		if (str == "int")
		{
			return TypedMetadataValue.TypeTag.Integer;
		}
		if (!(str == "string"))
		{
			return TypedMetadataValue.TypeTag.None;
		}
		return TypedMetadataValue.TypeTag.String;
	}

	public TypedMetadataValue(object val, TypedMetadataValue.TypeTag tag)
	{
		this.typeTag = tag;
		if (this.ValueMatchesTag(val, tag))
		{
			this.value = val;
		}
	}

	public object GetValue()
	{
		return this.value;
	}

	public bool SetValue(object val)
	{
		if (this.ValueMatchesTag(val, this.typeTag))
		{
			this.value = val;
			return true;
		}
		return false;
	}

	public bool ValueMatchesTag(object val, TypedMetadataValue.TypeTag tag)
	{
		if (val == null)
		{
			return false;
		}
		switch (tag)
		{
		case TypedMetadataValue.TypeTag.Float:
			return val is float;
		case TypedMetadataValue.TypeTag.Integer:
			return val is int;
		case TypedMetadataValue.TypeTag.String:
			return val is string;
		default:
			return false;
		}
	}

	public static void Write(TypedMetadataValue tmv, BinaryWriter writer)
	{
		if (tmv == null)
		{
			return;
		}
		writer.Write((int)tmv.typeTag);
		switch (tmv.typeTag)
		{
		case TypedMetadataValue.TypeTag.Float:
			writer.Write((float)tmv.value);
			return;
		case TypedMetadataValue.TypeTag.Integer:
			writer.Write((int)tmv.value);
			return;
		case TypedMetadataValue.TypeTag.String:
			writer.Write((string)tmv.value);
			return;
		default:
			return;
		}
	}

	public static TypedMetadataValue Read(BinaryReader reader)
	{
		object val = null;
		TypedMetadataValue.TypeTag tag = (TypedMetadataValue.TypeTag)reader.ReadInt32();
		switch (tag)
		{
		case TypedMetadataValue.TypeTag.Float:
			val = reader.ReadSingle();
			break;
		case TypedMetadataValue.TypeTag.Integer:
			val = reader.ReadInt32();
			break;
		case TypedMetadataValue.TypeTag.String:
			val = reader.ReadString();
			break;
		}
		return new TypedMetadataValue(val, tag);
	}

	public override bool Equals(object other)
	{
		TypedMetadataValue typedMetadataValue = other as TypedMetadataValue;
		return typedMetadataValue != null && this.value.Equals(typedMetadataValue.value) && this.typeTag.Equals(typedMetadataValue.typeTag);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public TypedMetadataValue Clone()
	{
		object val = null;
		switch (this.typeTag)
		{
		case TypedMetadataValue.TypeTag.Float:
			val = (this.value as float?);
			break;
		case TypedMetadataValue.TypeTag.Integer:
			val = (this.value as int?);
			break;
		case TypedMetadataValue.TypeTag.String:
			val = (this.value as string);
			break;
		}
		return new TypedMetadataValue(val, this.typeTag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TypedMetadataValue.TypeTag typeTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public object value;

	public enum TypeTag
	{
		None,
		Float,
		Integer,
		String
	}
}
