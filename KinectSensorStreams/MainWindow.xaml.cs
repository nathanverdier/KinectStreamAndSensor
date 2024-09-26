using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
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

using KinectSensorStreamsLibrary;
using KinectSensorStreamsLibrary.KinectSensorStream;
using Model;

namespace KinectSensorStreams
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppManager appManager;
        public MainWindow()
        {
            InitializeComponent();
            appManager = new AppManager();
            this.DataContext = appManager;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            appManager.KinectStream.Stop();
            appManager.KinectManager.StopSensor();
        }
    }
}
