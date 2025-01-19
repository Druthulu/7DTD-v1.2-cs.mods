# 7DTD-v1.2-cs.mods
 Collection of my c# mods for this game version


Acid collector is just a framework atm, need to reduce the asset and code the dll to add the desired functionality
		
	also try to make it only work during rain

	acid collector only when rain
	using System;
	using HarmonyLib;

	namespace Harmony
	{
		[HarmonyPatch(typeof(TileEntityDewCollector), "HandleSkyCheck")]
		public class DewCollectorSkyCheck_Patch
		{
			public static void Postfix(TileEntityDewCollector __instance, ref bool __result)
			{
				bool canUseIndoors;
				__instance.blockValue.Block.Properties.ParseBool("CanUseIndoors", out canUseIndoors, false);
				__result = (__result && !canUseIndoors);
			}
		}
	}



Joke mod is a work in progress, most works, some really broken, need to rebuild assets into unique packages and test again


	vehicles

	unknown type create enity
	null ref


	zombies errors

	lots of erroers becuase of shared files, need to get seperate unity3d files.


RandomSizes needs more work, seperately they both work, but I want to combine them and add configuration settings xml


Trader markers works great for dedi server,
	Would like to be able to detect and scan the save file without the need for the user to copy it over

	Single player version of this mod, would be totally seperate functionality, but requesed so its on the todo list

	string text = Path.Combine(Directory.GetCurrentDirectory(), "Mods\\TimeDilation\\Config\\settings.xml");


Weed effects,
	Works, want to add more stuff, like silly sounds buff, and celebration kill confetti

		announce to the server someone is blazin
	mod say to server
	this.sayToServer(this.thresholdNormalTime.ToString() + "+ players are connected.  Normal time is in effect.");


	
	<buff name="joke_SillySounds" name_key="joke_SillySounds" description_key="joke_SillySoundsDesc" icon="ui_game_symbol_twitch_sound_effects" icon_color="175,175,255">
		<stack_type value="replace"/>
		<duration value="300"/>
		<display_value value="duration"/>

		<effect_group>
			<passive_effect name="BuffBlink" operation="base_set" value="2" duration="0,3" tags="joke_SillySounds"/>

			<triggered_effect trigger="onSelfEnteredGame" action="RemoveBuff" buff="joke_SillySounds"/>
		</effect_group>

		<effect_group>
			<triggered_effect trigger="onSelfBuffStart" action="AddBuff" buff="joke_SillySoundsHidden"/>
		</effect_group>
	</buff>	

	<buff name="joke_SillySoundsHidden" hidden="true">
		<stack_type value="ignore"/>
		<duration value="0"/><update_rate value="1"/>
		
		<effect_group>
			<triggered_effect trigger="onSelfBuffStart" action="AltSounds" enabled="true"/>

			<triggered_effect trigger="onSelfBuffRemove" action="AltSounds" enabled="false"/>

		</effect_group>

		<effect_group> <!-- terminate only if all silly buffs are gone -->
			<requirements>
				<requirement name="!HasBuff" buff="twitch_voteSillySounds"/>
				<requirement name="!HasBuff" buff="twitch_buffSillySounds"/>
				<requirement name="!HasBuff" buff="joke_SillySounds"/>
			</requirements>
				<triggered_effect trigger="onSelfBuffUpdate" action="RemoveBuff" buff="joke_SillySoundsHidden"/>
		</effect_group>
		
	</buff>


	<buff name="joke_buffCelebrate" name_key="joke_buffCelebrate" description_key="joke_buffCelebrateDesc" icon="ui_game_symbol_twitch_celebrate" icon_color="175,175,255">
		<stack_type value="replace"/>
		<duration value="90"/>
		<display_value value="duration"/>

		<effect_group>
			<passive_effect name="BuffBlink" operation="base_set" value="2" duration="0,3" tags="joke_buffCelebrate"/>

			<triggered_effect trigger="onSelfEnteredGame" action="RemoveBuff" buff="joke_buffCelebrate"/>
		</effect_group>

		<effect_group>
			<passive_effect name="CelebrationKill" operation="base_set" value="1" />
		</effect_group>
	</buff>



New Ideas

	changing the supply plane model

		<set xpath="//entity_class[@name='supplyPlane']/property[@name='Prefab']/@value">#@modfolder:Resources/RMFVehicles.unity3d?CardboardRocketVehicle.prefab</set>
		<set xpath="//entity_class[@name='supplyPlane']/property[@name='Mesh']/@value">#@modfolder:Resources/RMFVehicles.unity3d?CardboardRocketVehicle.prefab</set>


	Dangerous safes
		play an alarm when opening safes

		<configs>

		<!-- Adds an alarm to certain locked containers // only triggers when destroyed -->
		<append xpath="/blocks/block[@name='cntGunSafe']">
			<property name="DowngradeEvent" value="block_safe_alarm_start" />    
		</append>

		<append xpath="/blocks/block[@name='cntLootChestHero']">
			<property name="DowngradeEvent" value="block_safe_alarm_start" />    
		</append>

		<append xpath="/blocks/block[@name='cntHardenedChestSecure']">
			<property name="DowngradeEvent" value="block_safe_alarm_start" />    
		</append>

		<append xpath="/blocks/block[@name='cntHardenedChestSecureT5']">
			<property name="DowngradeEvent" value="block_safe_alarm_start" />    
		</append>


		</configs>		




	Custom beds

		bedroll has alternate placement models, add them

		look up this other helper blocks, like appliances, maybe add more things for decorations like pool table


	Yeet
		for throwing, sound meme yeet


