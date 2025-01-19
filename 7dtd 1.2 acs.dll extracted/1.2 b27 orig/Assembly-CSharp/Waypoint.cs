using System;
using System.IO;

public class Waypoint
{
	public int InviterEntityId
	{
		get
		{
			return this.inviterEntityId;
		}
		set
		{
			this.inviterEntityId = value;
			PlatformUserIdentifierAbs primaryId = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(value).PrimaryId;
			this.name.Update(this.name.Text, primaryId);
		}
	}

	public Waypoint()
	{
		this.name = new AuthoredText();
	}

	public Waypoint Clone()
	{
		return new Waypoint
		{
			pos = this.pos,
			icon = this.icon,
			name = AuthoredText.Clone(this.name),
			bTracked = this.bTracked,
			ownerId = this.ownerId,
			entityId = this.entityId,
			navObject = this.navObject,
			hiddenOnCompass = this.hiddenOnCompass,
			bIsAutoWaypoint = this.bIsAutoWaypoint,
			bUsingLocalizationId = this.bUsingLocalizationId,
			IsSaved = this.IsSaved,
			inviterEntityId = this.inviterEntityId
		};
	}

	public void Read(BinaryReader _br, int version = 5)
	{
		this.pos = StreamUtils.ReadVector3i(_br);
		this.icon = _br.ReadString();
		this.name = AuthoredText.FromStream(_br);
		this.bTracked = _br.ReadBoolean();
		if (version > 2)
		{
			this.hiddenOnCompass = _br.ReadBoolean();
		}
		if (version > 1)
		{
			this.ownerId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.entityId = _br.ReadInt32();
		}
		if (version > 3)
		{
			this.bIsAutoWaypoint = _br.ReadBoolean();
			this.bUsingLocalizationId = _br.ReadBoolean();
		}
		if (version > 4)
		{
			this.inviterEntityId = _br.ReadInt32();
		}
	}

	public void Write(BinaryWriter _bw)
	{
		StreamUtils.Write(_bw, this.pos);
		if (this.icon == null)
		{
			_bw.Write("");
		}
		else
		{
			_bw.Write(this.icon);
		}
		AuthoredText.ToStream(this.name, _bw);
		_bw.Write(this.bTracked);
		_bw.Write(this.hiddenOnCompass);
		this.ownerId.ToStream(_bw, false);
		_bw.Write(this.entityId);
		_bw.Write(this.bIsAutoWaypoint);
		_bw.Write(this.bUsingLocalizationId);
		_bw.Write(this.inviterEntityId);
	}

	public override bool Equals(object obj)
	{
		Waypoint waypoint = obj as Waypoint;
		return waypoint != null && (waypoint.pos.Equals(this.pos) && waypoint.icon == this.icon && waypoint.name.Text == this.name.Text && object.Equals(waypoint.ownerId, this.ownerId)) && waypoint.entityId == this.entityId;
	}

	public override int GetHashCode()
	{
		int num = (((17 * 31 + this.pos.GetHashCode()) * 31 + this.icon.GetHashCode()) * 31 + this.name.Text.GetHashCode()) * 31;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = this.ownerId;
		return (num + ((platformUserIdentifierAbs != null) ? platformUserIdentifierAbs.GetHashCode() : 0)) * 31 + this.entityId.GetHashCode();
	}

	public bool CanBeViewedBy(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.entityId == -1 || (_userIdentifier != null && _userIdentifier.Equals(this.ownerId));
	}

	public override string ToString()
	{
		string[] array = new string[6];
		array[0] = "Waypoint name:";
		int num = 1;
		AuthoredText authoredText = this.name;
		array[num] = ((authoredText != null) ? authoredText.ToString() : null);
		array[2] = " icon:";
		array[3] = this.icon;
		array[4] = " Entity ID:";
		array[5] = this.entityId.ToString();
		return string.Concat(array);
	}

	public Vector3i pos;

	public string icon;

	public AuthoredText name;

	public bool bTracked;

	public PlatformUserIdentifierAbs ownerId;

	public int entityId = -1;

	public long MapObjectKey;

	public bool hiddenOnCompass;

	public NavObject navObject;

	public bool bIsAutoWaypoint;

	public bool bUsingLocalizationId;

	public bool IsSaved = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inviterEntityId;
}
