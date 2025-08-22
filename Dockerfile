# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and project
COPY *.sln ./
COPY SoloBill/SoloBill.csproj SoloBill/
RUN dotnet restore

# copy everything and publish
COPY . .
RUN dotnet publish SoloBill/SoloBill.csproj -c Release -o /app/out

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Render requires apps to listen on port 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SoloBill.dll"]