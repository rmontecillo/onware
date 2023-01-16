using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using health_path.Model;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IDbConnection _connection;

    public ScheduleController(ILogger<ScheduleController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ScheduleEvent>> Fetch()
    {
        var dbResults = ReadData();
        var preparedResults = new List<ScheduleEvent>();
        var query = dbResults.GroupBy(g => g.Item1.Id);
        foreach (var group in query)
        {
            var eventScheduleList = group.First(f => f.Item1.Id == group.Key).Item1;
            foreach (var cat in group)
            {
                eventScheduleList.Recurrences.Add(cat.Item2);
            }
            preparedResults.Add(eventScheduleList);
        }

        return Ok(preparedResults);
    }

    private IEnumerable<(ScheduleEvent, ScheduleEventRecurrence)> ReadData()
    {
        var sql = @"
            SELECT e.*, r.*
            FROM Event e
            JOIN EventRecurrence r ON e.Id = r.EventId
            ORDER BY e.Id, r.DayOfWeek, r.StartTime, r.EndTime
        ";
        return _connection.Query<ScheduleEvent, ScheduleEventRecurrence, (ScheduleEvent, ScheduleEventRecurrence)>(sql, (e, r) => (e, r));
    }
}
