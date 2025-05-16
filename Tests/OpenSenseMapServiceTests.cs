using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenSenseMapApiService.Models;
using OpenSenseMapProxyApi.Services;
using Xunit;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;


public class OpenSenseMapServiceTests
{
    [Fact]
    public async Task RegisterUserAsync_ReturnsSuccessMessage()
    {
        var responseContent = "{\"message\":\"User registered successfully\"}";
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(handler.Object);

        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });

        var service = new OpenSenseMapService(httpClient, optionsMock.Object);
  

        var result = await service.RegisterUserAsync(new OpenSenseMapUser
        {
            name = "testuser",
            email = "test@example.com",
            password = "Password123"
        });

        Assert.Contains("User registered successfully", result);
    }


    [Fact]
    public async Task CreateSenseBoxAsync_ShouldReturnSuccessResponse()
    {
        // Arrange
        var jwtToken = "mock-jwt-token";
        var request = new NewSenseBoxRequestDto
        {
            Name = "TestBox",
            Exposure = "outdoor",
            Model = "homeV2",
            Location = new LocationDto { Latitude = 10.0, Longitude = 20.0 },
            Sensors = new List<SensorDto>
            {
                new SensorDto
                {
                    Title = "Temperature",
                    Unit = "°C",
                    SensorType = "DHT22",
                    Icon = "thermometer"
                }
            }
        };

        var expectedResponse = new NewSenseBoxResponseDto
        {
            Id = "abc123",
            Message = "SenseBox successfully created"
        };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/json"),
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });

        var service = new OpenSenseMapService(httpClient, optionsMock.Object);

        // Act
        var result = await service.CreateSenseBoxAsync(request, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc123", result.Id);
        Assert.Equal("SenseBox successfully created", result.Message);
    }

    [Fact]
    public async Task CreateSenseBoxAsync_ShouldThrowException_OnErrorResponse()
    {
        // Arrange
        var jwtToken = "mock-jwt-token";
        var request = new NewSenseBoxRequestDto
        {
            Name = "InvalidBox",
            Exposure = "outdoor",
            Location = new LocationDto { Latitude = 10.0, Longitude = 20.0 },
            Sensors = new List<SensorDto>()
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.BadRequest,
               Content = new StringContent("Invalid request", Encoding.UTF8, "application/json")
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });

        var service = new OpenSenseMapService(httpClient, optionsMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateSenseBoxAsync(request, jwtToken));

        Assert.Contains("SenseBox creation failed", ex.Message);
    }
    [Fact]
    public async Task GetSenseBoxByIdAsync_ShouldReturnSenseBox_WhenBoxExists()
    {
        // Arrange
        var boxId = "1234567890abcdef";
        var expectedBox = new SenseBoxResponseDto
        {
            _id = boxId,
            Name = "Test Box",
            Exposure = "outdoor",
            Model = "homeV2",
            Location = new LocationResponseDto { Coordinates = new[] { 13.405, 52.52 } },
            Sensors = new List<SensorDto>
        {
            new SensorDto
            {
                Title = "Temperature",
                Unit = "°C",
                SensorType = "DHT22",
                Icon = "thermometer"
            }
        }
        };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(JsonSerializer.Serialize(expectedBox), Encoding.UTF8, "application/json")
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });
        var service = new OpenSenseMapService(httpClient, optionsMock.Object);
        // Act
        var result = await service.GetSenseBoxByIdAsync(boxId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boxId, result._id);
        Assert.Equal("Test Box", result.Name);
        Assert.Equal("outdoor", result.Exposure);
        Assert.Equal("homeV2", result.Model);
        Assert.NotNull(result.Location);
        Assert.Equal(2, result.Location.Coordinates.Length);
    }

    [Fact]
    public async Task GetSenseBoxByIdAsync_ShouldThrowException_OnErrorResponse()
    {
        // Arrange
        var boxId = "nonexistent-id";

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.NotFound,
               Content = new StringContent("Box not found")
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });
        var service = new OpenSenseMapService(httpClient, optionsMock.Object);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetSenseBoxByIdAsync(boxId));
        Assert.Contains("Failed to retrieve SenseBox", ex.Message);
    }
    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_OnSuccess()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });
        var service = new OpenSenseMapService(httpClient, optionsMock.Object);
        var result = await service.LogoutAsync("mock-jwt");

        Assert.True(result);
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrowException_OnFailure()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.Unauthorized,
               Content = new StringContent("Invalid token")
           });

        var httpClient = new HttpClient(handlerMock.Object);
        var optionsMock = new Mock<IOptions<OpenSenseMapOptions>>();
        optionsMock.Setup(opt => opt.Value).Returns(new OpenSenseMapOptions
        {
            BaseUrl = "https://api.opensensemap.org/"
        });
        var service = new OpenSenseMapService(httpClient, optionsMock.Object);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.LogoutAsync("bad-token"));

        Assert.Contains("Logout failed", ex.Message);
    }
}