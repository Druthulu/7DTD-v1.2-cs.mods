using System;

public class RandomSizeHelper
{
	public static bool AllowedRandomSize(EntityAlive entity)
	{
		bool result = false;
		if (entity.isEntityRemote)
		{
			return false;
		}
		if (entity is EntityZombie)
		{
            result = RandomSizesZA_Init.randomZombieSizes;
		}
        if (entity is EntityAnimal)
        {
            result = RandomSizesZA_Init.randomAnimalSizes;
        }
        EntityClass entityClass = EntityClass.list[entity.entityClass];
		if (entityClass.Properties.Values.ContainsKey("RandomSize"))
		{
			result = StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"], 0, -1, true);
		}
		return result;
	}
}
