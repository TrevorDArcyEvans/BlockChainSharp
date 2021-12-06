namespace BlockChainSharp.Core
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Security.Cryptography;
  using System.Text;
  using Newtonsoft.Json;
  using RestSharp;

  public class BlockChain<T> where T : class
  {
    private T _currentData;
    private List<Block<T>> _chain = new();
    private readonly HashSet<Node> _nodes = new();
    private Block<T> LastBlock => _chain.Last();

    public int Rewards { get; private set; }
    public string NodeId { get; }

    // ctor
    public BlockChain()
    {
      NodeId = Guid.NewGuid().ToString().Replace("-", "");
      CreateNewBlock(proof: 100, previousHash: "1"); //genesis block
    }

    // private functionality
    private static bool IsValidChain(List<Block<T>> chain)
    {
      var lastBlock = chain.First();
      var currentIndex = 1;
      while (currentIndex < chain.Count)
      {
        var block = chain.ElementAt(currentIndex);

        // Check that the hash of the block is correct
        if (block.PreviousHash != GetHash(lastBlock))
        {
          return false;
        }

        // Check that the Proof of Work is correct
        if (!IsValidProof(lastBlock.Proof, block.Proof, lastBlock.PreviousHash))
        {
          return false;
        }

        lastBlock = block;
        currentIndex++;
      }

      return true;
    }

    /// <summary>
    /// Check other nodes to see if they have mined the current block.
    /// Will update chain with longest chain from other nodes.
    /// </summary>
    /// <param name="chainType">Type of data stored on chain.  Used to retrieve full chain from other nodes.</param>
    /// <returns>true if another node has longest chain, false if we have the longest chain</returns>
    private bool ResolveConflicts(string chainType)
    {
      List<Block<T>> newChain = null;

      foreach (var node in _nodes)
      {
        var client = new RestClient(node.Address);
        var request = new RestRequest($"/{chainType}/GetFullChain", DataFormat.Json);
        var response = client.Get<List<Block<T>>>(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
          continue;
        }

        var chain = response.Data;

        if (chain.Count > _chain.Count && IsValidChain(chain))
        {
          newChain = chain;
        }
      }

      if (newChain is null)
      {
        // We have the longest chain
        return false;
      }

      // Someone else has a longer chain, so update ours
      _chain = newChain;
      return true;
    }

    private Block<T> CreateNewBlock(int proof, string previousHash = null)
    {
      var block = new Block<T>
      {
        Index = _chain.Count,
        Timestamp = DateTime.UtcNow,
        Data = _currentData,
        Proof = proof,
        PreviousHash = previousHash ?? GetHash(LastBlock)
      };

      _chain.Add(block);
      _currentData = null;

      return block;
    }

    private static int CreateProofOfWork(int lastProof, string previousHash)
    {
      var proof = 0;
      while (!IsValidProof(lastProof, proof, previousHash))
      {
        proof++;
      }

      return proof;
    }

    private static bool IsValidProof(int lastProof, int proof, string previousHash)
    {
      var guess = $"{lastProof}{proof}{previousHash}";
      var result = GetSha256(guess);
      return result.StartsWith("0000");
    }

    private static string GetHash(Block<T> block)
    {
      var blockText = JsonConvert.SerializeObject(block);
      return GetSha256(blockText);
    }

    private static string GetSha256(string data)
    {
      var sha256 = new SHA256Managed();
      var hashBuilder = new StringBuilder();

      var bytes = Encoding.Unicode.GetBytes(data);
      var hash = sha256.ComputeHash(bytes);

      foreach (var x in hash)
      {
        hashBuilder.Append($"{x:x2}");
      }

      return hashBuilder.ToString();
    }

    // web server calls
    public Block<T> Mine(string chainType)
    {
      if (_currentData is null)
      {
        return null;
      }

      var proof = CreateProofOfWork(LastBlock.Proof, LastBlock.PreviousHash);
      var block = CreateNewBlock(proof /*, _lastBlock.PreviousHash*/);

      // Finished mining, so see if we get a reward
      if (!ResolveConflicts(chainType))
      {
        // we have the longest chain
        Rewards++;
      }

      return block;
    }

    public List<Block<T>> GetFullChain()
    {
      return _chain;
    }

    public HashSet<Node> RegisterNodes(IEnumerable<Uri> nodes)
    {
      foreach (var node in nodes)
      {
        _nodes.Add(new Node(node));
      }

      return _nodes;
    }

    public List<Block<T>> Resolve(string chainType)
    {
      _ = ResolveConflicts(chainType);
      return _chain;
    }

    /// <summary>
    /// Put specified data into chain.
    /// </summary>
    /// <param name="data">Data to store in chain</param>
    /// <returns>index of block with data</returns>
    public int CreateData(T data)
    {
      _currentData = data;

      return LastBlock != null ? LastBlock.Index + 1 : 0;
    }
  }
}
