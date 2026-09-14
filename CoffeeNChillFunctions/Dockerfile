FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish CoffeeNChillFunctions.csproj -c Release -o /home/site/wwwroot

FROM base AS final
COPY --from=build /home/site/wwwroot /home/site/wwwroot