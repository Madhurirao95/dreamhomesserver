namespace SPOTAHOME.Models.Repository.Interfaces
{
    public interface IAccountRepository : IDataRepository<Account>
    {
        public Account? GetByEmail(string email);
    }
}
