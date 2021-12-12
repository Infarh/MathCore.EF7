using Domain;
using MathCore.EF7.Controllers;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ApiServer.Controllers
{
    public class StudentController : ApiController<Student>
    {
        public StudentController(IRepository<Student> repository, ILogger<StudentController> logger) : base(repository, logger)
        {
        }
    }
}