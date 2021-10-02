using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using MathCore.EF7.Controllers;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace BlazorServerWebApp.Server.Controllers
{
    public class StudentController : ApiController<Student>
    {
        public StudentController(IRepository<Student, int> repository, ILogger<ApiController<Student, int>> logger) : base(repository, logger)
        {
            
        }
    }
}
