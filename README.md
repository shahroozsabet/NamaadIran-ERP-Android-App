NamaadIran ERP Android App, Xamarin, GNU GPL v3
----
Author: Shahrooz Sabet, shahrooz.sabet@gmail.com, 2014/04/09, Updated: 20150610

This app is developed with Xamarin Cross platform development framework 
for implementing an ERP application in Android devices and having an eye 
to support IOS and Windows devices in future.

This app is built in a modular way in order to improve collaboration between all the teams in development of the ERP system. 
This means that all the UI menu and forms are stored in the builtin SQLite DB, like Namaad Iran ERP itself.

The DB can be updated via BasicHttp authentication WCF web service call from the server.

This app currently has two systems in the ERP, a Sale system and a Building managment system(BMS).
Just one form for issuing Sale factor in the Sale system is built and BMS App in NBMS(Namaad co. BMS) branch is developed. 

Integrated Barcode Scanning is supported too.

For future work is planned to use DLL instead of simply call form classes in order to build different ERP modules form and UI.