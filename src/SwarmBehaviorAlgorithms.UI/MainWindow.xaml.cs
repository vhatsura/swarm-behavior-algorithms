using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using SwarmBehaviorAlgorithms.UI.Models;

namespace SwarmBehaviorAlgorithms.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<object> Robots;
        private List<Cargo> Cargos;
        private List<Target> Targets;

        private readonly Random _random = new Random();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddCargo_OnClick(object sender, RoutedEventArgs e)
        {
            Robots = new List<object>();
            Cargos = new List<Cargo>();
            Targets = new List<Target>();
            
            CanvasControl.Children.Clear();

            for (var i = 0; i < NumberOfCargos.Value; i++)
            {
                var newCargo = new Cargo
                {
                    Position = new Models.Position
                    {
                        X = _random.Next(0, 500),
                        Y = _random.Next(0, 500)
                    }
                };
                
                var target = new Target
                {
                    Position = new Models.Position
                    {
                        X = _random.Next(0, 500),
                        Y = _random.Next(0, 500)
                    }
                };
                
                Targets.Add(target);

                newCargo.Target = target;
                Cargos.Add(newCargo);
            }

            foreach (var cargo in Cargos)
            {
                var triangle = new P();
            }
        }
    }
}