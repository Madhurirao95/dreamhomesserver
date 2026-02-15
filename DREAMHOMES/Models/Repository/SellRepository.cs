using Microsoft.EntityFrameworkCore;
using DREAMHOMES.Models.Repository.Db_Context;
using DREAMHOMES.Models.Repository.Interfaces;
using NetTopologySuite.Geometries;

namespace DREAMHOMES.Models.Repository
{
    public class SellRepository : ISellRepository
    {
        private readonly DreamhomesContext _context;

        public SellRepository(DreamhomesContext context)
        {
            _context = context;
        }

        public async Task Add(SellerInformation entity)
        {
            // Extract to common method.
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(entity.Location.X, entity.Location.Y));

            entity.Location = currentLocation;

            _context.Add(entity);
            await _context.SaveChangesAsync();
        }

        public Task Delete(SellerInformation entity)
        {
            throw new NotImplementedException();
        }

        public async Task<SellerInformation> Get(long id)
        {
           return await _context.SellerInformation
                .Include(x => x.Documents)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<IEnumerable<SellerInformation>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task Update(SellerInformation entityToUpdate)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(entityToUpdate.Location.X, entityToUpdate.Location.Y));

            entityToUpdate.Location = currentLocation;
            _context.SellerInformation.Update(entityToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SellerInformation>> GetAllByAddress(string streetAddress, string city, string zipCode, string country, SellerInformation.States state)
        {
            return await _context.SellerInformation
                         .Where(x => streetAddress == x.StreetAddress
                               && city == x.City
                               && zipCode == x.ZipCode
                               && country == x.CountryCode
                               && x.State == state)
                         .ToListAsync();
        }

        public async Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId)
        {
            return await _context.SellerInformation
                   .Where(x => x.UserId == userId)
                   .Include(x => x.Documents)
                   .ToListAsync();
        }

        public async Task<(IEnumerable<SellerInformation>, int)> GetAllListingByCoordinates(double coordinatex, double coordinatey, int page, int pageSize)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var centerPoint = geometryFactory.CreatePoint(new Coordinate(coordinatex, coordinatey));

            // 100 km in degrees (approximate bounding box)
            // 1 degree ≈ 111 km, so 100 km ≈ 0.9 degrees
            // Add buffer for corners: 100 km * 1.5 = 150 km ≈ 1.35 degrees
            const double boundingBoxDegrees = 1.35; // ~150 km to account for corners

            double minLon = coordinatex - boundingBoxDegrees;
            double maxLon = coordinatex + boundingBoxDegrees;
            double minLat = coordinatey - boundingBoxDegrees;
            double maxLat = coordinatey + boundingBoxDegrees;

            // First filter: Get all properties in the bounding box (fast database query)
            var queryInBox = _context.SellerInformation
                .Where(l => l.Location.X >= minLon && l.Location.X <= maxLon &&
                            l.Location.Y >= minLat && l.Location.Y <= maxLat)
                .Include(x => x.Documents);

            // Get all results from bounding box
            var allInBox = await queryInBox.ToListAsync();

            // Second filter: Calculate actual distance using Haversine formula (accurate)
            var filteredResults = allInBox
                .Where(item => CalculateDistance(coordinatex, coordinatey,
                                                item.Location.X, item.Location.Y) <= 100.0) // 100 km radius
                .ToList();

            var totalCount = filteredResults.Count;

            // Apply pagination
            var pagedResults = filteredResults
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedResults, totalCount);
        }

        // Haversine formula for accurate distance calculation in km
        private double CalculateDistance(double lon1, double lat1, double lon2, double lat2)
        {
            const double R = 6371; // Earth's radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Asin(Math.Sqrt(a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
