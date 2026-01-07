using System.Collections.Generic;
using MARecognition.Miscellaneuous;
using MARecognition.Services;
using Xunit;
namespace MARecognition.Tests.Fusion
{
    public class FusionServiceTests
    {
        [Fact]
        public async Task FuseAsync_ReturnsCorrectAction_WhenJsonIsValid() // tests normal operation with valid JSON
        {
            // Arrange
            var mockResponse = "{ \"action\": \"walking\", \"reasoning\": \"Two sensors agree.\" }";
            var mockSk = new SemanticKernelServiceMock(mockResponse);
            var service = new FusionService(mockSk);

            var request = new FusionRequest
            {
                Context = "User is outside",
                Predictions = new Dictionary<string, string>
            {
                { "vision", "walking" },
                { "imu", "walking" },
                { "audio", "running" }
            }
            };

            // Act
            var result = await service.FuseAsync(request);

            // Assert
            Assert.Equal("walking", result.FinalAction);
            Assert.Equal("Two sensors agree.", result.Reasoning);
        }

        [Fact]
        public async Task FuseAsync_ReturnsUnknown_WhenJsonIsMalformed() // tests handling of malformed JSON
        {
            // Arrange
            var mockResponse = "this is not json";
            var mockSk = new SemanticKernelServiceMock(mockResponse);
            var service = new FusionService(mockSk);

            var request = new FusionRequest
            {
                Context = "Test context",
                Predictions = new Dictionary<string, string>
            {
                { "vision", "sitting" }
            }
            };

            // Act
            var result = await service.FuseAsync(request);

            // Assert
            Assert.Equal("unknown", result.FinalAction);
            Assert.Contains("unparsable", result.Reasoning.ToLower());
        }

        [Fact]
        public async Task FuseAsync_SendsCorrectPrompt_ToSemanticKernel() // tests that the prompt is constructed correctly
        {
            // Arrange
            string capturedPrompt = "";
            var mockSk = new SemanticKernelServiceMock("{\"action\":\"test\",\"reasoning\":\"ok\"}")
            {
                // Override AskAsync to capture the prompt since it disappears into the LLM normally
                AskAsyncOverride = (prompt) =>
                {
                    capturedPrompt = prompt;
                    return Task.FromResult("{\"action\":\"test\",\"reasoning\":\"ok\"}");
                }
            };

            var service = new FusionService(mockSk);

            var request = new FusionRequest
            {
                Context = "Indoor environment",
                Predictions = new Dictionary<string, string>
            {
                { "vision", "standing" },
                { "imu", "walking" }
            }
            };

            // Act
            await service.FuseAsync(request);

            // Assert
            Assert.Contains("Indoor environment", capturedPrompt);
            Assert.Contains("vision: standing", capturedPrompt);
            Assert.Contains("imu: walking", capturedPrompt);
            Assert.Contains("Respond ONLY in this JSON format", capturedPrompt);
        }


        [Fact]
        public async Task FuseAsync_ParsesJsonCorrectly() // tests that the parsing logic works correctly
        {
            // Arrange
            var mockResponse = "{ \"action\": \"running\", \"reasoning\": \"Audio strongly indicates running.\" }";
            var mockSk = new SemanticKernelServiceMock(mockResponse);
            var service = new FusionService(mockSk);

            var request = new FusionRequest
            {
                Context = "Test context",
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "walking" }
                }
            };

            // Act
            var result = await service.FuseAsync(request);

            // Assert
            Assert.Equal("running", result.FinalAction);
            Assert.Equal("Audio strongly indicates running.", result.Reasoning);
        }

        [Fact]
        public async Task FuseAsync_ReturnsFinalFusedAction() // tests that the fusion logic works end-to-end
        {
            // Arrange
            var mockResponse = "{ \"action\": \"standing\", \"reasoning\": \"Vision and IMU agree.\" }";
            var mockSk = new SemanticKernelServiceMock(mockResponse);
            var service = new FusionService(mockSk);

            var request = new FusionRequest
            {
                Context = "Indoor",
                Predictions = new Dictionary<string, string>
                {
                    { "vision", "standing" },
                    { "imu", "standing" },
                    { "audio", "walking" }
                }
            };

            // Act
            var result = await service.FuseAsync(request);

            // Assert
            Assert.Equal("standing", result.FinalAction);
            Assert.Equal("Vision and IMU agree.", result.Reasoning);
        }


    }
}
