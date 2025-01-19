using System;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.targetPosition = base.transform.position;
		this.targetRotation = base.transform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1))
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				Vector3 eulerAngles = this.light.transform.eulerAngles;
				eulerAngles.y += Input.GetAxis("Mouse X") * this.turnSpeed;
				eulerAngles.x -= Input.GetAxis("Mouse Y") * this.turnSpeed;
				this.light.transform.rotation = Quaternion.Euler(eulerAngles);
			}
			else
			{
				Vector3 eulerAngles2 = this.targetRotation.eulerAngles;
				eulerAngles2.y += Input.GetAxis("Mouse X") * this.turnSpeed;
				eulerAngles2.x -= Input.GetAxis("Mouse Y") * this.turnSpeed;
				this.targetRotation = Quaternion.Euler(eulerAngles2);
			}
		}
		if (Input.GetMouseButton(2))
		{
			float d = Input.GetAxis("Mouse X") * this.panSpeed * Time.deltaTime;
			float d2 = Input.GetAxis("Mouse Y") * this.panSpeed * Time.deltaTime;
			this.targetPosition -= base.transform.right * d + base.transform.up * d2;
		}
		float d3 = Input.GetKey(KeyCode.Q) ? (this.moveSpeed * Time.deltaTime) : 0f;
		float d4 = Input.GetKey(KeyCode.E) ? (this.moveSpeed * Time.deltaTime) : 0f;
		float d5 = Input.GetAxis("Horizontal") * this.moveSpeed * Time.deltaTime;
		float d6 = Input.GetAxis("Vertical") * this.moveSpeed * Time.deltaTime;
		float d7 = Input.GetKey(KeyCode.LeftShift) ? this.shiftSpeed : 1f;
		this.targetPosition += d7 * (base.transform.right * d5 + base.transform.forward * d6 + base.transform.up * d3 - base.transform.up * d4);
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Input.GetMouseButton(1))
		{
			this.targetPosition += base.transform.forward * axis * this.zoomSpeed;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, this.targetPosition, Time.deltaTime * this.moveSmoothing);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, this.targetRotation, Time.deltaTime * this.turnSmoothing);
	}

	public Light light;

	public float moveSpeed = 10f;

	public float turnSpeed = 4f;

	public float zoomSpeed = 10f;

	public float panSpeed = 10f;

	public float shiftSpeed = 4f;

	public float moveSmoothing = 5f;

	public float turnSmoothing = 5f;

	public float zoomSmoothing = 5f;

	public float panSmoothing = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 targetPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion targetRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMouseInWindow = true;
}
