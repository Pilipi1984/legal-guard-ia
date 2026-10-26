# La imagen oficial de Playwright ya incluye Chromium y sus dependencias del sistema.
# Mantén la versión del tag alineada con la del paquete Microsoft.Playwright.
FROM mcr.microsoft.com/playwright/dotnet:v1.49.0-noble AS runtime
WORKDIR /app
COPY publish/ .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PropTechIALegalGuardAPI.Api.dll"]
