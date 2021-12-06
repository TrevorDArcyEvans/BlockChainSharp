using System;
using System.Collections.Generic;
using BlockChainSharp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlockChainSharp.Server.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public abstract class BlockChainController<T> : ControllerBase where T : new()
  {
    private readonly BlockChain<T> _blockChain;
    private readonly ILogger<BlockChainController<T>> _logger;

    public BlockChainController(
      BlockChain<T> blockChain,
      ILogger<BlockChainController<T>> logger)
    {
      _blockChain = blockChain;
      _logger = logger;
    }

    [HttpGet]
    [Route("NodeId")]
    public ActionResult<string> NodeId()
    {
      return Ok(_blockChain.NodeId);
    }

    [HttpGet]
    [Route("GetFullChain")]
    public ActionResult<List<Block<T>>> GetFullChain()
    {
      return Ok(_blockChain.GetFullChain());
    }

    [HttpGet]
    [Route("Mine")]
    public ActionResult<Block<T>> Mine()
    {
      return Ok(_blockChain.Mine());
    }

    [HttpPost]
    [Route("Nodes/RegisterNodes")]
    public ActionResult<IEnumerable<Node>> RegisterNodes(
      [FromBody] IEnumerable<Uri> nodes)
    {
      return Ok(_blockChain.RegisterNodes(nodes));
    }

    [HttpGet]
    [Route("Nodes/Resolve")]
    public ActionResult<List<Block<T>>> Resolve()
    {
      var chainType = ControllerContext.ActionDescriptor.ControllerName;
      return Ok(_blockChain.Resolve(chainType));
    }
  }
}
