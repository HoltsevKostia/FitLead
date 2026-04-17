FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FitLead/FitLead.Common/FitLead.Common.csproj FitLead/FitLead.Common/
COPY FitLead/FitLead.Domain/FitLead.Domain.csproj FitLead/FitLead.Domain/
COPY FitLead/FitLead.Application/FitLead.Application.csproj FitLead/FitLead.Application/
COPY FitLead/FitLead.Infrastructure/FitLead.Infrastructure.csproj FitLead/FitLead.Infrastructure/
COPY FitLead/FitLead.Api/FitLead.Api.csproj FitLead/FitLead.Api/

RUN dotnet restore FitLead/FitLead.Api/FitLead.Api.csproj

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
