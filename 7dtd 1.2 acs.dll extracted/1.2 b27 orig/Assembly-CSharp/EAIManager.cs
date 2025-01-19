using System;
using System.Collections.Generic;
using GamePath;
using UnityEngine;

public class EAIManager
{
	public EAIManager(EntityAlive _entity)
	{
		this.entity = _entity;
		this.random = _entity.world.aiDirector.random;
		this.entity.rand = this.random;
		this.tasks = new EAITaskList(this);
		this.targetTasks = new EAITaskList(this);
		this.interestDistance = 10f;
	}

	public void CopyPropertiesFromEntityClass(EntityClass ec)
	{
		ec.Properties.ParseFloat(EntityClass.PropAIFeralSense, ref this.feralSense);
		ec.Properties.ParseFloat(EntityClass.PropAIGroupCircle, ref this.groupCircle);
		ec.Properties.ParseFloat(EntityClass.PropAINoiseSeekDist, ref this.noiseSeekDist);
		ec.Properties.ParseFloat(EntityClass.PropAISeeOffset, ref this.seeOffset);
		Vector2 vector = new Vector2(1f, 1f);
		ec.Properties.ParseVec(EntityClass.PropAIPathCostScale, ref vector);
		this.pathCostScale = this.random.RandomRange(vector.x, vector.y);
		this.partialPathHeightScale = 1f - this.pathCostScale;
		string @string = ec.Properties.GetString("AITask");
		if (@string.Length <= 0)
		{
			int num = 1;
			string text;
			for (;;)
			{
				string key = EntityClass.PropAITask + num.ToString();
				if (!ec.Properties.Values.TryGetValue(key, out text) || text.Length == 0)
				{
					goto IL_194;
				}
				EAIBase eaibase = EAIManager.CreateInstance(text);
				if (eaibase == null)
				{
					break;
				}
				eaibase.Init(this.entity);
				DictionarySave<string, string> dictionarySave = ec.Properties.ParseKeyData(key);
				if (dictionarySave != null)
				{
					try
					{
						eaibase.SetData(dictionarySave);
					}
					catch (Exception ex)
					{
						Log.Error("EAIManager {0} SetData error {1}", new object[]
						{
							text,
							ex
						});
					}
				}
				this.tasks.AddTask(num, eaibase);
				num++;
			}
			throw new Exception("Class '" + text + "' not found!");
		}
		this.ParseTasks(@string, this.tasks);
		IL_194:
		string string2 = ec.Properties.GetString("AITarget");
		if (string2.Length > 0)
		{
			this.ParseTasks(string2, this.targetTasks);
			return;
		}
		int num2 = 1;
		string text2;
		for (;;)
		{
			string key2 = EntityClass.PropAITargetTask + num2.ToString();
			if (!ec.Properties.Values.TryGetValue(key2, out text2) || text2.Length == 0)
			{
				return;
			}
			EAIBase eaibase2 = EAIManager.CreateInstance(text2);
			if (eaibase2 == null)
			{
				break;
			}
			eaibase2.Init(this.entity);
			DictionarySave<string, string> dictionarySave2 = ec.Properties.ParseKeyData(key2);
			if (dictionarySave2 != null)
			{
				try
				{
					eaibase2.SetData(dictionarySave2);
				}
				catch (Exception ex2)
				{
					Log.Error("EAIManager {0} SetData error {1}", new object[]
					{
						text2,
						ex2
					});
				}
			}
			this.targetTasks.AddTask(num2, eaibase2);
			num2++;
		}
		throw new Exception("Class '" + text2 + "' not found!");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseTasks(string _str, EAITaskList _list)
	{
		int num = 1;
		for (int i = 0; i < _str.Length; i++)
		{
			if (char.IsLetter(_str[i]))
			{
				int num2 = _str.IndexOf('|', i + 1);
				if (num2 < 0)
				{
					num2 = _str.Length;
				}
				string text = _str.Substring(i, num2 - i);
				string text2 = text;
				string text3 = null;
				int num3 = text.IndexOf(' ');
				if (num3 >= 0)
				{
					text2 = text.Substring(0, num3);
					text3 = text.Substring(num3 + 1);
				}
				EAIBase eaibase = EAIManager.CreateInstance(text2);
				if (eaibase == null)
				{
					throw new Exception("Class '" + text2 + "' not found!");
				}
				eaibase.Init(this.entity);
				if (text3 != null)
				{
					DictionarySave<string, string> dictionarySave = DynamicProperties.ParseData(text3);
					if (dictionarySave != null)
					{
						try
						{
							eaibase.SetData(dictionarySave);
						}
						catch (Exception ex)
						{
							Log.Error("EAIManager {0} SetData error {1}", new object[]
							{
								text2,
								ex
							});
						}
					}
				}
				_list.AddTask(num, eaibase);
				num++;
				i = num2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EAIBase CreateInstance(string _className)
	{
		return (EAIBase)Activator.CreateInstance(EAIManager.GetType(_className));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Type GetType(string _className)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_className);
		if (num <= 2414274217U)
		{
			if (num <= 1340592684U)
			{
				if (num <= 220555081U)
				{
					if (num != 87276885U)
					{
						if (num == 220555081U)
						{
							if (_className == "SetNearestEntityAsTarget")
							{
								return typeof(EAISetNearestEntityAsTarget);
							}
						}
					}
					else if (_className == "BlockIf")
					{
						return typeof(EAIBlockIf);
					}
				}
				else if (num != 244691017U)
				{
					if (num != 1005439377U)
					{
						if (num == 1340592684U)
						{
							if (_className == "Territorial")
							{
								return typeof(EAITerritorial);
							}
						}
					}
					else if (_className == "ApproachSpot")
					{
						return typeof(EAIApproachSpot);
					}
				}
				else if (_className == "BreakBlock")
				{
					return typeof(EAIBreakBlock);
				}
			}
			else if (num <= 1771441078U)
			{
				if (num != 1728706612U)
				{
					if (num == 1771441078U)
					{
						if (_className == "Look")
						{
							return typeof(EAILook);
						}
					}
				}
				else if (_className == "Wander")
				{
					return typeof(EAIWander);
				}
			}
			else if (num != 1994098438U)
			{
				if (num != 2294454340U)
				{
					if (num == 2414274217U)
					{
						if (_className == "RunawayWhenHurt")
						{
							return typeof(EAIRunawayWhenHurt);
						}
					}
				}
				else if (_className == "DestroyArea")
				{
					return typeof(EAIDestroyArea);
				}
			}
			else if (_className == "BlockingTargetTask")
			{
				return typeof(EAIBlockingTargetTask);
			}
		}
		else if (num <= 3549489919U)
		{
			if (num <= 2454737095U)
			{
				if (num != 2423584467U)
				{
					if (num == 2454737095U)
					{
						if (_className == "ApproachDistraction")
						{
							return typeof(EAIApproachDistraction);
						}
					}
				}
				else if (_className == "Leap")
				{
					return typeof(EAILeap);
				}
			}
			else if (num != 3085126581U)
			{
				if (num != 3546899167U)
				{
					if (num == 3549489919U)
					{
						if (_className == "RangedAttackTarget")
						{
							return typeof(EAIRangedAttackTarget);
						}
					}
				}
				else if (_className == "ApproachAndAttackTarget")
				{
					return typeof(EAIApproachAndAttackTarget);
				}
			}
			else if (_className == "TakeCover")
			{
				return typeof(EAITakeCover);
			}
		}
		else if (num <= 3659636919U)
		{
			if (num != 3618649518U)
			{
				if (num == 3659636919U)
				{
					if (_className == "RangedAttackTarget2")
					{
						return typeof(EAIRangedAttackTarget2);
					}
				}
			}
			else if (_className == "SetNearestCorpseAsTarget")
			{
				return typeof(EAISetNearestCorpseAsTarget);
			}
		}
		else if (num != 3938759995U)
		{
			if (num != 4112963184U)
			{
				if (num == 4183380984U)
				{
					if (_className == "SetAsTargetIfHurt")
					{
						return typeof(EAISetAsTargetIfHurt);
					}
				}
			}
			else if (_className == "Dodge")
			{
				return typeof(EAIDodge);
			}
		}
		else if (_className == "RunawayFromEntity")
		{
			return typeof(EAIRunawayFromEntity);
		}
		Log.Warning("EAIManager GetType slow lookup for {0}", new object[]
		{
			_className
		});
		return Type.GetType("EAI" + _className);
	}

	public void Update()
	{
		this.interestDistance = Mathf.MoveTowards(this.interestDistance, 10f, 0.004166667f);
		this.targetTasks.OnUpdateTasks();
		this.tasks.OnUpdateTasks();
		this.UpdateDebugName();
	}

	public void UpdateDebugName()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks))
		{
			EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			this.entity.DebugNameInfo = this.MakeDebugName(primaryPlayer);
		}
	}

