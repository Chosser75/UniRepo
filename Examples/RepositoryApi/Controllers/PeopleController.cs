using Microsoft.AspNetCore.Mvc;
using RepositoryApi.Interfaces;
using Person = Infrastructure.Database.Models.Entities.Person;

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

    [HttpPut]
    public async Task Put([FromBody] Person person)
    {
        await _handler.UpdateAsync(person);
    }

    [HttpPatch]
    public async Task Patch([FromBody] Person person)
    {
        await _handler.UpdateModifiedFieldsAsync(person);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id)
    {
        await _handler.DeleteAsync(id);
    }
}