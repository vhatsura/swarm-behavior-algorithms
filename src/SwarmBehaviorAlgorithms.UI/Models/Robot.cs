using System;

namespace SwarmBehaviorAlgorithms.UI.Models
{
    public class Robot
    {
        public Robot(Position position, Direction direction)
        {
            Position = position;
            Direction = direction;
        }

        public Position Position { get; }
        public Direction Direction { get; set; }

        public bool IsStopped { get; set; }
        public bool JobIsDone { get; set; }

        public IPosition Target { get; set; }
        //public int CargoNumber => Target.Number;

        public double Weight { get; set; }

        public double Distance => Target == null
            ? double.NaN
            : Math.Floor(Math.Sqrt(Math.Pow(Math.Abs(Position.X - Target.Position.X), 2) +
                                   Math.Pow(Math.Abs(Position.Y - Target.Position.Y), 2)));

        public override string ToString() => new ToStringBuilder(nameof(Robot))
        {
            {nameof(Position), Position}, {nameof(Direction), Direction}, {nameof(Target), Target},
            {nameof(Distance), Distance}, {nameof(IsStopped), IsStopped}
        }.ToString();
    }
}