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

    [HttpGet]
    public IEnumerable<Person> Get()
    {
        return _handler.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var person = await _handler.GetAsync(id);

        return person is null
            ? NotFound()
            : Ok(person);
    }

    [HttpPost]
    public async Task<Guid> Post([FromBody] Person person)
    {
        return await _handler.AddAsync(person);
    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}