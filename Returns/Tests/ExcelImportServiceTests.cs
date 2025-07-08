using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.Helpers.Excel;
using Returns.Helpers.Excel.Configurations;
using Returns.Interfaces;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Returns.Tests
{
    public class ExcelImportServiceTests
    {
        private readonly Mock<ILogger<ExcelImportService>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IFormProcessorFactory> _processorFactoryMock;
        private readonly ExcelImportService _excelImportService;

        public ExcelImportServiceTests()
        {
            _loggerMock = new Mock<ILogger<ExcelImportService>>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _processorFactoryMock = new Mock<IFormProcessorFactory>();
            
            _excelImportService = new ExcelImportService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                _processorFactoryMock.Object);
        }

        [Fact]
        public async Task ImportFormAsync_WithInvalidFile_ReturnsValidationError()
        {
            // Arrange
            var file = CreateMockFormFile("test.txt", "Invalid content");
            var formType = "CAPITAL_ADEQUACY";
            var returnId = "RET-2024-001";

            // Act
            var result = await _excelImportService.ImportFormAsync(file, formType, returnId);

            // Assert
            Assert.Equal(SubmissionStatus.ValidationError, result.Status);
            Assert.Contains("Invalid file format", result.Messages[0]);
        }

        [Fact]
        public async Task ImportFormAsync_WithValidFile_CallsProcessor()
        {
            // Arrange
            var file = CreateMockFormFile("test.xlsx", "Excel content");
            var formType = "CAPITAL_ADEQUACY";
            var returnId = "RET-2024-001";
            
            var processorMock = new Mock<IFormProcessor>();
            processorMock.Setup(p => p.ProcessAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(new FormProcessResult
                {
                    Success = true,
                    Messages = new List<string> { "Processed successfully" },
                    FormId = "FORM-001"
                });

            _processorFactoryMock.Setup(f => f.GetProcessor(formType))
                .Returns(processorMock.Object);

            // Act
            var result = await _excelImportService.ImportFormAsync(file, formType, returnId);

            // Assert
            Assert.Equal(SubmissionStatus.Success, result.Status);
            Assert.Equal("FORM-001", result.Data);
            processorMock.Verify(p => p.ProcessAsync(file, returnId), Times.Once);
        }

        [Fact]
        public async Task ParseFormAsync_WithConfiguration_ParsesCorrectly()
        {
            // Arrange
            var configuration = new TestFormConfiguration();
            var file = CreateMockExcelFile();

            // Act
            var result = await _excelImportService.ParseFormAsync(file, configuration);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CS/123/2024", result.SaccoCsNumber);
            Assert.Equal("2024-Q1", result.Period);
        }

        private IFormFile CreateMockFormFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
                .Returns(Task.CompletedTask);

            return fileMock.Object;
        }

        private IFormFile CreateMockExcelFile()
        {
            // In a real test, you would create an actual Excel file
            // For now, we'll simulate it
            return CreateMockFormFile("test.xlsx", "Excel content");
        }
    }

    // Test configuration
    public class TestFormConfiguration : BaseFormConfiguration<TestFormDto>
    {
        public override string FormType => "TEST_FORM";
        public override int DataStartRow => 5;
        public override int DataEndRow => 10;
        public override IRowMapper<TestFormDto> RowMapper => new TestRowMapper();
    }

    public class TestFormDto : BaseFormStatement
    {
        public string TestData { get; set; }
    }

    public class TestRowMapper : IRowMapper<TestFormDto>
    {
        public TestFormDto MapRow(IXLRow row, int rowNumber)
        {
            return new TestFormDto
            {
                TestData = row.Cell("A").Value.ToString()
            };
        }

        public bool ShouldSkipRow(IXLRow row, int rowNumber)
        {
            return string.IsNullOrWhiteSpace(row.Cell("A").Value.ToString());
        }
    }
}