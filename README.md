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

Create ACL
DECLARE
v_principal VARCHAR2(20) DEFAULT 'APPS';
v_host VARCHAR2(200) DEFAULT '192.168.0.50';
acl_file VARCHAR2(200) DEFAULT 'JS_EMAIL.xml';
BEGIN
-- Check if ACL exists and drop it if needed
BEGIN
DBMS_NETWORK_ACL_ADMIN.DROP_ACL(acl => acl_file);
EXCEPTION
WHEN OTHERS THEN
NULL; -- Ignore errors if ACL doesn't exist
END;

-- Create ACL
DBMS_NETWORK_ACL_ADMIN.CREATE_ACL(
acl => acl_file,
description => 'JS_EMAIL',
principal => v_principal,
is_grant => TRUE,
privilege => 'connect');

DBMS_NETWORK_ACL_ADMIN.ADD_PRIVILEGE(
acl => acl_file,
principal => v_principal,
is_grant => TRUE,
privilege => 'resolve',
start_date => NULL,
end_date => NULL
);

-- Specify the port range (or set to NULL for any port)
DBMS_NETWORK_ACL_ADMIN.ASSIGN_ACL(
acl => acl_file,
host => v_host,
lower_port => NULL,
upper_port => NULL
);

COMMIT;
END;
/


SELECT PRINCIPAL, HOST, lower_port, upper_port, acl, 'connect' AS PRIVILEGE,
DECODE(DBMS_NETWORK_ACL_ADMIN.CHECK_PRIVILEGE_ACLID(aclid, PRINCIPAL, 'connect'), 1,'GRANTED', 0,'DENIED', NULL) PRIVILEGE_STATUS
FROM DBA_NETWORK_ACLS
JOIN DBA_NETWORK_ACL_PRIVILEGES USING (ACL, ACLID)
UNION ALL
SELECT PRINCIPAL, HOST, NULL lower_port, NULL upper_port, acl, 'resolve' AS PRIVILEGE,

DECODE(DBMS_NETWORK_ACL_ADMIN.CHECK_PRIVILEGE_ACLID(aclid, PRINCIPAL, 'resolve'), 1,'GRANTED', 0,'DENIED', NULL) PRIVILEGE_STATUS
FROM DBA_NETWORK_ACLS
JOIN DBA_NETWORK_ACL_PRIVILEGES USING (ACL, ACLID)


DECLARE
l_principal VARCHAR2(20) := 'APEX_230100';
BEGIN
DBMS_NETWORK_ACL_ADMIN.append_host_ace (
host => 'oracle-base.com',
lower_port => 8080,
upper_port => 8080,
ace => xs$ace_type(privilege_list => xs$name_list('http'),
principal_name => l_principal,
principal_type => xs_acl.ptype_db));
END;


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

Hosting
This application runs on the user’s computer and listens on the local IP address.
Ensure firewall rules allow TCP connections on port 12345.
Oracle APEX can query the kl_serial_data_log table to display real‑time weight data in dashboards or forms.

Business Impact
Eliminates manual weight entry.
Improves accuracy in inventory and dispatch workflows.
Demonstrates Oracle APEX integration with external IoT devices.
Scalable approach for other device integrations (barcode scanners, sensors).
