using System;
using Microsoft.Extensions.DependencyInjection;
using PropTechIALegalGuardAPI.Infrastructure.DependencyInjection;

var services = new ServiceCollection();
services.AddPropTechIALegalGuardAPIClients(new Uri("https://localhost"));

using var provider = services.BuildServiceProvider();

Console.WriteLine("Api client generated and ready. Configure real BaseAddress real nefore use.");
