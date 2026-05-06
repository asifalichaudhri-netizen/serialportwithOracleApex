# Automating Weight Capture in Oracle APEX via Serial Port Integration

## Overview
This project demonstrates how to integrate a digital weight scale with Oracle Database and Oracle APEX using a C# listener program.  
The solution automates weight capture, eliminating manual entry and improving accuracy in inventory and manufacturing workflows.

## Features
- Reads weight data directly from a serial port (COM1).
- Cleans and validates numeric values using regex.
- Inserts data into Oracle table with logging.
- Provides a TCP listener for client requests.
- Robust error handling and retry logic.
- Local logging to file for monitoring and troubleshooting.

## Requirements
.NET Framework / .NET Core
Oracle.ManagedDataAccess package (NuGet)
Oracle Database (tested on 12c/19c/26ai)
Digital weight scale with RS‑232 or USB support


## Database Setup
Create the table to store weight scale data:
```sql
CREATE TABLE kl_serial_data_log (
    barcode_no   VARCHAR2(30),
    scale_data   NUMBER,
    creation_date DATE,
    terminal_ip  VARCHAR2(30)
);

## Installation
git clone https://github.com/<your-username>/oracle-apex-serial-weight-integration.git
Open the solution in Visual Studio.
Add the Oracle.ManagedDataAccess NuGet package:
Install-Package Oracle.ManagedDataAccess
Update the Oracle connection string in Program.cs with your database credentials.

Build the solution.

Usage
Connect your weight scale to the COM1 port (adjust port settings if needed).

Run the application:
The application will:
Listen for client POST requests on port 12345.
Read weight data from the serial port.
Insert the data into kl_serial_data_log.
Log events to C:\Logs\SerialPortReader.log.

## Hosting
This application runs on the user’s computer and listens on the local IP address.
Ensure firewall rules allow TCP connections on port 12345.
Oracle APEX can query the kl_serial_data_log table to display real‑time weight data in dashboards or forms.

## Business Impact
Eliminates manual weight entry.
Improves accuracy in inventory and dispatch workflows.
Demonstrates Oracle APEX integration with external IoT devices.
Scalable approach for other device integrations (barcode scanners, sensors).
