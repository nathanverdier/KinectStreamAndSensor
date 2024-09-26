using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using KinectSensorStreamsLibrary;

namespace KinectSensorStreamsViewModel
{
    public class KinectManagerViewModel : ObservableObject
    { 
        private readonly KinectManager kinectManager;

        public KinectManagerViewModel(KinectManager kinectManager) => this.kinectManager = kinectManager;

        [ObservableProperty]
        public string EllipseColor
        {
            get
            {
                if (kinectManager.Status) { return "Green"; }
                else { return "Red"; }
            }
        }
    }
}
