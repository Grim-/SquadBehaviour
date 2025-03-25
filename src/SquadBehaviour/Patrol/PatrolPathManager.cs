using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquadBehaviour
{
    //public class PatrolPathManager : MapComponent
    //{
    //    public Dictionary<int, PatrolPath> patrolPaths = new Dictionary<int, PatrolPath>();


    //    public int NextID => patrolPaths != null ? patrolPaths.Count + 1 : 0;

    //    public PatrolPathManager(Map map) : base(map) 
    //    {
        
    //    }

    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Collections.Look(ref patrolPaths, "patrolPaths", LookMode.Deep);
    //    }
    //    public void AddPatrolPath(PatrolPath path)
    //    {
    //        AddPatrolPath(NextID, path);
    //    }
    //    public void AddPatrolPath(int Id, PatrolPath path)
    //    {
    //        patrolPaths.Add(Id, path);
    //    }

    //    public List<PatrolPath> GetAllPaths()
    //    {
    //        return patrolPaths.Values.ToList();
    //    }
    //}
}
