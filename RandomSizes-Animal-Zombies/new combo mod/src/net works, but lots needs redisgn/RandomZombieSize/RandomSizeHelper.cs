using System;
using System.Collections.Generic;

public class RandomSizeHelper
{
	public static bool AllowedRandomSize(EntityAlive entity)
	{
		//FastTags<TagGroup.Global> entityTags = entity.EntityTags;
        List<string> entityTagList = entity.EntityTags.GetTagNames();
		if (entity.isEntityRemote)
		{
			//Log.Out("[RandomSizesZA Debug] entity is remote, name: {0} entity", entity.entityName);
            //return false; // disabled for now, not sure why this is restricted
            //Log.Out("[RandomSizesZA Debug] remote entity has cvar: {0} Custom Cvar value is: {1}", entity.Buffs.HasCustomVar("RandomSize"), entity.Buffs.GetCustomVar("RandomSize"));
            //return false;
		}
        if (entityTagList.Contains("animal"))
        {
			if (RandomSizesZA_Init.animalMin <= RandomSizesZA_Init.animalMax)
			{
                return RandomSizesZA_Init.randomAnimalSizes;
            }
            else
            {
                Log.Out("[RandomSizesZA] animalMin: {0} is not less than animalMax {1}. Cannot randomize animal size. Please verify your setting choices.", RandomSizesZA_Init.animalMin, RandomSizesZA_Init.animalMax);
				return false;
			}
        }
        else if (entityTagList.Contains("zombie"))
        {
            if (RandomSizesZA_Init.zombieMin <= RandomSizesZA_Init.zombieMax)
            {
                return RandomSizesZA_Init.randomZombieSizes;
            }
            else
            {
                Log.Out("[RandomSizesZA] zombieMin: {0} is not less than zombieMax {1}. Cannot randomize zombie size. Please verify your setting choices.", RandomSizesZA_Init.zombieMin, RandomSizesZA_Init.zombieMax);
                return false;
            }
        }
        Log.Out("[RandomSizesZA Debug] entity tags does not contains animal or zombie, : {0}", string.Join(", ", entityTagList));
        return false;
	}
}
