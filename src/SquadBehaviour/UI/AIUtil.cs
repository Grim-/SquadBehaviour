namespace SquadBehaviour
{
    public static class AIUtil
    {
		//private static List<Thing> tmpThreats = new List<Thing>();
		//public static Job TryGetFleeJob(Pawn pawn)
		//{
		//	if (!SelfDefenseUtility.ShouldStartFleeing(pawn))
		//	{
		//		return null;
		//	}
		//	IntVec3 c;
		//	if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
		//	{
		//		c = pawn.CurJob.targetA.Cell;
		//	}
		//	else
		//	{
		//		tmpThreats.Clear();
		//		List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
		//		for (int i = 0; i < potentialTargetsFor.Count; i++)
		//		{
		//			Thing thing = potentialTargetsFor[i].Thing;
		//			if (SelfDefenseUtility.ShouldFleeFrom(thing, pawn, false, false))
		//			{
		//				tmpThreats.Add(thing);
		//			}
		//		}
		//		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
		//		for (int j = 0; j < list.Count; j++)
		//		{
		//			Thing thing2 = list[j];
		//			if (SelfDefenseUtility.ShouldFleeFrom(thing2, pawn, false, false))
		//			{
		//				tmpThreats.Add(thing2);
		//			}
		//		}
		//		if (!tmpThreats.Any<Thing>())
		//		{
		//			Log.Error(pawn.LabelShort + " decided to flee but there is not any threat around.");
		//			Region region = pawn.GetRegion(RegionType.Set_Passable);
		//			if (region == null)
		//			{
		//				return null;
		//			}
		//			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate (Region reg)
		//			{
		//				List<Thing> list2 = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
		//				for (int k = 0; k < list2.Count; k++)
		//				{
		//					Thing thing3 = list2[k];
		//					if (SelfDefenseUtility.ShouldFleeFrom(thing3, pawn, false, false))
		//					{
		//						tmpThreats.Add(thing3);
		//						Log.Warning(string.Format("  Found a viable threat {0}; tests are {1}, {2}, {3}", new object[]
		//						{
		//							thing3.LabelShort,
		//							thing3.Map.attackTargetsCache.Debug_CheckIfInAllTargets(thing3 as IAttackTarget),
		//							thing3.Map.attackTargetsCache.Debug_CheckIfHostileToFaction(pawn.Faction, thing3 as IAttackTarget),
		//							thing3 is IAttackTarget
		//						}));
		//					}
		//				}
		//				return false;
		//			}, 9, RegionType.Set_Passable);
		//			if (!tmpThreats.Any<Thing>())
		//			{
		//				return null;
		//			}
		//		}
		//		c = CellFinderLoose.GetFleeDest(pawn, tmpThreats, 23f);
		//		tmpThreats.Clear();
		//	}
		//	return JobMaker.MakeJob(JobDefOf.FleeAndCower, c);
		//}
	}
}
