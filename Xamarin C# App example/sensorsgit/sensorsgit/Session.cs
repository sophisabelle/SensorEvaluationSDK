using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace sensorsgit
{
    public class Session
    {
        public string id;
        public int numberOfSensors;
        public DateTime dateTimeUploaded;
        private DateTime startTime;
        public int numberOfRecords = 0;
        public string patientId;
        private string pathname;
        public ObservableCollection<Sensor> connectedSensorsList;



        public Session(ObservableCollection<Sensor> foundSensorsList)
        {
            this.id = "s" + DateTime.Now.ToString("yyyyMMddhhmmssff");
            this.connectedSensorsList = new ObservableCollection<Sensor>();
            this.numberOfSensors = 0;
            foreach (var sensor in foundSensorsList)
            {
                if (sensor.Connected == true)
                {
                    connectedSensorsList.Add(sensor);
                    this.numberOfSensors++;
                }
            }

        }

        public void Record() {

            // create file
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // set path
            pathname = Path.Combine(documents, this.id + ".tmp");

            using (StreamWriter writer = new StreamWriter(pathname, true))
            {
                //write header
                string header = "sessionID,patientID,startTime,numberOfSensors,sensorName,sensorPosition,timeStamp,accX,accY,accZ,gyroX,gyroY,gyroZ,magX,magY,magZ";
                writer.WriteLine(header);
            }

            // set startTime
            this.startTime = DateTime.Now;

            SubscribeAll();
        }

        private void SubscribeAll()
        {
            // set subscriptions for all sensors
            foreach (Sensor sensor in this.connectedSensorsList)
            {
                sensor.doSubscription(this);
            }

        }

        private void UnsubscribeAll()
        {
            foreach (Sensor sensor in this.connectedSensorsList)
            {
                sensor.unsubscribe();
            }

        }

        public void WriteToFile(Sensor sensor, SensorData data) {
        
            using (StreamWriter writer = new StreamWriter(pathname, true))
            {
                //write header
                string write = this.id + "," + patientId + "," + this.startTime.Ticks + "," + this.numberOfSensors + "," + sensor.Name + "," + sensor.SensorPosition.ToString() + "," + data.timeStamp.ToString() + "," + data.accX + "," + data.accY + "," + data.accZ + "," + data.gyroX + "," + data.gyroY + "," + data.gyroZ + "," + data.magX + "," + data.magY + "," + data.magZ;
                writer.WriteLine(write);
                long length = new System.IO.FileInfo(pathname).Length;
                Console.WriteLine(length);
            }

        }

        public void StopSession() {
            UnsubscribeAll();
        }

        public async Task<bool> UploadSession(CloudUpload cloudUpload) {

            using (StreamWriter writer = new StreamWriter(pathname, true))
            {
                cloudUpload.SendToBlobAsync(pathname, writer, this.id);
            }

            Console.WriteLine("Receive file upload notifications\n");
            await cloudUpload.ReceiveFileUploadNotificationAsync();
            return true;
        }

        public async void DisconnectAll()
        {
            foreach (var item in this.connectedSensorsList)
            {
                if (item.Connected == true)
                {
                    await Plugin.Movesense.CrossMovesense.Current.DisconnectMdsAsync(item.Id);
                    item.Connected = false;
                }
            }
        }

    }
}
