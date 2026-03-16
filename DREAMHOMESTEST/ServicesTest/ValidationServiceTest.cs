using DREAMHOMES.Services;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMESTEST.ServicesTest
{
    [TestFixture]
    public class ValidationServiceTest
    {
        private ValidationService _validationService;

        [SetUp]
        public void SetUp()
        {
            _validationService = new ValidationService();
        }

        [Test]
        public async Task ValidateAll_WithNoValidationMethods_ShouldReturnEmptyList()
        {
            // ACT
            var results = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task Add_WithValidationMethod_ShouldAddMethod()
        {
            // ARRANGE
            var validationCalled = false;
            _validationService.Add(async () =>
            {
                validationCalled = true;
                return new List<ValidationResult>();
            });

            // ACT
            await _validationService.ValidateAll();

            // ASSERT
            Assert.That(validationCalled, Is.True);
        }

        [Test]
        public async Task ValidateAll_WithSingleValidationMethod_ShouldReturnResults()
        {
            // ARRANGE
            var validationResults = new List<ValidationResult>
            {
                new ValidationResult("Error 1")
            };

            _validationService.Add(async () => validationResults);

            // ACT
            var results = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().ErrorMessage, Is.EqualTo("Error 1"));
        }

        [Test]
        public async Task ValidateAll_WithMultipleValidationMethods_ShouldReturnAllResults()
        {
            // ARRANGE
            var results1 = new List<ValidationResult> { new ValidationResult("Error 1") };
            var results2 = new List<ValidationResult> { new ValidationResult("Error 2"), new ValidationResult("Error 3") };
            var results3 = new List<ValidationResult> { new ValidationResult("Error 4") };

            _validationService.Add(async () => results1);
            _validationService.Add(async () => results2);
            _validationService.Add(async () => results3);

            // ACT
            var allResults = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(allResults.Count(), Is.EqualTo(4));
            var errorMessages = allResults.Select(r => r.ErrorMessage).ToList();
            Assert.That(errorMessages, Does.Contain("Error 1"));
            Assert.That(errorMessages, Does.Contain("Error 2"));
            Assert.That(errorMessages, Does.Contain("Error 3"));
            Assert.That(errorMessages, Does.Contain("Error 4"));
        }

        [Test]
        public async Task ValidateAll_WithValidationMethodReturningEmptyList_ShouldHandleCorrectly()
        {
            // ARRANGE
            _validationService.Add(async () => new List<ValidationResult>());
            _validationService.Add(async () => new List<ValidationResult> { new ValidationResult("Error 1") });
            _validationService.Add(async () => new List<ValidationResult>());

            // ACT
            var results = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().ErrorMessage, Is.EqualTo("Error 1"));
        }

        [Test]
        public async Task ValidateAll_ShouldInvokeAllMethodsInOrder()
        {
            // ARRANGE
            var callOrder = new List<int>();

            _validationService.Add(async () =>
            {
                callOrder.Add(1);
                return new List<ValidationResult>();
            });

            _validationService.Add(async () =>
            {
                callOrder.Add(2);
                return new List<ValidationResult>();
            });

            _validationService.Add(async () =>
            {
                callOrder.Add(3);
                return new List<ValidationResult>();
            });

            // ACT
            await _validationService.ValidateAll();

            // ASSERT
            Assert.That(callOrder, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ValidateAll_WhenValidationMethodThrows_ShouldPropagateException()
        {
            // ARRANGE
            _validationService.Add(async () => throw new InvalidOperationException("Validation error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _validationService.ValidateAll());
        }

        [Test]
        public async Task ValidateAll_WhenOneMethodThrows_ShouldNotInvokeSubsequentMethods()
        {
            // ARRANGE
            var secondMethodCalled = false;

            _validationService.Add(async () => throw new InvalidOperationException("Error"));

            _validationService.Add(async () =>
            {
                secondMethodCalled = true;
                return new List<ValidationResult>();
            });

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _validationService.ValidateAll());
            Assert.That(secondMethodCalled, Is.False);
        }

        [Test]
        public async Task Add_ShouldNotInvokeMethodImmediately()
        {
            // ARRANGE
            var methodInvoked = false;

            // ACT
            _validationService.Add(async () =>
            {
                methodInvoked = true;
                return new List<ValidationResult>();
            });

            // ASSERT - Method should not be called yet
            Assert.That(methodInvoked, Is.False);

            // Now call ValidateAll to confirm it gets invoked
            await _validationService.ValidateAll();
            Assert.That(methodInvoked, Is.True);
        }

        [Test]
        public async Task ValidateAll_WithMultipleCalls_ShouldExecuteMethodsMultipleTimes()
        {
            // ARRANGE
            var callCount = 0;

            _validationService.Add(async () =>
            {
                callCount++;
                return new List<ValidationResult>();
            });

            // ACT
            await _validationService.ValidateAll();
            var firstCallCount = callCount;
            
            await _validationService.ValidateAll();
            var secondCallCount = callCount;

            // ASSERT
            Assert.That(firstCallCount, Is.EqualTo(1));
            Assert.That(secondCallCount, Is.EqualTo(2));
        }

        [Test]
        public async Task ValidateAll_WithMultipleMethods_ShouldCombineAllValidationResults()
        {
            // ARRANGE
            var method1Results = new List<ValidationResult>
            {
                new ValidationResult("First error", new[] { "Field1" })
            };

            var method2Results = new List<ValidationResult>
            {
                new ValidationResult("Second error", new[] { "Field2" }),
                new ValidationResult("Third error", new[] { "Field3" })
            };

            _validationService.Add(async () => method1Results);
            _validationService.Add(async () => method2Results);

            // ACT
            var results = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(3));
            var resultsList = results.ToList();
            Assert.That(resultsList[0].ErrorMessage, Is.EqualTo("First error"));
            Assert.That(resultsList[1].ErrorMessage, Is.EqualTo("Second error"));
            Assert.That(resultsList[2].ErrorMessage, Is.EqualTo("Third error"));
        }

        [Test]
        public async Task ValidateAll_WithAsyncValidationMethod_ShouldHandleAsyncOperations()
        {
            // ARRANGE
            _validationService.Add(async () =>
            {
                await Task.Delay(10); // Simulate async operation
                return new List<ValidationResult> { new ValidationResult("Async error") };
            });

            // ACT
            var results = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().ErrorMessage, Is.EqualTo("Async error"));
        }

        [Test]
        public async Task ValidateAll_CanBeCalledMultipleTimesWithDifferentResults()
        {
            // ARRANGE
            var errorCount = 0;

            _validationService.Add(async () =>
            {
                errorCount++;
                return Enumerable.Range(1, errorCount)
                    .Select(i => new ValidationResult($"Error {i}"))
                    .ToList() as IEnumerable<ValidationResult>;
            });

            // ACT
            var firstResults = await _validationService.ValidateAll();
            var secondResults = await _validationService.ValidateAll();

            // ASSERT
            Assert.That(firstResults.Count(), Is.EqualTo(1));
            Assert.That(secondResults.Count(), Is.EqualTo(2));
        }
    }
}
