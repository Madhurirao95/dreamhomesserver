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

        public void Add(SellerInformation entity)
        {
            // Extract to common method.
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(entity.Location.X, entity.Location.Y));

            entity.Location = currentLocation;

            _context.Add(entity);
            _context.SaveChanges();
        }

        public void Delete(SellerInformation entity)
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

        public void Update(SellerInformation entityToUpdate)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(entityToUpdate.Location.X, entityToUpdate.Location.Y));

            entityToUpdate.Location = currentLocation;
            _context.SellerInformation.Update(entityToUpdate);
            _context.SaveChanges();
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

            // Perform the spatial query
            var query = _context.SellerInformation.Where(l => l.Location.IsWithinDistance(centerPoint, 100000)).Include(x => x.Documents);
            var results =  await query
            .Select(item => new
             {
                 item,
                 TotalCount = query.Count()
             })
            .Skip(page * pageSize)
            .Take(pageSize)
            .Where(l => l.item.Location.IsWithinDistance(centerPoint, 100000))
            .ToListAsync();

            return (results.Select(x => x.item), results.FirstOrDefault()?.TotalCount ?? query.Count());
        }
    }
}
