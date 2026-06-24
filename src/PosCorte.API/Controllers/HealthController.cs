using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] PosCorteDbContext db)
        {
            try
            {
                var ok = await db.Database.CanConnectAsync();
                if (!ok)
                    return StatusCode(503, new { status = "degraded", database = "unreachable" });

                return Ok(new { status = "ok", database = "connected", time = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "degraded",
                    database = "error",
                    hint = "Verifique DB_PASSWORD ou ConnectionStrings__DefaultConnection no Railway.",
                    detail = ex.Message
                });
            }
        }
    }
}
