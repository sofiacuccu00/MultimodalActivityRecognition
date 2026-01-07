using System.Collections.Generic;
using System.Threading.Tasks;
using MARecognition.Controllers;
using MARecognition.Interfaces;
using MARecognition.Miscellaneuous;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MARecognition.Tests.Controllers
{
    public class FusionControllerTests
    {
        [Fact]
        public async Task Fuse_ReturnsBadRequest_WhenPredictionsAreMissing() // Test for missing predictions
        {
            // Arrange
            var mockService = new FusionServiceMock(new FusionResult());
            var controller = new FusionController(mockService);

            var request = new FusionRequest
            {
                Context = "Test",
                Predictions = null // Missing predictions
            };

            // Act
            var result = await controller.Fuse(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No predictions provided.", badRequest.Value);
        }

        [Fact]
        public async Task Fuse_ReturnsOk_WithFusionResult() // Test for successful fusion
        {
            // Arrange
            var expected = new FusionResult
            {
                FinalAction = "walking",
                Reasoning = "Vision and IMU agree."
            };

            var mockService = new FusionServiceMock(expected);
            var controller = new FusionController(mockService);

            var request = new FusionRequest
            {
                Context = "Outdoor",
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "walking" },
                    { "imu", "walking" }
                }
            };

            // Act
            var result = await controller.Fuse(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<FusionResult>(ok.Value);

            Assert.Equal("walking", response.FinalAction);
            Assert.Equal("Vision and IMU agree.", response.Reasoning);
        }

        [Fact]
        public async Task Fuse_PassesRequestToService() // Test to ensure the controller passes the request correctly to the service
        {
            // Arrange
            var mockService = new FusionServiceMock(new FusionResult());
            var controller = new FusionController(mockService);

            var request = new FusionRequest
            {
                Context = "Indoor",
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "standing" }
                }
            };

            // Act
            await controller.Fuse(request);

            // Assert
            Assert.NotNull(mockService.CapturedRequest);
            Assert.Equal("Indoor", mockService.CapturedRequest.Context);
            Assert.Equal("standing", mockService.CapturedRequest.Predictions["vision"]);
        }


        [Fact]
        public async Task Fuse_ReturnsBadRequest_WhenContextIsMissing() // Additional test for missing context
        {
            var mockService = new FusionServiceMock(new FusionResult());
            var controller = new FusionController(mockService);

            var request = new FusionRequest
            {
                Context = null,
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "walking" }
                }
            };

            var result = await controller.Fuse(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Context is required.", badRequest.Value);
        }

        [Fact]
        public async Task Fuse_ReturnsBadRequest_WhenContextIsEmpty() // Additional test for empty context
        {
            var mockService = new FusionServiceMock(new FusionResult());
            var controller = new FusionController(mockService);

            var request = new FusionRequest
            {
                Context = "",
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "walking" }
                }
            };

            var result = await controller.Fuse(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Context is required.", badRequest.Value);
        }


    }
}
