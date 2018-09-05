using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace sensorsgit
{

    public class Connection
    {
        public ObservableCollection<Sensor> ConnectedSensorsList { get; set; }

        public Connection(ObservableCollection<Sensor> connectedSensorsList)
        {
            this.ConnectedSensorsList = new ObservableCollection<Sensor>();
            foreach (var sensor in connectedSensorsList)
            {
                if (sensor.Connected == true)
                {
                   this.ConnectedSensorsList.Add(sensor);
                }
            }
        }


        public bool LocationsSet()
        {
            var check = true;
            foreach (Sensor sensor in this.ConnectedSensorsList)
            {
                if (sensor.SensorPosition == 0)
                {
                    check = false;
                }
            }
            return check;
        }

        public void UpdateLocation(int selectedIndex, string position, string sensorName)
        {
            foreach (Sensor sensor in this.ConnectedSensorsList)
            {
                if (sensor.Name == sensorName)
                {
                    sensor.SensorPosition = selectedIndex + 1;
                    sensor.PositionDescription = position;
                }
            }
        }


        public bool locationsUnique()
        {
            List<int> list = new List<int>();
            foreach (Sensor sensor in this.ConnectedSensorsList)
            {
                list.Add(sensor.SensorPosition);
            }
            bool unique = list.Distinct().Count() == list.Count();
            return unique;

        }

        public async Task<bool> DisconnectAll()
        {
            //Disconnect devices
            foreach (var item in this.ConnectedSensorsList)
            {
                await movesense.DisconnectMdsAsync(item.Id);
            }

        }
    }


}
