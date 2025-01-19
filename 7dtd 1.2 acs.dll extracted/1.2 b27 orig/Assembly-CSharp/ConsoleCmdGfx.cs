﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGfx : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"gfx"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Graphics commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Graphics commands:\naf <value> - anisotropic filtering off, on or force on (0, 1, 2)\ndr <scale> <min> <max> - set dynamic res scale (0 auto, .1 to 1 force, -1 for off) and min/max FPS\ndt <value> - toggle distant terrain or set value (0 or 1)\ndti <value> - set distant terrain instancing (0 or 1)\ndtmaxlod <value> - set distant terrain max LOD (0 to 5)\ndtpix <value> - set distant terrain pixel error (1 to 200)\nkey name <value> - set shader keyword (0 or 1)\npp name <value> - set postprocessing name (enable, ambientOcclusion (ao), auto exposure (ae), bloom, colorGrading (cg), etc.) to value (0 or 1)\nres <width> <height> - set screen resolution\nresetrev - clear graphics preset revision\nskin <value> - set skin bone count (1, 2, 4, 5+ (all))\nst name <value> - set streaming name (budget (0 disables), discard, forceload, reduction) to value\ntex - show texture info\ntexbias <value> - set bias on all textures\ntexlimit <value> - set limit (0-x)\nviewdist <value> - Set view distance in chunks";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 1632531277U)
		{
			if (num <= 1227161470U)
			{
				if (num <= 862676408U)
				{
					if (num != 804546073U)
					{
						if (num == 862676408U)
						{
							if (text == "skin")
							{
								int num2 = 2;
								if (_params.Count >= 2)
								{
									int.TryParse(_params[1], out num2);
									if (num2 < 1)
									{
										num2 = 1;
									}
									if (num2 > 4)
									{
										num2 = 255;
									}
								}
								QualitySettings.skinWeights = (SkinWeights)num2;
								return;
							}
						}
					}
					else if (text == "res")
					{
						int num3 = 1920;
						int num4 = 1080;
						if (_params.Count >= 3)
						{
							int.TryParse(_params[1], out num3);
							num3 = Utils.FastClamp(num3, 640, 8192);
							int.TryParse(_params[2], out num4);
							num4 = Utils.FastClamp(num4, 480, 8192);
						}
						GameOptionsManager.SetResolution(num3, num4, FullScreenMode.Windowed);
						return;
					}
				}
				else if (num != 942383232U)
				{
					if (num != 1193061779U)
					{
						if (num == 1227161470U)
						{
							if (text == "af")
							{
								int num5 = 0;
								if (_params.Count >= 2)
								{
									int.TryParse(_params[1], out num5);
								}
								QualitySettings.anisotropicFiltering = ((num5 == 0) ? AnisotropicFiltering.Disable : ((num5 == 1) ? AnisotropicFiltering.Enable : AnisotropicFiltering.ForceEnable));
								return;
							}
						}
					}
					else if (text == "dr")
					{
						if (GameManager.Instance.World != null)
						{
							float scale = -1f;
							if (_params.Count >= 2)
							{
								float.TryParse(_params[1], out scale);
							}
							float fpsMin = -1f;
							if (_params.Count >= 3)
							{
								float.TryParse(_params[2], out fpsMin);
							}
							float fpsMax = -1f;
							if (_params.Count >= 4)
							{
								float.TryParse(_params[3], out fpsMax);
							}
							GameManager.Instance.World.GetPrimaryPlayer().renderManager.SetDynamicResolution(scale, fpsMin, fpsMax);
							return;
						}
						return;
					}
				}
				else if (text == "be")
				{
					int num6 = 0;
					if (_params.Count >= 2)
					{
						int.TryParse(_params[1], out num6);
					}
					string name = "";
					if (_params.Count >= 3)
					{
						name = _params[2];
					}
					int num7 = GameManager.Instance.World.m_ChunkManager.SetBlockEntitiesVisible(num6 != 0, name);
					Log.Out("Set {0}", new object[]
					{
						num7
					});
					return;
				}
			}
			else if (num <= 1293727493U)
			{
				if (num != 1238466413U)
				{
					if (num != 1263673922U)
					{
						if (num == 1293727493U)
						{
							if (text == "dt")
							{
								if (this.FindDistantTerrain())
								{
									if (_params.Count < 2)
									{
										ConsoleCmdGfx.distantTerrainObj.SetActive(!ConsoleCmdGfx.distantTerrainObj.activeSelf);
									}
									else
									{
										ConsoleCmdGfx.distantTerrainObj.SetActive(_params[1] != "0");
									}
									SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain " + (ConsoleCmdGfx.distantTerrainObj.activeSelf ? "active" : "not active"));
									return;
								}
								return;
							}
						}
					}
					else if (text == "st")
					{
						if (_params.Count < 2)
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No cmd name");
							return;
						}
						float num8 = 0f;
						if (_params.Count >= 3)
						{
							float.TryParse(_params[2], out num8);
						}
						string text2 = _params[1].ToLower();
						num = <PrivateImplementationDetails>.ComputeStringHash(text2);
						if (num <= 3775669363U)
						{
							if (num <= 1871725842U)
							{
								if (num != 968532104U)
								{
									if (num != 1871725842U)
									{
										goto IL_AC0;
									}
									if (!(text2 == "reduction"))
									{
										goto IL_AC0;
									}
									goto IL_AB7;
								}
								else
								{
									if (!(text2 == "budget"))
									{
										goto IL_AC0;
									}
									goto IL_A8B;
								}
							}
							else if (num != 2815551048U)
							{
								if (num != 3648362799U)
								{
									if (num != 3775669363U)
									{
										goto IL_AC0;
									}
									if (!(text2 == "d"))
									{
										goto IL_AC0;
									}
									goto IL_A93;
								}
								else if (!(text2 == "active"))
								{
									goto IL_AC0;
								}
							}
							else
							{
								if (!(text2 == "forceload"))
								{
									goto IL_AC0;
								}
								goto IL_AA5;
							}
						}
						else if (num <= 3826002220U)
						{
							if (num != 3809224601U)
							{
								if (num != 3826002220U)
								{
									goto IL_AC0;
								}
								if (!(text2 == "a"))
								{
									goto IL_AC0;
								}
							}
							else
							{
								if (!(text2 == "f"))
								{
									goto IL_AC0;
								}
								goto IL_AA5;
							}
						}
						else if (num != 3876335077U)
						{
							if (num != 4035330433U)
							{
								if (num != 4144776981U)
								{
									goto IL_AC0;
								}
								if (!(text2 == "r"))
								{
									goto IL_AC0;
								}
								goto IL_AB7;
							}
							else
							{
								if (!(text2 == "discard"))
								{
									goto IL_AC0;
								}
								goto IL_A93;
							}
						}
						else
						{
							if (!(text2 == "b"))
							{
								goto IL_AC0;
							}
							goto IL_A8B;
						}
						bool streamingMipmapsActive;
						if (_params.Count >= 3 && bool.TryParse(_params[2], out streamingMipmapsActive))
						{
							QualitySettings.streamingMipmapsActive = streamingMipmapsActive;
							return;
						}
						QualitySettings.streamingMipmapsActive = !QualitySettings.streamingMipmapsActive;
						return;
						IL_A8B:
						QualitySettings.streamingMipmapsMemoryBudget = num8;
						return;
						IL_A93:
						Texture.streamingTextureDiscardUnusedMips = (num8 != 0f);
						return;
						IL_AA5:
						Texture.streamingTextureForceLoadAll = (num8 != 0f);
						return;
						IL_AB7:
						QualitySettings.streamingMipmapsMaxLevelReduction = (int)num8;
						return;
						IL_AC0:
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown st " + _params[1]);
						return;
					}
				}
				else if (text == "lodbias")
				{
					float lodBias = GameOptionsManager.GetLODBias();
					if (_params.Count >= 2)
					{
						lodBias = float.Parse(_params[1]);
					}
					QualitySettings.lodBias = lodBias;
					return;
				}
			}
			else if (num != 1397500531U)
			{
				if (num != 1460122494U)
				{
					if (num == 1632531277U)
					{
						if (text == "pp")
						{
							if (_params.Count < 2)
							{
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No effect name");
								return;
							}
							float value = 0f;
							if (_params.Count >= 3)
							{
								float.TryParse(_params[2], out value);
							}
							float value2 = 0f;
							if (_params.Count >= 4)
							{
								float.TryParse(_params[3], out value2);
							}
							string command = _params[1].ToLower();
							this.SetPostProcessing(command, value, value2);
							return;
						}
					}
				}
				else if (text == "dtpix")
				{
					if (this.FindDistantTerrain())
					{
						float num9 = 5f;
						if (_params.Count >= 2)
						{
							float.TryParse(_params[1], out num9);
						}
						Terrain[] componentsInChildren = ConsoleCmdGfx.distantTerrainObj.GetComponentsInChildren<Terrain>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].heightmapPixelError = num9;
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain pixerr {0}", new object[]
						{
							num9
						});
						return;
					}
					return;
				}
			}
			else if (text == "texbias")
			{
				float mipMapBias = 0f;
				if (_params.Count >= 2)
				{
					mipMapBias = float.Parse(_params[1]);
				}
				Texture[] array = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];
				for (int j = 0; j < array.Length; j++)
				{
					array[j].mipMapBias = mipMapBias;
				}
				return;
			}
		}
		else if (num <= 3018460268U)
		{
			if (num <= 2266829395U)
			{
				if (num != 1746258028U)
				{
					if (num == 2266829395U)
					{
						if (text == "d2")
						{
							ConsoleCmdGfx.debugFloat2 = 0f;
							if (_params.Count >= 2)
							{
								float.TryParse(_params[1], out ConsoleCmdGfx.debugFloat2);
								return;
							}
							return;
						}
					}
				}
				else if (text == "key")
				{
					if (_params.Count < 2)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No key name");
						return;
					}
					int num10 = 0;
					if (_params.Count >= 3)
					{
						int.TryParse(_params[2], out num10);
					}
					string keyword = _params[1].ToUpper();
					Shader.DisableKeyword(keyword);
					if (num10 != 0)
					{
						Shader.EnableKeyword(keyword);
						return;
					}
					return;
				}
			}
			else if (num != 2701180604U)
			{
				if (num != 2869024766U)
				{
					if (num == 3018460268U)
					{
						if (text == "dtmaxlod")
						{
							if (this.FindDistantTerrain())
							{
								int num11 = 0;
								if (_params.Count >= 2)
								{
									int.TryParse(_params[1], out num11);
								}
								Terrain[] componentsInChildren = ConsoleCmdGfx.distantTerrainObj.GetComponentsInChildren<Terrain>();
								for (int i = 0; i < componentsInChildren.Length; i++)
								{
									componentsInChildren[i].heightmapMaximumLOD = num11;
								}
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain maxlod {0}", new object[]
								{
									num11
								});
								return;
							}
							return;
						}
					}
				}
				else if (text == "tex")
				{
					Resources.UnloadUnusedAssets();
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Textures:");
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" limit {0}, stream {1} (reduction {2})", new object[]
					{
						GameRenderManager.TextureMipmapLimit,
						QualitySettings.streamingMipmapsActive,
						QualitySettings.streamingMipmapsMaxLevelReduction
					});
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" mem - current {0} MB, target {1} MB, desired {2} MB, total {3} MB", new object[]
					{
						Texture.currentTextureMemory / 1048576UL,
						Texture.targetTextureMemory / 1048576UL,
						Texture.desiredTextureMemory / 1048576UL,
						Texture.totalTextureMemory / 1048576UL
					});
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" normal - count {0}, mem {1} MB", new object[]
					{
						Texture.nonStreamingTextureCount,
						Texture.nonStreamingTextureMemory / 1048576UL
					});
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" streaming - count {0}, pending {1}, loading {2}", new object[]
					{
						Texture.streamingTextureCount,
						Texture.streamingTexturePendingLoadCount,
						Texture.streamingTextureLoadingCount
					});
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" streaming - budget {0} MB, renderer count {1}, mip uploads {2}", new object[]
					{
						QualitySettings.streamingMipmapsMemoryBudget,
						Texture.streamingRendererCount,
						Texture.streamingMipmapUploadCount
					});
					return;
				}
			}
			else if (text == "mesh")
			{
				if (_params.Count < 4)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Missing params");
					return;
				}
				int num12 = int.Parse(_params[1]);
				string name2 = _params[2];
				float value3 = float.Parse(_params[3]);
				MeshDescription.meshes[num12].material.SetFloat(name2, value3);
				return;
			}
		}
		else if (num <= 3493117700U)
		{
			if (num != 3211183224U)
			{
				if (num != 3353837740U)
				{
					if (num == 3493117700U)
					{
						if (text == "dti")
						{
							if (this.FindDistantTerrain())
							{
								int num13 = 0;
								if (_params.Count >= 2)
								{
									int.TryParse(_params[1], out num13);
								}
								Terrain[] componentsInChildren = ConsoleCmdGfx.distantTerrainObj.GetComponentsInChildren<Terrain>();
								for (int i = 0; i < componentsInChildren.Length; i++)
								{
									componentsInChildren[i].drawInstanced = (num13 != 0);
								}
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain instanced {0}", new object[]
								{
									num13 != 0
								});
								return;
							}
							return;
						}
					}
				}
				else if (text == "texreport")
				{
					string filename = string.Empty;
					if (_params.Count >= 2)
					{
						filename = _params[1];
					}
					this.LogTextureReport(filename);
					return;
				}
			}
			else if (text == "viewdist")
			{
				if (GameManager.Instance.World == null)
				{
					int num14 = 6;
					if (_params.Count >= 2)
					{
						num14 = int.Parse(_params[1]);
						num14 = Utils.FastClamp(num14, 1, 22);
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("viewdist {0}", new object[]
					{
						num14
					});
					GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, num14);
					return;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Requires main menu");
				return;
			}
		}
		else if (num != 3529248109U)
		{
			if (num != 3775669363U)
			{
				if (num == 3895971341U)
				{
					if (text == "resetrev")
					{
						GamePrefs.Set(EnumGamePrefs.OptionsGfxResetRevision, 0);
						GamePrefs.Instance.Save();
						return;
					}
				}
			}
			else if (text == "d")
			{
				ConsoleCmdGfx.debugFloat = 0f;
				if (_params.Count >= 2)
				{
					float.TryParse(_params[1], out ConsoleCmdGfx.debugFloat);
					return;
				}
				return;
			}
		}
		else if (text == "texlimit")
		{
			int textureMipmapLimit = 0;
			if (_params.Count >= 2)
			{
				textureMipmapLimit = int.Parse(_params[1]);
			}
			GameRenderManager.TextureMipmapLimit = textureMipmapLimit;
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindDistantTerrain()
	{
		if (!ConsoleCmdGfx.distantTerrainObj)
		{
			ConsoleCmdGfx.distantTerrainObj = GameObject.Find("/DistantUnityTerrain");
			if (!ConsoleCmdGfx.distantTerrainObj)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain gameobject not found");
				return false;
			}
		}
		return true;
	}

	public void SetPostProcessing(string command, float value, float value2)
	{
		Camera main = Camera.main;
		if (!main)
		{
			return;
		}
		PostProcessLayer component = main.GetComponent<PostProcessLayer>();
		if (!component)
		{
			return;
		}
		PostProcessVolume component2 = main.GetComponent<PostProcessVolume>();
		if (!component2)
		{
			return;
		}
		HBAO component3 = main.GetComponent<HBAO>();
		NGSS_FrustumShadows_7DTD component4 = main.GetComponent<NGSS_FrustumShadows_7DTD>();
		PostProcessProfile profile = component2.profile;
		if (!profile)
		{
			return;
		}
		PostProcessLayer exists = null;
		PostProcessEffectSettings postProcessEffectSettings = null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(command);
		if (num <= 1697523182U)
		{
			if (num <= 1109718137U)
			{
				if (num <= 558893406U)
				{
					if (num != 212153001U)
					{
						if (num != 277167948U)
						{
							if (num != 558893406U)
							{
								goto IL_5FE;
							}
							if (!(command == "ssrit"))
							{
								goto IL_5FE;
							}
							profile.GetSetting<ScreenSpaceReflections>().maximumIterationCount.Override((int)value);
							value = 1f;
							goto IL_5FE;
						}
						else if (!(command == "ambientocclusion"))
						{
							goto IL_5FE;
						}
					}
					else
					{
						if (!(command == "antialiasing"))
						{
							goto IL_5FE;
						}
						goto IL_4ED;
					}
				}
				else if (num <= 821021760U)
				{
					if (num != 593936670U)
					{
						if (num != 821021760U)
						{
							goto IL_5FE;
						}
						if (!(command == "contactshadows"))
						{
							goto IL_5FE;
						}
						component4.enabled = (value != 0f);
						goto IL_5FE;
					}
					else
					{
						if (!(command == "debugviews"))
						{
							goto IL_5FE;
						}
						goto IL_5FE;
					}
				}
				else if (num != 954099112U)
				{
					if (num != 1109718137U)
					{
						goto IL_5FE;
					}
					if (!(command == "ao"))
					{
						goto IL_5FE;
					}
				}
				else
				{
					if (!(command == "colorgrading"))
					{
						goto IL_5FE;
					}
					goto IL_540;
				}
				component3.enabled = (value != 0f);
				goto IL_5FE;
			}
			if (num <= 1263850675U)
			{
				if (num != 1210383851U)
				{
					if (num != 1260172255U)
					{
						if (num != 1263850675U)
						{
							goto IL_5FE;
						}
						if (!(command == "vignette"))
						{
							goto IL_5FE;
						}
						postProcessEffectSettings = profile.GetSetting<Vignette>();
						goto IL_5FE;
					}
					else
					{
						if (!(command == "dv"))
						{
							goto IL_5FE;
						}
						goto IL_5FE;
					}
				}
				else
				{
					if (!(command == "ae"))
					{
						goto IL_5FE;
					}
					goto IL_4B2;
				}
			}
			else if (num <= 1479119945U)
			{
				if (num != 1277494327U)
				{
					if (num != 1479119945U)
					{
						goto IL_5FE;
					}
					if (!(command == "ca"))
					{
						goto IL_5FE;
					}
					goto IL_532;
				}
				else if (!(command == "aa"))
				{
					goto IL_5FE;
				}
			}
			else if (num != 1512675183U)
			{
				if (num != 1697523182U)
				{
					goto IL_5FE;
				}
				if (!(command == "mb"))
				{
					goto IL_5FE;
				}
				goto IL_582;
			}
			else
			{
				if (!(command == "cg"))
				{
					goto IL_5FE;
				}
				goto IL_540;
			}
			IL_4ED:
			if (!exists)
			{
				goto IL_5FE;
			}
			World world = GameManager.Instance.World;
			if (world == null)
			{
				goto IL_5FE;
			}
			world.GetPrimaryPlayer().renderManager.SetAntialiasing((int)value, value2, component);
			goto IL_5FE;
			IL_540:
			postProcessEffectSettings = profile.GetSetting<ColorGrading>();
			goto IL_5FE;
		}
		if (num <= 3292465347U)
		{
			if (num <= 2717086271U)
			{
				if (num != 2506126412U)
				{
					if (num != 2704977974U)
					{
						if (num != 2717086271U)
						{
							goto IL_5FE;
						}
						if (!(command == "fog"))
						{
							goto IL_5FE;
						}
						component.fog.enabled = (value != 0f);
						goto IL_5FE;
					}
					else
					{
						if (!(command == "ssrq"))
						{
							goto IL_5FE;
						}
						profile.GetSetting<ScreenSpaceReflections>().resolution.Override((value == 0f) ? ScreenSpaceReflectionResolution.Downsampled : ScreenSpaceReflectionResolution.FullSize);
						value = 1f;
						goto IL_5FE;
					}
				}
				else
				{
					if (!(command == "grain"))
					{
						goto IL_5FE;
					}
					postProcessEffectSettings = profile.GetSetting<Grain>();
					goto IL_5FE;
				}
			}
			else if (num <= 2923669535U)
			{
				if (num != 2747068069U)
				{
					if (num != 2923669535U)
					{
						goto IL_5FE;
					}
					if (!(command == "autoexposure"))
					{
						goto IL_5FE;
					}
					goto IL_4B2;
				}
				else if (!(command == "depthoffield"))
				{
					goto IL_5FE;
				}
			}
			else if (num != 2945169614U)
			{
				if (num != 3292465347U)
				{
					goto IL_5FE;
				}
				if (!(command == "ssr"))
				{
					goto IL_5FE;
				}
				goto IL_58D;
			}
			else
			{
				if (!(command == "enable"))
				{
					goto IL_5FE;
				}
				component.enabled = (value != 0f);
				goto IL_5FE;
			}
		}
		else if (num <= 3848846198U)
		{
			if (num != 3569326331U)
			{
				if (num != 3840843484U)
				{
					if (num != 3848846198U)
					{
						goto IL_5FE;
					}
					if (!(command == "dof"))
					{
						goto IL_5FE;
					}
				}
				else
				{
					if (!(command == "bloom"))
					{
						goto IL_5FE;
					}
					postProcessEffectSettings = profile.GetSetting<Bloom>();
					goto IL_5FE;
				}
			}
			else
			{
				if (!(command == "dithering"))
				{
					goto IL_5FE;
				}
				goto IL_5FE;
			}
		}
		else if (num <= 4145125459U)
		{
			if (num != 3879710290U)
			{
				if (num != 4145125459U)
				{
					goto IL_5FE;
				}
				if (!(command == "ssrdist"))
				{
					goto IL_5FE;
				}
				profile.GetSetting<ScreenSpaceReflections>().maximumMarchDistance.Override(value);
				value = 1f;
				goto IL_5FE;
			}
			else
			{
				if (!(command == "chromaticaberration"))
				{
					goto IL_5FE;
				}
				goto IL_532;
			}
		}
		else if (num != 4148491186U)
		{
			if (num != 4222231496U)
			{
				goto IL_5FE;
			}
			if (!(command == "screenspacereflection"))
			{
				goto IL_5FE;
			}
			goto IL_58D;
		}
		else
		{
			if (!(command == "motionblur"))
			{
				goto IL_5FE;
			}
			goto IL_582;
		}
		postProcessEffectSettings = profile.GetSetting<DepthOfField>();
		goto IL_5FE;
		IL_58D:
		postProcessEffectSettings = profile.GetSetting<ScreenSpaceReflections>();
		goto IL_5FE;
		IL_4B2:
		postProcessEffectSettings = profile.GetSetting<AutoExposure>();
		goto IL_5FE;
		IL_532:
		postProcessEffectSettings = profile.GetSetting<ChromaticAberration>();
		goto IL_5FE;
		IL_582:
		postProcessEffectSettings = profile.GetSetting<MotionBlur>();
		IL_5FE:
		if (postProcessEffectSettings != null)
		{
			component.enabled = false;
			postProcessEffectSettings.enabled.Override(value != 0f);
			component.enabled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogTextureReport(string filename)
	{
		bool streamingMipmapsActive = QualitySettings.streamingMipmapsActive;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Streaming Enabled, Streaming Texture Count,Non Streaming Texture Count,Total Texture Memory (No Budget),Memory Budget,Desired Streaming Memory Budget,Current Target Memory Budget,Non Streaming Memory");
		stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}\n", new object[]
		{
			streamingMipmapsActive,
			Texture.streamingTextureCount,
			Texture.nonStreamingTextureCount,
			Texture.totalTextureMemory * 9.5367431640625E-07,
			QualitySettings.streamingMipmapsMemoryBudget,
			Texture.desiredTextureMemory * 9.5367431640625E-07,
			Texture.targetTextureMemory * 9.5367431640625E-07,
			Texture.nonStreamingTextureMemory * 9.5367431640625E-07
		});
		stringBuilder.AppendLine("Texture Name,Type,Is Readable,Streaming Enabled,Minimum Mip,Calculated Mip,Requested Mip,Desired Mip,Priority,Loaded Mip,Format,Width,Height,Mip Count,Texture Size (All Mips),Readable Size,Desired Size,Loaded Size, Is Streamed");
		foreach (Texture2D texture2D in Resources.FindObjectsOfTypeAll<Texture2D>())
		{
			int num = ProfilerUtils.CalculateTextureSizeBytes(texture2D, 0);
			int loadedMipmapLevel = texture2D.loadedMipmapLevel;
			int num2 = ProfilerUtils.CalculateTextureSizeBytes(texture2D, loadedMipmapLevel);
			int num3 = texture2D.isReadable ? num : 0;
			int num4 = ProfilerUtils.CalculateTextureSizeBytes(texture2D, texture2D.desiredMipmapLevel);
			stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n", new object[]
			{
				texture2D.name,
				"Texture2D",
				texture2D.isReadable,
				texture2D.streamingMipmaps,
				texture2D.minimumMipmapLevel,
				texture2D.calculatedMipmapLevel,
				texture2D.requestedMipmapLevel,
				texture2D.desiredMipmapLevel,
				texture2D.streamingMipmapsPriority,
				loadedMipmapLevel,
				texture2D.format,
				texture2D.width,
				texture2D.height,
				texture2D.mipmapCount,
				num,
				num3,
				num4,
				num2,
				texture2D.AreMipMapsStreamed()
			});
		}
		foreach (Cubemap cubemap in Resources.FindObjectsOfTypeAll<Cubemap>())
		{
			int num5 = ProfilerUtils.CalculateTextureSizeBytes(cubemap, 0);
			int loadedMipmapLevel2 = cubemap.loadedMipmapLevel;
			int num6 = ProfilerUtils.CalculateTextureSizeBytes(cubemap, loadedMipmapLevel2);
			int num7 = cubemap.isReadable ? num5 : 0;
			int num8 = ProfilerUtils.CalculateTextureSizeBytes(cubemap, cubemap.desiredMipmapLevel);
			stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n", new object[]
			{
				cubemap.name,
				"Cubemap",
				cubemap.isReadable,
				cubemap.streamingMipmaps,
				-1,
				-1,
				cubemap.requestedMipmapLevel,
				cubemap.desiredMipmapLevel,
				cubemap.streamingMipmapsPriority,
				loadedMipmapLevel2,
				cubemap.format,
				cubemap.width,
				cubemap.height,
				cubemap.mipmapCount,
				num5,
				num7,
				num8,
				num6,
				cubemap.AreMipMapsStreamed()
			});
		}
		if (string.IsNullOrEmpty(filename))
		{
			Log.Out(stringBuilder.ToString());
			return;
		}
		string tempFileName = PlatformManager.NativePlatform.Utils.GetTempFileName(filename, ".csv");
		try
		{
			File.WriteAllText(tempFileName, stringBuilder.ToString());
			Log.Out("Wrote texreport to " + tempFileName);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	public static float debugFloat;

	public static float debugFloat2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject distantTerrainObj;
}
