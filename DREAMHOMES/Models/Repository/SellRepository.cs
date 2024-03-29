using Microsoft.EntityFrameworkCore;
using DREAMHOMES.Models.Repository.Db_Context;
using DREAMHOMES.Models.Repository.Interfaces;

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
    }
}
