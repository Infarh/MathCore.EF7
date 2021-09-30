using Domain;
using MathCore.EF7.Controllers;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ApiServer.Controllers
{
    public class StudentsController : ApiController<Student>
    {
        public StudentsController(IRepository<Student, int> repository, ILogger<ControllerBaseActionResultApi<Student, int>> logger) : base(repository, logger)
        {
        }
    }
}