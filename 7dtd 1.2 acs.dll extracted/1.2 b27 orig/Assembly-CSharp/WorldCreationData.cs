using System;
using System.Xml.Linq;

public class WorldCreationData
{
	public WorldCreationData(string _levelDir)
	{
		try
		{
			XDocument xdocument = SdXDocument.Load(_levelDir + "/world.xml");
			if (xdocument.Root == null)
			{
				throw new Exception("No root node in world.xml!");
			}
			foreach (XElement propertyNode in xdocument.Root.Elements("property"))
			{
				this.Properties.Add(propertyNode, true);
			}
		}
		catch (Exception)
		{
		}
	}

	public void Apply(World _world, WorldState _worldState)
	{
		if (this.Properties.Values.ContainsKey("ProviderId"))
		{
			_worldState.providerId = (EnumChunkProviderId)int.Parse(this.Properties.Values["ProviderId"]);
		}
	}

	public const string PropProviderId = "ProviderId";

	public const string PropWorld_Class = "World.Class";

	public const string PropWorldEnvironment_Prefab = "WorldEnvironment.Prefab";

	public const string PropWorldEnvironment_Class = "WorldEnvironment.Class";

	public const string PropWorldBiomeProvider_Class = "WorldBiomeProvider.Class";

	public const string PropWorldTerrainGenerator_Class = "WorldTerrainGenerator.Class";

	public DynamicProperties Properties = new DynamicProperties();
}
