FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY FitLead/FitLead.Common/FitLead.Common.csproj FitLead/FitLead.Common/
COPY FitLead/FitLead.Common/packages.lock.json FitLead/FitLead.Common/
COPY FitLead/FitLead.Domain/FitLead.Domain.csproj FitLead/FitLead.Domain/
COPY FitLead/FitLead.Domain/packages.lock.json FitLead/FitLead.Domain/
COPY FitLead/FitLead.Application/FitLead.Application.csproj FitLead/FitLead.Application/
COPY FitLead/FitLead.Application/packages.lock.json FitLead/FitLead.Application/
COPY FitLead/FitLead.Infrastructure/FitLead.Infrastructure.csproj FitLead/FitLead.Infrastructure/
COPY FitLead/FitLead.Infrastructure/packages.lock.json FitLead/FitLead.Infrastructure/
COPY FitLead/FitLead.Api/FitLead.Api.csproj FitLead/FitLead.Api/
COPY FitLead/FitLead.Api/packages.lock.json FitLead/FitLead.Api/

RUN dotnet restore FitLead/FitLead.Api/FitLead.Api.csproj --locked-mode

COPY FitLead/ FitLead/

RUN dotnet publish FitLead/FitLead.Api/FitLead.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitLead.Api.dll"]
