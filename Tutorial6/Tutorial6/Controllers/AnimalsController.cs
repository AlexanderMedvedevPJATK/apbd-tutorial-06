using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial6.DTOs;
using Tutorial6.Models;

namespace Tutorial6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("{orderBy?}")]
    public IActionResult GetAnimals(string orderBy = "Name")
    {
        // List of valid sort options
        var validSortOptions = new List<string> { "name", "description", "category", "area" };
        orderBy = char.ToUpper(orderBy[0]) + orderBy[1..];
        if (!validSortOptions.Contains(orderBy.ToLower()))
        {
            return BadRequest($"Invalid sort parameter. Valid options are: {string.Join(", ", validSortOptions)}");
        }
        
        
        // Open connection
        // !!! using for automatic disposal of connection
        using var connection = new SqlConnection(_configuration.GetConnectionString("Docker"));
        connection.Open();

        // Create commands
        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = $"SELECT * FROM Animal ORDER BY {orderBy}";
        // command.Parameters.AddWithValue("@orderBy", orderBy);

        var reader = command.ExecuteReader();

        var animals = new List<Animal>();

        var idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descriptionOrdinal = reader.GetOrdinal("Description");
        var categoryOrdinal = reader.GetOrdinal("Category");
        var areaOrdinal = reader.GetOrdinal("Area");

        while (reader.Read())
        {
            animals.Add(new Animal
            {
                IdAnimal = reader.GetInt32(idAnimalOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Category = reader.GetString(categoryOrdinal),
                Area = reader.GetString(areaOrdinal)
            });
        }

        return Ok(animals);
    }

    [HttpPost]
    public IActionResult AddAnimal(AddAnimal animal)
    {
        // Open connection
        using var connection = new SqlConnection(_configuration.GetConnectionString("Docker"));
        connection.Open();

        // Create commands
        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO Animal (Name, Description, Category, Area)" +
                              "VALUES (@name, @description, @category, @area)";
        command.Parameters.AddWithValue("@name", animal.Name);
        command.Parameters.AddWithValue("@description", animal.Description);
        command.Parameters.AddWithValue("@category", animal.Category);
        command.Parameters.AddWithValue("@area", animal.Area);

        // Execute  command
        command.ExecuteNonQuery();

        return Created("", animal);
    }
    
    [HttpPut("{id:int}")]
    public IActionResult UpdateAnimal(int id, AddAnimal animal)
    {
        // Open connection
        using var connection = new SqlConnection(_configuration.GetConnectionString("Docker"));
        connection.Open();

        // Create commands
        using var updateCommand = new SqlCommand();
        updateCommand.Connection = connection;
        updateCommand.CommandText = "UPDATE Animal SET Name = @name," +
                                    "Description = @description," +
                                    "Category = @category," +
                                    "Area = @area WHERE IdAnimal = @idAnimal";
        updateCommand.Parameters.AddWithValue("@name", animal.Name);
        updateCommand.Parameters.AddWithValue("@description", animal.Description);
        updateCommand.Parameters.AddWithValue("@category", animal.Category);
        updateCommand.Parameters.AddWithValue("@area", animal.Area);
        updateCommand.Parameters.AddWithValue("@idAnimal", id);

        // Execute  command
        var updatedRows = updateCommand.ExecuteNonQuery();
        if (updatedRows == 0) return NotFound();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public IActionResult DeleteAnimal(int id)
    {
        // Open connection
        using var connection = new SqlConnection(_configuration.GetConnectionString("Docker"));
        connection.Open();
        
        // Find animal 
        using var findCommand = new SqlCommand();
        findCommand.Connection = connection;
        findCommand.CommandText = "DELETE FROM Animal WHERE IdAnimal = @idAnimal";
        findCommand.Parameters.AddWithValue("@idAnimal", id);

        // Execute  command
        var rowsDeleted = findCommand.ExecuteNonQuery();
        
        if (rowsDeleted == 0) return NotFound();
        return NoContent();
    }
    
}