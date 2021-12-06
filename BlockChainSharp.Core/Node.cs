namespace BlockChainSharp.Core
{
  using System;

  public sealed class Node
  {
    public Uri Address { get; }

    public Node(Uri address)
    {
      Address = address;
    }

    #region Equals overrides

    private bool Equals(Node other)
    {
      return Address.Equals(other.Address);
    }

    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj) || obj is Node other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Address.GetHashCode();
    }

    #endregion
  }
}
