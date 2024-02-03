using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using UniRepo.Interfaces;

namespace RepositoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        // The repository is injected directly into the controller for simplicity. Normally you would use a handler.
        private readonly IUniversalRepository<UniRepoContext, UserRole, Guid> _repository;

        public UserRolesController(IUniversalRepository<UniRepoContext, UserRole, Guid> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<UserRole> Get()
        {
            return _repository.GetAll();
        }

        [HttpGet("{key1}/{key2}")]
        public async Task<IActionResult> GetByCompositeKey(Guid key1, Guid key2)
        {
            var userRole = await _repository.GetByIdAsync(new[] { key1, key2 });

            return userRole is null
                ? NotFound()
                : Ok(userRole);
        }
    }
}
