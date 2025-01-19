using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class BaseChallengeObjective
	{
		public byte CurrentFileVersion { get; set; }

		public virtual ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Invalid;
			}
		}

		public virtual ChallengeClass.UINavTypes NavType
		{
			get
			{
				return ChallengeClass.UINavTypes.None;
			}
		}

		public bool Complete
		{
			get
			{
				return this.complete;
			}
			set
			{
				if (this.complete != value)
				{
					this.complete = value;
					if (this.ValueChanged != null)
					{
						this.ValueChanged();
					}
				}
			}
		}

		public int Current
		{
			get
			{
				return this.current;
			}
			set
			{
				if (this.current != value)
				{
					this.current = value;
					if (this.ValueChanged != null)
					{
						this.ValueChanged();
					}
				}
			}
		}

		public virtual void BaseInit()
		{
		}

		public EntityPlayerLocal Player
		{
			get
			{
				return this.Owner.Owner.Player;
			}
		}

		public void ResetComplete()
		{
			this.Complete = false;
			this.current = 0;
		}

		public event ObjectiveValueChanged ValueChanged;

		public string ObjectiveText
		{
			get
			{
				return string.Format("{0} {1}", this.DescriptionText, this.StatusText);
			}
		}

		public virtual string DescriptionText
		{
			get
			{
				return "";
			}
		}

		public virtual string StatusText
		{
			get
			{
				return string.Format("{0}/{1}", this.current, this.MaxCount);
			}
		}

		public virtual float FillAmount
		{
			get
			{
				return (float)this.current / (float)this.MaxCount;
			}
		}

		public virtual void Init()
		{
		}

		public virtual void HandleOnCreated()
		{
		}

		public virtual bool HandleCheckStatus()
		{
			this.Complete = this.CheckObjectiveComplete(false);
			return this.Complete;
		}

		public virtual void UpdateStatus()
		{
		}

		public virtual void HandleAddHooks()
		{
		}

		public virtual void HandleRemoveHooks()
		{
		}

		public virtual void HandleTrackingStarted()
		{
			this.IsTracking = true;
		}

		public virtual void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
		{
			this.current = obj.current;
			this.MaxCount = objFromClass.MaxCount;
			this.ShowRequirements = objFromClass.ShowRequirements;
			this.complete = (this.Current >= this.MaxCount);
		}

		public virtual void HandleTrackingEnded()
		{
			this.IsTracking = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleUpdatingCurrent()
		{
		}

		public static BaseChallengeObjective ReadObjective(byte _currentVersion, ChallengeObjectiveType _type, BinaryReader _br)
		{
			BaseChallengeObjective baseChallengeObjective = null;
			switch (_type)
			{
			case ChallengeObjectiveType.BlockPlace:
				baseChallengeObjective = new ChallengeObjectiveBlockPlace();
				break;
			case ChallengeObjectiveType.BlockUpgrade:
				baseChallengeObjective = new ChallengeObjectiveBlockUpgrade();
				break;
			case ChallengeObjectiveType.Bloodmoon:
				baseChallengeObjective = new ChallengeObjectiveBloodmoon();
				break;
			case ChallengeObjectiveType.Craft:
				baseChallengeObjective = new ChallengeObjectiveCraft();
				break;
			case ChallengeObjectiveType.CureDebuff:
				baseChallengeObjective = new ChallengeObjectiveCureDebuff();
				break;
			case ChallengeObjectiveType.EnterBiome:
				baseChallengeObjective = new ChallengeObjectiveEnterBiome();
				break;
			case ChallengeObjectiveType.Gather:
				baseChallengeObjective = new ChallengeObjectiveGather();
				break;
			case ChallengeObjectiveType.GatherIngredient:
				baseChallengeObjective = new ChallengeObjectiveGatherIngredient();
				break;
			case ChallengeObjectiveType.Harvest:
				baseChallengeObjective = new ChallengeObjectiveHarvest();
				break;
			case ChallengeObjectiveType.Hold:
				baseChallengeObjective = new ChallengeObjectiveHold();
				break;
			case ChallengeObjectiveType.Kill:
				baseChallengeObjective = new ChallengeObjectiveKill();
				break;
			case ChallengeObjectiveType.QuestComplete:
				baseChallengeObjective = new ChallengeObjectiveQuestComplete();
				break;
			case ChallengeObjectiveType.Scrap:
				baseChallengeObjective = new ChallengeObjectiveScrap();
				break;
			case ChallengeObjectiveType.Survive:
				baseChallengeObjective = new ChallengeObjectiveSurvive();
				break;
			case ChallengeObjectiveType.Trader:
				baseChallengeObjective = new ChallengeObjectiveTrader();
				break;
			case ChallengeObjectiveType.Wear:
				baseChallengeObjective = new ChallengeObjectiveWear();
				break;
			case ChallengeObjectiveType.Use:
				baseChallengeObjective = new ChallengeObjectiveUseItem();
				break;
			case ChallengeObjectiveType.ChallengeComplete:
				baseChallengeObjective = new ChallengeObjectiveChallengeComplete();
				break;
			case ChallengeObjectiveType.MeetTrader:
				baseChallengeObjective = new ChallengeObjectiveMeetTrader();
				break;
			case ChallengeObjectiveType.KillByTag:
				baseChallengeObjective = new ChallengeObjectiveKillByTag();
				break;
			case ChallengeObjectiveType.ChallengeStatAwarded:
				baseChallengeObjective = new ChallengeObjectiveChallengeStatAwarded();
				break;
			case ChallengeObjectiveType.SpendSkillPoint:
				baseChallengeObjective = new ChallengeObjectiveSpendSkillPoint();
				break;
			case ChallengeObjectiveType.Twitch:
				baseChallengeObjective = new ChallengeObjectiveTwitch();
				break;
			}
			if (baseChallengeObjective != null)
			{
				baseChallengeObjective.Read(_currentVersion, _br);
			}
			return baseChallengeObjective;
		}

		public virtual Recipe GetRecipeItem()
		{
			return null;
		}

		public virtual Recipe[] GetRecipeItems()
		{
			if (this.Owner.NeedsPreRequisites)
			{
				Recipe recipeFromRequirements = this.Owner.GetRecipeFromRequirements();
				if (recipeFromRequirements != null)
				{
					return new Recipe[]
					{
						recipeFromRequirements
					};
				}
			}
			return null;
		}

		public virtual void Read(byte _currentVersion, BinaryReader _br)
		{
			this.current = _br.ReadInt32();
		}

		public void WriteObjective(BinaryWriter _bw)
		{
			_bw.Write((byte)this.ObjectiveType);
			this.Write(_bw);
		}

		public virtual void Write(BinaryWriter _bw)
		{
			_bw.Write(this.current);
		}

		public virtual bool CheckObjectiveComplete(bool handleComplete = true)
		{
			this.HandleUpdatingCurrent();
			if (this.Current >= this.MaxCount)
			{
				this.Current = this.MaxCount;
				this.Complete = true;
				if (handleComplete)
				{
					this.Owner.HandleComplete();
				}
				return true;
			}
			if (handleComplete)
			{
				this.Owner.HandleComplete();
			}
			this.Complete = false;
			return false;
		}

		public virtual BaseChallengeObjective Clone()
		{
			return null;
		}

		public virtual void ParseElement(XElement e)
		{
			if (e.HasAttribute("count"))
			{
				this.MaxCount = StringParsers.ParseSInt32(e.GetAttribute("count"), 0, -1, NumberStyles.Integer);
			}
			if (e.HasAttribute("show_requirements"))
			{
				this.ShowRequirements = StringParsers.ParseBool(e.GetAttribute("show_requirements"), 0, -1, true);
			}
		}

		public static byte FileVersion = 1;

		public int MaxCount = 1;

		public bool IsRequirement;

		public Challenge Owner;

		public ChallengeClass OwnerClass;

		public bool ShowRequirements = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool complete;

		public bool IsTracking;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int current;
	}
}
