﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
	public class XmlData
	{
		public XmlData()
		{
			this.soundGroupName = "Invalid";
			this.maxVoices = 1;
			this.maxVoicesPerEntity = 5;
			this.audioClipMap = new List<ClipSourceMap>();
			this.noiseData = new NoiseData();
			this.localCrouchVolumeScale = 0.5f;
			this.crouchNoiseScale = 0.5f;
			this.noiseScale = 1f;
			this.maxRepeatRate = 0.1f;
			this.voicesPlaying = 0;
			this.lastRecordedPlayTime = Time.time;
			this.maxVolume = 1f;
			this.sequence = false;
			this.runningVolumeScale = 1f;
			this.lowestPitch = 1f;
			this.highestPitch = 1f;
			this.distantFadeStart = -1f;
			this.distantFadeEnd = -1f;
			this.channel = XmlData.Channel.Environment;
			this.priority = 99;
		}

		public bool Update()
		{
			if (this.maxRepeatRate > 0f)
			{
				float time = Time.time;
				float num = time - this.lastRecordedPlayTime;
				this.voicesPlaying = Mathf.Clamp(this.voicesPlaying - (int)(num / this.maxRepeatRate), 0, 999);
				if (this.voicesPlaying >= this.maxVoices)
				{
					return false;
				}
				if (num < this.maxRepeatRate)
				{
					return false;
				}
				this.voicesPlaying++;
				this.lastRecordedPlayTime = time;
			}
			return true;
		}

		public List<ClipSourceMap> GetClipList()
		{
			if (Manager.Instance.bUseAltSounds && this.altAudioClipMap != null)
			{
				return this.altAudioClipMap;
			}
			if (this.hasProfanity && GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity))
			{
				return this.cleanClipMap;
			}
			return this.audioClipMap;
		}

		public ClipSourceMap GetRandomClip()
		{
			List<ClipSourceMap> clipList = this.GetClipList();
			int num = 0;
			int count = clipList.Count;
			if (count > 1)
			{
				if (count == 2)
				{
					num = (this.randomLastIndex ^ 1);
				}
				else
				{
					num = Manager.random.RandomRange(count - 1);
					if (num >= this.randomLastIndex)
					{
						num++;
					}
				}
				this.randomLastIndex = num;
			}
			return clipList[num];
		}

		public void AddAltClipSourceMap(ClipSourceMap csm)
		{
			if (this.altAudioClipMap == null)
			{
				this.altAudioClipMap = new List<ClipSourceMap>();
			}
			this.altAudioClipMap.Add(csm);
		}

		public string soundGroupName;

		public int maxVoices;

		public List<ClipSourceMap> audioClipMap;

		public List<ClipSourceMap> altAudioClipMap;

		public List<ClipSourceMap> cleanClipMap;

		public NoiseData noiseData;

		public float localCrouchVolumeScale;

		public float runningVolumeScale;

		public float crouchNoiseScale;

		public float noiseScale;

		public float maxRepeatRate;

		public int voicesPlaying;

		public float lastRecordedPlayTime;

		public bool playImmediate;

		public bool sequence;

		public float maxVolume;

		public float lowestPitch;

		public float highestPitch;

		public float distantFadeStart;

		public float distantFadeEnd;

		public int maxVoicesPerEntity;

		public bool hasProfanity;

		public XmlData.Channel channel;

		public int priority;

		public bool vibratesController = true;

		public float vibrationStrengthMultiplier = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int randomLastIndex;

		public enum Channel
		{
			Mouth,
			Environment
		}
	}
}
