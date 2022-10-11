using sai3.imitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sai3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p1N = float.Parse(p1.Text);
                var p2N = float.Parse(p2.Text);

                var smp = new SMP(p1N, p2N, 2, 2);
                var statist = smp.Run(500000);
                PrintStates(statist);
                PrintStatistics(statist);

            }catch(Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        private void PrintStatistics(SMP.Statistics s)
        {
            A.Content = s.A;
            Q.Content = s.Q;
            Wc.Content = s.wc;
            Wq.Content = s.wq;
            Lq.Content = s.Lq;
            Lc.Content = s.Lc;
            Ch1.Content = s.Chan1;
            Ch2.Content = s.Chan2;
            block.Content = s.Pbl;
            refuse.Content = s.refuse;
        }

        private void PrintStates(SMP.Statistics s) {
            List<string> keys = new List<string>(s.states.Keys);
            keys.Sort();
            States.Children.Clear();
            foreach (string key in keys)
            {
                var l = new Label();
                l.Content = $"P{key}={s.states[key]}";
                States.Children.Add(l);
            }
        }
    }
}
