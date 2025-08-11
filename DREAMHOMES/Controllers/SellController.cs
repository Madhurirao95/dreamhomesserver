using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DREAMHOMES.Controllers.DTOs.SellerInformation;
using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;
using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;
using System.Security.Claims;

namespace DREAMHOMES.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SellController : Controller
    {
        private readonly ILogger<SellController> _logger;
        private readonly IMapper _mapper;
        private readonly ISellService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public SellController(ILogger<SellController> logger, IMapper mapper, ISellService sellService, UserManager<IdentityUser> userManager) { 
            _logger = logger;
            _mapper = mapper;
            _service = sellService;
            _userManager = userManager;
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
            AssignDocuments(sellerInformationPostPutDTO, sellerInformation, email);

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

            var existingSellerInformation = await _service.GetSellerInformationById(id);

            if (existingSellerInformation == null)
            {
                return new BadRequestObjectResult(new { Message = "Oops! The Listing being updated is not found. Please check the id."});
            }

            // get and assign new documents.
            existingSellerInformation.Documents.Clear();
            AssignDocuments(sellerInformationPostPutDTO, existingSellerInformation, existingSellerInformation.User.Email);

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
        /// Gets all the Listing posted by the <see cref="IdentityUser"/>
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
        [Authorize]
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

        private async void AssignDocuments(SellerInformationPostPutDTO sellerInformationPostPutDTO, SellerInformation sellerInformation, string email)
        {
            sellerInformation.Documents = new List<Document>();
            foreach (var file in sellerInformationPostPutDTO.Documents)
            {
                var basePath = Path.Combine("Documents");
                bool basePathExists = Directory.Exists(basePath);

                if (!basePathExists)
                {
                    Directory.CreateDirectory(basePath);
                }

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(basePath, file.FileName);
                var extension = Path.GetExtension(file.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }


                var document = new Document
                {
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    Name = file.FileName,
                    FilePath = filePath,
                    FileType = file.ContentType,
                    Extension = extension,
                    Size = file.Length,
                    AuthorName = email
                };
                sellerInformation.Documents.Add(document);
            }
        }
    }
}
