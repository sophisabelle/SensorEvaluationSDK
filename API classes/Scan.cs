using System;
using System.Collections.ObjectModel;
using Plugin.BluetoothLE;

namespace sensorsgit
{
    public class Scan
    {
        public ObservableCollection<Sensor> foundSensorsList;
        private IDisposable _scan;
        public IAdapter BleAdapter => CrossBleAdapter.Current;

        public Scan()
        {
            foundSensorsList = new ObservableCollection<Sensor>();
        }

        public void startScan() {

            Console.WriteLine("Starting scan");

            var scanning = BleAdapter.IsScanning;

            BleAdapter.WhenStatusChanged().Subscribe(status =>
            {
                if (status == AdapterStatus.PoweredOn)
                {
                    DoScan();
                }
                //else {
                //    Console.WriteLine("Bluetooth off");
                //    DisplayAlert("Couldn't connect", "Please turn on your bluetooth", "OK");
                //}

            });
            
        }

        public void DoScan() {
            Console.WriteLine("starting scan");
            _scan?.Dispose();
            _scan = BleAdapter.ScanForUniqueDevices().Subscribe(device =>
            {
                if (device.Name != null)
                {
                    if (device.Name.StartsWith("Movesense"))
                    {
                        Console.WriteLine("found device");
                        Console.WriteLine(device.Name + " " + device.Uuid);
                        var sensor = new Sensor(device.Name, device.Uuid);
                        foundSensorsList.Add(sensor);
                    }
                }
            });
        }

        public void stopScan() {
            _scan?.Dispose();
            BleAdapter.StopScan();
            foundSensorsList.Clear();
        }

        public void disconnectAll() {
            Console.WriteLine("disconnecting from all sensors");
            
        }

        public int getNumberConnected() {
            var count = 0;
            foreach (var sensor in foundSensorsList)
            {
                if (sensor.Connected == true) {
                    count++;
                }
            }
            return count;
        }

        public async Task<bool> DisconnectAll()
        {
            foreach (var item in this.foundSensorsList)
            {
                if (item.Connected == true)
                {
                    await movesense.DisconnectMdsAsync(item.Id);
                    item.Connected = false;
                }
            }
            return true;
        }
    }
}
