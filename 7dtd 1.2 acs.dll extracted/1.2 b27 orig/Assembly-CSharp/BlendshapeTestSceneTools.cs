using System;
using UnityEngine;

public class BlendshapeTestSceneTools : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.myAnim = base.GetComponent<Animator>();
		this.myAudio = base.GetComponent<AudioSource>();
		if (this.myAnim != null)
		{
			this.maxLayers = this.myAnim.layerCount;
			Debug.Log("Number of layers in controller is " + this.maxLayers.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space) && this.myAudio != null)
		{
			this.currentAnim++;
			if (this.currentAnim == this.maxLayers)
			{
				this.currentAnim = 0;
				for (int i = 1; i < this.maxLayers - 1; i++)
				{
					this.myAnim.SetLayerWeight(i, 0f);
				}
			}
			Debug.Log("Current Layer is: " + this.currentAnim.ToString());
			this.myAudio.clip = this.audioClips[this.currentAnim];
			this.myAudio.Play();
			this.myAnim.SetLayerWeight(this.currentAnim, 1f);
			this.myAnim.SetTrigger("RestartAnim");
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime, 0f);
		}
		Input.GetKeyUp(KeyCode.W);
		Input.GetKeyUp(KeyCode.S);
		Input.GetKeyUp(KeyCode.E);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator myAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource myAudio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int layerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int maxLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnRate = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float endLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentAnim;

	public AudioClip[] audioClips;
}
