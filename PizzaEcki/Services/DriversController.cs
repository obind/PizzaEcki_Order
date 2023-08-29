
using PizzaEcki.Database;
using SharedLibrary;
using System;
using System.Collections.Generic;
using SharedLibrary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace PizzaEcki.Services
{
    [ApiController]
    [Route("api/[controller]")]

        public class DriversController : ControllerBase
        {
            private readonly DatabaseManager _dbManager;

            public DriversController(DatabaseManager dbManager)
            {
                _dbManager = dbManager;
            }

            [HttpGet]
            public ActionResult<List<Driver>> GetDrivers()
            {
                var drivers = _dbManager.GetAllDrivers();
                return Ok(drivers);
            }
        }
    
}
