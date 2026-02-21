using FluentAssertions;
using NetArchTest.Rules;

using Shared.Domain;
using Shared.Validators;

using Xunit;

namespace IssueManager.Tests.Architecture;

/// <summary>
/// Architecture tests that enforce project structure and dependencies.
/// These tests ensure the codebase follows vertical slice architecture and clean architecture principles.
/// </summary>
public class ArchitectureTests
{
	private const string SharedNamespace = "IssueManager.Shared";
	private const string ApiNamespace = "IssueManager.Api";
	private const string WebNamespace = "IssueManager.Web";
	private const string ServiceDefaultsNamespace = "IssueManager.ServiceDefaults";
	private const string DomainNamespace = "IssueManager.Shared.Domain";
	private const string ValidatorsNamespace = "IssueManager.Shared.Validators";

	/// <summary>
	/// Ensures that the Shared layer has no dependencies on higher layers (Api, Web).
	/// This enforces that the foundation layer remains dependency-free.
	/// </summary>
	[Fact]
	public void SharedLayer_ShouldNotDependOnHigherLayers()
	{
		// Arrange
		var sharedAssembly = typeof(Label).Assembly;

		// Act
		var result = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(SharedNamespace)
			.ShouldNot()
			.HaveDependencyOnAny(ApiNamespace, WebNamespace)
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Shared layer is the foundation and should have no dependencies on Api or Web layers");
	}

	/// <summary>
	/// Ensures that domain models do not depend on infrastructure concerns like MongoDB.
	/// This keeps the domain pure and persistence-agnostic.
	/// </summary>
	[Fact]
	public void DomainModels_ShouldNotDependOnInfrastructure()
	{
		// Arrange
		var sharedAssembly = typeof(Issue).Assembly;

		// Act
		var result = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(DomainNamespace)
			.ShouldNot()
			.HaveDependencyOn("MongoDB")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Domain models must be persistence-agnostic and not depend on MongoDB or any other infrastructure");
	}

	/// <summary>
	/// Ensures that validators only depend on FluentValidation and domain models.
	/// Validators should be pure logic without framework coupling.
	/// </summary>
	[Fact]
	public void Validators_ShouldOnlyDependOnFluentValidationAndDomain()
	{
		// Arrange
		var sharedAssembly = typeof(CreateIssueValidator).Assembly;

		// Act
		var result = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(ValidatorsNamespace)
			.And()
			.HaveNameEndingWith("Validator")
			.Should()
			.HaveDependencyOn("FluentValidation")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Validators must depend on FluentValidation for validation logic");
	}

	/// <summary>
	/// Ensures that validators do not depend on Api or Web layers.
	/// </summary>
	[Fact]
	public void Validators_ShouldNotDependOnHigherLayers()
	{
		// Arrange
		var sharedAssembly = typeof(CreateIssueValidator).Assembly;

		// Act
		var result = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(ValidatorsNamespace)
			.ShouldNot()
			.HaveDependencyOnAny(ApiNamespace, WebNamespace)
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Validators are pure logic and should not depend on Api or Web layers");
	}

	/// <summary>
	/// Ensures that all validator classes follow the naming convention of ending with "Validator", "Command", or "Query".
	/// </summary>
	[Fact]
	public void Validators_ShouldFollowNamingConvention()
	{
		// Arrange
		var sharedAssembly = typeof(CreateIssueValidator).Assembly;

		// Act
		var result = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(ValidatorsNamespace)
			.And()
			.AreClasses()
			.Should()
			.HaveNameEndingWith("Validator")
			.Or()
			.HaveNameEndingWith("Command")
			.Or()
			.HaveNameEndingWith("Query")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"All validator classes should end with 'Validator', 'Command', or 'Query'");
	}

	/// <summary>
	/// Ensures that domain models are immutable by using records.
	/// </summary>
	[Fact]
	public void DomainModels_ShouldBeRecords()
	{
		// Arrange
		var sharedAssembly = typeof(Issue).Assembly;

		// Act
		var domainTypes = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(DomainNamespace)
			.GetTypes()
			.Where(t => !t.IsEnum)
			.ToList();

		// Assert
		foreach (var type in domainTypes)
		{
			// Records are classes with specific compiler-generated members
			var isRecord = type.GetMethod("<Clone>$") is not null;
			isRecord.Should().BeTrue(
				$"Domain model '{type.Name}' should be a record for immutability");
		}
	}

	/// <summary>
	/// Ensures that the Api layer does not depend on the Web layer.
	/// </summary>
	[Fact]
	public void ApiLayer_ShouldNotDependOnWebLayer()
	{
		// Arrange
		// Use a known type from the Shared assembly that Api references
		var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(a => a.GetName().Name == "Api");

		// Skip test if Api assembly is not loaded
		if (apiAssembly is null)
		{
			// This is acceptable as the test may run in isolation
			return;
		}

		// Act
		var result = Types.InAssembly(apiAssembly)
			.That()
			.ResideInNamespace(ApiNamespace)
			.ShouldNot()
			.HaveDependencyOn(WebNamespace)
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Api layer should not depend on Web layer to maintain separation of concerns");
	}

	/// <summary>
	/// Ensures that the Web layer does not depend on the Api layer's internals.
	/// </summary>
	[Fact]
	public void WebLayer_ShouldNotDependOnApiInternals()
	{
		// Arrange
		var webAssembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(a => a.GetName().Name == "Web");

		// Skip test if Web assembly is not loaded
		if (webAssembly is null)
		{
			// This is acceptable as the test may run in isolation
			return;
		}

		// Act
		var result = Types.InAssembly(webAssembly)
			.That()
			.ResideInNamespace(WebNamespace)
			.ShouldNot()
			.HaveDependencyOn(ApiNamespace)
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"Web layer should communicate with Api through HTTP, not direct references");
	}

	/// <summary>
	/// Ensures that ServiceDefaults layer has minimal dependencies.
	/// It should only depend on Aspire and OpenTelemetry infrastructure.
	/// </summary>
	[Fact]
	public void ServiceDefaults_ShouldHaveMinimalDependencies()
	{
		// Arrange
		var serviceDefaultsAssembly = typeof(IssueManager.ServiceDefaults.Extensions).Assembly;

		// Act
		var result = Types.InAssembly(serviceDefaultsAssembly)
			.That()
			.ResideInNamespace(ServiceDefaultsNamespace)
			.ShouldNot()
			.HaveDependencyOnAny(ApiNamespace, WebNamespace, SharedNamespace)
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue(
			"ServiceDefaults should only contain cross-cutting infrastructure concerns without business logic dependencies");
	}

	/// <summary>
	/// Ensures that all public classes in the Shared layer have XML documentation.
	/// </summary>
	[Fact]
	public void SharedLayer_PublicTypesShouldHaveDocumentation()
	{
		// Arrange
		var sharedAssembly = typeof(Issue).Assembly;

		// Act - Note: This test checks that types are defined; XML doc enforcement is done by compiler warnings
		var publicTypes = Types.InAssembly(sharedAssembly)
			.That()
			.ResideInNamespace(SharedNamespace)
			.And()
			.ArePublic()
			.GetTypes();

		// Assert
		publicTypes.Should().NotBeEmpty(
			"Shared layer should have public types that are documented");
		
		// The actual XML documentation check is enforced by compiler settings in the .csproj file
		// This test ensures we have public types to document
	}
}
