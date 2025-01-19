using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class PathAbstractions
{
	public static void InvalidateCaches()
	{
		for (int i = 0; i < PathAbstractions.allSearchDefs.Count; i++)
		{
			PathAbstractions.allSearchDefs[i].InvalidateCache();
		}
	}

	public static bool CacheEnabled
	{
		get
		{
			return PathAbstractions.cacheEnabled;
		}
		set
		{
			PathAbstractions.cacheEnabled = value;
			if (!value)
			{
				PathAbstractions.InvalidateCaches();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PathAbstractions.SearchDefinition> allSearchDefs = new List<PathAbstractions.SearchDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> userDataPath = new Func<string>(GameIO.GetUserGameDataDir);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> gameDataPath = new Func<string>(GameIO.GetApplicationPath);

	public static readonly PathAbstractions.SearchDefinition WorldsSearchPaths = new PathAbstractions.SearchDefinition(true, null, false, false, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathSaves("World", true),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.userDataPath, "GeneratedWorlds", false),
		new PathAbstractions.SearchPathMods("Worlds", false),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Worlds", false)
	});

	public static readonly PathAbstractions.SearchDefinition PrefabsSearchPaths = new PathAbstractions.SearchDefinition(false, ".tts", true, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.userDataPath, "LocalPrefabs", false),
		new PathAbstractions.SearchPathMods("Prefabs", false),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Prefabs", false)
	});

	public static readonly PathAbstractions.SearchDefinition PrefabImpostersSearchPaths = new PathAbstractions.SearchDefinition(false, ".mesh", false, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.userDataPath, "LocalPrefabs", false),
		new PathAbstractions.SearchPathMods("Prefabs", false),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Prefabs", false)
	});

	public static readonly PathAbstractions.SearchDefinition RwgStampsSearchPaths = new PathAbstractions.SearchDefinition(false, "", true, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.userDataPath, "LocalStamps", false),
		new PathAbstractions.SearchPathMods("Stamps", false),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Stamps", false)
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool cacheEnabled;

	public enum EAbstractedLocationType
	{
		HostSave,
		LocalSave,
		UserDataPath,
		Mods,
		GameData,
		None
	}

	public readonly struct AbstractedLocation : IEquatable<PathAbstractions.AbstractedLocation>, IComparable<PathAbstractions.AbstractedLocation>, IComparable
	{
		public string FullPath
		{
			get
			{
				return this.Folder + "/" + this.FileNameNoExtension + this.Extension;
			}
		}

		public string FullPathNoExtension
		{
			get
			{
				return this.Folder + "/" + this.FileNameNoExtension;
			}
		}

		public AbstractedLocation(PathAbstractions.EAbstractedLocationType _type, string _name, string _fullPath, string _relativePath, bool _isFolder, Mod _containingMod = null)
		{
			_fullPath = ((_fullPath != null) ? _fullPath.Replace("\\", "/") : null);
			this.Type = _type;
			this.Name = Path.GetFileName(_name);
			string directoryName = Path.GetDirectoryName(_fullPath);
			this.Folder = ((directoryName != null) ? directoryName.Replace("\\", "/") : null);
			this.RelativePath = _relativePath;
			this.FileNameNoExtension = Path.GetFileNameWithoutExtension(_fullPath);
			this.Extension = Path.GetExtension(_fullPath);
			this.Extension = (string.IsNullOrEmpty(this.Extension) ? null : this.Extension);
			this.IsFolder = _isFolder;
			this.ContainingMod = _containingMod;
		}

		public AbstractedLocation(PathAbstractions.EAbstractedLocationType _type, string _name, string _folder, string _relativePath, string _fileNameNoExtension, string _extension, bool _isFolder, Mod _containingMod = null)
		{
			this.Type = _type;
			this.Name = _name;
			this.Folder = ((_folder != null) ? _folder.Replace("\\", "/") : null);
			this.RelativePath = _relativePath;
			this.FileNameNoExtension = _fileNameNoExtension;
			this.Extension = _extension;
			this.IsFolder = _isFolder;
			this.ContainingMod = _containingMod;
		}

		public bool Exists()
		{
			if (this.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				return false;
			}
			if (!this.IsFolder)
			{
				return SdFile.Exists(this.FullPath);
			}
			return SdDirectory.Exists(this.FullPath);
		}

		public bool Equals(PathAbstractions.AbstractedLocation _other)
		{
			return this.Type == _other.Type && (this.Type == PathAbstractions.EAbstractedLocationType.None || (this.IsFolder == _other.IsFolder && string.Equals(this.Name, _other.Name) && GameIO.PathsEquals(this.FullPath, _other.FullPath, true)));
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (_obj is PathAbstractions.AbstractedLocation)
			{
				PathAbstractions.AbstractedLocation other = (PathAbstractions.AbstractedLocation)_obj;
				return this.Equals(other);
			}
			return false;
		}

		public static bool operator ==(PathAbstractions.AbstractedLocation _a, PathAbstractions.AbstractedLocation _b)
		{
			return _a.Equals(_b);
		}

		public static bool operator !=(PathAbstractions.AbstractedLocation _a, PathAbstractions.AbstractedLocation _b)
		{
			return !(_a == _b);
		}

		public override int GetHashCode()
		{
			return (((this.Name != null) ? this.Name.GetHashCode() : 0) * 397 ^ (int)this.Type) * 397 ^ ((this.FullPath != null) ? this.FullPath.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return this.Name + " (src: " + this.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>() + ")";
		}

		public int CompareTo(PathAbstractions.AbstractedLocation _other)
		{
			int num = string.Compare(this.Name, _other.Name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			int num2 = this.Type.CompareTo(_other.Type);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = string.Compare(this.FileNameNoExtension, _other.FileNameNoExtension, StringComparison.OrdinalIgnoreCase);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = string.Compare(this.Extension, _other.Extension, StringComparison.OrdinalIgnoreCase);
			if (num4 != 0)
			{
				return num4;
			}
			return string.Compare(this.Folder, _other.Folder, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (_obj is PathAbstractions.AbstractedLocation)
			{
				PathAbstractions.AbstractedLocation other = (PathAbstractions.AbstractedLocation)_obj;
				return this.CompareTo(other);
			}
			throw new ArgumentException("Object must be of type AbstractedLocation");
		}

		public static readonly PathAbstractions.AbstractedLocation None = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.None, null, null, null, false, null);

		public readonly PathAbstractions.EAbstractedLocationType Type;

		public readonly string Name;

		public readonly string Folder;

		public readonly string RelativePath;

		public readonly string FileNameNoExtension;

		public readonly string Extension;

		public readonly bool IsFolder;

		public readonly Mod ContainingMod;
	}

	public class SearchDefinition
	{
		public SearchDefinition(bool _isFolder, string _extension, bool _removeExtension, bool _recursive, params PathAbstractions.SearchPath[] _paths)
		{
			this.IsFolder = _isFolder;
			this.Extension = _extension;
			this.RemoveExtension = _removeExtension;
			this.Recursive = _recursive;
			if (this.IsFolder && this.Recursive)
			{
				throw new Exception("SearchDefinition can not be set to target folders and search recursively at the same time!");
			}
			this.paths = new List<PathAbstractions.SearchPath>(_paths);
			for (int i = 0; i < _paths.Length; i++)
			{
				_paths[i].SetOwner(this);
			}
			PathAbstractions.allSearchDefs.Add(this);
		}

		public PathAbstractions.AbstractedLocation GetLocation(string _name, string _worldName = null, string _gameName = null)
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanMatch)
				{
					PathAbstractions.AbstractedLocation location = searchPath.GetLocation(_name, _worldName, _gameName);
					if (location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						return location;
					}
				}
			}
			return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.None, _name, null, null, this.IsFolder, null);
		}

		public List<PathAbstractions.AbstractedLocation> GetAvailablePathsList(Regex _nameMatch = null, string _worldName = null, string _gameName = null, bool _ignoreDuplicateNames = false)
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanMatch)
				{
					searchPath.GetAvailablePathsList(list, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames);
				}
			}
			return list;
		}

		public void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _resultList, Regex _nameMatch = null, string _worldName = null, string _gameName = null, bool _ignoreDuplicateNames = false)
		{
			if (_resultList == null)
			{
				_resultList = new List<PathAbstractions.AbstractedLocation>();
			}
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanMatch)
				{
					searchPath.GetAvailablePathsList(_resultList, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames);
				}
			}
		}

		public void InvalidateCache()
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				searchPath.InvalidateCache();
			}
		}

		public readonly bool IsFolder;

		public readonly string Extension;

		public readonly bool RemoveExtension;

		public readonly bool Recursive;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<PathAbstractions.SearchPath> paths;
	}

	public abstract class SearchPath
	{
		public virtual bool CanMatch
		{
			get
			{
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SearchPath(string _relativePath, bool _pathIsTarget)
		{
			this.RelativePath = _relativePath;
			this.PathIsTarget = _pathIsTarget;
		}

		public void SetOwner(PathAbstractions.SearchDefinition _owner)
		{
			this.Owner = _owner;
		}

		public abstract PathAbstractions.AbstractedLocation GetLocation(string _name, string _worldName, string _gameName);

		public abstract void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames);

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.AbstractedLocation getLocationSingleBase(PathAbstractions.EAbstractedLocationType _locationType, string _basePath, string _name, string _worldName, string _gameName, Mod _containingMod, string _subfolder = null)
		{
			this.UseCache(_worldName, _gameName);
			if (!SdDirectory.Exists(_basePath))
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			string text = _basePath + "/" + _name + this.Owner.Extension;
			if (this.Owner.IsFolder)
			{
				if (SdDirectory.Exists(text))
				{
					return new PathAbstractions.AbstractedLocation(_locationType, _name, text, _subfolder, this.Owner.IsFolder, _containingMod);
				}
			}
			else
			{
				if (SdFile.Exists(text))
				{
					string name = this.Owner.RemoveExtension ? GameIO.RemoveExtension(_name, this.Owner.Extension) : _name;
					return new PathAbstractions.AbstractedLocation(_locationType, name, text, _subfolder, this.Owner.IsFolder, _containingMod);
				}
				if (this.Owner.Recursive)
				{
					foreach (string text2 in SdDirectory.GetDirectories(_basePath))
					{
						PathAbstractions.AbstractedLocation locationSingleBase = this.getLocationSingleBase(_locationType, text2, _name, _worldName, _gameName, _containingMod, Path.GetFileName(text2));
						if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
						{
							return locationSingleBase;
						}
					}
				}
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void getAvailablePathsSingleBase(List<PathAbstractions.AbstractedLocation> _targetList, PathAbstractions.EAbstractedLocationType _locationType, string _basePath, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames, Mod _containingMod, string _subfolder = null)
		{
			if (!SdDirectory.Exists(_basePath))
			{
				return;
			}
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(_basePath);
			SdFileSystemInfo[] array;
			SdFileSystemInfo[] array2;
			if (this.Owner.IsFolder)
			{
				array = sdDirectoryInfo.GetDirectories();
				array2 = array;
			}
			else
			{
				array = sdDirectoryInfo.GetFiles("*" + this.Owner.Extension, SearchOption.TopDirectoryOnly);
				array2 = array;
			}
			array = array2;
			for (int i = 0; i < array.Length; i++)
			{
				SdFileSystemInfo sdFileSystemInfo = array[i];
				if ((this.Owner.Extension == null || sdFileSystemInfo.Name.EndsWith(this.Owner.Extension, StringComparison.Ordinal)) && (_nameMatch == null || _nameMatch.IsMatch(sdFileSystemInfo.Name)))
				{
					string filename = this.Owner.RemoveExtension ? GameIO.RemoveExtension(sdFileSystemInfo.Name, this.Owner.Extension) : sdFileSystemInfo.Name;
					if (!_ignoreDuplicateNames || !_targetList.Exists((PathAbstractions.AbstractedLocation _location) => _location.Name.Equals(filename)))
					{
						_targetList.Add(new PathAbstractions.AbstractedLocation(_locationType, filename, sdFileSystemInfo.FullName, _subfolder, this.Owner.IsFolder, _containingMod));
					}
				}
			}
			if (!this.Owner.IsFolder && this.Owner.Recursive)
			{
				foreach (string text in SdDirectory.GetDirectories(_basePath))
				{
					this.getAvailablePathsSingleBase(_targetList, _locationType, text, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, _containingMod, Path.GetFileName(text));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool UseCache(string _worldName, string _gameName)
		{
			bool flag = PathAbstractions.CacheEnabled && string.IsNullOrEmpty(_worldName) && string.IsNullOrEmpty(_gameName);
			if (flag && !this.locationsCachePopulated)
			{
				this.PopulateCache();
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.AbstractedLocation GetCachedLocation(string _name, bool _ignoreName = false)
		{
			if (this.locationsCache.Count == 0)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			if (_ignoreName)
			{
				foreach (KeyValuePair<string, IList<PathAbstractions.AbstractedLocation>> keyValuePair in this.locationsCache)
				{
					if (keyValuePair.Value.Count != 0)
					{
						return keyValuePair.Value[0];
					}
				}
				return PathAbstractions.AbstractedLocation.None;
			}
			IList<PathAbstractions.AbstractedLocation> list;
			if (!this.locationsCache.TryGetValue(this.Owner.RemoveExtension ? _name : (_name + this.Owner.Extension), out list))
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			if (list.Count <= 0)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			return list[0];
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void GetCachedPathList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (this.locationsCache.Count == 0)
			{
				return;
			}
			using (Dictionary<string, IList<PathAbstractions.AbstractedLocation>>.Enumerator enumerator = this.locationsCache.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, IList<PathAbstractions.AbstractedLocation>> kvp = enumerator.Current;
					if ((_nameMatch == null || _nameMatch.IsMatch(kvp.Key)) && (!_ignoreDuplicateNames || !_targetList.Exists((PathAbstractions.AbstractedLocation _location) => _location.Name.Equals(kvp.Key))))
					{
						int num = _ignoreDuplicateNames ? Mathf.Min(1, kvp.Value.Count) : kvp.Value.Count;
						for (int i = 0; i < num; i++)
						{
							_targetList.Add(kvp.Value[i]);
						}
					}
				}
			}
		}

		public void InvalidateCache()
		{
			this.locationsCache.Clear();
			this.locationsCachePopulated = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void PopulateCache();

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.SearchDefinition Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string RelativePath;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly bool PathIsTarget;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Dictionary<string, IList<PathAbstractions.AbstractedLocation>> locationsCache = new CaseInsensitiveStringDictionary<IList<PathAbstractions.AbstractedLocation>>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool locationsCachePopulated;
	}

	public class SearchPathBasic : PathAbstractions.SearchPath
	{
		public SearchPathBasic(PathAbstractions.EAbstractedLocationType _locationType, Func<string> _basePath, string _relativePath, bool _pathIsTarget = false) : base(_relativePath, _pathIsTarget)
		{
			this.locationType = _locationType;
			this.basePath = _basePath;
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				return base.GetCachedLocation(_name, false);
			}
			PathAbstractions.AbstractedLocation locationSingleBase = base.getLocationSingleBase(this.locationType, this.basePath() + "/" + this.RelativePath, _name, _worldName, _gameName, null, null);
			if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
			{
				return locationSingleBase;
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				base.GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			base.getAvailablePathsSingleBase(_targetList, this.locationType, this.basePath() + "/" + this.RelativePath, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			base.getAvailablePathsSingleBase(list, this.locationType, this.basePath() + "/" + this.RelativePath, null, null, null, false, null, null);
			this.locationsCache.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = list[i];
				IList<PathAbstractions.AbstractedLocation> list2;
				if (!this.locationsCache.TryGetValue(abstractedLocation.Name, out list2))
				{
					list2 = new List<PathAbstractions.AbstractedLocation>();
					this.locationsCache[abstractedLocation.Name] = list2;
				}
				list2.Add(abstractedLocation);
			}
			this.locationsCachePopulated = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PathAbstractions.EAbstractedLocationType locationType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<string> basePath;
	}

	public class SearchPathSaves : PathAbstractions.SearchPath
	{
		public override bool CanMatch
		{
			get
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null;
			}
		}

		public SearchPathSaves(string _relativePath, bool _pathIsTarget = false) : base(_relativePath, _pathIsTarget)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ValueTuple<string, PathAbstractions.EAbstractedLocationType> GetSaveFolder(string _worldName, string _gameName)
		{
			if (!string.IsNullOrEmpty(_worldName) && !string.IsNullOrEmpty(_gameName))
			{
				return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(GameIO.GetSaveGameDir(_worldName, _gameName), PathAbstractions.EAbstractedLocationType.HostSave);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(GameIO.GetSaveGameDir(), PathAbstractions.EAbstractedLocationType.HostSave);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(GameIO.GetSaveGameLocalDir(), PathAbstractions.EAbstractedLocationType.LocalSave);
			}
			return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(null, PathAbstractions.EAbstractedLocationType.None);
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				return base.GetCachedLocation(_name, true);
			}
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder(_worldName, _gameName);
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			if (item == null)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				return new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null);
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				base.GetCachedPathList(_targetList, null, _ignoreDuplicateNames);
				return;
			}
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder(_worldName, _gameName);
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			if (item == null)
			{
				return;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				_targetList.Add(new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder(null, null);
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			this.locationsCache.Clear();
			this.locationsCachePopulated = true;
			if (item == null)
			{
				return;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				PathAbstractions.AbstractedLocation abstractedLocation = new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null);
				List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
				this.locationsCache[abstractedLocation.Name] = list;
				list.Add(abstractedLocation);
			}
		}
	}

	public class SearchPathMods : PathAbstractions.SearchPath
	{
		public override bool CanMatch
		{
			get
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null;
			}
		}

		public SearchPathMods(string _relativePath, bool _pathIsTarget = false) : base(_relativePath, _pathIsTarget)
		{
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, string _worldName, string _gameName)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				return base.GetCachedLocation(_name, false);
			}
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				PathAbstractions.AbstractedLocation locationSingleBase = base.getLocationSingleBase(PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, _name, _worldName, _gameName, mod, null);
				if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
				{
					return locationSingleBase;
				}
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, string _worldName, string _gameName, bool _ignoreDuplicateNames)
		{
			if (base.UseCache(_worldName, _gameName))
			{
				base.GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				base.getAvailablePathsSingleBase(_targetList, PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, _nameMatch, _worldName, _gameName, _ignoreDuplicateNames, mod, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				base.getAvailablePathsSingleBase(list, PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, null, null, null, false, mod, null);
			}
			this.locationsCache.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = list[i];
				IList<PathAbstractions.AbstractedLocation> list2;
				if (!this.locationsCache.TryGetValue(abstractedLocation.Name, out list2))
				{
					list2 = new List<PathAbstractions.AbstractedLocation>();
					this.locationsCache[abstractedLocation.Name] = list2;
				}
				list2.Add(abstractedLocation);
			}
			this.locationsCachePopulated = true;
		}
	}
}
