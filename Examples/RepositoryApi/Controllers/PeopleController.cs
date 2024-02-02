using Microsoft.AspNetCore.Mvc;
using RepositoryApi.Interfaces;
using Person = Infrastructure.Database.Models.Entities.Person;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RepositoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeopleController : ControllerBase
{
    private readonly IPeopleControllerHandler _handler;

    public PeopleController(IPeopleControllerHandler handler)
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

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetFirstName(Guid id)
    {
        var firstName = await _handler.GetFirstNameAsync(id);

        return string.IsNullOrWhiteSpace(firstName)
            ? NotFound()
            : Ok(firstName);
    }

    [HttpGet("[action]/{date}")]
    public async Task<IEnumerable<Person>> GetYounger(DateTime date) => await _handler.GetYoungerAsync(date);

    [HttpGet("[action]/{firstName}")]
    public async Task<IActionResult> GetByFirstName(string firstName)
    {
        var person = await _handler.GetByFirstNameAsync(firstName);

        return person is null
            ? NotFound()
            : Ok(person);
    }

    [HttpGet("[action]/{date}")]
    public IEnumerable<string> GetYoungerNames(DateTime date) => _handler.GetYoungerNames(date);

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