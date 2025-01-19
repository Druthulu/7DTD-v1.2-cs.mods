using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

public static class MiscFromXml
{
	public static IEnumerator Create(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			yield break;
		}
		foreach (XElement xelement in root.Elements("animation"))
		{
			XElement xelement2 = xelement;
			if (xelement2.Name == "animation")
			{
				using (IEnumerator<XElement> enumerator2 = xelement2.Elements("hold_type").GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						XElement element = enumerator2.Current;
						if (!element.HasAttribute("id"))
						{
							throw new Exception("Attribute 'id' missing in hold_type");
						}
						int num = 0;
						if (!int.TryParse(element.GetAttribute("id"), out num))
						{
							throw new Exception("Unknown hold_type id for animation");
						}
						float num2 = 0f;
						if (element.HasAttribute("ray_cast"))
						{
							num2 = StringParsers.ParseFloat(element.GetAttribute("ray_cast"), 0, -1, NumberStyles.Any);
						}
						float rayCastMoving = num2;
						if (element.HasAttribute("ray_cast_moving"))
						{
							rayCastMoving = StringParsers.ParseFloat(element.GetAttribute("ray_cast_moving"), 0, -1, NumberStyles.Any);
						}
						float num3 = Constants.cMinHolsterTime;
						if (element.HasAttribute("holster"))
						{
							num3 = Utils.FastMax(StringParsers.ParseFloat(element.GetAttribute("holster"), 0, -1, NumberStyles.Any), num3);
						}
						float num4 = Constants.cMinUnHolsterTime;
						if (element.HasAttribute("unholster"))
						{
							num4 = Utils.FastMax(StringParsers.ParseFloat(element.GetAttribute("unholster"), 0, -1, NumberStyles.Any), num4);
						}
						Vector3 position = Vector3.zero;
						if (element.HasAttribute("third_person_position"))
						{
							position = StringParsers.ParseVector3(element.GetAttribute("third_person_position"), 0, -1);
						}
						Vector3 rotation = Vector3.zero;
						if (element.HasAttribute("third_person_rotation"))
						{
							rotation = StringParsers.ParseVector3(element.GetAttribute("third_person_rotation"), 0, -1);
						}
						bool twoHanded = false;
						if (element.HasAttribute("two_handed"))
						{
							twoHanded = StringParsers.ParseBool(element.GetAttribute("two_handed"), 0, -1, true);
						}
						AnimationDelayData.AnimationDelay[num] = new AnimationDelayData.AnimationDelays(num2, rayCastMoving, num3, num4, twoHanded);
						AnimationGunjointOffsetData.AnimationGunjointOffset[num] = new AnimationGunjointOffsetData.AnimationGunjointOffsets(position, rotation);
					}
					continue;
				}
			}
			if (xelement.Name == "smell")
			{
				foreach (XElement element2 in xelement.Elements("smell"))
				{
					if (!element2.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing in smell");
					}
					string attribute = element2.GetAttribute("name");
					if (!element2.HasAttribute("range"))
					{
						throw new Exception("Attribute 'range' missing in smell name='" + attribute + "'");
					}
					float range = StringParsers.ParseFloat(element2.GetAttribute("range"), 0, -1, NumberStyles.Any);
					if (!element2.HasAttribute("belt_range"))
					{
						throw new Exception("Attribute 'belt_range' missing in smell name='" + attribute + "'");
					}
					float beltRange = StringParsers.ParseFloat(element2.GetAttribute("belt_range"), 0, -1, NumberStyles.Any);
					float heatMapStrength = 0f;
					if (element2.HasAttribute("heat_map_strength"))
					{
						heatMapStrength = StringParsers.ParseFloat(element2.GetAttribute("heat_map_strength"), 0, -1, NumberStyles.Any);
					}
					float num5 = 100f;
					if (element2.HasAttribute("heat_map_time"))
					{
						num5 = StringParsers.ParseFloat(element2.GetAttribute("heat_map_time"), 0, -1, NumberStyles.Any);
					}
					num5 *= 10f;
					AIDirectorData.AddSmell(attribute, new AIDirectorData.Smell(attribute, range, beltRange, heatMapStrength, (ulong)num5));
				}
			}
		}
		int num6 = 0;
		int num7 = 0;
		foreach (XElement xelement3 in root.Elements("trigger_effects"))
		{
			foreach (XElement element3 in xelement3.Elements("trigger_effect"))
			{
				if (element3.HasAttribute("type_ds"))
				{
					num6++;
				}
				else if (element3.HasAttribute("type_xb"))
				{
					num7++;
				}
			}
		}
		TriggerEffectManager.ControllerTriggerEffectsDS.Clear();
		TriggerEffectManager.ControllerTriggerEffectsXb.Clear();
		TriggerEffectManager.ControllerTriggerEffectsDS.EnsureCapacity(num6);
		TriggerEffectManager.ControllerTriggerEffectsXb.EnsureCapacity(num7);
		using (IEnumerator<XElement> enumerator = root.Elements("trigger_effects").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement4 = enumerator.Current;
				foreach (XElement xelement5 in xelement4.Elements("trigger_effect"))
				{
					if (!xelement5.HasAttribute("name"))
					{
						Debug.LogError("Every Trigger effect requires a name attribute set to a unique value");
					}
					else
					{
						string attribute2 = xelement5.GetAttribute("name");
						string text;
						string text2;
						if (xelement5.TryGetAttribute("type_ds", out text))
						{
							byte[] strengths;
							TriggerEffectManager.EffectDualsense effect;
							byte frequency;
							byte strength;
							byte position2;
							byte endPosition;
							byte amplitudeEndStrength;
							if (text.ContainsCaseInsensitive("Weapon"))
							{
								if (!TriggerEffectDualsenseParsers.ParseWeaponEffects(text, xelement5, attribute2, out strengths, out effect, out frequency, out strength, out position2, out endPosition, out amplitudeEndStrength))
								{
									continue;
								}
							}
							else if (text.ContainsCaseInsensitive("Feedback"))
							{
								if (text.ContainsCaseInsensitive("MultipointFeedback") || text.ContainsCaseInsensitive("FeedbackMultipoint"))
								{
									strength = 0;
									position2 = 0;
									endPosition = 0;
									amplitudeEndStrength = 0;
									frequency = 0;
									effect = TriggerEffectManager.EffectDualsense.FeedbackMultipoint;
									if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(text, xelement5, attribute2, out strengths))
									{
										continue;
									}
								}
								else if (text.ContainsCaseInsensitive("SlopeFeedback") || text.ContainsCaseInsensitive("FeedbackSlope"))
								{
									strengths = null;
									frequency = 0;
									effect = TriggerEffectManager.EffectDualsense.FeedbackSlope;
									if (!TriggerEffectDualsenseParsers.ParseStartEndPosition(text, xelement5, attribute2, out position2, out endPosition))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseStartEndStrengths(text, xelement5, attribute2, out strength, out amplitudeEndStrength))
									{
										continue;
									}
								}
								else
								{
									strength = 0;
									strengths = null;
									endPosition = 0;
									amplitudeEndStrength = 0;
									frequency = 0;
									effect = TriggerEffectManager.EffectDualsense.FeedbackSingle;
									if (!TriggerEffectDualsenseParsers.ParsePosition(text, xelement5, attribute2, out position2))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseStrength(text, xelement5, attribute2, out strength))
									{
										continue;
									}
								}
							}
							else if (text.ContainsCaseInsensitive("Vibration"))
							{
								if (text.ContainsCaseInsensitive("Multipoint"))
								{
									strength = 0;
									amplitudeEndStrength = 0;
									frequency = 0;
									position2 = 0;
									endPosition = 0;
									effect = TriggerEffectManager.EffectDualsense.VibrationMultipoint;
									if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(text, xelement5, attribute2, out strengths))
									{
										continue;
									}
								}
								else
								{
									if (text.ContainsCaseInsensitive("Slope"))
									{
										Debug.LogWarning("Trigger effectType: " + text + "(type_ds) is not implemented");
										continue;
									}
									endPosition = 0;
									strength = 0;
									strengths = null;
									effect = TriggerEffectManager.EffectDualsense.VibrationSingle;
									if (!TriggerEffectDualsenseParsers.ParsePosition(text, xelement5, attribute2, out position2) || !TriggerEffectDualsenseParsers.ParseAmplitude(text, xelement5, attribute2, out amplitudeEndStrength))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseFrequency(text, xelement5, attribute2, out frequency))
									{
										continue;
									}
								}
							}
							else
							{
								if (text.ContainsCaseInsensitive("NoEffect"))
								{
									Debug.LogError("Trigger effectType cannot be redefined: " + attribute2 + ":NoEffect");
									continue;
								}
								Debug.LogError("Trigger effectType Not supported: " + attribute2 + ":" + text);
								continue;
							}
							if (!TriggerEffectManager.ControllerTriggerEffectsDS.TryAdd(attribute2, new TriggerEffectManager.TriggerEffectDS
							{
								Effect = effect,
								Frequency = frequency,
								Position = position2,
								EndPosition = endPosition,
								Strength = strength,
								AmplitudeEndStrength = amplitudeEndStrength,
								Strengths = strengths
							}))
							{
								Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute2);
							}
						}
						else if (xelement5.TryGetAttribute("type_xb", out text2))
						{
							TriggerEffectManager.EffectXbox effect2;
							float startPosition;
							float endPosition2;
							float strength2;
							float endStrength;
							if (text2.ContainsCaseInsensitive("Feedback"))
							{
								if (text2.ContainsCaseInsensitive("SlopeFeedback") || text2.ContainsCaseInsensitive("FeedbackSlope"))
								{
									effect2 = TriggerEffectManager.EffectXbox.FeedbackSlope;
									if (!TriggerEffectXboxParsers.ParseStartEndPosition(text2, xelement5, attribute2, out startPosition, out endPosition2))
									{
										continue;
									}
									if (!TriggerEffectXboxParsers.ParseStartEndStrength(text2, xelement5, attribute2, out strength2, out endStrength))
									{
										continue;
									}
								}
								else
								{
									startPosition = 0f;
									endPosition2 = 0f;
									endStrength = 0f;
									effect2 = TriggerEffectManager.EffectXbox.FeedbackSingle;
									if (!TriggerEffectXboxParsers.ParseStrength(text2, xelement5, attribute2, out strength2))
									{
										continue;
									}
								}
							}
							else if (text2.ContainsCaseInsensitive("Vibration"))
							{
								if (text2.ContainsCaseInsensitive("SlopeVibration") || text2.ContainsCaseInsensitive("VibrationSlope"))
								{
									effect2 = TriggerEffectManager.EffectXbox.VibrationSlope;
									if (!TriggerEffectXboxParsers.ParseStartEndPosition(text2, xelement5, attribute2, out startPosition, out endPosition2))
									{
										continue;
									}
									if (!TriggerEffectXboxParsers.ParseStartEndStrength(text2, xelement5, attribute2, out strength2, out endStrength))
									{
										continue;
									}
								}
								else
								{
									startPosition = 0f;
									endPosition2 = 0f;
									endStrength = 0f;
									effect2 = TriggerEffectManager.EffectXbox.VibrationSingle;
									if (!TriggerEffectXboxParsers.ParseStrength(text2, xelement5, attribute2, out strength2))
									{
										continue;
									}
								}
							}
							else
							{
								if (text2.ContainsCaseInsensitive("NoEffect"))
								{
									Debug.LogError("Trigger effectType cannot be redefined: " + text2);
									continue;
								}
								Debug.LogError("Trigger effectType Not supported: " + text2);
								continue;
							}
							if (!TriggerEffectManager.ControllerTriggerEffectsXb.TryAdd(attribute2, new TriggerEffectManager.TriggerEffectXB
							{
								Effect = effect2,
								StartPosition = startPosition,
								EndPosition = endPosition2,
								Strength = strength2,
								EndStrength = endStrength
							}))
							{
								Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute2);
							}
						}
						else
						{
							Debug.LogError("Trigger effect needs an xb_type or a ds_type: " + attribute2);
						}
					}
				}
			}
			yield break;
		}
		yield break;
	}
}
