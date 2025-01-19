using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class ItemActionData : IXMLData
	{
		public DataItem<float> Delay
		{
			get
			{
				return this.pDelay;
			}
			set
			{
				this.pDelay = value;
			}
		}

		public DataItem<float> Range
		{
			get
			{
				return this.pRange;
			}
			set
			{
				this.pRange = value;
			}
		}

		public DataItem<string> SoundStart
		{
			get
			{
				return this.pSoundStart;
			}
			set
			{
				this.pSoundStart = value;
			}
		}

		public DataItem<string> SoundRepeat
		{
			get
			{
				return this.pSoundRepeat;
			}
			set
			{
				this.pSoundRepeat = value;
			}
		}

		public DataItem<string> SoundEnd
		{
			get
			{
				return this.pSoundEnd;
			}
			set
			{
				this.pSoundEnd = value;
			}
		}

		public DataItem<string> SoundEmpty
		{
			get
			{
				return this.pSoundEmpty;
			}
			set
			{
				this.pSoundEmpty = value;
			}
		}

		public DataItem<string> SoundReload
		{
			get
			{
				return this.pSoundReload;
			}
			set
			{
				this.pSoundReload = value;
			}
		}

		public DataItem<string> SoundWarning
		{
			get
			{
				return this.pSoundWarning;
			}
			set
			{
				this.pSoundWarning = value;
			}
		}

		public DataItem<string> StaminaUsage
		{
			get
			{
				return this.pStaminaUsage;
			}
			set
			{
				this.pStaminaUsage = value;
			}
		}

		public DataItem<string> UseTime
		{
			get
			{
				return this.pUseTime;
			}
			set
			{
				this.pUseTime = value;
			}
		}

		public DataItem<string> FocusedBlockname1
		{
			get
			{
				return this.pFocusedBlockname1;
			}
			set
			{
				this.pFocusedBlockname1 = value;
			}
		}

		public DataItem<string> FocusedBlockname2
		{
			get
			{
				return this.pFocusedBlockname2;
			}
			set
			{
				this.pFocusedBlockname2 = value;
			}
		}

		public DataItem<string> FocusedBlockname3
		{
			get
			{
				return this.pFocusedBlockname3;
			}
			set
			{
				this.pFocusedBlockname3 = value;
			}
		}

		public DataItem<string> FocusedBlockname4
		{
			get
			{
				return this.pFocusedBlockname4;
			}
			set
			{
				this.pFocusedBlockname4 = value;
			}
		}

		public DataItem<string> FocusedBlockname5
		{
			get
			{
				return this.pFocusedBlockname5;
			}
			set
			{
				this.pFocusedBlockname5 = value;
			}
		}

		public DataItem<string> FocusedBlockname6
		{
			get
			{
				return this.pFocusedBlockname6;
			}
			set
			{
				this.pFocusedBlockname6 = value;
			}
		}

		public DataItem<string> FocusedBlockname7
		{
			get
			{
				return this.pFocusedBlockname7;
			}
			set
			{
				this.pFocusedBlockname7 = value;
			}
		}

		public DataItem<string> FocusedBlockname8
		{
			get
			{
				return this.pFocusedBlockname8;
			}
			set
			{
				this.pFocusedBlockname8 = value;
			}
		}

		public DataItem<string> FocusedBlockname9
		{
			get
			{
				return this.pFocusedBlockname9;
			}
			set
			{
				this.pFocusedBlockname9 = value;
			}
		}

		public DataItem<string> ChangeItemTo
		{
			get
			{
				return this.pChangeItemTo;
			}
			set
			{
				this.pChangeItemTo = value;
			}
		}

		public DataItem<string> ChangeBlockTo
		{
			get
			{
				return this.pChangeBlockTo;
			}
			set
			{
				this.pChangeBlockTo = value;
			}
		}

		public DataItem<string> DoBlockAction
		{
			get
			{
				return this.pDoBlockAction;
			}
			set
			{
				this.pDoBlockAction = value;
			}
		}

		public DataItem<float> GainHealth
		{
			get
			{
				return this.pGainHealth;
			}
			set
			{
				this.pGainHealth = value;
			}
		}

		public DataItem<float> GainFood
		{
			get
			{
				return this.pGainFood;
			}
			set
			{
				this.pGainFood = value;
			}
		}

		public DataItem<float> GainWater
		{
			get
			{
				return this.pGainWater;
			}
			set
			{
				this.pGainWater = value;
			}
		}

		public DataItem<float> GainStamina
		{
			get
			{
				return this.pGainStamina;
			}
			set
			{
				this.pGainStamina = value;
			}
		}

		public DataItem<float> GainSickness
		{
			get
			{
				return this.pGainSickness;
			}
			set
			{
				this.pGainSickness = value;
			}
		}

		public DataItem<float> GainWellness
		{
			get
			{
				return this.pGainWellness;
			}
			set
			{
				this.pGainWellness = value;
			}
		}

		public DataItem<string> Buff
		{
			get
			{
				return this.pBuff;
			}
			set
			{
				this.pBuff = value;
			}
		}

		public DataItem<string> BuffChance
		{
			get
			{
				return this.pBuffChance;
			}
			set
			{
				this.pBuffChance = value;
			}
		}

		public DataItem<string> Debuff
		{
			get
			{
				return this.pDebuff;
			}
			set
			{
				this.pDebuff = value;
			}
		}

		public DataItem<string> CreateItem
		{
			get
			{
				return this.pCreateItem;
			}
			set
			{
				this.pCreateItem = value;
			}
		}

		public DataItem<int> ConditionRaycastBlock
		{
			get
			{
				return this.pConditionRaycastBlock;
			}
			set
			{
				this.pConditionRaycastBlock = value;
			}
		}

		public DataItem<int> GainGas
		{
			get
			{
				return this.pGainGas;
			}
			set
			{
				this.pGainGas = value;
			}
		}

		public DataItem<bool> Consume
		{
			get
			{
				return this.pConsume;
			}
			set
			{
				this.pConsume = value;
			}
		}

		public DataItem<string> Blockname
		{
			get
			{
				return this.pBlockname;
			}
			set
			{
				this.pBlockname = value;
			}
		}

		public DataItem<float> ThrowStrengthDefault
		{
			get
			{
				return this.pThrowStrengthDefault;
			}
			set
			{
				this.pThrowStrengthDefault = value;
			}
		}

		public DataItem<float> ThrowStrengthMax
		{
			get
			{
				return this.pThrowStrengthMax;
			}
			set
			{
				this.pThrowStrengthMax = value;
			}
		}

		public DataItem<float> MaxStrainTime
		{
			get
			{
				return this.pMaxStrainTime;
			}
			set
			{
				this.pMaxStrainTime = value;
			}
		}

		public DataItem<int> MagazineSize
		{
			get
			{
				return this.pMagazineSize;
			}
			set
			{
				this.pMagazineSize = value;
			}
		}

		public DataItem<string> MagazineItem
		{
			get
			{
				return this.pMagazineItem;
			}
			set
			{
				this.pMagazineItem = value;
			}
		}

		public DataItem<float> ReloadTime
		{
			get
			{
				return this.pReloadTime;
			}
			set
			{
				this.pReloadTime = value;
			}
		}

		public DataItem<string> BulletIcon
		{
			get
			{
				return this.pBulletIcon;
			}
			set
			{
				this.pBulletIcon = value;
			}
		}

		public DataItem<int> RaysPerShot
		{
			get
			{
				return this.pRaysPerShot;
			}
			set
			{
				this.pRaysPerShot = value;
			}
		}

		public DataItem<float> RaysSpread
		{
			get
			{
				return this.pRaysSpread;
			}
			set
			{
				this.pRaysSpread = value;
			}
		}

		public DataItem<float> Sphere
		{
			get
			{
				return this.pSphere;
			}
			set
			{
				this.pSphere = value;
			}
		}

		public DataItem<int> CrosshairMinDistance
		{
			get
			{
				return this.pCrosshairMinDistance;
			}
			set
			{
				this.pCrosshairMinDistance = value;
			}
		}

		public DataItem<int> CrosshairMaxDistance
		{
			get
			{
				return this.pCrosshairMaxDistance;
			}
			set
			{
				this.pCrosshairMaxDistance = value;
			}
		}

		public DataItem<int> DamageEntity
		{
			get
			{
				return this.pDamageEntity;
			}
			set
			{
				this.pDamageEntity = value;
			}
		}

		public DataItem<float> DamageBlock
		{
			get
			{
				return this.pDamageBlock;
			}
			set
			{
				this.pDamageBlock = value;
			}
		}

		public DataItem<string> ParticlesMuzzleFire
		{
			get
			{
				return this.pParticlesMuzzleFire;
			}
			set
			{
				this.pParticlesMuzzleFire = value;
			}
		}

		public DataItem<string> ParticlesMuzzleSmoke
		{
			get
			{
				return this.pParticlesMuzzleSmoke;
			}
			set
			{
				this.pParticlesMuzzleSmoke = value;
			}
		}

		public DataItem<float> BlockRange
		{
			get
			{
				return this.pBlockRange;
			}
			set
			{
				this.pBlockRange = value;
			}
		}

		public DataItem<bool> AutoFire
		{
			get
			{
				return this.pAutoFire;
			}
			set
			{
				this.pAutoFire = value;
			}
		}

		public DataItem<float> HordeMeterRate
		{
			get
			{
				return this.pHordeMeterRate;
			}
			set
			{
				this.pHordeMeterRate = value;
			}
		}

		public DataItem<float> HordeMeterDistance
		{
			get
			{
				return this.pHordeMeterDistance;
			}
			set
			{
				this.pHordeMeterDistance = value;
			}
		}

		public DataItem<string> HitmaskOverride
		{
			get
			{
				return this.pHitmaskOverride;
			}
			set
			{
				this.pHitmaskOverride = value;
			}
		}

		public DataItem<bool> SingleMagazineUsage
		{
			get
			{
				return this.pSingleMagazineUsage;
			}
			set
			{
				this.pSingleMagazineUsage = value;
			}
		}

		public DataItem<string> BulletMaterial
		{
			get
			{
				return this.pBulletMaterial;
			}
			set
			{
				this.pBulletMaterial = value;
			}
		}

		public DataItem<bool> InfiniteAmmo
		{
			get
			{
				return this.pInfiniteAmmo;
			}
			set
			{
				this.pInfiniteAmmo = value;
			}
		}

		public DataItem<float> ZoomMaxOut
		{
			get
			{
				return this.pZoomMaxOut;
			}
			set
			{
				this.pZoomMaxOut = value;
			}
		}

		public DataItem<float> ZoomMaxIn
		{
			get
			{
				return this.pZoomMaxIn;
			}
			set
			{
				this.pZoomMaxIn = value;
			}
		}

		public DataItem<string> ZoomOverlay
		{
			get
			{
				return this.pZoomOverlay;
			}
			set
			{
				this.pZoomOverlay = value;
			}
		}

		public DataItem<int> Velocity
		{
			get
			{
				return this.pVelocity;
			}
			set
			{
				this.pVelocity = value;
			}
		}

		public DataItem<float> FlyTime
		{
			get
			{
				return this.pFlyTime;
			}
			set
			{
				this.pFlyTime = value;
			}
		}

		public DataItem<float> LifeTime
		{
			get
			{
				return this.pLifeTime;
			}
			set
			{
				this.pLifeTime = value;
			}
		}

		public DataItem<float> CollisionRadius
		{
			get
			{
				return this.pCollisionRadius;
			}
			set
			{
				this.pCollisionRadius = value;
			}
		}

		public DataItem<int> ProjectileInitialVelocity
		{
			get
			{
				return this.pProjectileInitialVelocity;
			}
			set
			{
				this.pProjectileInitialVelocity = value;
			}
		}

		public DataItem<string> Fertileblock
		{
			get
			{
				return this.pFertileblock;
			}
			set
			{
				this.pFertileblock = value;
			}
		}

		public DataItem<string> Adjacentblock
		{
			get
			{
				return this.pAdjacentblock;
			}
			set
			{
				this.pAdjacentblock = value;
			}
		}

		public DataItem<int> RepairAmount
		{
			get
			{
				return this.pRepairAmount;
			}
			set
			{
				this.pRepairAmount = value;
			}
		}

		public DataItem<int> UpgradeHitOffset
		{
			get
			{
				return this.pUpgradeHitOffset;
			}
			set
			{
				this.pUpgradeHitOffset = value;
			}
		}

		public DataItem<string> AllowedUpgradeItems
		{
			get
			{
				return this.pAllowedUpgradeItems;
			}
			set
			{
				this.pAllowedUpgradeItems = value;
			}
		}

		public DataItem<string> RestrictedUpgradeItems
		{
			get
			{
				return this.pRestrictedUpgradeItems;
			}
			set
			{
				this.pRestrictedUpgradeItems = value;
			}
		}

		public DataItem<string> UpgradeActionSound
		{
			get
			{
				return this.pUpgradeActionSound;
			}
			set
			{
				this.pUpgradeActionSound = value;
			}
		}

		public DataItem<string> RepairActionSound
		{
			get
			{
				return this.pRepairActionSound;
			}
			set
			{
				this.pRepairActionSound = value;
			}
		}

		public DataItem<string> ReferenceItem
		{
			get
			{
				return this.pReferenceItem;
			}
			set
			{
				this.pReferenceItem = value;
			}
		}

		public DataItem<string> Mesh
		{
			get
			{
				return this.pMesh;
			}
			set
			{
				this.pMesh = value;
			}
		}

		public DataItem<int> ActionIdx
		{
			get
			{
				return this.pActionIdx;
			}
			set
			{
				this.pActionIdx = value;
			}
		}

		public DataItem<string> Title
		{
			get
			{
				return this.pTitle;
			}
			set
			{
				this.pTitle = value;
			}
		}

		public DataItem<string> Description
		{
			get
			{
				return this.pDescription;
			}
			set
			{
				this.pDescription = value;
			}
		}

		public DataItem<string> RecipesToLearn
		{
			get
			{
				return this.pRecipesToLearn;
			}
			set
			{
				this.pRecipesToLearn = value;
			}
		}

		public DataItem<string> InstantiateOnLoad
		{
			get
			{
				return this.pInstantiateOnLoad;
			}
			set
			{
				this.pInstantiateOnLoad = value;
			}
		}

		public DataItem<string> SoundDraw
		{
			get
			{
				return this.pSoundDraw;
			}
			set
			{
				this.pSoundDraw = value;
			}
		}

		public DataItem<DamageBonusData> DamageBonus
		{
			get
			{
				return this.pDamageBonus;
			}
			set
			{
				this.pDamageBonus = value;
			}
		}

		public DataItem<ExplosionData> Explosion
		{
			get
			{
				return this.pExplosion;
			}
			set
			{
				this.pExplosion = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			List<IDataItem> list = new List<IDataItem>();
			if (_recursive && this.pDamageBonus != null)
			{
				list.AddRange(this.pDamageBonus.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pExplosion != null)
			{
				list.AddRange(this.pExplosion.Value.GetDisplayValues(true));
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRange;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundStart;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundRepeat;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundEnd;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundEmpty;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundReload;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundWarning;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pStaminaUsage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pUseTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname1;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname2;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname3;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname4;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname5;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname6;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname7;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname8;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFocusedBlockname9;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pChangeItemTo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pChangeBlockTo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDoBlockAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainHealth;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainFood;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainWater;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainStamina;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainSickness;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGainWellness;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBuff;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBuffChance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDebuff;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCreateItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pConditionRaycastBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pGainGas;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pConsume;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBlockname;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pThrowStrengthDefault;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pThrowStrengthMax;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pMaxStrainTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pMagazineSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMagazineItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pReloadTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBulletIcon;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRaysPerShot;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRaysSpread;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pSphere;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pCrosshairMinDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pCrosshairMaxDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pDamageBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pParticlesMuzzleFire;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pParticlesMuzzleSmoke;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pBlockRange;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pAutoFire;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pHordeMeterRate;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pHordeMeterDistance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pHitmaskOverride;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pSingleMagazineUsage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBulletMaterial;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pInfiniteAmmo;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pZoomMaxOut;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pZoomMaxIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pZoomOverlay;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pVelocity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pFlyTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pLifeTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pCollisionRadius;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pProjectileInitialVelocity;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFertileblock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAdjacentblock;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRepairAmount;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pUpgradeHitOffset;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAllowedUpgradeItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRestrictedUpgradeItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pUpgradeActionSound;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRepairActionSound;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pReferenceItem;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMesh;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pActionIdx;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pTitle;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDescription;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pRecipesToLearn;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pInstantiateOnLoad;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundDraw;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<DamageBonusData> pDamageBonus;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ExplosionData> pExplosion;

		public static class Parser
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public static DataItem<string> ParseItem(string _string, PositionXmlElement _elem)
			{
				string startValue;
				try
				{
					startValue = stringParser.Parse(ParserUtils.ParseStringAttribute(_elem, "value", true, null));
				}
				catch (Exception innerException)
				{
					throw new InvalidValueException(string.Concat(new string[]
					{
						"Could not parse attribute \"",
						_elem.Name,
						"\" value \"",
						ParserUtils.ParseStringAttribute(_elem, "value", true, null),
						"\""
					}), _elem.LineNumber, innerException);
				}
				return new DataItem<string>(_string, startValue);
			}

			public static ItemAction Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemAction";
				Type type = Type.GetType(typeof(ItemActionData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ItemAction itemAction = (ItemAction)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing ItemAction", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ItemActionData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing ItemAction", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
						if (num <= 2050383924U)
						{
							if (num <= 973038343U)
							{
								if (num <= 477934085U)
								{
									if (num <= 264004213U)
									{
										if (num <= 91459346U)
										{
											if (num != 32426346U)
											{
												if (num == 91459346U)
												{
													if (name == "CrosshairMinDistance")
													{
														int startValue;
														try
														{
															startValue = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
														}
														catch (Exception innerException)
														{
															throw new InvalidValueException(string.Concat(new string[]
															{
																"Could not parse attribute \"",
																positionXmlElement.Name,
																"\" value \"",
																ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
																"\""
															}), positionXmlElement.LineNumber, innerException);
														}
														DataItem<int> pCrosshairMinDistance = new DataItem<int>("CrosshairMinDistance", startValue);
														itemAction.pCrosshairMinDistance = pCrosshairMinDistance;
													}
												}
											}
											else if (name == "HitmaskOverride")
											{
												string startValue2;
												try
												{
													startValue2 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException2)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException2);
												}
												DataItem<string> pHitmaskOverride = new DataItem<string>("HitmaskOverride", startValue2);
												itemAction.pHitmaskOverride = pHitmaskOverride;
											}
										}
										else if (num != 132142556U)
										{
											if (num != 248841283U)
											{
												if (num == 264004213U)
												{
													if (name == "ProjectileInitialVelocity")
													{
														int startValue3;
														try
														{
															startValue3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
														}
														catch (Exception innerException3)
														{
															throw new InvalidValueException(string.Concat(new string[]
															{
																"Could not parse attribute \"",
																positionXmlElement.Name,
																"\" value \"",
																ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
																"\""
															}), positionXmlElement.LineNumber, innerException3);
														}
														DataItem<int> pProjectileInitialVelocity = new DataItem<int>("ProjectileInitialVelocity", startValue3);
														itemAction.pProjectileInitialVelocity = pProjectileInitialVelocity;
													}
												}
											}
											else if (name == "RaysSpread")
											{
												float startValue4;
												try
												{
													startValue4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException4)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException4);
												}
												DataItem<float> pRaysSpread = new DataItem<float>("RaysSpread", startValue4);
												itemAction.pRaysSpread = pRaysSpread;
											}
										}
										else if (name == "Mesh")
										{
											string startValue5;
											try
											{
												startValue5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException5)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException5);
											}
											DataItem<string> pMesh = new DataItem<string>("Mesh", startValue5);
											itemAction.pMesh = pMesh;
										}
									}
									else if (num <= 388984571U)
									{
										if (num != 383713930U)
										{
											if (num == 388984571U)
											{
												if (name == "DamageEntity")
												{
													int startValue6;
													try
													{
														startValue6 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException6)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException6);
													}
													DataItem<int> pDamageEntity = new DataItem<int>("DamageEntity", startValue6);
													itemAction.pDamageEntity = pDamageEntity;
												}
											}
										}
										else if (name == "AutoFire")
										{
											bool startValue7;
											try
											{
												startValue7 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException7)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException7);
											}
											DataItem<bool> pAutoFire = new DataItem<bool>("AutoFire", startValue7);
											itemAction.pAutoFire = pAutoFire;
										}
									}
									else if (num != 422257095U)
									{
										if (num != 441323083U)
										{
											if (num == 477934085U)
											{
												if (name == "MaxStrainTime")
												{
													float startValue8;
													try
													{
														startValue8 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException8)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException8);
													}
													DataItem<float> pMaxStrainTime = new DataItem<float>("MaxStrainTime", startValue8);
													itemAction.pMaxStrainTime = pMaxStrainTime;
												}
											}
										}
										else if (name == "RaysPerShot")
										{
											int startValue9;
											try
											{
												startValue9 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException9)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException9);
											}
											DataItem<int> pRaysPerShot = new DataItem<int>("RaysPerShot", startValue9);
											itemAction.pRaysPerShot = pRaysPerShot;
										}
									}
									else if (name == "GainGas")
									{
										int startValue10;
										try
										{
											startValue10 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException10)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException10);
										}
										DataItem<int> pGainGas = new DataItem<int>("GainGas", startValue10);
										itemAction.pGainGas = pGainGas;
									}
								}
								else if (num <= 678358751U)
								{
									if (num <= 573162709U)
									{
										if (num != 550270072U)
										{
											if (num == 573162709U)
											{
												if (name == "SoundEmpty")
												{
													string startValue11;
													try
													{
														startValue11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException11)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException11);
													}
													DataItem<string> pSoundEmpty = new DataItem<string>("SoundEmpty", startValue11);
													itemAction.pSoundEmpty = pSoundEmpty;
												}
											}
										}
										else if (name == "Delay")
										{
											float startValue12;
											try
											{
												startValue12 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException12)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException12);
											}
											DataItem<float> pDelay = new DataItem<float>("Delay", startValue12);
											itemAction.pDelay = pDelay;
										}
									}
									else if (num != 573416248U)
									{
										if (num != 617902505U)
										{
											if (num == 678358751U)
											{
												if (name == "GainStamina")
												{
													float startValue13;
													try
													{
														startValue13 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException13)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException13);
													}
													DataItem<float> pGainStamina = new DataItem<float>("GainStamina", startValue13);
													itemAction.pGainStamina = pGainStamina;
												}
											}
										}
										else if (name == "Title")
										{
											string startValue14;
											try
											{
												startValue14 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException14)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException14);
											}
											DataItem<string> pTitle = new DataItem<string>("Title", startValue14);
											itemAction.pTitle = pTitle;
										}
									}
									else if (name == "Sphere")
									{
										float startValue15;
										try
										{
											startValue15 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException15)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException15);
										}
										DataItem<float> pSphere = new DataItem<float>("Sphere", startValue15);
										itemAction.pSphere = pSphere;
									}
								}
								else if (num <= 859861643U)
								{
									if (num != 738624775U)
									{
										if (num != 843848237U)
										{
											if (num == 859861643U)
											{
												if (name == "SoundRepeat")
												{
													string startValue16;
													try
													{
														startValue16 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException16)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException16);
													}
													DataItem<string> pSoundRepeat = new DataItem<string>("SoundRepeat", startValue16);
													itemAction.pSoundRepeat = pSoundRepeat;
												}
											}
										}
										else if (name == "CollisionRadius")
										{
											float startValue17;
											try
											{
												startValue17 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException17)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException17);
											}
											DataItem<float> pCollisionRadius = new DataItem<float>("CollisionRadius", startValue17);
											itemAction.pCollisionRadius = pCollisionRadius;
										}
									}
									else if (name == "ThrowStrengthDefault")
									{
										float startValue18;
										try
										{
											startValue18 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException18)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException18);
										}
										DataItem<float> pThrowStrengthDefault = new DataItem<float>("ThrowStrengthDefault", startValue18);
										itemAction.pThrowStrengthDefault = pThrowStrengthDefault;
									}
								}
								else if (num != 883311634U)
								{
									if (num != 906214847U)
									{
										if (num == 973038343U)
										{
											if (name == "InfiniteAmmo")
											{
												bool startValue19;
												try
												{
													startValue19 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException19)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException19);
												}
												DataItem<bool> pInfiniteAmmo = new DataItem<bool>("InfiniteAmmo", startValue19);
												itemAction.pInfiniteAmmo = pInfiniteAmmo;
											}
										}
									}
									else if (name == "ChangeBlockTo")
									{
										string startValue20;
										try
										{
											startValue20 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException20)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException20);
										}
										DataItem<string> pChangeBlockTo = new DataItem<string>("ChangeBlockTo", startValue20);
										itemAction.pChangeBlockTo = pChangeBlockTo;
									}
								}
								else if (name == "ZoomOverlay")
								{
									string startValue21;
									try
									{
										startValue21 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException21)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException21);
									}
									DataItem<string> pZoomOverlay = new DataItem<string>("ZoomOverlay", startValue21);
									itemAction.pZoomOverlay = pZoomOverlay;
								}
							}
							else if (num <= 1725856265U)
							{
								if (num <= 1237474039U)
								{
									if (num <= 1173181999U)
									{
										if (num != 1157581142U)
										{
											if (num == 1173181999U)
											{
												if (name == "BlockRange")
												{
													float startValue22;
													try
													{
														startValue22 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException22)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException22);
													}
													DataItem<float> pBlockRange = new DataItem<float>("BlockRange", startValue22);
													itemAction.pBlockRange = pBlockRange;
												}
											}
										}
										else if (name == "SingleMagazineUsage")
										{
											bool startValue23;
											try
											{
												startValue23 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException23)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException23);
											}
											DataItem<bool> pSingleMagazineUsage = new DataItem<bool>("SingleMagazineUsage", startValue23);
											itemAction.pSingleMagazineUsage = pSingleMagazineUsage;
										}
									}
									else if (num != 1180803064U)
									{
										if (num != 1221226493U)
										{
											if (num == 1237474039U)
											{
												if (name == "DamageBonus")
												{
													DamageBonusData startValue24 = DamageBonusData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<DamageBonusData> pDamageBonus = new DataItem<DamageBonusData>("DamageBonus", startValue24);
													itemAction.pDamageBonus = pDamageBonus;
												}
											}
										}
										else if (name == "RecipesToLearn")
										{
											string startValue25;
											try
											{
												startValue25 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException24)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException24);
											}
											DataItem<string> pRecipesToLearn = new DataItem<string>("RecipesToLearn", startValue25);
											itemAction.pRecipesToLearn = pRecipesToLearn;
										}
									}
									else if (name == "SoundStart")
									{
										string startValue26;
										try
										{
											startValue26 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException25)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException25);
										}
										DataItem<string> pSoundStart = new DataItem<string>("SoundStart", startValue26);
										itemAction.pSoundStart = pSoundStart;
									}
								}
								else if (num <= 1330610628U)
								{
									if (num != 1259223448U)
									{
										if (num != 1292404073U)
										{
											if (num == 1330610628U)
											{
												if (name == "CrosshairMaxDistance")
												{
													int startValue27;
													try
													{
														startValue27 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException26)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException26);
													}
													DataItem<int> pCrosshairMaxDistance = new DataItem<int>("CrosshairMaxDistance", startValue27);
													itemAction.pCrosshairMaxDistance = pCrosshairMaxDistance;
												}
											}
										}
										else if (name == "SoundEnd")
										{
											string startValue28;
											try
											{
												startValue28 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException27)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException27);
											}
											DataItem<string> pSoundEnd = new DataItem<string>("SoundEnd", startValue28);
											itemAction.pSoundEnd = pSoundEnd;
										}
									}
									else if (name == "ZoomMaxOut")
									{
										float startValue29;
										try
										{
											startValue29 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException28)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException28);
										}
										DataItem<float> pZoomMaxOut = new DataItem<float>("ZoomMaxOut", startValue29);
										itemAction.pZoomMaxOut = pZoomMaxOut;
									}
								}
								else if (num != 1601156781U)
								{
									if (num != 1660679176U)
									{
										if (num == 1725856265U)
										{
											if (name == "Description")
											{
												string startValue30;
												try
												{
													startValue30 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException29)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException29);
												}
												DataItem<string> pDescription = new DataItem<string>("Description", startValue30);
												itemAction.pDescription = pDescription;
											}
										}
									}
									else if (name == "BuffChance")
									{
										string startValue31;
										try
										{
											startValue31 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException30)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException30);
										}
										DataItem<string> pBuffChance = new DataItem<string>("BuffChance", startValue31);
										itemAction.pBuffChance = pBuffChance;
									}
								}
								else if (name == "ReloadTime")
								{
									float startValue32;
									try
									{
										startValue32 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException31)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException31);
									}
									DataItem<float> pReloadTime = new DataItem<float>("ReloadTime", startValue32);
									itemAction.pReloadTime = pReloadTime;
								}
							}
							else if (num <= 1811911295U)
							{
								if (num <= 1744800819U)
								{
									if (num != 1728023200U)
									{
										if (num == 1744800819U)
										{
											if (name == "FocusedBlockname5")
											{
												string startValue33;
												try
												{
													startValue33 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException32)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException32);
												}
												DataItem<string> pFocusedBlockname = new DataItem<string>("FocusedBlockname5", startValue33);
												itemAction.pFocusedBlockname5 = pFocusedBlockname;
											}
										}
									}
									else if (name == "FocusedBlockname4")
									{
										string startValue34;
										try
										{
											startValue34 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException33)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException33);
										}
										DataItem<string> pFocusedBlockname2 = new DataItem<string>("FocusedBlockname4", startValue34);
										itemAction.pFocusedBlockname4 = pFocusedBlockname2;
									}
								}
								else if (num != 1761578438U)
								{
									if (num != 1778356057U)
									{
										if (num == 1811911295U)
										{
											if (name == "FocusedBlockname1")
											{
												string startValue35;
												try
												{
													startValue35 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException34)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException34);
												}
												DataItem<string> pFocusedBlockname3 = new DataItem<string>("FocusedBlockname1", startValue35);
												itemAction.pFocusedBlockname1 = pFocusedBlockname3;
											}
										}
									}
									else if (name == "FocusedBlockname7")
									{
										string startValue36;
										try
										{
											startValue36 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException35)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException35);
										}
										DataItem<string> pFocusedBlockname4 = new DataItem<string>("FocusedBlockname7", startValue36);
										itemAction.pFocusedBlockname7 = pFocusedBlockname4;
									}
								}
								else if (name == "FocusedBlockname6")
								{
									string startValue37;
									try
									{
										startValue37 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException36)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException36);
									}
									DataItem<string> pFocusedBlockname5 = new DataItem<string>("FocusedBlockname6", startValue37);
									itemAction.pFocusedBlockname6 = pFocusedBlockname5;
								}
							}
							else if (num <= 1929345774U)
							{
								if (num != 1828688914U)
								{
									if (num != 1845466533U)
									{
										if (num == 1929345774U)
										{
											if (name == "LifeTime")
											{
												float startValue38;
												try
												{
													startValue38 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException37)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException37);
												}
												DataItem<float> pLifeTime = new DataItem<float>("LifeTime", startValue38);
												itemAction.pLifeTime = pLifeTime;
											}
										}
									}
									else if (name == "FocusedBlockname3")
									{
										string startValue39;
										try
										{
											startValue39 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException38)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException38);
										}
										DataItem<string> pFocusedBlockname6 = new DataItem<string>("FocusedBlockname3", startValue39);
										itemAction.pFocusedBlockname3 = pFocusedBlockname6;
									}
								}
								else if (name == "FocusedBlockname2")
								{
									string startValue40;
									try
									{
										startValue40 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException39)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException39);
									}
									DataItem<string> pFocusedBlockname7 = new DataItem<string>("FocusedBlockname2", startValue40);
									itemAction.pFocusedBlockname2 = pFocusedBlockname7;
								}
							}
							else if (num != 1929354628U)
							{
								if (num != 1946132247U)
								{
									if (num == 2050383924U)
									{
										if (name == "RestrictedUpgradeItems")
										{
											string startValue41;
											try
											{
												startValue41 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException40)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException40);
											}
											DataItem<string> pRestrictedUpgradeItems = new DataItem<string>("RestrictedUpgradeItems", startValue41);
											itemAction.pRestrictedUpgradeItems = pRestrictedUpgradeItems;
										}
									}
								}
								else if (name == "FocusedBlockname9")
								{
									string startValue42;
									try
									{
										startValue42 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException41)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException41);
									}
									DataItem<string> pFocusedBlockname8 = new DataItem<string>("FocusedBlockname9", startValue42);
									itemAction.pFocusedBlockname9 = pFocusedBlockname8;
								}
							}
							else if (name == "FocusedBlockname8")
							{
								string startValue43;
								try
								{
									startValue43 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException42)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException42);
								}
								DataItem<string> pFocusedBlockname9 = new DataItem<string>("FocusedBlockname8", startValue43);
								itemAction.pFocusedBlockname8 = pFocusedBlockname9;
							}
						}
						else if (num <= 3213271394U)
						{
							if (num <= 2731875518U)
							{
								if (num <= 2292684416U)
								{
									if (num <= 2079371571U)
									{
										if (num != 2054944794U)
										{
											if (num == 2079371571U)
											{
												if (name == "Fertileblock")
												{
													string startValue44;
													try
													{
														startValue44 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
													}
													catch (Exception innerException43)
													{
														throw new InvalidValueException(string.Concat(new string[]
														{
															"Could not parse attribute \"",
															positionXmlElement.Name,
															"\" value \"",
															ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
															"\""
														}), positionXmlElement.LineNumber, innerException43);
													}
													DataItem<string> pFertileblock = new DataItem<string>("Fertileblock", startValue44);
													itemAction.pFertileblock = pFertileblock;
												}
											}
										}
										else if (name == "ParticlesMuzzleSmoke")
										{
											string startValue45;
											try
											{
												startValue45 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException44)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException44);
											}
											DataItem<string> pParticlesMuzzleSmoke = new DataItem<string>("ParticlesMuzzleSmoke", startValue45);
											itemAction.pParticlesMuzzleSmoke = pParticlesMuzzleSmoke;
										}
									}
									else if (num != 2205678605U)
									{
										if (num != 2214691755U)
										{
											if (num == 2292684416U)
											{
												if (name == "Explosion")
												{
													ExplosionData startValue46 = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<ExplosionData> pExplosion = new DataItem<ExplosionData>("Explosion", startValue46);
													itemAction.pExplosion = pExplosion;
												}
											}
										}
										else if (name == "GainSickness")
										{
											float startValue47;
											try
											{
												startValue47 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException45)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException45);
											}
											DataItem<float> pGainSickness = new DataItem<float>("GainSickness", startValue47);
											itemAction.pGainSickness = pGainSickness;
										}
									}
									else if (name == "ParticlesMuzzleFire")
									{
										string startValue48;
										try
										{
											startValue48 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException46)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException46);
										}
										DataItem<string> pParticlesMuzzleFire = new DataItem<string>("ParticlesMuzzleFire", startValue48);
										itemAction.pParticlesMuzzleFire = pParticlesMuzzleFire;
									}
								}
								else if (num <= 2391273097U)
								{
									if (num != 2296213333U)
									{
										if (num == 2391273097U)
										{
											if (name == "GainWater")
											{
												float startValue49;
												try
												{
													startValue49 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException47)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException47);
												}
												DataItem<float> pGainWater = new DataItem<float>("GainWater", startValue49);
												itemAction.pGainWater = pGainWater;
											}
										}
									}
									else if (name == "UseTime")
									{
										string startValue50;
										try
										{
											startValue50 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException48)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException48);
										}
										DataItem<string> pUseTime = new DataItem<string>("UseTime", startValue50);
										itemAction.pUseTime = pUseTime;
									}
								}
								else if (num != 2508081957U)
								{
									if (num != 2654093974U)
									{
										if (num == 2731875518U)
										{
											if (name == "ActionIdx")
											{
												int startValue51;
												try
												{
													startValue51 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException49)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException49);
												}
												DataItem<int> pActionIdx = new DataItem<int>("ActionIdx", startValue51);
												itemAction.pActionIdx = pActionIdx;
											}
										}
									}
									else if (name == "SoundDraw")
									{
										string startValue52;
										try
										{
											startValue52 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException50)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException50);
										}
										DataItem<string> pSoundDraw = new DataItem<string>("SoundDraw", startValue52);
										itemAction.pSoundDraw = pSoundDraw;
									}
								}
								else if (name == "RepairActionSound")
								{
									string startValue53;
									try
									{
										startValue53 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException51)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException51);
									}
									DataItem<string> pRepairActionSound = new DataItem<string>("RepairActionSound", startValue53);
									itemAction.pRepairActionSound = pRepairActionSound;
								}
							}
							else if (num <= 2951397452U)
							{
								if (num <= 2776146097U)
								{
									if (num != 2735859570U)
									{
										if (num == 2776146097U)
										{
											if (name == "GainWellness")
											{
												float startValue54;
												try
												{
													startValue54 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException52)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException52);
												}
												DataItem<float> pGainWellness = new DataItem<float>("GainWellness", startValue54);
												itemAction.pGainWellness = pGainWellness;
											}
										}
									}
									else if (name == "Range")
									{
										float startValue55;
										try
										{
											startValue55 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException53)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException53);
										}
										DataItem<float> pRange = new DataItem<float>("Range", startValue55);
										itemAction.pRange = pRange;
									}
								}
								else if (num != 2889970889U)
								{
									if (num != 2916596296U)
									{
										if (num == 2951397452U)
										{
											if (name == "ThrowStrengthMax")
											{
												float startValue56;
												try
												{
													startValue56 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException54)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException54);
												}
												DataItem<float> pThrowStrengthMax = new DataItem<float>("ThrowStrengthMax", startValue56);
												itemAction.pThrowStrengthMax = pThrowStrengthMax;
											}
										}
									}
									else if (name == "SoundWarning")
									{
										string startValue57;
										try
										{
											startValue57 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException55)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException55);
										}
										DataItem<string> pSoundWarning = new DataItem<string>("SoundWarning", startValue57);
										itemAction.pSoundWarning = pSoundWarning;
									}
								}
								else if (name == "Blockname")
								{
									string startValue58;
									try
									{
										startValue58 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException56)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException56);
									}
									DataItem<string> pBlockname = new DataItem<string>("Blockname", startValue58);
									itemAction.pBlockname = pBlockname;
								}
							}
							else if (num <= 3036802414U)
							{
								if (num != 2970340076U)
								{
									if (num != 3027266612U)
									{
										if (num == 3036802414U)
										{
											if (name == "GainFood")
											{
												float startValue59;
												try
												{
													startValue59 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException57)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException57);
												}
												DataItem<float> pGainFood = new DataItem<float>("GainFood", startValue59);
												itemAction.pGainFood = pGainFood;
											}
										}
									}
									else if (name == "HordeMeterRate")
									{
										float startValue60;
										try
										{
											startValue60 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException58)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException58);
										}
										DataItem<float> pHordeMeterRate = new DataItem<float>("HordeMeterRate", startValue60);
										itemAction.pHordeMeterRate = pHordeMeterRate;
									}
								}
								else if (name == "Buff")
								{
									string startValue61;
									try
									{
										startValue61 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException59)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException59);
									}
									DataItem<string> pBuff = new DataItem<string>("Buff", startValue61);
									itemAction.pBuff = pBuff;
								}
							}
							else if (num != 3118731031U)
							{
								if (num != 3124789842U)
								{
									if (num == 3213271394U)
									{
										if (name == "MagazineSize")
										{
											int startValue62;
											try
											{
												startValue62 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException60)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException60);
											}
											DataItem<int> pMagazineSize = new DataItem<int>("MagazineSize", startValue62);
											itemAction.pMagazineSize = pMagazineSize;
										}
									}
								}
								else if (name == "Velocity")
								{
									int startValue63;
									try
									{
										startValue63 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException61)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException61);
									}
									DataItem<int> pVelocity = new DataItem<int>("Velocity", startValue63);
									itemAction.pVelocity = pVelocity;
								}
							}
							else if (name == "DoBlockAction")
							{
								itemAction.pDoBlockAction = ItemActionData.Parser.ParseItem("DoBlockAction", positionXmlElement);
							}
						}
						else if (num <= 3880205310U)
						{
							if (num <= 3448204316U)
							{
								if (num <= 3261865014U)
								{
									if (num != 3239575924U)
									{
										if (num == 3261865014U)
										{
											if (name == "ConditionRaycastBlock")
											{
												int startValue64;
												try
												{
													startValue64 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException62)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException62);
												}
												DataItem<int> pConditionRaycastBlock = new DataItem<int>("ConditionRaycastBlock", startValue64);
												itemAction.pConditionRaycastBlock = pConditionRaycastBlock;
											}
										}
									}
									else if (name == "BulletMaterial")
									{
										string startValue65;
										try
										{
											startValue65 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException63)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException63);
										}
										DataItem<string> pBulletMaterial = new DataItem<string>("BulletMaterial", startValue65);
										itemAction.pBulletMaterial = pBulletMaterial;
									}
								}
								else if (num != 3297724363U)
								{
									if (num != 3370626489U)
									{
										if (num == 3448204316U)
										{
											if (name == "GainHealth")
											{
												float startValue66;
												try
												{
													startValue66 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException64)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException64);
												}
												DataItem<float> pGainHealth = new DataItem<float>("GainHealth", startValue66);
												itemAction.pGainHealth = pGainHealth;
											}
										}
									}
									else if (name == "SoundReload")
									{
										string startValue67;
										try
										{
											startValue67 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException65)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException65);
										}
										DataItem<string> pSoundReload = new DataItem<string>("SoundReload", startValue67);
										itemAction.pSoundReload = pSoundReload;
									}
								}
								else if (name == "ZoomMaxIn")
								{
									float startValue68;
									try
									{
										startValue68 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException66)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException66);
									}
									DataItem<float> pZoomMaxIn = new DataItem<float>("ZoomMaxIn", startValue68);
									itemAction.pZoomMaxIn = pZoomMaxIn;
								}
							}
							else if (num <= 3781194609U)
							{
								if (num != 3646476408U)
								{
									if (num != 3699885409U)
									{
										if (num == 3781194609U)
										{
											if (name == "ReferenceItem")
											{
												string startValue69;
												try
												{
													startValue69 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException67)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException67);
												}
												DataItem<string> pReferenceItem = new DataItem<string>("ReferenceItem", startValue69);
												itemAction.pReferenceItem = pReferenceItem;
											}
										}
									}
									else if (name == "Consume")
									{
										bool startValue70;
										try
										{
											startValue70 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException68)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException68);
										}
										DataItem<bool> pConsume = new DataItem<bool>("Consume", startValue70);
										itemAction.pConsume = pConsume;
									}
								}
								else if (name == "RepairAmount")
								{
									int startValue71;
									try
									{
										startValue71 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException69)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException69);
									}
									DataItem<int> pRepairAmount = new DataItem<int>("RepairAmount", startValue71);
									itemAction.pRepairAmount = pRepairAmount;
								}
							}
							else if (num != 3788546643U)
							{
								if (num != 3860496195U)
								{
									if (num == 3880205310U)
									{
										if (name == "CreateItem")
										{
											string startValue72;
											try
											{
												startValue72 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException70)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException70);
											}
											DataItem<string> pCreateItem = new DataItem<string>("CreateItem", startValue72);
											itemAction.pCreateItem = pCreateItem;
										}
									}
								}
								else if (name == "ChangeItemTo")
								{
									string startValue73;
									try
									{
										startValue73 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException71)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException71);
									}
									DataItem<string> pChangeItemTo = new DataItem<string>("ChangeItemTo", startValue73);
									itemAction.pChangeItemTo = pChangeItemTo;
								}
							}
							else if (name == "AllowedUpgradeItems")
							{
								string startValue74;
								try
								{
									startValue74 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException72)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException72);
								}
								DataItem<string> pAllowedUpgradeItems = new DataItem<string>("AllowedUpgradeItems", startValue74);
								itemAction.pAllowedUpgradeItems = pAllowedUpgradeItems;
							}
						}
						else if (num <= 4005067140U)
						{
							if (num <= 3894590787U)
							{
								if (num != 3893059105U)
								{
									if (num == 3894590787U)
									{
										if (name == "FlyTime")
										{
											float startValue75;
											try
											{
												startValue75 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException73)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException73);
											}
											DataItem<float> pFlyTime = new DataItem<float>("FlyTime", startValue75);
											itemAction.pFlyTime = pFlyTime;
										}
									}
								}
								else if (name == "StaminaUsage")
								{
									string startValue76;
									try
									{
										startValue76 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException74)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException74);
									}
									DataItem<string> pStaminaUsage = new DataItem<string>("StaminaUsage", startValue76);
									itemAction.pStaminaUsage = pStaminaUsage;
								}
							}
							else if (num != 3922517809U)
							{
								if (num != 3965826732U)
								{
									if (num == 4005067140U)
									{
										if (name == "MagazineItem")
										{
											string startValue77;
											try
											{
												startValue77 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException75)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException75);
											}
											DataItem<string> pMagazineItem = new DataItem<string>("MagazineItem", startValue77);
											itemAction.pMagazineItem = pMagazineItem;
										}
									}
								}
								else if (name == "UpgradeActionSound")
								{
									string startValue78;
									try
									{
										startValue78 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException76)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException76);
									}
									DataItem<string> pUpgradeActionSound = new DataItem<string>("UpgradeActionSound", startValue78);
									itemAction.pUpgradeActionSound = pUpgradeActionSound;
								}
							}
							else if (name == "DamageBlock")
							{
								float startValue79;
								try
								{
									startValue79 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException77)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException77);
								}
								DataItem<float> pDamageBlock = new DataItem<float>("DamageBlock", startValue79);
								itemAction.pDamageBlock = pDamageBlock;
							}
						}
						else if (num <= 4124220918U)
						{
							if (num != 4013882141U)
							{
								if (num != 4069726011U)
								{
									if (num == 4124220918U)
									{
										if (name == "InstantiateOnLoad")
										{
											string startValue80;
											try
											{
												startValue80 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException78)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException78);
											}
											DataItem<string> pInstantiateOnLoad = new DataItem<string>("InstantiateOnLoad", startValue80);
											itemAction.pInstantiateOnLoad = pInstantiateOnLoad;
										}
									}
								}
								else if (name == "UpgradeHitOffset")
								{
									int startValue81;
									try
									{
										startValue81 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException79)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException79);
									}
									DataItem<int> pUpgradeHitOffset = new DataItem<int>("UpgradeHitOffset", startValue81);
									itemAction.pUpgradeHitOffset = pUpgradeHitOffset;
								}
							}
							else if (name == "HordeMeterDistance")
							{
								float startValue82;
								try
								{
									startValue82 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException80)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException80);
								}
								DataItem<float> pHordeMeterDistance = new DataItem<float>("HordeMeterDistance", startValue82);
								itemAction.pHordeMeterDistance = pHordeMeterDistance;
							}
						}
						else if (num != 4129641136U)
						{
							if (num != 4139408471U)
							{
								if (num == 4265352596U)
								{
									if (name == "BulletIcon")
									{
										string startValue83;
										try
										{
											startValue83 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException81)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException81);
										}
										DataItem<string> pBulletIcon = new DataItem<string>("BulletIcon", startValue83);
										itemAction.pBulletIcon = pBulletIcon;
									}
								}
							}
							else if (name == "Debuff")
							{
								string startValue84;
								try
								{
									startValue84 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException82)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException82);
								}
								DataItem<string> pDebuff = new DataItem<string>("Debuff", startValue84);
								itemAction.pDebuff = pDebuff;
							}
						}
						else if (name == "Adjacentblock")
						{
							string startValue85;
							try
							{
								startValue85 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception innerException83)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, innerException83);
							}
							DataItem<string> pAdjacentblock = new DataItem<string>("Adjacentblock", startValue85);
							itemAction.pAdjacentblock = pAdjacentblock;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						name = positionXmlElement.Name;
						int num2 = dictionary2[name];
						dictionary2[name] = num2 + 1;
					}
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ItemActionData.Parser.knownAttributesMultiplicity)
				{
					int num3 = dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0;
					if ((keyValuePair.Value.hasMin && num3 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num3 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num3.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return itemAction;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Delay",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Range",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundStart",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundRepeat",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundEnd",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundEmpty",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundReload",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundWarning",
					new Range<int>(true, 0, true, 1)
				},
				{
					"StaminaUsage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UseTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname1",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname2",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname3",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname4",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname5",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname6",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname7",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname8",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FocusedBlockname9",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ChangeItemTo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ChangeBlockTo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DoBlockAction",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainHealth",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainFood",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainWater",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainStamina",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainSickness",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainWellness",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Buff",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BuffChance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Debuff",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CreateItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ConditionRaycastBlock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainGas",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Consume",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Blockname",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowStrengthDefault",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowStrengthMax",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MaxStrainTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MagazineSize",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MagazineItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ReloadTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BulletIcon",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RaysPerShot",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RaysSpread",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Sphere",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CrosshairMinDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CrosshairMaxDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageEntity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageBlock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticlesMuzzleFire",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticlesMuzzleSmoke",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BlockRange",
					new Range<int>(true, 0, true, 1)
				},
				{
					"AutoFire",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HordeMeterRate",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HordeMeterDistance",
					new Range<int>(true, 0, true, 1)
				},
				{
					"HitmaskOverride",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SingleMagazineUsage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BulletMaterial",
					new Range<int>(true, 0, true, 1)
				},
				{
					"InfiniteAmmo",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomMaxOut",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomMaxIn",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ZoomOverlay",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Velocity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FlyTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"LifeTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CollisionRadius",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ProjectileInitialVelocity",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Fertileblock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Adjacentblock",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairAmount",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UpgradeHitOffset",
					new Range<int>(true, 0, true, 1)
				},
				{
					"AllowedUpgradeItems",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RestrictedUpgradeItems",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UpgradeActionSound",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairActionSound",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ReferenceItem",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Mesh",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ActionIdx",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Title",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Description",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RecipesToLearn",
					new Range<int>(true, 0, true, 1)
				},
				{
					"InstantiateOnLoad",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundDraw",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageBonus",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Explosion",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
