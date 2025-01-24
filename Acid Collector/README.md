This mod is not even ready, just a concept at this point, still need to add c# logic to edit tools

I wrote all the XML file to create a new dew collecotr window and controled the tools,

I added the prefab to a unity file, it loads, but doesn't let you push e to access

I instead use the default dew collector model,
however, it opens the default dew collector window,
In the XUI file there is a windowgroup that links to the windows, 
but I dont think there is an XML to link the Push E on a dew collector to use different windows, would have to dig in the DLL more,

Not gonna do that at this time. Stopping point.



entra notes



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

XUI/windows.xml

	<window name="windowDewCollector" width="{windowWidth}" height="378" controller="DewCollectorWindow" panel="Left" anchor_side="bottomright" visible="false" cursor_area="true" >
			<panel name="header" height="43" depth="0" disableautobackground="true" anchor_left="queue,0,-3" anchor_right="queue,1,0" >
				<sprite depth="1" name="headerbackground" sprite="ui_game_panel_header" anchor_left="queue,0,-3" anchor_right="queue,1,0" />
				<sprite depth="3" name="windowIcon" style="icon32px" pivot="center" pos="21,-21" sprite="ui_game_symbol_loot_sack" />
				<label depth="2" name="lootName" pos="39,-6" width="180" height="32" text_key="xuiDewCollector" font_size="32" />
				
				<rect anchor_left="queue,1,-300" anchor_right="queue,1,0" pivot="topleft" pos="0,0" controller="ContainerStandardControls" createuiwidget="true" visible="false">
					<!-- <button   depth="3" name="btnSort"             sprite="ui_game_symbol_sort"          tooltip_key="lblSortContainer"        pos="194, -22" style="icon32px, press, hover" pivot="center" sound="[paging_click]" /> -->
					<!-- <button   depth="3" name="btnMoveAll"          sprite="ui_game_symbol_store_all_up"     tooltip="{take_all_tooltip}"          pos="274, -22" style="icon32px, press, hover" pivot="center" sound="[paging_click]" /> -->
					<!-- <button   depth="3" name="btnMoveFillAndSmart" sprite="ui_game_symbol_store_similar_up" tooltip_key="xuiLootTakeFillAndSmart" pos="234, -22" style="icon32px, press, hover" pivot="center" sound="[paging_click]" /> -->
				</rect>
				
			</panel>
			<rect name="content" pos="3,-49" >
				<sprite depth="0" name="bg" color="255,255,255,1" type="sliced" sprite="menu_empty" anchor_left="queue,0,-3" anchor_bottom="queue,0,0" anchor_right="queue,1,0" anchor_top="queue,1,49" />
				<grid depth="12" name="queue" rows="1" cols="3" cell_width="75" cell_height="75" repeat_content="true" controller="DewCollectorContainer" required_item="drinkJarEmpty">
					<dewcollector_stack controller="DewCollectorStack" override_stack_count="1" name="0"/>
				</grid>
			</rect>
		</window>
		
	<window name="windowDewCollectorMods" width="228" height="121" panel="Right" cursor_area="true" >
	<!--#$-IGS END.-$#-->
		<panel style="header.panel">
			<sprite style="header.icon" sprite="ui_game_symbol_assemble"/>
			<label style="header.name" text="MODS" text_key="xuiTools" />
		</panel>

		<rect name="content" depth="0" pos="0,-46" height="75" disablefallthrough="true">

			<grid name="inventory" rows="1" cols="3" pos="3,-3" cell_width="75" cell_height="75" controller="DewCollectorModGrid" repeat_content="true"
			required_mods="toolDewGatherer,toolDewTarp,toolDewFilter" required_mods_only="true">
				<item_stack controller="RequiredItemStack" name="0"/>
			</grid>
		</rect>
	</window>

XUI/XUI.xml


		<window_group name="dewcollector" left_panel_valign_top="false" controller="XUiC_DewCollectorWindowGroup" close_compass_on_open="true" defaultSelected="0">
			<window name="windowDewCollector"/>
			<window name="windowDewCollectorMods" />
			<window name="windowNonPagingHeader" />
		</window_group>
		
Blocks.xml

	<!-- *** DEW_COLLECTOR -->
<block name="cntDewCollector">
	<property name="Class" value="DewCollector"/>
	<property name="UnlockedBy" value="craftingWorkstations"/>
	<property name="WorkstationIcon" value="ui_game_symbol_water"/>
	<property name="CreativeMode" value="Player"/>
	<property name="DescriptionKey" value="dewCollectorDesc"/>
	<property name="Material" value="MFuelBarrelPolymer"/>
	<property name="Shape" value="ModelEntity"/>
	<property name="Model" value="Entities/Furniture/collectorDewPrefab"/>
	<property name="MultiBlockDim" value="3,3,3"/>
	<property name="ImposterExchange" value="imposterBlock" param1="77"/>
	<property name="WaterFlow" value="permitted"/>
	<property name="Place" value="TowardsPlacerInverted"/>
	<property name="RestrictSubmergedPlacement" value="true"/>
	<property name="AllowedRotations" value="Basic90"/>
	<property name="Path" value="solid"/>
	<property name="IsDecoration" value="true"/>
	<property name="StabilitySupport" value="false"/>
	<property name="OpenSound" value="collector_open"/>
	<property name="CloseSound" value="collector_close"/>
	<property name="ConvertSound" value="collector_complete_item"/>
	<property name="MinConvertTime" value="21600"/> <!-- 21600 Game Seconds = 6 Game Hours -->
	<property name="MaxConvertTime" value="36000"/> <!-- 36000 Game Seconds = 10 Game Hours -->
	<property name="ConvertToItem" value="drinkJarRiverWater"/>
	<property name="ModdedConvertToItem" value="drinkJarBoiledWater"/>
	<property name="ModdedConvertSpeed" value="2"/>
	<property name="ModdedConvertCount" value="2"/>
	<property name="ModTransformNames" value="1,2,3"/>
	<property name="ModTypes" value="Speed,Count,Type"/>
	<property name="EconomicValue" value="5"/>
	<property name="EconomicBundleSize" value="1"/>
	<property name="SellableToTrader" value="false"/>
	<property name="MaxDamage" value="200"/>
	<property name="HeatMapStrength" value="2"/>
	<property name="HeatMapTime" value="5000"/>
	<property name="HeatMapFrequency" value="1000"/>
	<property class="RepairItems">
		<property name="resourceScrapPolymers" value="50"/>
	</property>
	<drop event="Harvest" name="resourceScrapPolymers" count="10,15" tag="allHarvest"/>
	<drop event="Harvest" name="resourceMetalPipe" count="1,3" tag="allHarvest"/>
	<drop event="Destroy" name="resourceScrapPolymers" count="10,15"/>
	<property name="DestroyFX" value="blockdestroy_cloth,collector_destroy"/>
	<property name="SortOrder1" value="B281"/>
	<property name="SortOrder2" value="0100"/>
	<property name="Group" value="Basics,Food/Cooking,Building,advBuilding"/>
	<property name="Tags" value="workstationSkill,twitch_workstation"/>
	<property name="FilterTags" value="MC_playerBlocks,SC_decor"/>
	<property name="SoundPickup" value="dewcollector_grab"/>
	<property name="SoundPlace" value="dewcollector_place"/>
</block>


