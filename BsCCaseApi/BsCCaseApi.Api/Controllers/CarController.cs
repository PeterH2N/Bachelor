using BsCCaseApi.Library.Models;
using BsCCaseApi.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace BsCCaseApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CarController(ICarService carService)
{
    [HttpGet("{carId:int}")]
    public Task<Car> Get(int carId)
    {
        return carService.GetCarById(carId);
    }

    [HttpPut]
    public Task Create([FromBody] Car car)
    {
        return carService.CreateCar(car);
    }

    [HttpPatch]
    public Task<Car> Update([FromBody] Car car)
    {
        return carService.UpdateCar(car);
    }
}