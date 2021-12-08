# CryptoPythonP2PSmartContractSystem
A Cryptocurrency using a smart contract system that uses the python P2P app developed in a previous repository to act as a blockchain application and perform various transactions on the blockchain. Mining simply involves executing the encrypted python code. 

A cryptocurrency implemented using a smart contract system. Involves a web service API (using ASP.NET MVC), WPF clients and some .NET Remoting. 
Blockchain that requires machine to compute arbitrary python code in order to produce the next block.
Note computation/mining only occurs after 5 transactions have occurred. 

Note this program isnt very secure and doesnt scale well, then again most blockchains don't. Its more of a proof of concept and demonstration of what I was taught at uni.

Originally developed by me in Distributed Computing when I was studying software engineer. Enhancements were made later on in my spare time. 

Built in Visual Studio Code.

Compile and Run (in Visual Studio Code)
1. Build entire solution
2. Right click web server "P2PService", go to debug, start new instance. 
3. Right click "WalletClientApplication", go to debug, start new instance.
4. Repeat step 3 >= 1 more time. 
5. On one of the clients add 5 transactions (give it some python code as input, 5 times).
