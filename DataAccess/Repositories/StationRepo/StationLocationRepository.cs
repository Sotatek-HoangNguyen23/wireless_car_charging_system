using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.StationRepo
{

    public class StationLocationRepository : IChargingLocationRepository
    {
        private readonly WccsContext _context;

        public StationLocationRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<StationLocation> AddStationLocation(StationLocation location)
        {
            _context.StationLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public StationLocation GetLocationById(int locationId)
        {
            throw new NotImplementedException();
        }
    }
}
