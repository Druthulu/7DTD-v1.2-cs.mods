using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class PhysicsBodyColliderConfiguration
{
	public PhysicsBodyColliderConfiguration()
	{
	}

	public PhysicsBodyColliderConfiguration(PhysicsBodyColliderConfiguration otherConfig)
	{
		this.Tag = otherConfig.Tag;
		this.CollisionLayer = otherConfig.CollisionLayer;
		this.RagdollLayer = otherConfig.RagdollLayer;
		this.CollisionScale = otherConfig.CollisionScale;
		this.CollisionOffset = otherConfig.CollisionOffset;
		this.RagdollScale = otherConfig.RagdollScale;
		this.RagdollOffset = otherConfig.RagdollOffset;
		this.Path = otherConfig.Path;
		this.Type = otherConfig.Type;
		this.EnabledFlags = otherConfig.EnabledFlags;
	}

	public void Write(XmlElement _elem)
	{
		XmlElement node = _elem.AddXmlElement("collider");
		node.AddXmlKeyValueProperty("tag", this.Tag);
		node.AddXmlKeyValueProperty("path", this.Path);
		node.AddXmlKeyValueProperty("collisionLayer", this.CollisionLayer.ToString());
		node.AddXmlKeyValueProperty("ragdollLayer", this.RagdollLayer.ToString());
		node.AddXmlKeyValueProperty("collisionScale", PhysicsBodyColliderConfiguration.vecToString(this.CollisionScale));
		node.AddXmlKeyValueProperty("ragdollScale", PhysicsBodyColliderConfiguration.vecToString(this.RagdollScale));
		node.AddXmlKeyValueProperty("collisionOffset", PhysicsBodyColliderConfiguration.vecToString(this.CollisionOffset));
		node.AddXmlKeyValueProperty("ragdollOffset", PhysicsBodyColliderConfiguration.vecToString(this.RagdollOffset));
		node.AddXmlKeyValueProperty("type", this.Type.ToStringCached<EnumColliderType>());
		string text = "";
		if ((this.EnabledFlags & EnumColliderEnabledFlags.Collision) != EnumColliderEnabledFlags.Disabled)
		{
			text += "collision";
		}
		if ((this.EnabledFlags & EnumColliderEnabledFlags.Ragdoll) != EnumColliderEnabledFlags.Disabled)
		{
			if (text.Length == 0)
			{
				text = "ragdoll";
			}
			else
			{
				text += ";ragdoll";
			}
		}
		if (text.Length == 0)
		{
			text = "disabled";
		}
		node.AddXmlKeyValueProperty("flags", text);
	}

	public static PhysicsBodyColliderConfiguration Read(XElement _e)
	{
		PhysicsBodyColliderConfiguration physicsBodyColliderConfiguration = new PhysicsBodyColliderConfiguration();
		DynamicProperties dynamicProperties = new DynamicProperties();
		foreach (XElement propertyNode in _e.Elements("property"))
		{
			dynamicProperties.Add(propertyNode, true);
		}
		physicsBodyColliderConfiguration.Tag = dynamicProperties.GetStringValue("tag");
		physicsBodyColliderConfiguration.Path = dynamicProperties.GetStringValue("path");
		if (dynamicProperties.Contains("collisionLayer"))
		{
			physicsBodyColliderConfiguration.CollisionLayer = int.Parse(dynamicProperties.GetStringValue("collisionLayer"));
			physicsBodyColliderConfiguration.RagdollLayer = int.Parse(dynamicProperties.GetStringValue("ragdollLayer"));
		}
		else
		{
			physicsBodyColliderConfiguration.CollisionLayer = int.Parse(dynamicProperties.GetStringValue("layer"));
			physicsBodyColliderConfiguration.RagdollLayer = physicsBodyColliderConfiguration.CollisionLayer;
		}
		physicsBodyColliderConfiguration.CollisionScale = PhysicsBodyColliderConfiguration.vecFromString(dynamicProperties.GetStringValue("collisionScale"));
		physicsBodyColliderConfiguration.RagdollScale = PhysicsBodyColliderConfiguration.vecFromString(dynamicProperties.GetStringValue("ragdollScale"));
		physicsBodyColliderConfiguration.CollisionOffset = PhysicsBodyColliderConfiguration.vecFromString(dynamicProperties.GetStringValue("collisionOffset"));
		physicsBodyColliderConfiguration.RagdollOffset = PhysicsBodyColliderConfiguration.vecFromString(dynamicProperties.GetStringValue("ragdollOffset"));
		physicsBodyColliderConfiguration.Type = EnumUtils.Parse<EnumColliderType>(dynamicProperties.GetStringValue("type"), false);
		physicsBodyColliderConfiguration.EnabledFlags = EnumColliderEnabledFlags.Disabled;
		string stringValue = dynamicProperties.GetStringValue("flags");
		if (stringValue != "disabled")
		{
			foreach (string a in stringValue.Split(';', StringSplitOptions.None))
			{
				if (a == "collision")
				{
					physicsBodyColliderConfiguration.EnabledFlags |= EnumColliderEnabledFlags.Collision;
				}
				else if (a == "ragdoll")
				{
					physicsBodyColliderConfiguration.EnabledFlags |= EnumColliderEnabledFlags.Ragdoll;
				}
			}
		}
		if (physicsBodyColliderConfiguration.RagdollLayer == 0 || physicsBodyColliderConfiguration.RagdollLayer == 27)
		{
			physicsBodyColliderConfiguration.RagdollLayer = 21;
		}
		return physicsBodyColliderConfiguration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string vecToString(Vector3 vec)
	{
		return string.Concat(new string[]
		{
			vec.x.ToCultureInvariantString(),
			" ",
			vec.y.ToCultureInvariantString(),
			" ",
			vec.z.ToCultureInvariantString()
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 vecFromString(string str)
	{
		string[] array = str.Split(' ', StringSplitOptions.None);
		if (array.Length == 3)
		{
			return new Vector3(StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any), StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any), StringParsers.ParseFloat(array[2], 0, -1, NumberStyles.Any));
		}
		if (array.Length < 1)
		{
			throw new FormatException("Vector3 expected");
		}
		return new Vector3(StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any), StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any), StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any));
	}

	public string Tag = "";

	public int CollisionLayer;

	public int RagdollLayer;

	public Vector3 CollisionScale = Vector3.one;

	public Vector3 RagdollScale = Vector3.one;

	public Vector3 CollisionOffset = Vector3.zero;

	public Vector3 RagdollOffset = Vector3.zero;

	public string Path = "";

	public EnumColliderType Type = EnumColliderType.Detail;

	public EnumColliderEnabledFlags EnabledFlags = EnumColliderEnabledFlags.All;
}
