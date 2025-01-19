using System;
using System.IO;

public class WaypointCollection
{
	public void Read(BinaryReader _br)
	{
		this.Collection.Clear();
		int version = (int)_br.ReadByte();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			Waypoint waypoint = new Waypoint();
			waypoint.Read(_br, version);
			this.Collection.Add(waypoint);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(5);
		int num = 0;
		for (int i = 0; i < this.Collection.list.Count; i++)
		{
			if (this.Collection.list[i].IsSaved)
			{
				num++;
			}
		}
		_bw.Write((ushort)num);
		for (int j = 0; j < num; j++)
		{
			if (this.Collection.list[j].IsSaved)
			{
				this.Collection.list[j].Write(_bw);
			}
		}
	}

	public WaypointCollection Clone()
	{
		WaypointCollection waypointCollection = new WaypointCollection();
		for (int i = 0; i < this.Collection.list.Count; i++)
		{
			waypointCollection.Collection.Add(this.Collection.list[i].Clone());
		}
		return waypointCollection;
	}

	public bool ContainsWaypoint(Waypoint _wp)
	{
		return this.Collection.hashSet.Contains(_wp);
	}

	public Waypoint GetEntityVehicleWaypoint(int _entityID)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (waypoint.entityId == _entityID)
			{
				return waypoint;
			}
		}
		return null;
	}

	public void UpdateVehicleWaypointsToMatch(WaypointCollection _other)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (waypoint.entityId != -1)
			{
				Waypoint entityVehicleWaypoint = _other.GetEntityVehicleWaypoint(waypoint.entityId);
				if (entityVehicleWaypoint != null)
				{
					entityVehicleWaypoint.pos = waypoint.pos;
					entityVehicleWaypoint.ownerId = waypoint.ownerId;
				}
				else
				{
					_other.Collection.Add(waypoint.Clone());
				}
			}
		}
	}

	public Waypoint GetWaypointForNavObject(NavObject nav)
	{
		foreach (Waypoint waypoint in this.Collection.list)
		{
			if (nav == waypoint.navObject)
			{
				return waypoint;
			}
		}
		return null;
	}

	public const int cCurrentSaveVersion = 5;

	public HashSetList<Waypoint> Collection = new HashSetList<Waypoint>();
}
