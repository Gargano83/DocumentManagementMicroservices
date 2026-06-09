using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.CreateQuote
{
    public class CreateQuoteCommandValidatorTests
    {
        private readonly CreateQuoteCommandValidator _validator;

        public CreateQuoteCommandValidatorTests()
        {
            _validator = new CreateQuoteCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            var command = new CreateQuoteCommand(CustomerId: "CUST-123", ValidityDays: 30);

            // Act
            var result = _validator.Validate(instance: command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", 30)]
        [InlineData(null, 30)]
        public void Validate_WithEmptyCustomerId_ShouldHaveError(string customerId, int validityDays)
        {
            // Arrange
            var command = new CreateQuoteCommand(CustomerId: customerId, ValidityDays: validityDays);

            // Act
            var result = _validator.Validate(instance: command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuoteCommand.CustomerId));
        }

        [Theory]
        [InlineData("CUST-123", 0)]
        [InlineData("CUST-123", -5)]
        public void Validate_WithInvalidValidityDays_ShouldHaveError(string customerId, int validityDays)
        {
            // Arrange
            var command = new CreateQuoteCommand(CustomerId: customerId, ValidityDays: validityDays);

            // Act
            var result = _validator.Validate(instance: command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateQuoteCommand.ValidityDays));
        }
    }
}
