﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasExternalModels : TextureAtlas
{
	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		bool result;
		try
		{
			Stream stream = new MemoryStream(_tac.meshes[_idx].MetaData.bytes);
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				uint num = pooledBinaryReader.ReadUInt32();
				int num2 = 0;
				while ((long)num2 < (long)((ulong)num))
				{
					string key = pooledBinaryReader.ReadString();
					VoxelMeshExt3dModel voxelMeshExt3dModel = (VoxelMeshExt3dModel)VoxelMesh.Create(_idx, _tac.meshes[_idx].meshType, 500);
					voxelMeshExt3dModel.Read(pooledBinaryReader);
					this.Meshes[key] = voxelMeshExt3dModel;
					num2++;
				}
			}
			stream.Close();
			base.LoadTextureAtlas(_idx, _tac, _bLoadTextures);
			result = true;
		}
		catch (Exception ex)
		{
			Log.Error("Loading model file. " + ex.Message);
			Log.Error(ex.StackTrace);
			result = false;
		}
		return result;
	}

	public Dictionary<string, VoxelMeshExt3dModel> Meshes = new Dictionary<string, VoxelMeshExt3dModel>();
}
