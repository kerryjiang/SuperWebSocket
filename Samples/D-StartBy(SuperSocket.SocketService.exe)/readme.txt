Background:

After read the previous sample projec, you might have found if you have many these kind projects, the startup code in the method "Program.Main(string[] args)" is same.
So you needn't create same startup application for your projects, Instead, you can create a common startup console project which could be used for all these kind projects.
Different projects just have different configuration file and different required assemblies.

Because SuperWebSocket is base on SuperSocket, so we can use the startup project of SuperSocket directly. The startup project of SuperSocket is SuperSocket.SocketService, which is either a console application, or a windows service application.


How to get SuperSocket.SocketSerrvice?

SuperSocket.SocketService is included in SuperWebSocket's reference folder:
* SuperSocket.SocketService.exe: the main assembly, either a console application, or a windows service application
* InstallService.bat: the script which can insall SuperSocket.SocketService.exe as a windows service
* UninstallService.bat: the script which can uninsall SuperSocket.SocketService.exe from the windows service list

How to use?

1) copy all the files of SuperSocket.SocketService (SuperSocket.SocketService.exe, InstallService.bat, UninstallService.bat) to the deployment directory;
2) rename you configuration file to "SuperSocket.SocketService.exe.config", and then put into the deployment directory;
3) copy the required assemblies into the deployment directory;
4) run it by double click the file "SuperSocket.SocketService.exe", or install it as a windows service by InstallService.bat and then run the new installed service.

In the following sample projects, a simpler way is used. Which is including all the files of SuperSocket.SocketService into the main business project. But this way is not suggested in normal developing.