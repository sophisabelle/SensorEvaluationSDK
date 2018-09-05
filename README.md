# Sensor-Evaluation-SDK

Developing motion sensor apps with automatic trigger to Azure Machine Learning Studio Experiment to classify movement and visualise on a website

Repository includes
- database schema
- API
- example app
- example website
- function app code
- link to machine learning experiments

The app uses the following plugins:

  -	Plugin.BLE (version 1.3.0)
  -	Plugin.Movesense (1.1.0-beta)
  -	Microsoft.Azure.Devices (1.17.0)

For replication:
The azure function trigger needs to be set to blob uploads to a storage account in the settings.
Additionally an IoT hub needs to be considered, and connection strings need to be set up in the app.
