using BsCCaseApi.Domain.Models;

namespace BsCCaseApi.Models.Request;

public class CarDto
{
    public int CustomerId { get; set; }
    public string RegNo { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string VIN { get; set; }

    public Car ToCar()
    {
        return new Car
        {
            CustomerId = CustomerId,
            RegNo = RegNo,
            Make = Make,
            Model = Model,
            VIN = VIN
        };
    }
}