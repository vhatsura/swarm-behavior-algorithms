using System.Configuration;

namespace SwarmBehaviorAlgorithms.UI.Models
{
    public class Cargo
    {
        public bool IsTaken { get; set; }
        public bool IsAssigned { get; set; }
        public object Target { get; set; }
        
        public Position Position { get; set; }
    }
}