	public string MakeDebugName(EntityPlayer player)
	{
		EntityMoveHelper moveHelper = this.entity.moveHelper;
		string str = string.Empty;
		if (this.entity.IsSleeper)
		{
			str += string.Format("\nSleeper {0}{1}", this.entity.IsSleeping ? "Sleep " : "", this.entity.IsSleeperPassive ? "Passive" : "");
		}
		str += string.Format("\nHealth {0} / {1}, PCost {2}, InterestD {3}", new object[]
		{
			this.entity.Health,
			this.entity.GetMaxHealth(),
			this.pathCostScale.ToCultureInvariantString(".00"),
			this.interestDistance.ToCultureInvariantString("0.000")
		});
		string text = string.Format("\n{0}{1}", this.entity.IsAlert ? string.Format("Alert {0}, ", ((float)this.entity.GetAlertTicks() / 20f).ToCultureInvariantString("0.00")) : "", this.entity.HasInvestigatePosition ? string.Format("Investigate {0}, ", ((float)this.entity.GetInvestigatePositionTicks() / 20f).ToCultureInvariantString("0.00")) : "");
		if (text.Length > 1)
		{
			str += text;
		}
		string text2 = string.Format("\n{0}{1}{2}{3}{4}{5}", new object[]
		{
			moveHelper.IsActive ? string.Format("Move {0} {1},", this.entity.GetMoveSpeedAggro().ToCultureInvariantString(".00"), this.entity.GetSpeedModifier().ToCultureInvariantString(".00")) : "",
			moveHelper.IsBlocked ? string.Format("Blocked {0},", moveHelper.BlockedTime.ToCultureInvariantString("0.00")) : "",
			moveHelper.CanBreakBlocks ? "CanBrk, " : "",
			moveHelper.IsUnreachableAbove ? "UnreachAbove, " : "",
			moveHelper.IsUnreachableSide ? "UnreachSide, " : "",
			moveHelper.IsUnreachableSideJump ? "UnreachSideJump" : ""
		});
		if (text2.Length > 1)
		{
			str += text2;
		}
		if (this.entity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			str += string.Format("\nStun {0}, {1}", this.entity.bodyDamage.CurrentStun.ToStringCached<EnumEntityStunType>(), this.entity.bodyDamage.StunDuration.ToCultureInvariantString("0.00"));
		}
		if (this.entity.emodel && this.entity.emodel.IsRagdollActive)
		{
			str = str + "\nRagdoll " + this.entity.emodel.GetRagdollDebugInfo();
		}
		for (int i = 0; i < this.tasks.GetExecutingTasks().Count; i++)
		{
			EAITaskEntry eaitaskEntry = this.tasks.GetExecutingTasks()[i];
			str = str + "\n1 " + eaitaskEntry.action.ToString();
		}
		for (int j = 0; j < this.targetTasks.GetExecutingTasks().Count; j++)
		{
			EAITaskEntry eaitaskEntry2 = this.targetTasks.GetExecutingTasks()[j];
			str = str + "\n2 " + eaitaskEntry2.action.ToString();
		}
		if (this.entity.IsSleeping)
		{
			float distance = this.entity.GetDistance(player);
			float value;
			float value2;
			this.entity.GetSleeperDebugScale(distance, out value, out value2);
			string str2 = string.Format("\nLight {0:0} groan{1:0} wake{2:0}, Noise {3:0} groan{4:0} wake{5:0}", new object[]
			{
				player.Stealth.lightLevel.ToCultureInvariantString(),
				value2.ToCultureInvariantString(),
				value.ToCultureInvariantString(),
				this.entity.noisePlayerVolume.ToCultureInvariantString(),
				this.entity.noiseGroan.ToCultureInvariantString(),
				this.entity.noiseWake.ToCultureInvariantString()
			});
			str += str2;
		}
		else
		{
			float seeDistance = this.GetSeeDistance(player);
			float seeStealthDebugScale = this.entity.GetSeeStealthDebugScale(seeDistance);
			string str3 = string.Format("\nLight {0:0} sight {1:0}, noise {2:0} dist {3:0}", new object[]
			{
				player.Stealth.lightLevel.ToCultureInvariantString(),
				seeStealthDebugScale.ToCultureInvariantString(),
				this.entity.noisePlayerVolume.ToCultureInvariantString(),
				this.entity.noisePlayerDistance.ToCultureInvariantString()
			});
			str += str3;
		}
		return str + this.entity.MakeDebugNameInfo();
	}

