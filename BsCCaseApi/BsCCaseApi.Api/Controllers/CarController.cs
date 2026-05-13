using BsCCaseApi.Business.Services;
using BsCCaseApi.Domain.Models;
using BsCCaseApi.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CarController(ICarService carService)
{
    [HttpGet]
    public Task<List<Car>> GetAll()
    {
        return carService.GetAll();
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
    public Task<Car> Update([FromBody] CarDto car, Guid carId)
    {
        var carToUpdate = car.ToCar();
        carToUpdate.Id = carId;
        return carService.UpdateCar(car.ToCar());
    }
}