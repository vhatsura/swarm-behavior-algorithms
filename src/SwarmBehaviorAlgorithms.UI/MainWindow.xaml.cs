using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using SwarmBehaviorAlgorithms.UI.Annotations;
using SwarmBehaviorAlgorithms.UI.Models;
using Position = SwarmBehaviorAlgorithms.UI.Models.Position;

namespace SwarmBehaviorAlgorithms.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private const int FieldSize = 500;

        private const int NumberOfRuns = 100;

        private static readonly Random Random = new Random((int) DateTime.UtcNow.Ticks);

        private readonly List<(List<Robot> Robots, int Cycles)> _arrangementResult =
            new List<(List<Robot> Robots, int Cycles)>();

        private readonly List<(List<Robot> Robots, int Cycles)> _resourcesResult =
            new List<(List<Robot> Robots, int Cycles)>();

        private List<Cargo> _cargos;

        private int _numberOfCargos = 8;

        private int _numberOfMoves = 10;

        private int _numberOfSteps = 1;
        private List<Robot> _robots;
        private List<Target> _targets;

        private int _timeLimit = 500;

        private int h;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public SeriesCollection ArrangementSeriesCollection { get; } =
            new SeriesCollection(Mappers.Xy<int>().X((value, idx) => idx).Y(value => value));


        public SeriesCollection ResourceSeriesCollection { get; } =
            new SeriesCollection(Mappers.Xy<int>().X((value, idx) => idx).Y(value => value));

        public int NumberOfCargos
        {
            get => _numberOfCargos;
            set
            {
                _numberOfCargos = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfMoves
        {
            get => _numberOfMoves;
            set
            {
                _numberOfMoves = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfSteps
        {
            get => _numberOfSteps;
            set
            {
                _numberOfSteps = value;
                OnPropertyChanged();
            }
        }

        public int TimeLimit
        {
            get => _timeLimit;
            set
            {
                _timeLimit = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static int GetRandomNumber(int minimum = 0, int maximum = FieldSize) =>
            Random.Next(minimum, maximum + 1);

        private void AddCargo_OnClick(object sender, RoutedEventArgs e)
        {
            _robots = new List<Robot>();
            _cargos = new List<Cargo>();
            _targets = new List<Target>();

            CanvasControl.Children.Clear();

            for (var i = 0; i < NumberOfCargos; i++)
            {
                var target = new Target(i + 1, new Position(GetRandomNumber(), GetRandomNumber()));

                var newCargo = new Cargo(i + 1, new Position(GetRandomNumber(), GetRandomNumber()), target);

                _targets.Add(target);
                _cargos.Add(newCargo);
            }

            foreach (var cargo in _cargos) DrawCargo(cargo);

            foreach (var target in _targets) DrawTarget(target);
        }

        private void DrawCargo(Cargo cargo)
        {
            const int triangleHeight = 20;

            var triangle = new Polygon
            {
                Stroke = cargo.IsTaken ? Brushes.Brown : Brushes.Black,
                Fill = cargo.IsTaken ? Brushes.White : Brushes.LightSeaGreen,
                StrokeThickness = 1
            };

            var p1 = new Point(cargo.Position.X, cargo.Position.Y - triangleHeight / 2);
            var p2 = new Point(cargo.Position.X - triangleHeight / 2, cargo.Position.Y + triangleHeight / 2);
            var p3 = new Point(cargo.Position.X + triangleHeight / 2, cargo.Position.Y + triangleHeight / 2);

            triangle.Points = new PointCollection {p1, p2, p3};

            var textBlock = new TextBlock
            {
                Text = cargo.Number.ToString(),
                Foreground = Brushes.White,
                FontSize = 12
            };

            Canvas.SetLeft(textBlock, cargo.Position.X - triangleHeight / 6);
            Canvas.SetTop(textBlock, cargo.Position.Y - triangleHeight / 3);

            CanvasControl.Children.Add(triangle);
            CanvasControl.Children.Add(textBlock);
        }

        private void DrawTarget(Target target)
        {
            const int squareHeight = 20;

            var square = new Polygon
            {
                Stroke = Brushes.Aqua,
                Fill = Brushes.Black,
                StrokeThickness = 1
            };

            var p1 = new Point(target.Position.X + squareHeight / 2, target.Position.Y - squareHeight / 2);
            var p2 = new Point(target.Position.X - squareHeight / 2, target.Position.Y - squareHeight / 2);
            var p3 = new Point(target.Position.X - squareHeight / 2, target.Position.Y + squareHeight / 2);
            var p4 = new Point(target.Position.X + squareHeight / 2, target.Position.Y + squareHeight / 2);

            square.Points = new PointCollection {p1, p2, p3, p4};

            var textBlock = new TextBlock
            {
                Text = target.Number.ToString(),
                Foreground = Brushes.Chartreuse,
                FontSize = 12
            };

            Canvas.SetLeft(textBlock, target.Position.X - squareHeight / 6);
            Canvas.SetTop(textBlock, target.Position.Y - squareHeight / 3);

            CanvasControl.Children.Add(square);
            CanvasControl.Children.Add(textBlock);
        }

        private void Assess_OnClick(object sender, RoutedEventArgs e)
        {
            ArrangementAssess();
            ResourcesAsses();
        }

        private void ResourcesAsses()
        {
            _resourcesResult.Clear();

            Assess(() => GetRandomNumber(NumberOfCargos, NumberOfCargos + 10), TimeLimit, _resourcesResult);

            var moves = _resourcesResult.Sum(x => x.Cycles);
            var average = moves / _resourcesResult.Count;

            ResourceSeriesCollection.Clear();
            ResourceSeriesCollection.Add(new LineSeries
            {
                Values = new ChartValues<int>(_resourcesResult.Select(x => x.Robots.Count))
            });
        }

        private void Assess(Func<int> numberOfRobotsFunc, int timeLimit, IList<(List<Robot> Robots, int Cycle)> results)
        {
            for (var run = 0; run < NumberOfRuns; run++)
            {
                CleanupCargosAndTargets();

                var robots = GenerateRobots(numberOfRobotsFunc());

                int i;
                for (i = 0; i < timeLimit; i++)
                {
                    for (var j = 0; j < NumberOfMoves; j++) MoveRobots(robots, j == 0);

                    if (_targets.All(t => t.IsDelivered))
                    {
                        results.Add((robots, i));
                        break;
                    }
                }

                if (i == 500) results.Add((robots, 500));
            }
        }

        private void MoveRobots(List<Robot> robots, bool isNewCycle)
        {
            // Move robots
            foreach (var robot in robots) MoveRobot(robot);

            if (isNewCycle)
            {
                foreach (var robot in robots)
                {
                    robot.IsStopped = false;
                    robot.Direction = new Direction(GetRandomNumber(-1, 1), GetRandomNumber(-1, 1));
                }

                h++;
            }
        }

        private void ArrangementAssess()
        {
            _arrangementResult.Clear();

            Assess(() => NumberOfCargos, 500, _arrangementResult);

            var moves = _arrangementResult.Sum(x => x.Cycles);
            var average = moves / _arrangementResult.Count;

            ArrangementSeriesCollection.Clear();
            ArrangementSeriesCollection.Add(new LineSeries
            {
                Values = new ChartValues<int>(_arrangementResult.Select(x => x.Cycles))
            });
        }

        private void CleanupCargosAndTargets()
        {
            foreach (var cargo in _cargos)
            {
                cargo.IsAssigned = false;
                cargo.IsTaken = false;
            }

            foreach (var target in _targets) target.IsDelivered = false;
        }

        private List<Robot> GenerateRobots(int amount)
        {
            var robots = new List<Robot>();
            for (var i = 0; i < amount; i++)
            {
                var robot = new Robot(new Position(GetRandomNumber(), GetRandomNumber()),
                    new Direction(GetRandomNumber(-1, 1), GetRandomNumber(-1, 1)));
                robots.Add(robot);

                var cargoToAssign = GetRandomNumber(0, NumberOfCargos - 1);
                var cargoCounter = 0;

                while (_cargos[cargoToAssign].IsAssigned && cargoCounter < NumberOfCargos)
                {
                    cargoToAssign = (cargoToAssign + 1) % NumberOfCargos;
                    cargoCounter++;
                }

                robot.Target = _cargos[cargoToAssign];
                _cargos[cargoToAssign].IsAssigned = true;
            }

            if (_cargos.Any(c => !c.IsAssigned))
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new InvalidOperationException("Not all cargos were assigned");
            }

            if (robots.Any(r => r.Target == null))
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new InvalidOperationException("Not all robots have target");
            }

            return robots;
        }

        private void MoveRobot(Robot robot)
        {
            if (robot.IsStopped || robot.JobIsDone) return;

            var lastDist = robot.Distance;

            // если робот на текущем шаге не выходит за границы имитационного поля
            // по оси x или, если он уже за его пределами, не уходит ещё дальше
            if (robot.Position.X < FieldSize && robot.Position.X > 0 ||
                robot.Position.X >= FieldSize && robot.Direction.X != 1 ||
                robot.Position.X <= 0 && robot.Direction.X != -1)
                // делаем один шаг по оси x в установленном направлении
                robot.Position.X += NumberOfSteps * robot.Direction.X;

            // если робот на текущем шаге не выходит за границы имитационного поля
            // по оси y или, если он уже за его пределами, не уходит ещё дальше
            if (robot.Position.Y < FieldSize && robot.Position.Y > 0 ||
                robot.Position.Y >= FieldSize && robot.Direction.Y != 1 ||
                robot.Position.Y <= 0 && robot.Direction.Y != -1)
                // делаем один шаг по оси y в установленном направлении
                robot.Position.Y += NumberOfSteps * robot.Direction.Y;

            // если рассточние до цели увеличилось, останавливаем движение робота на текущем шаге имитации
            if (lastDist < robot.Distance) robot.IsStopped = true;

            // проверка на то, что робот достиг назначенной цели 
            if (Math.Abs(robot.Position.X - robot.Target.Position.X) < 10 &&
                Math.Abs(robot.Position.Y - robot.Target.Position.Y) < 10)
            {
                // если робот достиг груза, назаничить ему в качестве цели,
                // целевую точку, к которой был привязан данный груз
                if (robot.Target is Cargo cargo && cargo.IsTaken == false)
                {
                    cargo.IsTaken = true;
                    robot.Target = cargo.Target;
                }
                else if (robot.Target is Target target)
                {
                    // если достигнутая цель - целевая точка, остановить робота и 
                    // считать его работу выполненной
                    target.IsDelivered = true;
                    robot.JobIsDone = true;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Arrangement_OnDataClick(object sender, ChartPoint chartPoint)
        {
            var arrangementIdx = chartPoint.X;
        }
    }
}