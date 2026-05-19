using BsCCaseApi.Business.Services;
using BsCCaseApi.Domain.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CarController(ICarService carService) : ControllerBase
{
    [HttpGet]
    public Task<List<Car>> GetAll()
    {
        return carService.GetAllCars();
    }
    
    [HttpGet("{carId:guid}")]
    public Task<Car> Get(Guid carId)
    {
        return carService.GetCarById(carId);
    }

    [HttpPut]
    public Task Create([FromBody] CarDto car)
    {
        
        return carService.CreateCar(car.ToCar());
    }

    [HttpPatch("{carId:guid}")]
    public Task<Car?> Update([FromBody] CarDto car, Guid carId)
    {
        var carToUpdate = car.ToCar();
        carToUpdate.Id = carId;
        return carService.UpdateCar(carToUpdate);
    }

    [HttpPut]
    public Task<List<Car>> CreateRandom([FromQuery]int amount = 1)
    {
        return carService.CreateRandomCar(amount);
    }
}