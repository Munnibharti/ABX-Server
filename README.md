# ABX Server
This is C# client application capable of requesting and receiving stock ticker data from the ABX exchange server. 
2. Download and unzip this file to access the contents.
3. Enter the extracted folder.
4. Run the command "node main.js" to start the ABX exchange server.

Features:
Connect to the ABX server using TCP.
Request all available packets from the server.
Handles missing requirements by requesting specific packets.
Outputs the retrieved data to a JSOn file.

Requirements:
-.Net 8 Sdk
-visual studio 2022 or any compatible IDE
internet connection

How to run the code:
-clone the repository
git clone <repository-url>
Navigate to the Project Directory:
cd <repository-folder>
Initialize a Git Repository:
git init
Pull Dependencies:
git pull origin main

nstall Required Dependencies (Server)
-Install the Node.js dependencies for the ABX server:
npm install

Start the ABX Server:
node main.js

Prepare the Client Application:
Open the C# client project in Visual Studio 2022 or any compatible IDE.
Restore NuGet packages if required. This can usually be done automatically by the IDE when you build the project.

Build and Run the Client Application:
Build the Project: Resolve dependencies and compile the project by using the "Build" option in your IDE.
Run the Application: Execute the client application to connect to the ABX server and request stock ticker data.








