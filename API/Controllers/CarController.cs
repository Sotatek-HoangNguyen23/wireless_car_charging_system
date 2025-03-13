using API.Services;
using DataAccess.DTOs;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : Controller
    {

        private CarService _carService;

        public CarController(CarService carService)
        {
            _carService = carService;
        }

        [HttpGet("owner")]
        public ActionResult<List<MyCarsDTO>> GetCarByOwner([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var cars = _carService.GetCarByOwner(userId);

            if (cars == null || cars.Count == 0)
            {
                return NotFound("Not found");
            }

            return Ok(cars);
        }

        [HttpGet("detail/{carId}")]
        public ActionResult<CarDetailDTO> GetCarDetailById(int carId)
        {
            if (carId <= 0)
            {
                return BadRequest("Invalid car ID.");
            }

            var carDetail = _carService.GetCarDetailById(carId);

            if (carDetail == null)
            {
                return NotFound("Car not found.");
            }

            return Ok(carDetail);
        }


        [HttpGet("real-time-status/{carId}")]
        public ActionResult<ChargingStatusDTO> GetChargingStatus(int carId)
        {
            var result = _carService.GetChargingStatusById(carId);
            if (result == null)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }


        [HttpGet("charging-history")]
        public ActionResult<List<ChargingHistoryDTO>> GetChargingHistory(
            [FromQuery] int carId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int? chargingStationId)
        {
            var result = _carService.GetChargingHistory(carId, start, end, chargingStationId);
            if (result == null || result.Count == 0)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }

        [HttpDelete("Delete/{carId}")]
        public IActionResult DeleteCar(int carId)
        {
            _carService.deleteCar(carId);
            

            return Ok(new { message = "Car deleted successfully" });
        }

        [HttpGet("car-models")]
        public ActionResult<List<CarModel>> GetCarModels([FromQuery] string? search)
        {
            
                var carModels = _carService.GetCarModels(search);

                if (carModels == null || carModels.Count == 0)
                {
                    return NotFound(new { message = "No car models found." });
                }

                return Ok(carModels);
            
        }

        [HttpPost("add-car")]
        public IActionResult AddCar([FromBody] AddCarRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            if (request.CarModelId <= 0 || request.UserId <= 0 || string.IsNullOrEmpty(request.LicensePlate) || string.IsNullOrEmpty(request.CarName))
            {
                return BadRequest("Invalid input parameters.");
            }

            try
            {
                _carService.addCar(request.CarModelId, request.UserId, request.LicensePlate, request.CarName);
                return Ok(new { message = "Car added successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the car.", error = ex.Message });
            }
        }

        [HttpPut("edit-car")]
        public IActionResult EditCar([FromBody] EditCarRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            if (request.CarModelId <= 0 || request.CarId <= 0 || string.IsNullOrEmpty(request.LicensePlate) || string.IsNullOrEmpty(request.CarName))
            {
                return BadRequest("Invalid input parameters.");
            }

            try
            {
                _carService.editCar(request.CarModelId, request.CarId, request.LicensePlate, request.CarName);
                return Ok(new { message = "Car updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the car.", error = ex.Message });
            }
        }


    }
}
