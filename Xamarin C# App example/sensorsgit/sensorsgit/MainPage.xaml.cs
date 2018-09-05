using System;
//using System.Collections.Generic;
using System.Linq;
//using System.Reactive.Linq;
//using System.Text;
using System.Threading.Tasks;
//using Microsoft.Azure.Devices.Client;
//using Microsoft.Azure.Devices;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using Plugin.Movesense;
//using System.IO;
//using System.Collections.ObjectModel;
using System.Globalization;

namespace sensorsgit
{
    public partial class MainPage : ContentPage
    {
        // cloud connection variables
        static string deviceId = "sophiasiphone";
        static string deviceKey = "DEVICEKEY";
        static string iotHubUri = "IOTHUBURI";
        static string connectionString = "HostName=HOST.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=KEY";

        //other variables
        public IMovesense movesense = Plugin.Movesense.CrossMovesense.Current;
        public Scan sensorScan;
        public Session currentSession;
        public Connection currentConnection;


        public MainPage()
        {
            InitializeComponent();
        }

        void StartScanClicked(object sender, System.EventArgs e) {
            sensorScan = new Scan();
            sensorScan.startScan();
            scanButton.IsVisible = false;
            stopScanButton.IsVisible = true;
            setUpSessionButton.IsVisible = true;
            SensorView.ItemsSource = sensorScan.foundSensorsList;
        }

        async void StopScanClicked(object sender, System.EventArgs e) {
            await DisconnectAll();
            StoppingScan();
        }

        public void StoppingScan() {
            sensorScan.stopScan();
            scanButton.IsVisible = true;
            stopScanButton.IsVisible = false;
            setUpSessionButton.IsVisible = false;
            
        }

        async void DisconnectAllClicked(object sender, System.EventArgs e) {
            await DisconnectAll();
        }

        public async Task<bool> DisconnectAll() {
            foreach (var item in sensorScan.foundSensorsList)
            {
                if (item.Connected == true)
                {
                    await movesense.DisconnectMdsAsync(item.Id);
                    item.Connected = false;
                }
            }
            return true;
        }

        void SetUpSessionClicked(object sender, System.EventArgs e) {
            // check if any sensors are connected
            if (sensorScan.getNumberConnected() > 0) {
                // start session
                currentConnection = new Connection(sensorScan.foundSensorsList);
                Console.WriteLine(currentConnection.ConnectedSensorsList.Count());
                // stop scan and reset labels
                StoppingScan();
                welcomePage.IsVisible = false;
                sessionSetUpPage.IsVisible = true;
                SessionSetUpView.ItemsSource = currentConnection.ConnectedSensorsList;

            }
            else {
                // display alert - no sensors connected
                DisplayAlert("Couldn't start session", "At least one sensor must be connected", "OK");
            }
        }

        async void CancelClicked(object sender, System.EventArgs e) {
            welcomePage.IsVisible = true;
            sessionSetUpPage.IsVisible = false; 

            //Disconnect devices
            foreach (var item in currentConnection.ConnectedSensorsList)
            {
                await movesense.DisconnectMdsAsync(item.Id);
            }
            currentConnection = null;

        }

        void StartSessionClicked(object sender, System.EventArgs e) {
            // check that all sensors have a connected location
            if (currentConnection.LocationsSet() == false || patientIdentification.Text.Length==0){
                DisplayAlert("Missing values", "Please make sure you have selected locations for each sensor and entered a patient id", "Ok");
            }
            else if (currentConnection.locationsUnique() == false) {
                    DisplayAlert("Same locations", "Please make sure each sensor has a unique location", "Ok");
            }
            else {

                // create Session
                currentSession = new Session(currentConnection.ConnectedSensorsList);

                // set pages
                sessionPage.IsVisible = true;
                sessionSetUpPage.IsVisible = false;

                currentSession.patientId = patientIdentification.Text;
                SessionView.ItemsSource = currentSession.connectedSensorsList;
                patientLabel.Text = currentSession.patientId;
                sessionInfoLabel.Text = currentSession.id;

                // start recording session
                Console.WriteLine("record session...");
                currentSession.Record();

            }

        }

        void StopRecordingClicked(object sender, System.EventArgs e) {
            currentSession.StopSession();

            sessionLabel.IsVisible = false;
            recordedLabel.IsVisible = true;
            // show discard Session and Upload Session Button
            stopRecording.IsVisible = false;
            discardSession.IsVisible = true;
            uploadSession.IsVisible = true;
        }

        void DiscardSessionClicked(object sender, System.EventArgs e) {
            //reset session variables, and go back to previous page
            ExitSession();
        }

        async void UploadSessionClicked(object sender, System.EventArgs e) {

            var cloudUpload = new CloudUpload(deviceId, deviceKey, iotHubUri, connectionString);
            uploadSession.IsEnabled = false;
            discardSession.IsEnabled = false;
            await currentSession.UploadSession(cloudUpload);
            uploadSession.IsEnabled = true;
            discardSession.IsEnabled = true;
            await DisplayAlert("Successfully Uploaded", currentSession.id, "Ok");
            ExitSession();

        }

        void ExitSession() {

            currentSession = null;
            sessionSetUpPage.IsVisible = true;
            sessionPage.IsVisible = false;
            stopRecording.IsVisible = true;
            discardSession.IsVisible = false;
            uploadSession.IsVisible = false;
            sessionLabel.IsVisible = true;
            recordedLabel.IsVisible = false;

        }


        async void OnTap(object sender, ItemTappedEventArgs e)
        {
            var tappedSensor = e.Item as Sensor;

            if (tappedSensor.Connected == true) {
                // disconnect
                setUpSessionButton.IsEnabled = false;
                stopScanButton.IsEnabled = false;
                await movesense.DisconnectMdsAsync(tappedSensor.Id);
                tappedSensor.Connected = false;
                setUpSessionButton.IsEnabled = true;
                stopScanButton.IsEnabled = true;
                
            }
            else if (tappedSensor.Connected == false) {
                setUpSessionButton.IsEnabled = false;
                stopScanButton.IsEnabled = false;
                //connect
                await movesense.ConnectMdsAsync(tappedSensor.Id);
                tappedSensor.Connected = true;
                tappedSensor.setBattery();
                setUpSessionButton.IsEnabled = true;
                stopScanButton.IsEnabled = true;

            }


        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;

            int selectedIndex = picker.SelectedIndex;
            var s = picker.Title;

            if (selectedIndex != -1)
            {
                var selection = (string)picker.SelectedItem;
                currentConnection.UpdateLocation(selectedIndex, selection,  s);
            }
        }

    }

    public class NotBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)(value);
        }
    }

    public class ColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value == true) {
                return "#59b14e";
            }
            else {
                return "#d22828";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


}
