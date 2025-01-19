using System;
using Unity.Jobs;
using UnityEngine;

[DOTSCompilerGenerated]
[PublicizedFrom(EAccessModifier.Internal)]
public class __JobReflectionRegistrationOutput__2379460843
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForExtensions.EarlyJobInit<WaterSimulationApplyFlows>();
			IJobParallelForExtensions.EarlyJobInit<WaterSimulationCalcFlows>();
			IJobExtensions.EarlyJobInit<WaterSimulationPostProcess>();
			IJobExtensions.EarlyJobInit<WaterSimulationPreProcess>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		__JobReflectionRegistrationOutput__2379460843.CreateJobReflectionData();
	}
}
