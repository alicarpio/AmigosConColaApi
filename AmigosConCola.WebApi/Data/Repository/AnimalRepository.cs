using AmigosConCola.Core.Models;
using AmigosConCola.Core.Repositories;
using AmigosConCola.WebApi.Data.Database;
using AmigosConCola.WebApi.Data.Dto;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace AmigosConCola.WebApi.Data.Repository;

public class AnimalRepository : IAnimalRepository
{
    private readonly ApplicationDbContext _db;

    public AnimalRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ErrorOr<Animal>> Create(CreateAnimalParams parameters)
    {
        var dto = new AnimalDto
        {
            Name = parameters.Name,
            Age = parameters.Age,
            Gender = parameters.Gender.ToString(),
            ImageUrl = parameters.ImageUrl,
            Species = parameters.Species.ToString(),
            Weight = parameters.Weight,
            Story = parameters.Story,
            Code = parameters.Code,
            Location = parameters.Location
        };

        var result = await _db.Animals.AddAsync(dto);

        await _db.SaveChangesAsync();

        return result.Entity.ToDomain();
    }

    public Task<ErrorOr<IEnumerable<Animal>>> GetAll(PaginationParams parameters, GetAllAnimalsFilters filters)
    {
        var query = _db.Animals.AsQueryable();

        if (filters.Species is not null)
        {
            query = query.Where(x => x.Species.ToLower() == filters.Species.ToString()!.ToLower());
        }

        if (filters.Name is not null)
        {
            query = query.Where(x => x.Name.ToLower().Contains(filters.Name.ToLower()));
        }

        var result = query
            .OrderByDescending(x => x.Id)
            .Skip((parameters.Page - 1) * parameters.PerPage)
            .Take(parameters.PerPage)
            .Select(x => x.ToDomain().Value)
            .AsEnumerable()
            .ToErrorOr();

        return Task.FromResult(result);
    }

    public async Task<int> CountAll(GetAllAnimalsFilters filters)
    {
        var query = _db.Animals.AsQueryable();

        if (filters.Species is not null)
        {
            query = query.Where(x => x.Species.ToLower() == filters.Species.ToString()!.ToLower());
        }

        return await query.CountAsync();
    }

    public async Task<ErrorOr<Animal>> GetById(int id)
    {
        var animal = await _db.Animals.Where(x => x.Id == id).FirstOrDefaultAsync();

        if (animal is null)
        {
            return Error.NotFound(description: $"There is no animal with the id {id}");
        }

        return animal.ToDomain();
    }
}
