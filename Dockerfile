# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj & restore first (better layer caching)
COPY SoloBill/*.csproj SoloBill/
RUN dotnet restore SoloBill/SoloBill.csproj

# copy the rest and publish
COPY . .
RUN dotnet publish SoloBill/SoloBill.csproj -c Release -o /app/out

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render provides $PORT. Tell Kestrel to listen on it.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

COPY --from=build /app/out ./
CMD ["dotnet", "SoloBill.dll"]