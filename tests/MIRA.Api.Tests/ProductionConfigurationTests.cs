using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MIRA.Api.Configuracion;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class ProductionConfigurationTests
{
    private sealed class Host : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "ConfigurationTest";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    [Theory]
    [InlineData("SSL Mode=Prefer")]
    [InlineData("SSL Mode=Require")]
    [InlineData("SSL Mode=VerifyFull;Channel Binding=Prefer")]
    [InlineData("SSL Mode=VerifyFull;Channel Binding=Require;Include Error Detail=true")]
    public void ProductionRejectsUnverifiedTlsOrDetailedSqlErrors(string options)
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["ConnectionStrings:PostgreSql"]="Host=example.invalid;Database=unused;Username=unused;Password=unused;"+options }).Build();
        Assert.Throws<InvalidOperationException>(()=>new DbConnectionFactory(config,new Host()));
    }
    [Fact]
    public void ProductionAcceptsVerifiedTlsConfigurationWithoutConnecting()
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["ConnectionStrings:PostgreSql"]="Host=example.invalid;Database=unused;Username=unused;Password=unused;SSL Mode=VerifyFull;Channel Binding=Require" }).Build();
        var factory=new DbConnectionFactory(config,new Host());
        using var connection=factory.CreateConnection();
        Assert.Equal(System.Data.ConnectionState.Closed,connection.State);
    }
}
