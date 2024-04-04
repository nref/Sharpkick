using SharpKick;
using SharpKick.Infra;
using Typin;

await new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .UseDescription("Automation for Gigabyte monitors")
    .UseLamar(services => new CompositionRoot().ApplyTo(services))
    .Build()
    .RunAsync();
