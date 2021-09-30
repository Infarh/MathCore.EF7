using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Configuration;
using Simple.OData.Client;
using TestCommon;
using TestCommon.Service;

namespace BlazorServerWebApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentController : ControllerBase
    {

        private readonly ILogger<StudentController> _logger;
        private readonly IStudentService _Service;
        private readonly OdataClient _OdataClient;

        public StudentController(ILogger<StudentController> logger, IStudentService service, OdataClient OdataClient)
        {
            _logger = logger;
            _Service = service;
            _OdataClient = OdataClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Student>> GetAsync()
        {
            //var client = new ODataClient($"{BaseAddress}api/");
            try
            {
                var test = await _OdataClient.FindEntriesAsync("Students?$Filter=Name eq 'Alex'");
                //var test_1 = await _OdataClient.FindEntriesAsync("Students?$Select=Name,Age");
                //var test_3 = await _OdataClient.For<Student>().Skip(1).Top(20).Select(s => s.Name).FindEntriesAsync();
                //var p = await _OdataClient.For<Student>().Set(
                //    new Student()
                //    {
                //        Age = 15,
                //        Name = "Test Student Odata ADD_2"
                //    }).InsertEntryAsync();
                var p2 = await _OdataClient.For<Student>().Key(2).DeleteEntriesAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //var people = await client.For<Student>()
            //   .Filter(s => s.Age >18 && s.Age<20)
            //   .Top(2)
            //   .Select(s => new { s.Age })
            //   .FindEntriesAsync();


            //var x = ODataDynamic.Expression;
            //var test1 = await client.For(x.Student).Filter(x.Name == "Alex").FindEntriesAsync();
            //var test_2 = await client.For<Student>().Key(2).FindEntriesAsync(); //ToDo добавить методы в OData
            //foreach (var pac in packages)
            //{
            //}

            //foreach (var t in test1)
            //{
            //    Debug.WriteLine(t.Name);
            //}


            var d = await _Service.GetAllAsync();
            var data = new List<Student>(d);
            return data;
        }
    }
}
