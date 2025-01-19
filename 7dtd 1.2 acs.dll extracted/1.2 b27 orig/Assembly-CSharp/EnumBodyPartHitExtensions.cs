﻿using System;

public static class EnumBodyPartHitExtensions
{
	public static bool IsArm(this EnumBodyPartHit bodyPart)
	{
		return (bodyPart & EnumBodyPartHit.Arms) > EnumBodyPartHit.None;
	}

	public static bool IsLeg(this EnumBodyPartHit bodyPart)
	{
		return (bodyPart & EnumBodyPartHit.Legs) > EnumBodyPartHit.None;
	}

	public static bool IsLeftLeg(this EnumBodyPartHit bodyPart)
	{
		return (bodyPart & (EnumBodyPartHit.LeftUpperLeg | EnumBodyPartHit.LeftLowerLeg)) > EnumBodyPartHit.None;
	}

	public static bool IsRightLeg(this EnumBodyPartHit bodyPart)
	{
		return (bodyPart & (EnumBodyPartHit.RightUpperLeg | EnumBodyPartHit.RightLowerLeg)) > EnumBodyPartHit.None;
	}

	public static BodyPrimaryHit LowerToUpperLimb(this BodyPrimaryHit _primary)
	{
		switch (_primary)
		{
		case BodyPrimaryHit.LeftLowerArm:
			return BodyPrimaryHit.LeftUpperArm;
		case BodyPrimaryHit.RightLowerArm:
			return BodyPrimaryHit.RightUpperArm;
		case BodyPrimaryHit.LeftLowerLeg:
			return BodyPrimaryHit.LeftUpperLeg;
		case BodyPrimaryHit.RightLowerLeg:
			return BodyPrimaryHit.RightUpperLeg;
		default:
			return _primary;
		}
	}

	public static EnumBodyPartHit ToFlag(this BodyPrimaryHit _parts)
	{
		return (EnumBodyPartHit)(1 << _parts - BodyPrimaryHit.Torso);
	}

	public static bool IsMultiHit(this EnumBodyPartHit _parts)
	{
		EnumBodyPartHit enumBodyPartHit = _parts.ToPrimary().ToFlag();
		return (_parts & ~enumBodyPartHit) > EnumBodyPartHit.None;
	}

	public static BodyPrimaryHit ToPrimary(this EnumBodyPartHit _part)
	{
		if ((_part & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.Head;
		}
		if ((_part & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.LeftUpperLeg;
		}
		if ((_part & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.RightUpperLeg;
		}
		if ((_part & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.LeftLowerLeg;
		}
		if ((_part & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.RightLowerLeg;
		}
		if ((_part & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.LeftUpperArm;
		}
		if ((_part & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.RightUpperArm;
		}
		if ((_part & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.LeftLowerLeg;
		}
		if ((_part & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.RightLowerLeg;
		}
		if (_part != EnumBodyPartHit.None)
		{
			return BodyPrimaryHit.Torso;
		}
		return BodyPrimaryHit.None;
	}
}
