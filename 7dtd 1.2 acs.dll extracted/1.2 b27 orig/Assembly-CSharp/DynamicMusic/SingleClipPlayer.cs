using System;
using System.Collections;
using UniLinq;
using UnityEngine;

namespace DynamicMusic
{
	public abstract class SingleClipPlayer : Section, ISection, IPlayable, IFadeable, ICleanable
	{
		public override void Init()
		{
			base.Init();
			this.LoadRoutine = GameManager.Instance.StartCoroutine(this.InitializationCoroutine());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SingleClip GetSingleClip()
		{
			return Content.AllContent.OfType<SingleClip>().First((SingleClip c) => c.Section == base.Sect);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual IEnumerator InitializationCoroutine()
		{
			this.IsReady = false;
			this.clip = this.GetSingleClip();
			if (this.clip == null)
			{
				Log.Warning("content could not be cast as an object of type 'SingleClip'");
			}
			else
			{
				yield return this.clip.Load();
				this.src = UnityEngine.Object.Instantiate<AudioSource>(Resources.Load<AudioSource>(Content.SourcePathFor[base.Sect]));
				this.src.name = base.Sect.ToString();
				this.src.transform.SetParent(Section.parent.transform);
				this.src.clip = this.clip.Clip;
				this.IsReady = (base.IsInitialized = true);
			}
			this.LoadRoutine = null;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SingleClipPlayer()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SingleClip clip;
	}
}
