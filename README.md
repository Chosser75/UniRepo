Welcome to the UniRepo, a flexible and efficient ORM repository designed for .NET applications using Entity Framework. This repository provides a streamlined approach to performing CRUD operations, with a focus on performance and ease of use.


A quick example of how to use the UniversalRepository in your application:

// Initialize your DbContext.
var dbContext = new YourDbContext();

// Create an instance of the repository.
var repository = new UniversalRepository<YourDbContext, YourEntity, YourEntityPrimaryKeyType>(dbContext);

// Use the repository for CRUD operations.
var entity = await repository.GetByIdAsync(yourEntityId);
entity.YourProperty = "Updated Value";
await repository.PatchAsync(entity);


This project is licensed under the MIT License.
