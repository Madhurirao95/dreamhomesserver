using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DREAMHOMES.Controllers.DTOs.SellerInformation;
using DREAMHOMES.Services.Interfaces;
using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;

namespace DREAMHOMES.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class BuyController : Controller
    {
        private readonly ILogger<BuyController> _logger;
        private readonly IMapper _mapper;
        private readonly ISellService _service;

        public BuyController(ILogger<BuyController> logger, IMapper mapper, ISellService sellService) { 
            _logger = logger;
            _mapper = mapper;
            _service = sellService;
        }

        /// <summary>
        /// Gets all the Listings by the co-ordinates of a Location
        /// </summary>
        /// <returns>Result containing the Listings around the co-ordinates.</returns>
        [HttpGet("getAllListingByCoordinates")]
        public async Task<IActionResult> GetAllListingByCoordinates(double coordinatex, double coordinatey, int page, int pageSize)
        {
            var results = new List<SellerInformationDetailedGetDTO>();

            var models = await _service.GetAllListingByCoordinates(coordinatex, coordinatey, page, pageSize);

            foreach (var listing in models.Item1)
            {
                var dto = _mapper.Map<SellerInformationDetailedGetDTO>(listing);
                dto.DocumentList = new List<DocumentLiteDTO>();

                foreach (var document in listing.Documents)
                {
                    string filePath = document.FilePath;
                    string fileType = document.FileType;
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                    var documentDto = new DocumentLiteDTO
                    {
                        DocumentBase64 = Convert.ToBase64String(fileBytes),
                        DocumentType = fileType
                    };

                    dto.DocumentList.Add(documentDto);
                }
                results.Add(dto);
            }

            _logger.LogInformation("Listing fetch by co-ordinates successful!");
            var totalCount = models.Item2;
            return Ok(new { results, totalCount });
        }
    }
}
