using System;
namespace sensorsgit
{
    public class SensorData
    {
        public long timeStamp;
        public double accX;
        public double accY;
        public double accZ;
        public double gyroX;
        public double gyroY;
        public double gyroZ;
        public double magX;
        public double magY;
        public double magZ;

        public SensorData(long timeStamp, double[] accvals, double[] gyrovals, double[] magvals)
        {
            this.timeStamp = timeStamp;
            this.accX = accvals[0];
            this.accY = accvals[1];
            this.accZ = accvals[2];
            this.gyroX = gyrovals[0];
            this.gyroY = gyrovals[1];
            this.gyroZ = gyrovals[2];
            this.magX = magvals[0];
            this.magY = magvals[1];
            this.magZ = magvals[2];
        }
    }
}