	public bool CheckPath(PathInfo pathInfo)
	{
		List<EAITaskEntry> executingTasks = this.tasks.GetExecutingTasks();
		for (int i = 0; i < executingTasks.Count; i++)
		{
			if (executingTasks[i].action.IsPathUsageBlocked(pathInfo.path))
			{
				return false;
			}
		}
		return true;
	}

	public void DamagedByEntity()
	{
		EntityMoveHelper moveHelper = this.entity.moveHelper;
		if (moveHelper != null)
		{
			moveHelper.IsDestroyAreaTryUnreachable = false;
		}
		EAIDestroyArea task = this.tasks.GetTask<EAIDestroyArea>();
		if (task == null)
		{
			return;
		}
		task.Stop();
	}

	public void SleeperWokeUp()
	{
		for (int i = 0; i < this.targetTasks.Tasks.Count; i++)
		{
			this.targetTasks.Tasks[i].executeTime = 0f;
		}
	}

	public void FallHitGround(float distance)
	{
		if (distance >= 0.8f)
		{
			this.entity.ConditionalTriggerSleeperWakeUp();
		}
		if (distance >= 2.5f)
		{
			EntityMoveHelper moveHelper = this.entity.moveHelper;
			if (moveHelper.IsActive && (moveHelper.IsUnreachableSide || moveHelper.IsMoveToAbove()))
			{
				this.ClearTaskDelay<EAIDestroyArea>(this.tasks);
				moveHelper.UnreachablePercent += 0.3f;
				moveHelper.IsDestroyAreaTryUnreachable = true;
				Bounds bb = new Bounds(this.entity.position, new Vector3(20f, 10f, 20f));
				this.entity.world.GetEntitiesInBounds(typeof(EntityHuman), bb, this.allies);
				if (this.allies.Count >= 3)
				{
					for (int i = 0; i < 2; i++)
					{
						int index = this.entity.rand.RandomRange(this.allies.Count);
						EntityHuman entityHuman = (EntityHuman)this.allies[index];
						entityHuman.moveHelper.UnreachablePercent += 0.12f;
						entityHuman.moveHelper.IsDestroyAreaTryUnreachable = true;
					}
				}
				this.allies.Clear();
			}
		}
	}

