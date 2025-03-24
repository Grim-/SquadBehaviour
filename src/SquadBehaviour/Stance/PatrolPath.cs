using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class PatrolPath : IExposable
    {
        public List<IntVec3> points = new List<IntVec3>();

        public PatrolPath() { }

        public PatrolPath(List<IntVec3> pathPoints)
        {
            points = new List<IntVec3>(pathPoints);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref points, "points", LookMode.Value);
        }
    }
}
