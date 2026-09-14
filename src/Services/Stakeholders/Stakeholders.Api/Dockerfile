FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/ src/
RUN dotnet restore "src/Services/Stakeholders/Stakeholders.Api/Stakeholders.Api.csproj"
RUN dotnet publish "src/Services/Stakeholders/Stakeholders.Api/Stakeholders.Api.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Stakeholders.Api.dll"]
