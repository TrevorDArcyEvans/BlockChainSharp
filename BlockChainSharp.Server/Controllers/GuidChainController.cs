namespace BlockChainSharp.Server.Controllers
{
  using Core;
  using Model;
  using Microsoft.Extensions.Logging;

  public sealed class GuidChainController : BlockChainController<GuidBlock>
  {
    public GuidChainController(
      BlockChain<GuidBlock> blockChain,
      ILogger<GuidChainController> logger) :
      base(blockChain, logger)
    {
    }
  }
}
