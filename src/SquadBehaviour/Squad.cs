using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class Squad : IExposable, ILoadReferenceable
    {
        public int squadID = -1;
        public int uniqueID = -1;
        public string squadName = "Squad";
        public Pawn Leader;
        public ISquadLeader SquadLeader;
        public FormationUtils.FormationType FormationType = FormationUtils.FormationType.Column;
        public List<Pawn> Members = new List<Pawn>();
        public SquadHostility HostilityResponse = SquadHostility.Defensive;
        public float AggresionDistance = 10f;
        public float FollowDistance = 10f;
        public bool InFormation = true;

        public Squad()
        {

        }

        public Squad(int squadID, Pawn leader, FormationUtils.FormationType formationType, SquadHostility hostility)
        {
            this.squadID = squadID;
            this.uniqueID = Find.UniqueIDsManager.GetNextThingID();
            Leader = leader;

            if (!Leader.TryGetSquadLeader(out ISquadLeader squadLeader))
            {
                //Log.Error("Passed a pawn who is not a squad leader");
                return;
            }

            SquadLeader = squadLeader;
            FormationType = formationType;
            HostilityResponse = hostility;
        }

        public void SetHositilityResponse(SquadHostility squadHostilityResponse)
        {
            HostilityResponse = squadHostilityResponse;
        }
        public void SetFormation(FormationUtils.FormationType formationType)
        {
            FormationType = formationType;
        }

        public void SetFollowDistance(float distance)
        {
            FollowDistance = distance;
        }

        public void SetInFormation(bool inFormation)
        {
            InFormation = inFormation;
        }

        public void AddMember(Pawn pawn)
        {
            if (!Members.Contains(pawn))
            {
                Members.Add(pawn);
            }
        }

        public void RemoveMember(Pawn pawn)
        {
            Members.Remove(pawn);
        }
        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            if (!Members.Contains(pawn))
            {
                Log.Message("Tried to get formation position for {pawn} but they are not a member of the squad");
                return IntVec3.Invalid;
            }

            return FormationUtils.GetFormationPosition(
                FormationType,
                Origin.ToVector3(),
                OriginRotation,
                Members.IndexOf(pawn),
                Members.Count);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Leader, "Leader");
            Scribe_Collections.Look(ref Members, "Members", LookMode.Reference);
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_Values.Look(ref FormationType, "FormationType");
            Scribe_Values.Look(ref squadID, "squadID");
            Scribe_Values.Look(ref HostilityResponse, "HostilityResponse");
        }

        public string GetUniqueLoadID()
        {
            return "Squad_" + this.uniqueID;
        }
    }
}
