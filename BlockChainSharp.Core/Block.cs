namespace BlockChainSharp.Core
{
  using System;

  public class Block<T>
  {
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public T Data { get; set; }
    public int Proof { get; set; }
    public string PreviousHash { get; set; }

    public override string ToString()
    {
      return $"{Index} [{Timestamp:yyyy-MM-dd HH:mm:ss}] Proof: {Proof} | PrevHash: {PreviousHash}";
    }
  }
}
