namespace BlockChainSharp.Model
{
  using System;

  public sealed class GuidBlock
  {
    public Guid Guid { get; set; } = Guid.NewGuid();
  }
}
