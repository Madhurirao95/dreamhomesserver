using AutoMapper;
using DREAMHOMES.Models;
using DREAMHOMES.Models.Repository.Interfaces;
using DREAMHOMES.Services;
using DREAMHOMES.Services.Interfaces;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMESTEST.ServicesTest
{
    /// <summary>
    /// Unit tests for SellService class.
    /// Tests cover property listing operations including creation, updates, retrieval, and validation.
    /// </summary>
    [TestFixture]
    public class SellServiceTest
    {
        private Mock<ISellRepository> _mockSellRepository;
        private Mock<IValidationService> _mockValidationService;
        private Mock<IMapper> _mockMapper;
        private SellService _sellService;

        [SetUp]
        public void SetUp()
        {
            // Initialize mocks for all dependencies
            _mockSellRepository = new Mock<ISellRepository>();
            _mockValidationService = new Mock<IValidationService>();
            _mockMapper = new Mock<IMapper>();

            // Create SellService instance with mocked dependencies
            _sellService = new SellService(
                _mockSellRepository.Object,
                _mockValidationService.Object,
                _mockMapper.Object
            );
        }

        #region PostListing Tests

        /// <summary>
        /// Test: PostListing should return empty results when validation passes and listing is added successfully.
        /// Scenario: New property listing with no duplicates
        /// Expected: Listing added to repository, no validation errors returned
        /// </summary>
        [Test]
        public async Task PostListing_WithValidSellerInformation_ShouldAddListingAndReturnNoErrors()
        {
            // ARRANGE
            var sellerInfo = CreateValidSellerInformation();

            // Setup validation service to return no errors
            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>());

            _mockSellRepository
                .Setup(r => r.Add(It.IsAny<SellerInformation>()))
                .Returns(Task.CompletedTask);

            // ACT
            var results = await _sellService.PostListing(sellerInfo);

            // ASSERT
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(0), "Expected no validation errors");
            _mockSellRepository.Verify(r => r.Add(It.IsAny<SellerInformation>()), Times.Once, 
                "Repository.Add should be called exactly once");
        }

        /// <summary>
        /// Test: PostListing should NOT add listing when validation fails.
        /// Scenario: Duplicate property listing exists
        /// Expected: Listing NOT added to repository, validation error returned
        /// </summary>
        [Test]
        public async Task PostListing_WithDuplicateListing_ShouldReturnValidationErrorAndNotAddListing()
        {
            // ARRANGE
            var sellerInfo = CreateValidSellerInformation();
            var duplicateListing = CreateValidSellerInformation();
            duplicateListing.Id = 1;

            // Setup repository to return existing duplicate
            _mockSellRepository
                .Setup(r => r.GetAllByAddress(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<SellerInformation.States>()))
                .ReturnsAsync(new List<SellerInformation> { duplicateListing });

            // Setup validation service to collect validation results
            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>
                {
                    new ValidationResult("Listing with same Address already exists")
                });

            // ACT
            var results = await _sellService.PostListing(sellerInfo);

            // ASSERT
            Assert.That(results.Count(), Is.GreaterThan(0), "Expected validation errors");
            Assert.That(results.First().ErrorMessage, 
                Does.Contain("Listing with same Address"), "Error message should indicate duplicate");
            _mockSellRepository.Verify(r => r.Add(It.IsAny<SellerInformation>()), Times.Never, 
                "Repository.Add should NOT be called when validation fails");
        }

        /// <summary>
        /// Test: PostListing should handle repository exceptions gracefully.
        /// Scenario: Database error occurs during save
        /// Expected: Exception is thrown to caller
        /// </summary>
        [Test]
        public void PostListing_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var sellerInfo = CreateValidSellerInformation();

            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>());

            _mockSellRepository
                .Setup(r => r.Add(It.IsAny<SellerInformation>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sellService.PostListing(sellerInfo),
                "Should propagate repository exception");
        }

        #endregion

        #region UpdateListing Tests

        /// <summary>
        /// Test: UpdateListing should successfully update listing when validation passes.
        /// Scenario: Valid update with no conflicts
        /// Expected: Listing updated in repository, no validation errors
        /// </summary>
        [Test]
        public async Task UpdateListing_WithValidSellerInformation_ShouldUpdateListingAndReturnNoErrors()
        {
            // ARRANGE
            var existingListing = CreateValidSellerInformation();
            existingListing.Id = 1;

            var updatedListing = CreateValidSellerInformation();
            updatedListing.Id = 1;
            updatedListing.Description = "Updated description";

            // Setup mapper to map updated data
            _mockMapper
                .Setup(m => m.Map(updatedListing, existingListing))
                .Returns(updatedListing);

            // Setup validation to pass
            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>());

            _mockSellRepository
                .Setup(r => r.Update(It.IsAny<SellerInformation>()))
                .Returns(Task.CompletedTask);

            // ACT
            var results = await _sellService.UpdateListing(existingListing, updatedListing);

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(0), "Expected no validation errors");
            _mockSellRepository.Verify(r => r.Update(It.IsAny<SellerInformation>()), Times.Once,
                "Repository.Update should be called exactly once");
            _mockMapper.Verify(m => m.Map(updatedListing, existingListing), Times.Once,
                "Mapper should be called to map updated data");
        }

        /// <summary>
        /// Test: UpdateListing should NOT update listing when validation fails.
        /// Scenario: Update creates duplicate address
        /// Expected: Listing NOT updated, validation error returned
        /// </summary>
        [Test]
        public async Task UpdateListing_WithDuplicateAddress_ShouldReturnValidationErrorAndNotUpdate()
        {
            // ARRANGE
            var existingListing = CreateValidSellerInformation();
            existingListing.Id = 1;

            var updatedListing = CreateValidSellerInformation();
            updatedListing.Id = 1;

            var conflictingListing = CreateValidSellerInformation();
            conflictingListing.Id = 2; // Different ID = conflict

            _mockMapper
                .Setup(m => m.Map(updatedListing, existingListing))
                .Returns(updatedListing);

            // Setup repository to find conflicting listing
            _mockSellRepository
                .Setup(r => r.GetAllByAddress(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<SellerInformation.States>()))
                .ReturnsAsync(new List<SellerInformation> { conflictingListing });

            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>
                {
                    new ValidationResult("Address already in use by another listing")
                });

            // ACT
            var results = await _sellService.UpdateListing(existingListing, updatedListing);

            // ASSERT
            Assert.That(results.Count(), Is.GreaterThan(0), "Expected validation errors");
            _mockSellRepository.Verify(r => r.Update(It.IsAny<SellerInformation>()), Times.Never,
                "Repository.Update should NOT be called when validation fails");
        }

        /// <summary>
        /// Test: UpdateListing should allow update when address belongs to same listing (ID match).
        /// Scenario: Updating existing address for same property
        /// Expected: Update allowed and executed
        /// </summary>
        [Test]
        public async Task UpdateListing_SameListingAddress_ShouldAllowUpdate()
        {
            // ARRANGE
            var existingListing = CreateValidSellerInformation();
            existingListing.Id = 1;

            var sameListing = CreateValidSellerInformation();
            sameListing.Id = 1; // Same ID = no conflict

            var updatedListing = CreateValidSellerInformation();
            updatedListing.Id = 1;

            _mockMapper
                .Setup(m => m.Map(updatedListing, existingListing))
                .Returns(updatedListing);

            // Repository returns the same listing (no other conflicts)
            _mockSellRepository
                .Setup(r => r.GetAllByAddress(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<SellerInformation.States>()))
                .ReturnsAsync(new List<SellerInformation> { sameListing });

            _mockValidationService
                .Setup(v => v.ValidateAll())
                .ReturnsAsync(new List<ValidationResult>());

            _mockSellRepository
                .Setup(r => r.Update(It.IsAny<SellerInformation>()))
                .Returns(Task.CompletedTask);

            // ACT
            var results = await _sellService.UpdateListing(existingListing, updatedListing);

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(0), "Expected no errors when updating same address");
            _mockSellRepository.Verify(r => r.Update(It.IsAny<SellerInformation>()), Times.Once,
                "Update should be allowed for same listing ID");
        }

        #endregion

        #region GetAllListingBySeller Tests

        /// <summary>
        /// Test: GetAllListingBySeller should return all listings for a user.
        /// Scenario: User has multiple property listings
        /// Expected: All listings returned
        /// </summary>
        [Test]
        public async Task GetAllListingBySeller_WithValidUserId_ShouldReturnAllUserListings()
        {
            // ARRANGE
            var userId = "user123";
            var listings = new List<SellerInformation>
            {
                CreateValidSellerInformation(),
                CreateValidSellerInformation(),
                CreateValidSellerInformation()
            };

            _mockSellRepository
                .Setup(r => r.GetAllListingBySeller(userId))
                .ReturnsAsync(listings);

            // ACT
            var result = await _sellService.GetAllListingBySeller(userId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3), "Expected all three listings");
            _mockSellRepository.Verify(r => r.GetAllListingBySeller(userId), Times.Once,
                "Repository should be called with correct user ID");
        }

        /// <summary>
        /// Test: GetAllListingBySeller should return empty list when user has no listings.
        /// Scenario: New user with no properties
        /// Expected: Empty collection returned
        /// </summary>
        [Test]
        public async Task GetAllListingBySeller_WithNoListings_ShouldReturnEmptyList()
        {
            // ARRANGE
            var userId = "newuser";
            _mockSellRepository
                .Setup(r => r.GetAllListingBySeller(userId))
                .ReturnsAsync(new List<SellerInformation>());

            // ACT
            var result = await _sellService.GetAllListingBySeller(userId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0), "Expected empty list");
        }

        /// <summary>
        /// Test: GetAllListingBySeller should handle repository errors.
        /// Scenario: Database unavailable
        /// Expected: Exception propagated
        /// </summary>
        [Test]
        public void GetAllListingBySeller_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var userId = "user123";
            _mockSellRepository
                .Setup(r => r.GetAllListingBySeller(userId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sellService.GetAllListingBySeller(userId));
        }

        #endregion

        #region GetSellerInformationById Tests

        /// <summary>
        /// Test: GetSellerInformationById should return listing when it exists.
        /// Scenario: Valid listing ID
        /// Expected: Listing returned with all details
        /// </summary>
        [Test]
        public async Task GetSellerInformationById_WithValidId_ShouldReturnListing()
        {
            // ARRANGE
            var listingId = 1;
            var listing = CreateValidSellerInformation();
            listing.Id = listingId;

            _mockSellRepository
                .Setup(r => r.Get(listingId))
                .ReturnsAsync(listing);

            // ACT
            var result = await _sellService.GetSellerInformationById(listingId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(listingId));
            _mockSellRepository.Verify(r => r.Get(listingId), Times.Once,
                "Repository should be called with correct ID");
        }

        /// <summary>
        /// Test: GetSellerInformationById should return null when listing doesn't exist.
        /// Scenario: Non-existent listing ID
        /// Expected: Null returned
        /// </summary>
        [Test]
        public async Task GetSellerInformationById_WithInvalidId_ShouldReturnNull()
        {
            // ARRANGE
            var listingId = 999;
            _mockSellRepository
                .Setup(r => r.Get(listingId))
                .ReturnsAsync((SellerInformation)null);

            // ACT
            var result = await _sellService.GetSellerInformationById(listingId);

            // ASSERT
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// Test: GetSellerInformationById should handle repository errors.
        /// Scenario: Database connection lost
        /// Expected: Exception propagated
        /// </summary>
        [Test]
        public void GetSellerInformationById_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var listingId = 1;
            _mockSellRepository
                .Setup(r => r.Get(listingId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sellService.GetSellerInformationById(listingId));
        }

        #endregion

        #region GetAllListingByCoordinates Tests

        /// <summary>
        /// Test: GetAllListingByCoordinates should return paginated results near location.
        /// Scenario: Valid coordinates with results
        /// Expected: Correct listings and total count returned
        /// </summary>
        [Test]
        public async Task GetAllListingByCoordinates_WithValidCoordinates_ShouldReturnPaginatedResults()
        {
            // ARRANGE
            double latitude = 40.7128;
            double longitude = -74.0060;
            int page = 0;
            int pageSize = 10;

            var listings = new List<SellerInformation>
            {
                CreateValidSellerInformation(),
                CreateValidSellerInformation()
            };
            var totalCount = 2;

            _mockSellRepository
                .Setup(r => r.GetAllListingByCoordinates(latitude, longitude, page, pageSize))
                .ReturnsAsync((listings as IEnumerable<SellerInformation>, totalCount));

            // ACT
            var (results, count) = await _sellService.GetAllListingByCoordinates(latitude, longitude, page, pageSize);

            // ASSERT
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(2), "Expected 2 listings");
            Assert.That(count, Is.EqualTo(2), "Expected total count of 2");
            _mockSellRepository.Verify(
                r => r.GetAllListingByCoordinates(latitude, longitude, page, pageSize),
                Times.Once);
        }

        /// <summary>
        /// Test: GetAllListingByCoordinates should return empty results when no listings near location.
        /// Scenario: Area with no properties
        /// Expected: Empty collection with count 0
        /// </summary>
        [Test]
        public async Task GetAllListingByCoordinates_WithNoResultsNearby_ShouldReturnEmpty()
        {
            // ARRANGE
            double latitude = 40.7128;
            double longitude = -74.0060;
            int page = 0;
            int pageSize = 10;

            _mockSellRepository
                .Setup(r => r.GetAllListingByCoordinates(latitude, longitude, page, pageSize))
                .ReturnsAsync((new List<SellerInformation>() as IEnumerable<SellerInformation>, 0));

            // ACT
            var (results, count) = await _sellService.GetAllListingByCoordinates(latitude, longitude, page, pageSize);

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(0), "Expected empty results");
            Assert.That(count, Is.EqualTo(0), "Expected count of 0");
        }

        /// <summary>
        /// Test: GetAllListingByCoordinates should handle pagination correctly.
        /// Scenario: Request page 2 with 5 items per page
        /// Expected: Correct page returned with correct count
        /// </summary>
        [Test]
        public async Task GetAllListingByCoordinates_WithPagination_ShouldReturnCorrectPage()
        {
            // ARRANGE
            double latitude = 40.7128;
            double longitude = -74.0060;
            int page = 1; // Page 2 (0-indexed)
            int pageSize = 5;

            var listings = new List<SellerInformation>
            {
                CreateValidSellerInformation(),
                CreateValidSellerInformation(),
                CreateValidSellerInformation()
            };
            var totalCount = 13; // 13 total, so page 2 has 3 items

            _mockSellRepository
                .Setup(r => r.GetAllListingByCoordinates(latitude, longitude, page, pageSize))
                .ReturnsAsync((listings as IEnumerable<SellerInformation>, totalCount));

            // ACT
            var (results, count) = await _sellService.GetAllListingByCoordinates(latitude, longitude, page, pageSize);

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(3), "Expected 3 items on page 2");
            Assert.That(count, Is.EqualTo(13), "Expected total count of 13");
        }

        /// <summary>
        /// Test: GetAllListingByCoordinates should handle repository errors.
        /// Scenario: Spatial query fails
        /// Expected: Exception propagated
        /// </summary>
        [Test]
        public void GetAllListingByCoordinates_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            _mockSellRepository
                .Setup(r => r.GetAllListingByCoordinates(It.IsAny<double>(), It.IsAny<double>(),
                    It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Spatial query error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sellService.GetAllListingByCoordinates(40.7128, -74.0060, 0, 10));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a valid SellerInformation object for testing.
        /// </summary>
        private SellerInformation CreateValidSellerInformation()
        {
            return new SellerInformation
            {
                Id = 0,
                StreetAddress = "123 Main Street",
                City = "New York",
                State = SellerInformation.States.NY,
                ZipCode = "10001",
                CountryCode = "US",
                BedRooms = 3,
                BathRooms = 2,
                Area = 1500,
                YearBuilt = 2020,
                Type = SellerInformation.ListingType.House,
                BuildingType = SellerInformation.TypeOfBuilding.Resale,
                ListingPrice = 500000,
                HasGarage = true,
                NumberOfGarageSpace = 2,
                HasPool = false,
                HasFirePlace = true,
                NumberOfFirePlace = 1,
                HOA = 0,
                LotArea = 5000,
                LotAreaUnit = SellerInformation.AreaUnits.SqFt,
                AmountPerSqFt = 333.33,
                Description = "Beautiful property",
                Unit = null,
                Properties = "Hardwood floors, renovated kitchen",
                UserId = "user123",
                Status = SellerInformation.ListingStatus.Active,
                Location = null
            };
        }

        #endregion
    }
}
