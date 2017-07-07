Installation:

	- Open 'SQL Plus' app and run the following commands:

		/ as sysdba
		alter session set container = pdborcl;
		create user Q_Zone identified by 1;
		grant all privileges to Q_Zone;
		conn Q_Zone/1@pdborcl;

	- Now open 'SQL Developer' app, create a new connection to Q_Zone, and run all the scripts serially from "Scripts" folder.


Uninstallation:

	- Open 'SQL Plus' app and run the following commands:

		/ as sysdba
		alter session set container = pdborcl;
		drop user Q_Zone cascade;

