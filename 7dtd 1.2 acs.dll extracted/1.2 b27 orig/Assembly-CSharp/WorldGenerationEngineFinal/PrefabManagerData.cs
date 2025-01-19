using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class PrefabManagerData
	{
		public IEnumerator LoadPrefabs()
		{
			if (this.AllPrefabDatas.Count != 0)
			{
				yield break;
			}
			MicroStopwatch ms = new MicroStopwatch(true);
			List<PathAbstractions.AbstractedLocation> prefabs = PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, null, null, true);
			FastTags<TagGroup.Poi> filter = FastTags<TagGroup.Poi>.Parse("navonly,devonly,testonly,biomeonly");
			int num2;
			for (int i = 0; i < prefabs.Count; i = num2 + 1)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = prefabs[i];
				int num = abstractedLocation.Folder.LastIndexOf("/Prefabs/");
				if (num < 0 || !abstractedLocation.Folder.Substring(num + 8, 5).EqualsCaseInsensitive("/test"))
				{
					PrefabData prefabData = PrefabData.LoadPrefabData(abstractedLocation);
					try
					{
						if (prefabData != null && !prefabData.Tags.Test_AnySet(filter) && !prefabData.Tags.IsEmpty)
						{
							this.AllPrefabDatas[abstractedLocation.Name.ToLower()] = prefabData;
						}
					}
					catch (Exception)
					{
						Log.Error("Could not load prefab data for " + abstractedLocation.Name);
					}
					if (ms.ElapsedMilliseconds > 500L)
					{
						yield return null;
						ms.ResetAndRestart();
					}
				}
				num2 = i;
			}
			Log.Out("LoadPrefabs {0} of {1} in {2}", new object[]
			{
				this.AllPrefabDatas.Count,
				prefabs.Count,
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			yield break;
		}

		public void ShufflePrefabData(int _seed)
		{
			this.prefabDataList.Clear();
			this.AllPrefabDatas.CopyValuesTo(this.prefabDataList);
			PrefabManager.Shuffle<PrefabData>(_seed, ref this.prefabDataList);
		}

		public void Cleanup()
		{
			this.AllPrefabDatas.Clear();
			this.prefabDataList.Clear();
		}

		public Prefab GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi> _tags, int _townshipId, Vector2i size = default(Vector2i), bool useAnySizeSmaller = false)
		{
			Vector2i minSize = useAnySizeSmaller ? Vector2i.zero : size;
			List<PrefabData> list = this.prefabDataList.FindAll((PrefabData _pd) => !_pd.Tags.Test_AnySet(this.PartsAndTilesTags) && PrefabManager.isSizeValid(_pd, minSize, size) && _pd.Tags.Test_AnySet(_tags));
			if (list.Count == 0)
			{
				return null;
			}
			PrefabManager.Shuffle<PrefabData>(this.previewSeed, ref list);
			Prefab prefab = new Prefab();
			prefab.Load(list[0].location, true, true, false, false);
			this.previewSeed++;
			return prefab;
		}

		public readonly Dictionary<string, PrefabData> AllPrefabDatas = new Dictionary<string, PrefabData>();

		public List<PrefabData> prefabDataList = new List<PrefabData>();

		public readonly FastTags<TagGroup.Poi> PartsAndTilesTags = FastTags<TagGroup.Poi>.Parse("streettile,part");

		public readonly FastTags<TagGroup.Poi> WildernessTags = FastTags<TagGroup.Poi>.Parse("wilderness");

		public readonly FastTags<TagGroup.Poi> TraderTags = FastTags<TagGroup.Poi>.Parse("trader");

		[PublicizedFrom(EAccessModifier.Private)]
		public int previewSeed;
	}
}
