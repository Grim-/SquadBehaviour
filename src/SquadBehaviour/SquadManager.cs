using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class SquadManager : GameComponent
    {
        private Dictionary<Pawn, List<Squad>> squadCache = new Dictionary<Pawn, List<Squad>>();

        public SquadManager(Game game) : base()
        {
        }

        public void RegisterSquad(Pawn leader, Squad squad)
        {
            if (!squadCache.ContainsKey(leader))
            {
                squadCache[leader] = new List<Squad>();
            }
            squadCache[leader].Add(squad);
        }

        public void RemoveSquad(Pawn leader, Squad squad)
        {
            if (squadCache.ContainsKey(leader))
            {
                squadCache[leader].Remove(squad);
                if (squadCache[leader].Count == 0)
                {
                    squadCache.Remove(leader);
                }
            }
        }

        public List<Squad> GetSquadsForLeader(Pawn leader)
        {
            return squadCache.TryGetValue(leader, out var squads) ? squads : new List<Squad>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Pawn> leaders = new List<Pawn>(squadCache.Keys);
                List<List<Squad>> squadLists = new List<List<Squad>>();

                foreach (Pawn leader in leaders)
                {
                    squadLists.Add(squadCache[leader]);
                }

                Scribe_Collections.Look(ref leaders, "squadLeaders", LookMode.Reference);
                Scribe_Collections.Look(ref squadLists, "squadLists", LookMode.Deep);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<Pawn> leaders = null;
                List<List<Squad>> squadLists = null;

                Scribe_Collections.Look(ref leaders, "squadLeaders", LookMode.Reference);
                Scribe_Collections.Look(ref squadLists, "squadLists", LookMode.Deep);

                if (leaders != null && squadLists != null && leaders.Count == squadLists.Count)
                {
                    squadCache = new Dictionary<Pawn, List<Squad>>();
                    for (int i = 0; i < leaders.Count; i++)
                    {
                        if (leaders[i] != null)
                        {
                            squadCache[leaders[i]] = squadLists[i];
                        }
                    }
                }
                else
                {
                    squadCache = new Dictionary<Pawn, List<Squad>>();
                }
            }
        }
    }
}
