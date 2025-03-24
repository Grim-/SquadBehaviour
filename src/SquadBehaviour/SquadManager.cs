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
            Scribe_Collections.Look(ref squadCache, "squadCache", LookMode.Reference, LookMode.Deep);
        }

    }
}
