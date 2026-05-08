using FitLead.Domain.Trainings;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Trainings;

public sealed class MediaUrlTests
{
    [Theory]
    [InlineData("ftp://example.com/video.mp4")]
    [InlineData("file:///C:/videos/squat.mp4")]
    [InlineData("javascript:alert(1)")]
    public void Create_WithUnsupportedScheme_ShouldReturnFailure(string value)
    {
        var result = MediaUrl.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("media_url.invalid_scheme");
    }
}
