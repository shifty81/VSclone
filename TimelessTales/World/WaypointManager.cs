using System.Collections.Generic;

namespace TimelessTales.World
{
    /// <summary>
    /// Manages waypoints in the world
    /// </summary>
    public class WaypointManager
    {
        private readonly List<Waypoint> _waypoints = new();
        
        public IReadOnlyList<Waypoint> Waypoints => _waypoints;
        
        public void AddWaypoint(Waypoint waypoint)
        {
            _waypoints.Add(waypoint);
        }
        
        public void RemoveWaypoint(Waypoint waypoint)
        {
            _waypoints.Remove(waypoint);
        }
        
        public void ClearWaypoints()
        {
            _waypoints.Clear();
        }
        
        public Waypoint? GetNearestWaypoint(Microsoft.Xna.Framework.Vector3 position)
        {
            Waypoint? nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var waypoint in _waypoints)
            {
                if (!waypoint.IsVisible)
                    continue;
                    
                float distance = waypoint.GetDistanceTo(position);
                if (distance < nearestDistance)
                {
                    nearest = waypoint;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }
    }
}
