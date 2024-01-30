using SPOTAHOME.Models.Repository.Db_Context;
using SPOTAHOME.Models.Repository.Interfaces;

namespace SPOTAHOME.Models.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SpotAhomeContext _context;

        public AccountRepository(SpotAhomeContext context)
        {
            _context = context;
        }
        public void Add(Account entity)
        {
            _context.Accounts.Add(entity);
            _context.SaveChanges();
        }

        public Account? GetByEmail(string email)
        {
            return _context.Accounts.SingleOrDefault(x => x.Email == email);
        }

        public void Delete(Account entity)
        {
            throw new NotImplementedException();
        }

        public Account Get(long id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Account> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Update(Account entityToUpdate, Account entity)
        {
            throw new NotImplementedException();
        }
    }
}
