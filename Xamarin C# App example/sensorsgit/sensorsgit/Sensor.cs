using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Plugin.Movesense;


namespace sensorsgit
{
    public class Sensor : INotifyPropertyChanged
    {
        //public SensorData currentSensorData;
        public Plugin.Movesense.IMdsSubscription subscription;


        private string batteryCharge;
        public string BatteryCharge
        {
            get { return this.batteryCharge; }
            set
            {
                if (this.batteryCharge != value)
                {
                    this.batteryCharge = value;
                    this.PropertyHasChanged("BatteryCharge");
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.PropertyHasChanged("Name");
                }
            }
        }

        private System.Guid id;
        public System.Guid Id
        {
            get { return this.id; }
            set
            {
                if (this.id != value)
                {
                    this.id = value;
                    this.PropertyHasChanged("Id");
                }
            }
        }
        private int sensorPosition;
        public int SensorPosition
        {
            get { return this.sensorPosition; }
            set
            {
                if (this.sensorPosition != value)
                {
                    this.sensorPosition = value;
                    this.PropertyHasChanged("SensorPosition");
                }
            }
        }

        public String positionDescription;
        public String PositionDescription
        {
            get { return this.positionDescription; }
            set
            {
                if (this.positionDescription != value)
                {
                    this.positionDescription = value;
                    this.PropertyHasChanged("PositionDescription");
                }
            }
        }

        private bool connected;
        public bool Connected
        {
            get { return this.connected; }
            set
            {
                if (this.connected != value)
                {
                    this.connected = value;
                    this.PropertyHasChanged("Connected");
                }
            }
        }

        public Sensor(string name, System.Guid id)
        {
            this.id = id;
            this.name = name;
            this.connected = false;
        }

        public async void setBattery()
        {
            if (this.connected == true)
            {
                var battery = await Plugin.Movesense.CrossMovesense.Current.GetBatteryLevelAsync(this.name);
                var res = battery.ChargePercent.ToString() + "%";
                this.BatteryCharge = res;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void PropertyHasChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        public async void doSubscription(Session session)
        {
            //Plugin.Movesense.CrossMovesense.Current.

            subscription = await Plugin.Movesense.CrossMovesense.Current.SubscribeIMU9Async(
                         this.Name,
                         (d) =>
                         {
                             double[] arrayAcc = new double[] { d.body.ArrayAcc[0].X, d.body.ArrayAcc[0].Y, d.body.ArrayAcc[0].Y };
                             double[] arrayGyro = new double[] { d.body.ArrayGyro[0].X, d.body.ArrayGyro[0].Y, d.body.ArrayGyro[0].Y };
                             double[] arrayMag = new double[] { d.body.ArrayMagn[0].X, d.body.ArrayMagn[0].Y, d.body.ArrayMagn[0].Y };
                             SensorData dataPoint = new SensorData(d.body.Timestamp, arrayAcc, arrayGyro, arrayMag);
                             Console.WriteLine(this.Name);
                             session.WriteToFile(this, dataPoint);

                         },
                         26);

        }

        public void unsubscribe()
        {
            subscription.Unsubscribe();
        }

    }


}
