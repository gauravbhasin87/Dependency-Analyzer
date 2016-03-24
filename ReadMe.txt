

Dependency Analyzer is client-server console application with two servers and a client.
Each client and server jave their own Sender and Reciever.
Client has following communicators:
1. Client EchoCommunicator

Server has following communicator:
1. EchoCommunicator
2. NavigationCommunicator
3. TypeAnalysis Communicator
4. RelationCommunicator

Note: Both the servers need to be up for the application to do the relationship analysis of files on both the servers based on the merged type table.

Flow of Communication:

1. All the servers and client host the service at the designated port number
URLs of 2 servers and client are:
Client-http://localhost:8005/CommService
Server1-http://localhost:8000/CommService
Server2-http://localhost:8002/CommService

2. Servers boots up and generate type tables of files present on their machine. Then, pass these type tables to all other servers. In this case,
Server1 will pass its type table to Server2 and vice-versa.

Note: Client does not need to interact for generating type tables in the Servers.

3.
Client starts up and post the message to NavigationCommunicator of Server1 and request for list of projects on it.
Then, the Client passes the list to the RelationCommunicator of Server1 to do the relationship analysis of files in the prjects based on the merged typetable from both the servers.

Note: Press Any key on the Client Console to continue with the processing.

4. Similary, client post message to Server2 to do relationship analysis of files on servers

Note: Press Any key on the Client Console to continue with the processing.

5. After relationship analysis of files , each server send the result to client and display on the command line


Directory for storing Projects on each server is ./ProjectsDirectory.
Relationships found are saved in ./ of Client project.