	public float GetSeeDistance(Entity _seeEntity)
	{
		return this.entity.GetDistance(_seeEntity) - this.seeOffset;
	}

	public static float CalcSenseScale()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.ZombieFeralSense))
		{
		case 1:
			if (GameManager.Instance.World.IsDaytime())
			{
				return 1f;
			}
			break;
		case 2:
			if (GameManager.Instance.World.IsDark())
			{
				return 1f;
			}
			break;
		case 3:
			return 1f;
		}
		return 0f;
	}

	public void SetTargetOnlyPlayers(float _distance)
	{
		List<EAITaskEntry> list = this.tasks.Tasks;
		for (int i = 0; i < list.Count; i++)
		{
			EAIApproachAndAttackTarget eaiapproachAndAttackTarget = list[i].action as EAIApproachAndAttackTarget;
			if (eaiapproachAndAttackTarget != null)
			{
				eaiapproachAndAttackTarget.SetTargetOnlyPlayers();
			}
		}
		List<EAITaskEntry> list2 = this.targetTasks.Tasks;
		for (int j = 0; j < list2.Count; j++)
		{
			EAISetNearestEntityAsTarget eaisetNearestEntityAsTarget = list2[j].action as EAISetNearestEntityAsTarget;
			if (eaisetNearestEntityAsTarget != null)
			{
				eaisetNearestEntityAsTarget.SetTargetOnlyPlayers(_distance);
			}
		}
	}

	public List<T> GetTasks<T>() where T : class
	{
		return this.getTaskTypes<T>(this.tasks);
	}

	public List<T> GetTargetTasks<T>() where T : class
	{
		return this.getTaskTypes<T>(this.targetTasks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> getTaskTypes<T>(EAITaskList taskList) where T : class
	{
		List<T> list = new List<T>();
		for (int i = 0; i < taskList.Tasks.Count; i++)
		{
			EAITaskEntry eaitaskEntry = taskList.Tasks[i];
			if (eaitaskEntry.action is T)
			{
				list.Add(eaitaskEntry.action as T);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTaskDelay<T>(EAITaskList taskList) where T : class
	{
		for (int i = 0; i < taskList.Tasks.Count; i++)
		{
			EAITaskEntry eaitaskEntry = taskList.Tasks[i];
			if (eaitaskEntry.action is T)
			{
				eaitaskEntry.executeTime = 0f;
			}
		}
	}

	public static void ToggleAnimFreeze()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EAIManager.isAnimFreeze = !EAIManager.isAnimFreeze;
		List<Entity> list = world.Entities.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityAlive entityAlive = list[i] as EntityAlive;
			if (entityAlive && entityAlive.aiManager != null && !entityAlive.emodel.IsRagdollActive && entityAlive.emodel.avatarController)
			{
				Animator animator = entityAlive.emodel.avatarController.GetAnimator();
				if (animator)
				{
					animator.enabled = !EAIManager.isAnimFreeze;
				}
			}
		}
	}

	public const float cInterestDistanceMax = 10f;

	public float interestDistance;

	public float lookTime;

	public const float cSenseScaleMax = 1.6f;

	public float feralSense;

	public float groupCircle;

	public float noiseSeekDist;

	public float pathCostScale;

	public float partialPathHeightScale;

	public float seeOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAITaskList tasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAITaskList targetTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> allies = new List<Entity>();

	public static bool isAnimFreeze;
}
