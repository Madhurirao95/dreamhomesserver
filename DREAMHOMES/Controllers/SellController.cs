using AutoMapper;
using DREAMHOMES.Controllers.DTOs.SellerInformation;
using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;
using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DREAMHOMES.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SellController : Controller
    {
        private readonly ILogger<SellController> _logger;
        private readonly IValidator<SellerInformation> _validator;
        private readonly IMapper _mapper;
        private readonly ISellService _service;
        private readonly IDocumentService _documentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellController(ILogger<SellController> logger, IMapper mapper, ISellService sellService, IDocumentService documentService
            , IValidator<SellerInformation> validator, UserManager<ApplicationUser> userManager) { 
            _logger = logger;
            _mapper = mapper;
            _validator = validator;
            _service = sellService;
            _userManager = userManager;
            _documentService = documentService;
        }

        /// <summary>
        /// Posts a listing with all provided data.
        /// </summary>
        /// <param name="sellerInformationPostPutDTO">Represents data to be posted.</param>
        /// <returns>Result of the posting.</returns>
        [Authorize]
        [HttpPost("postListing")]
        public async Task<IActionResult> PostListing([FromForm] SellerInformationPostPutDTO sellerInformationPostPutDTO)
        {
            if (sellerInformationPostPutDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Oops! Cannot Post your Listing right now! Please try again!" });
            }

            var sellerInformation = _mapper.Map<SellerInformation>(sellerInformationPostPutDTO);

            var validationResult = await _validator.ValidateAsync(sellerInformation);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // get and assign user identity.
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var x = await _userManager.FindByNameAsync(email);

                if (x != null) { 
                    sellerInformation.UserId = x.Id;
                }
            }
            // get and assign documents.
            await this._documentService.AssignDocuments(sellerInformationPostPutDTO.Documents, email, sellerInformation);

            var errorResult = await _service.PostListing(sellerInformation);

            if (errorResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                _logger.LogInformation("Listing cannot be posted due to some errors!");
                return BadRequest(errorResult.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage));
            }

            _logger.LogInformation("Listing posted successfully!");

            return Ok();
        }

        /// <summary>
        /// Updates a listing with all provided data.
        /// </summary>
        /// <param name="sellerInformationPostPutDTO">Represents data to be updated.</param>
        /// <returns>Result of the posting.</returns>
        [Authorize]
        [HttpPut("updateListing/{id}")]
        public async Task<IActionResult> UpdateListing(int id, [FromForm] SellerInformationPostPutDTO sellerInformationPostPutDTO)
        {
            if (sellerInformationPostPutDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Oops! Cannot Post your Listing right now! Please try again!" });
            }

            var sellerInformation = _mapper.Map<SellerInformation>(sellerInformationPostPutDTO);

            var validationResult = await _validator.ValidateAsync(sellerInformation);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var existingSellerInformation = await _service.GetSellerInformationById(id);

            if (existingSellerInformation == null)
            {
                return new BadRequestObjectResult(new { Message = "Oops! The Listing being updated is not found. Please check the id."});
            }

            // get and assign new documents.
            await this._documentService.AssignDocuments(sellerInformationPostPutDTO.Documents, existingSellerInformation.User.Email, existingSellerInformation);

            var errorResult = await _service.UpdateListing(existingSellerInformation, sellerInformation);

            if (errorResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                _logger.LogInformation("Listing cannot be updated due to some errors!");
                return BadRequest(errorResult.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage));
            }

            _logger.LogInformation("Listing updated successfully!");

            return Ok();
        }

        /// <summary>
        /// Gets all the Listing posted by the <see cref="ApplicationUser"/>
        /// </summary>
        /// <returns>Result containing the List of Postings.</returns>
        [Authorize]
        [HttpGet("getAllListing")]
        public async Task<IActionResult> GetAllListingBySeller()
        {
            var results = new List<SellerInformationLiteGetDTO>();
            var allListings = new List<SellerInformation>();

            // get the current user identity.
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var x = await _userManager.FindByNameAsync(email);

                if (x != null)
                {
                    allListings.AddRange(await _service.GetAllListingBySeller(x.Id));
                }
            }

            foreach (var listing in allListings)
            {
                var document = listing.Documents.First();
                string filePath = document.FilePath;
                string fileType = document.FileType;
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

               
                var dto = _mapper.Map<SellerInformationLiteGetDTO>(listing);
                var documentDto = new DocumentLiteDTO();
                documentDto.DocumentBase64 = Convert.ToBase64String(fileBytes);
                documentDto.DocumentType = fileType;

                dto.RandomDocument = documentDto;

                results.Add(dto);
            }
            _logger.LogInformation("Listing fetch successful!");

            return Ok(results);
        }

        /// <summary>
        /// Gets the <see cref="SellerInformation"/> by ID.
        /// </summary>
        /// <param name="id">The id of the <see cref="SellerInformation"/>to get the details.</param>
        /// <returnsThe details of the <see cref="SellerInformation"/>></returns>
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetSellerInformationById(int id)
        {
            var result = await _service.GetSellerInformationById(id);
            var resultDto = _mapper.Map<SellerInformationDetailedDTO>(result);

            var documentDtoList = new List<DocumentLiteDTO>();
            foreach (var document in result.Documents)
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(document.FilePath);

                var documentDto = new DocumentLiteDTO();
                documentDto.DocumentBase64 = Convert.ToBase64String(fileBytes);
                documentDto.DocumentType = document.FileType;
                documentDto.DocumentName = document.Name + document.Extension;

                documentDtoList.Add(documentDto);
            }

            resultDto.DocumentList = documentDtoList;

            return Ok(resultDto);
        }
    }
}
