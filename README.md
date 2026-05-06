# serialportwithOracleApex
C# program to integrate a digital weight scale via serial port with Oracle Database and APEX for automated weight capture.

**create table **

create table kl_serial_data_log (
barcode_no varchar2(30),
scale_data number, 
creation_date date,
terminal_ip varchar2(30))

Add **Oracle.ManagedDataAccess** Package to you solution.


