using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFileDownloadTool
{
   class Program
   {

      static int Main(string[] args)
      {
         if(args.Length == 0)
         {
            Console.WriteLine("Must pass in a project file name.");
            Console.WriteLine("Usage: ProjectFileDownload c:\\ProjectFile.adf");
            return -5;// No file name passed in.
         }
         const int ExclusiveAccessCommand = 4521984;
         AMC_DriveManager300.CDriveManager DriveManager = new AMC_DriveManager300.CDriveManager();
         Console.WriteLine("Opening project file.");
         int result = DriveManager.OpenProject(args[0]);// Open project file.
         if(result != 1)
         {
            Console.WriteLine("Could not open project file. Error code = " + result);
            Console.WriteLine("Project file name passed in - " + args[0]);
            return -3;// Could not open project file
         }
         const int maxNumberOfDevices = 10;
         int numberOfDevices = 0;
         uint [] deviceList = new uint[maxNumberOfDevices];
         Console.WriteLine("Connecting to drive.");
         //Set DriveManager to communicate over USB
         DriveManager.SetDriveCommunicationInterface(AMC_DriveManager300.DRIVE_MANAGER_COMMUNICATION_INTERFACES.USB);
         DriveManager.SetBaudRate("230400");

         //Get AMC drives connected over USB.
         DriveManager.GetNetworkDevices(maxNumberOfDevices, out numberOfDevices, out deviceList[0]);
         if(numberOfDevices < 1)
         {
            return -4;// No AMC Drives connected over USB.
         }
         AMC_DriveManager300.DRIVE_MANAGER_INTERFACE_ERRORS interfaceError = DriveManager.InitComPortByID(deviceList[0]);

         DriveManager.AttemptToConnect();
         int connected = DriveManager.GetConnectionStatus();
         if(connected != 1)
         {
            return -1;// could not communicate with drive.
         }

         Console.WriteLine("Writing project file to drive.");
         AMC_DriveManager300.PARAMETER_ERRORS parameterError = DriveManager.SetParameterValueInt(ExclusiveAccessCommand, 255);// Get Write access.
         int writeToDriveResult = DriveManager.WriteParameterToDrive(ExclusiveAccessCommand);
         int writeProjectResult = DriveManager.WriteProjectToDrive();
         if (writeProjectResult != 1)
         {
            Console.WriteLine("Error writing project to drive. Error code = " + writeProjectResult);
            return -2;// Could not write project to drive.
         }

         Console.WriteLine("Storing parameters to non-volatile memory.");
         DriveManager.StoreDriveParameter();

         parameterError = DriveManager.SetParameterValueInt(ExclusiveAccessCommand, 0);// Release Write access.
         writeToDriveResult = DriveManager.WriteParameterToDrive(ExclusiveAccessCommand);

         DriveManager.Disconnect();
         Console.WriteLine("Completed.");
         return 1;// Success.
      }
   }
}
