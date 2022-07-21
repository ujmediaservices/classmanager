#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClassManagerAPI/ClassManagerAPI.csproj", "ClassManagerAPI/"]
RUN dotnet restore "ClassManagerAPI/ClassManagerAPI.csproj"
COPY . .
WORKDIR "/src/ClassManagerAPI"
RUN dotnet build "ClassManagerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClassManagerAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClassManagerAPI.dll"]