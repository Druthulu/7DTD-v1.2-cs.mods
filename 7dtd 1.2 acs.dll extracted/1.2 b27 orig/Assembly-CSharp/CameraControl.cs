using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.originalRotation = base.transform.localRotation;
		this.textObject.GetComponentInChildren<Text>().text = "PAUSED";
		this.cameraLight = base.transform.GetComponent<Light>();
		this.cameraLight.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.bPaused = !this.bPaused;
		}
		this.textObject.SetActive(this.bPaused);
		if (this.bPaused)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			this.cameraLight.enabled = !this.cameraLight.enabled;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0f)
		{
			this.cameraLight.spotAngle += 3f;
		}
		else if (axis < 0f)
		{
			this.cameraLight.spotAngle -= 3f;
		}
		Vector3 a = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			a += base.transform.forward;
		}
		if (Input.GetKey(KeyCode.S))
		{
			a -= base.transform.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			a -= base.transform.right;
		}
		if (Input.GetKey(KeyCode.D))
		{
			a += base.transform.right;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			a += base.transform.up;
		}
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
		{
			a -= base.transform.up;
		}
		float d = Input.GetKey(KeyCode.LeftShift) ? (this.speed * 2f) : this.speed;
		base.transform.position += a * d;
		this.rotationX += Input.GetAxis("Mouse X") * this.sensitivityX;
		this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
		Quaternion rhs = Quaternion.AngleAxis(this.rotationX, Vector3.up);
		Quaternion rhs2 = Quaternion.AngleAxis(this.rotationY, -Vector3.right);
		base.transform.localRotation = this.originalRotation * rhs * rhs2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion originalRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light cameraLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationY;

	public float sensitivityX = 2f;

	public float sensitivityY = 2f;

	public float speed = 0.1f;

	public GameObject textObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bPaused;
}
