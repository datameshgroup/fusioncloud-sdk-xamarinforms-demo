# Fusion Cloud - Xamarin Forms demo

Demo code for sending a Fusion Cloud payment using Xamarin Forms. 

See the [DataMesh Fusion API](https://datameshgroup.github.io/fusion/overview) for more information. 


## Getting started

### Configure Visual Studio 

* Install the ".NET Multi-platform App development" workload. Include optional Xamarin component 
* Install Android Emulator (minimum Android 13, API 33)

### Configure Android Emulator

Some versions of Android may require an SSL certificate install in order to work with the DataMesh test environment.

* Copy `/certs/Trusted Secure Certificate Authority 5.crt` to your Android device
  * If running the emulator, from VS run Tools→Android→ADP Command Prompt, then the command `adb push "..\..\certs\Trusted Secure Certificate Authority 5.crt"  /storage/emulated/0/Download`
* Navigate to Settings → Security and privacy → Other security settings
* Select Install from device storage → CA Certificate → Install anyway
* Browse to and install `Trusted Secure Certificate Authority 5.crt`

### Launch solution 

The app is already pre-configured for the development environment. You will need to enter the SaleID and POIID for your test account provided by DataMesh. 

## Dependencies

* Visual Studio 2022
* Xamarin Forms 5.0
* .netstandard2.1