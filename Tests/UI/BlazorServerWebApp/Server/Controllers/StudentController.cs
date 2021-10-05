using Microsoft.Extensions.Logging;
using Domain;
using MathCore.EF7.Controllers;
using MathCore.EF7.Interfaces.Repositories;

namespace BlazorServerWebApp.Server.Controllers
{
    public class StudentController : ApiController<Student>
    {
        public StudentController(IRepository<Student> repository, ILogger<ApiController<Student>> logger) : base(repository, logger)
        {
            
        }
    }
}
