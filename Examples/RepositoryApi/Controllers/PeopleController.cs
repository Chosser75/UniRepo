using Infrastructure.Database.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryApi.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RepositoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeopleController : ControllerBase
{
    private readonly IDatabaseControllerHandler _handler;

    public PeopleController(IDatabaseControllerHandler handler)
    {
        _handler = handler;
    }

    // GET: api/<PeopleController>
    [HttpGet]
    public IEnumerable<Person> Get()
    {
        return _handler.GetPeople();
    }

    // GET api/<PeopleController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<PeopleController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<PeopleController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<PeopleController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}