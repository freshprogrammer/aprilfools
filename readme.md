##April fools prank application
###Description
Starts silently and runs in background. Automatically creates a random schedule of pranks spread out for the next 8 hour user session. Can be remotely controlled and disabled from a simple php page. Default exe name is "jucheck.exe". This application does not read or write anything from local disc and uploads current prank schedule via web ~every 5 seconds.

###Setup
Just run application for autonomous execution. For remote control and monitoring place the prankConroller.php file on a PHP enabled server then, as cmd line arg, pass the URL to the **directory** where the prankerConroller is located. The file name is hard coded and not included so the end used cannot locate it in the cmd line args.
Sample cmd line args for start delay(seconds) and **dir** url where controller is located: " 300 http://0.0.8.8/customApps/"


###Hotkeys:
- CTRL+WIN+F4 Global Pause
- CTRL+WIN+F2 Global Resume
- Alt+Shfit+F4 Kill application - non reversible without manual restart