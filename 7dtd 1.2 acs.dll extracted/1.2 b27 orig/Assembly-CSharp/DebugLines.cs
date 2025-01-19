using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugLines : MonoBehaviour
{
	public static DebugLines Create(string _name, Transform _parentT, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = DebugLines.Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		return debugLines;
	}

	public static DebugLines Create(string _name, Transform _parentT, Vector3 _pos1, Vector3 _pos2, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = DebugLines.Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, _pos1 - Origin.position);
		lineRenderer.SetPosition(1, _pos2 - Origin.position);
		return debugLines;
	}

	public static DebugLines CreateAttached(string _name, Transform _parentT, Vector3 _pos1, Vector3 _pos2, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = DebugLines.Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.useWorldSpace = false;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		lineRenderer.positionCount = 2;
		Vector3 position = _parentT.InverseTransformPoint(_pos1 - Origin.position);
		lineRenderer.SetPosition(0, position);
		Vector3 position2 = _parentT.InverseTransformPoint(_pos2 - Origin.position);
		lineRenderer.SetPosition(1, position2);
		return debugLines;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DebugLines Create(string _name, Transform _parentT)
	{
		DebugLines debugLines = null;
		string text = "DebugLines";
		if (_name != null)
		{
			text += _name;
			DebugLines.lines.TryGetValue(_name, out debugLines);
		}
		if (!debugLines)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("Prefabs/Debug/DebugLines"), _parentT);
			gameObject.name = text;
			debugLines = gameObject.transform.GetComponent<DebugLines>();
			if (_name != null)
			{
				debugLines.keyName = _name;
				DebugLines.lines[_name] = debugLines;
			}
		}
		else
		{
			debugLines.transform.SetParent(_parentT, false);
		}
		debugLines.line = debugLines.GetComponent<LineRenderer>();
		debugLines.line.positionCount = 0;
		return debugLines;
	}

	public void AddPoint(Vector3 _pos)
	{
		int positionCount = this.line.positionCount;
		this.line.positionCount = positionCount + 1;
		Vector3 position = _pos - Origin.position;
		if (!this.line.useWorldSpace)
		{
			position = base.transform.InverseTransformPoint(position);
		}
		this.line.SetPosition(positionCount, position);
	}

	public void AddCube(Vector3 _cornerPos1, Vector3 _cornerPos2)
	{
		Vector3 pos = _cornerPos1;
		Vector3 vector = _cornerPos2 - _cornerPos1;
		this.AddPoint(pos);
		pos.x += vector.x;
		this.AddPoint(pos);
		pos.y += vector.y;
		this.AddPoint(pos);
		pos.y -= vector.y;
		this.AddPoint(pos);
		pos.z += vector.z;
		this.AddPoint(pos);
		pos.y += vector.y;
		this.AddPoint(pos);
		pos.y -= vector.y;
		this.AddPoint(pos);
		pos.x -= vector.x;
		this.AddPoint(pos);
		pos.y += vector.y;
		this.AddPoint(pos);
		pos.y -= vector.y;
		this.AddPoint(pos);
		pos.z -= vector.z;
		this.AddPoint(pos);
		pos.y += vector.y;
		this.AddPoint(pos);
		pos.x += vector.x;
		this.AddPoint(pos);
		pos.z += vector.z;
		this.AddPoint(pos);
		pos.x -= vector.x;
		this.AddPoint(pos);
		pos.z -= vector.z;
		this.AddPoint(pos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.duration -= Time.deltaTime;
		if (this.duration <= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (this.keyName != null)
		{
			DebugLines.lines.Remove(this.keyName);
		}
	}

	public static Vector3 InsideOffsetV = new Vector3(0.05f, 0.05f, 0.05f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cName = "DebugLines";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Dictionary<string, DebugLines> lines = new Dictionary<string, DebugLines>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string keyName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LineRenderer line;
}
