using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly ILogger<NewsletterController> _logger;
    private readonly IDbConnection _connection;

    public NewsletterController(ILogger<NewsletterController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpPost]
    public ActionResult Subscribe(string Email)
    {
        var eMail = "";
        var emailSplit =  Email.Split(new string[]{"@"},StringSplitOptions.RemoveEmptyEntries);
        eMail = emailSplit[0].Replace(".", "");
        eMail = String.Join("@",eMail,emailSplit[1]);

        var inserted = _connection.Execute(@"
            INSERT INTO NewsletterSubscription (Email)
            SELECT *
            FROM ( VALUES (@Email) ) AS V(Email)
            WHERE NOT EXISTS ( SELECT * FROM NewsletterSubscription e WHERE e.Email = v.Email )
        ", new { Email = eMail });

        return inserted == 0 ? Conflict("email is already subscribed") : Ok();
    }
}
