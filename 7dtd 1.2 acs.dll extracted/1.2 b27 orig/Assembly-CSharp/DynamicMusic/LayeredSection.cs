using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public abstract class LayeredSection<T> : Section, ISection, IPlayable, IFadeable, ICleanable where T : ILayerMixer, new()
	{
		public LayeredSection()
		{
			this.Mixer = Activator.CreateInstance<T>();
		}

		public override void Init()
		{
			base.Init();
			this.Reset();
			this.IsReady = (base.IsInitialized = true);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Reset()
		{
			this.cursor = 0;
			this.IsReady = (this.IsDone = false);
			if (this.src)
			{
				this.src.loop = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return this.LoadContentCoroutine();
			AudioSource src = this.src;
			if (src != null)
			{
				src.Play();
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator LoadContentCoroutine()
		{
			this.IsReady = false;
			this.Mixer.Sect = base.Sect;
			yield return this.Mixer.Load();
			if (!this.src)
			{
				using (LayeredSection<T>.s_LoadContentMarker.Auto())
				{
					this.streamsToFill = new Queue<float[]>();
					this.initialLoad = true;
					this.src = UnityEngine.Object.Instantiate<AudioSource>(Resources.Load<AudioSource>(Content.SourcePathFor[base.Sect]));
					this.src.transform.SetParent(Section.parent.transform);
					this.src.name = base.Sect.ToString();
					this.src.loop = true;
					this.src.priority = 0;
					this.src.clip = AudioClip.Create(base.Sect.ToString(), Content.SamplesFor[base.Sect], 2, 44100, true, new AudioClip.PCMReaderCallback(this.PCMReaderCallback));
				}
				yield return null;
				this.initialLoad = false;
				while (this.streamsToFill.Count > 0)
				{
					this.FillStream(this.streamsToFill.Dequeue());
					yield return null;
				}
				this.streamsToFill = null;
			}
			this.IsReady = (base.IsInitialized = true);
			this.LoadRoutine = null;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void PCMReaderCallback(float[] data)
		{
			if (this.initialLoad)
			{
				this.streamsToFill.Enqueue(data);
				return;
			}
			this.FillStream(data);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void FillStream(float[] data)
		{
			if (data == null)
			{
				Log.Warning("FillStream: data is null");
				return;
			}
			using (LayeredSection<T>.s_FillStreamMarker.Auto())
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (i >= data.Length)
					{
						Log.Warning("FillStream: data out of bounds. Data Length {0} i = {1}", new object[]
						{
							data.Length,
							i
						});
						break;
					}
					int num = i;
					int num2 = this.cursor;
					this.cursor = num2 + 1;
					data[num] = this.Mixer[num2];
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_FillStreamMarker = new ProfilerMarker("DynamicMusic.LayeredSection.FillStream");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_LoadContentMarker = new ProfilerMarker("DynamicMusic.LayeredSection.LoadContentCoroutine");

		[PublicizedFrom(EAccessModifier.Protected)]
		public T Mixer;

		public int cursor;

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<float[]> streamsToFill;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool initialLoad;
	}
}
