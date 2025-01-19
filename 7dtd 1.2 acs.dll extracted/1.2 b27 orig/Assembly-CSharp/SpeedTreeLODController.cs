using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTreeLODController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.lodGroup = base.gameObject.GetComponent<LODGroup>();
		base.GetComponentsInChildren<Tree>(SpeedTreeLODController.tempTrees);
		foreach (Tree tree in SpeedTreeLODController.tempTrees)
		{
			Renderer renderer;
			if (tree.hasSpeedTreeWind && tree.TryGetComponent<Renderer>(out renderer) && renderer.motionVectorGenerationMode == MotionVectorGenerationMode.Object)
			{
				tree.gameObject.AddMissingComponent<SpeedTreeMotionVectorHelper>().Init(renderer);
			}
		}
		SpeedTreeLODController.tempTrees.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		int lodCount = this.lodGroup.lodCount;
		if (lodCount > 0)
		{
			this.lods = this.lodGroup.GetLODs();
			float screenRelativeTransitionHeight = Mathf.Lerp(0.02f, 0.03f, this.lodGroup.size / 5f);
			this.lods[lodCount - 1].screenRelativeTransitionHeight = screenRelativeTransitionHeight;
			if (lodCount > 1)
			{
				float num = 0.17f;
				float num2 = (0.58f - num) / ((float)(lodCount - 2) + 0.001f);
				for (int i = lodCount - 2; i >= 0; i--)
				{
					this.lods[i].screenRelativeTransitionHeight = num;
					num += num2;
				}
			}
			this.lodGroup.SetLODs(this.lods);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LODGroup lodGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LOD[] lods;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Tree> tempTrees = new List<Tree>();
}
