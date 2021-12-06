namespace BlockChainSharp.Core
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Security.Cryptography;
  using System.Text;
  using Newtonsoft.Json;

  public class BlockChain<T> where T : new()
  {
    private T _currentData = new();
    private List<Block<T>> _chain = new();
    private readonly HashSet<Node> _nodes = new();
    private Block<T> LastBlock => _chain.Last();

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
      Block<T> block;
      var lastBlock = chain.First();
      var currentIndex = 1;
      while (currentIndex < chain.Count)
      {
        block = chain.ElementAt(currentIndex);
        Debug.WriteLine($"{lastBlock}");
        Debug.WriteLine($"{block}");
        Debug.WriteLine("----------------------------");

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

    private bool ResolveConflicts(string chainType)
    {
       List<Block<T>> newChain = null;

      foreach (var node in _nodes)
      {
        var url = new Uri(node.Address, $"/{chainType}/GetFullChain");
        var request = (HttpWebRequest)WebRequest.Create(url);
        var response = (HttpWebResponse)request.GetResponse();

        if (response.StatusCode == HttpStatusCode.OK)
        {
          var json = new StreamReader(response.GetResponseStream()).ReadToEnd();
          var chain = JsonConvert. DeserializeObject<List<Block<T>>>(json);

          if (chain.Count > _chain.Count && IsValidChain(chain))
          {
            newChain = chain;
          }
        }
      }

      if (newChain != null)
      {
        _chain = newChain;
        return true;
      }

      return false;
    }

    private Block<T> CreateNewBlock(int proof, string previousHash = null)
    {
      var block = new Block<T>
      {
        Index = _chain.Count,
        Timestamp = DateTime.UtcNow,
        Data = _currentData,
        Proof = proof,
        PreviousHash = previousHash ?? GetHash(_chain.Last())
      };

      _chain.Add(block);
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
    public Block<T> Mine()
    {
      var proof = CreateProofOfWork(LastBlock.Proof, LastBlock.PreviousHash);

      CreateTransaction(sender: "0", recipient: NodeId, amount: 1);
      var block = CreateNewBlock(proof /*, _lastBlock.PreviousHash*/);

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

    public int CreateTransaction(string sender, string recipient, int amount)
    {
      _currentData = new T();

      return LastBlock != null ? LastBlock.Index + 1 : 0;
    }
  }
}
