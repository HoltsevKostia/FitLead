using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.ClientBodyMetrics;

public sealed class BodyMetricValidationTests(IntegrationTestFixture fixture)
    : BodyMetricsTestBase(fixture)
{
    [Fact]
    public async Task CreateEmptyEntry_ShouldReturnValidationError()
    {
        var setup = await CreateClientWithMetricsAsync("body-metrics-empty");

        var response = await setup.Metrics.CreateAsync(
            new DateOnly(2026, 5, 20));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("body_metric_entry.create.empty_entry");
    }

    [Theory]
    [InlineData(NumericField.WeightKg, 0, "body_metric_entry.create.weight_kg_out_of_range")]
    [InlineData(NumericField.BodyFatPercent, 81, "body_metric_entry.create.body_fat_percent_out_of_range")]
    [InlineData(NumericField.ChestCm, 0, "body_metric_entry.create.chest_cm_out_of_range")]
    [InlineData(NumericField.WaistCm, 301, "body_metric_entry.create.waist_cm_out_of_range")]
    [InlineData(NumericField.HipsCm, 0, "body_metric_entry.create.hips_cm_out_of_range")]
    [InlineData(NumericField.ArmCm, 301, "body_metric_entry.create.arm_cm_out_of_range")]
    [InlineData(NumericField.ThighCm, 0, "body_metric_entry.create.thigh_cm_out_of_range")]
    public async Task CreateWithInvalidNumericValue_ShouldReturnValidationError(
        NumericField field,
        decimal value,
        string expectedErrorCode)
    {
        var setup = await CreateClientWithMetricsAsync($"body-metrics-invalid-{field}");

        var response = field switch
        {
            NumericField.WeightKg => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), weightKg: value),
            NumericField.BodyFatPercent => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), bodyFatPercent: value),
            NumericField.ChestCm => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), chestCm: value),
            NumericField.WaistCm => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), waistCm: value),
            NumericField.HipsCm => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), hipsCm: value),
            NumericField.ArmCm => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), armCm: value),
            NumericField.ThighCm => await setup.Metrics.CreateAsync(new DateOnly(2026, 5, 20), thighCm: value),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be(expectedErrorCode);
    }

    public enum NumericField
    {
        WeightKg,
        BodyFatPercent,
        ChestCm,
        WaistCm,
        HipsCm,
        ArmCm,
        ThighCm
    }
}
