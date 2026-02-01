using AutoMapper;
using DREAMHOMES.Models;
using DREAMHOMES.Models.Repository.Interfaces;
using DREAMHOMES.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMES.Services
{
    public class SellService : ISellService
    {
        private readonly IValidationService _validationService;
        private readonly ISellRepository _sellRepository;
        private readonly IMapper _mapper;
        public SellService(ISellRepository sellRepository, IValidationService validationService, IMapper mapper)
        {
            _sellRepository = sellRepository;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ValidationResult>> PostListing(SellerInformation sellerInformation)
        {
            var results = new List<ValidationResult>();

            _validationService.Add(async () => await CheckDuplicateListing(sellerInformation));

            results.AddRange(await _validationService.ValidateAll());

            if (results.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                return results;
            }

            await _sellRepository.Add(sellerInformation);

            return results;
        }

        public async Task<IEnumerable<ValidationResult>> UpdateListing(SellerInformation existingSellerInformation, SellerInformation sellerInformation)
        {
            var results = new List<ValidationResult>();

            var updatedEntity = _mapper.Map(sellerInformation, existingSellerInformation);

            _validationService.Add(async () => await CheckDuplicateListing(updatedEntity));

            results.AddRange(await _validationService.ValidateAll());

            if (results.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                return results;
            }

            await _sellRepository.Update(updatedEntity);

            return results;
        }


        public async Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId)
        {
            return await _sellRepository.GetAllListingBySeller(userId);
        }

        public async Task<SellerInformation> GetSellerInformationById(int id)
        {
            return await _sellRepository.Get(id);
        }

        public async Task<(IEnumerable<SellerInformation>, int)> GetAllListingByCoordinates(double coordinatex, double coordinatey, int page, int pageSize)
        {
            return await _sellRepository.GetAllListingByCoordinates(coordinatex, coordinatey, page, pageSize);
        }

        private async Task<IEnumerable<ValidationResult>> CheckDuplicateListing(SellerInformation sellerInformation)
        {
            var results = new List<ValidationResult>();

            var existingEntities = await _sellRepository.GetAllByAddress(sellerInformation.StreetAddress, sellerInformation.City, sellerInformation.ZipCode, sellerInformation.CountryCode, sellerInformation.State);

            if (existingEntities.Any() && existingEntities.Any(x => x.Id  != sellerInformation.Id))
            {
                results.Add(new ValidationResult ("Listing with same Address (combination of Street Address, Zip Code, City, State and Country) already exists. You will not be allowed to add the same again!" ));
            }

            return results;
        }
    }
}
