using Microsoft.Xna.Framework;

namespace TimelessTales.World
{
    /// <summary>
    /// Represents a waypoint marker in the world
    /// </summary>
    public class Waypoint
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public bool IsVisible { get; set; }
        
        public Waypoint(string name, Vector3 position, Color color)
        {
            Name = name;
            Position = position;
            Color = color;
            IsVisible = true;
        }
        
        /// <summary>
        /// Calculate the horizontal distance to this waypoint
        /// </summary>
        public float GetDistanceTo(Vector3 playerPosition)
        {
            Vector2 playerPos2D = new Vector2(playerPosition.X, playerPosition.Z);
            Vector2 waypointPos2D = new Vector2(Position.X, Position.Z);
            return Vector2.Distance(playerPos2D, waypointPos2D);
        }
        
        /// <summary>
        /// Calculate the angle to this waypoint relative to player's facing direction
        /// </summary>
        public float GetAngleTo(Vector3 playerPosition, float playerYaw)
        {
            Vector2 toWaypoint = new Vector2(
                Position.X - playerPosition.X,
                Position.Z - playerPosition.Z
            );
            
            if (toWaypoint.LengthSquared() == 0)
                return 0;
            
            toWaypoint.Normalize();
            
            // Calculate angle from north (0 radians)
            float waypointAngle = MathF.Atan2(toWaypoint.X, -toWaypoint.Y);
            
            // Calculate relative angle from player's facing direction
            float relativeAngle = waypointAngle - playerYaw;
            
            // Normalize to -PI to PI range
            while (relativeAngle > MathF.PI) relativeAngle -= MathF.PI * 2;
            while (relativeAngle < -MathF.PI) relativeAngle += MathF.PI * 2;
            
            return relativeAngle;
        }
    }
}
