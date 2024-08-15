namespace Db.Api.Controllers;

using Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ILogger<DatabaseController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetData")]
    public IEnumerable<VectorDocument> Get(string searchString)
    {
        throw new NotImplementedException();
    } 

    [HttpPost(Name = "InsertQueries")]
    public void Post(string[] queries)
    {
        throw new NotImplementedException();
    }
}
