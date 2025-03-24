using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    //public class SquadOrder_RunToPointAndExplode : SquadOrderWorker
    //{
    //    public override bool CanExecuteOrder(LocalTargetInfo Target)
    //    {
    //        //and has some explosive hediff
    //        return !this.SquadMember.Pawn.DeadOrDowned;
    //    }

    //    public override void ExecuteOrder(LocalTargetInfo Target)
    //    {
    //        //pawns with hediff
    //        foreach (var item in SquadLeader.SquadMembersPawns)
    //        {
    //            //execute job of running to point and exploding

    //            if (item.jobs != null)
    //            {
    //                item.jobs.StartJob(JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_UndeadExplodeAt, Target.Cell));
    //            }
    //        }

    //    }
    //}


    //public class JobDriver_UndeadExplodeAt : JobDriver
    //{
    //    private IntVec3 targetLocation;


    //    public override bool TryMakePreToilReservations(bool errorOnFailed)
    //    {
    //        return targetLocation.IsValid;
    //    }

    //    protected override IEnumerable<Toil> MakeNewToils()
    //    {
    //        Toil gotoToil = Toils_Goto.GotoCell(TargetLocA, PathEndMode.OnCell);
    //        yield return gotoToil;

    //        Toil waitThenExplode = Toils_General.Wait(150);
    //        waitThenExplode.AddFinishAction(() =>
    //        {
    //            GenExplosion.DoExplosion(this.pawn.Position, this.pawn.Map, 5f, DamageDefOf.Bomb, this.pawn);
    //            this.EndJobWith(JobCondition.Succeeded);
    //            this.pawn.Destroy();
    //        });
    //        yield return waitThenExplode;
    //    }
    //}
}
