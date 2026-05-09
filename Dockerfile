FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SkilloPlatform/ ./SkilloPlatform/
COPY SkilloPlatform.Tests/ ./SkilloPlatform.Tests/
COPY Skillo.sln ./
RUN dotnet restore SkilloPlatform/SkilloPlatform.csproj
RUN dotnet publish SkilloPlatform/SkilloPlatform.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "SkilloPlatform.dll"]
