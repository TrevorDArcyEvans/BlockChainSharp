# BlockChain Implementation in C#

Based on the following articles:
* [Learn Blockchains by Building One](https://medium.com/p/117428612f46)
  * [blockchain github](https://github.com/dvf/blockchain.git).
* [Blockchain explained using C# implementation](https://towardsdatascience.com/blockchain-explained-using-c-implementation-fb60f29b9f07)
* [Building A Blockchain In .NET Core - Basic Blockchain](https://www.c-sharpcorner.com/article/blockchain-basics-building-a-blockchain-in-net-core/)

## Highlights
* store arbitrary data on the chain
  * data must be JSON serialisable
  * _GuidBlock_ provided as a sample data structure
* rewards are given in _BlockChain_ code
  * most other sample implementations conflate data and rewards
* functionality exposed via _WebAPI_
* _Swagger_ UI to allow testing

## Requirements
* .NET Core 5.0

## Getting started
* clone repository
```bash
  $ dotnet restore
  $ dotnet build
  $ dotnet run --project BlockChainSharp.Server
```
* open [Swagger UI](https://localhost:5001/swagger/index.html)

## Further Work
* persist chain
* persist _NodeId_?
* parallelise _ResolveConflicts_ when retrieving chains from other nodes 
* lock data when mining a block
* support multiple pieces of data ie list of data



