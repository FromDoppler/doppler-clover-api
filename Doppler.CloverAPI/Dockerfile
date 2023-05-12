#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim AS build
WORKDIR /src
COPY ["Doppler.CloverAPI/Doppler.CloverAPI.csproj", "Doppler.CloverAPI/"]
RUN dotnet restore "Doppler.CloverAPI/Doppler.CloverAPI.csproj"
COPY . .
WORKDIR "/src/Doppler.CloverAPI"
RUN dotnet build "Doppler.CloverAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Doppler.CloverAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Doppler.CloverAPI.dll"]
