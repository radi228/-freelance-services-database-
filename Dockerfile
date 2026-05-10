FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore SkilloPlatform/SkilloPlatform.csproj
RUN dotnet publish SkilloPlatform/SkilloPlatform.csproj -c Release -o /skillo

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /skillo
COPY --from=build /skillo .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
CMD ["dotnet", "/skillo/SkilloPlatform.dll"]
