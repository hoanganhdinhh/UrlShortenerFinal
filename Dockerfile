# ===== Base runtime (Linux) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Render cung cấp biến PORT (thường 10000). Bind Kestrel vào biến này.
ENV ASPNETCORE_URLS=http://+:$PORT
# (Expose chỉ mang tính thông tin, không bắt buộc)
EXPOSE 8080

# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj và restore theo đúng cấu trúc repo
COPY ["UrlShortener.MVC/UrlShortener.MVC.csproj", "UrlShortener.MVC/"]
# Nếu thực sự có project services, giữ đúng tên thư mục & csproj
# COPY ["UrlShortener.Services/UrlShortener.Services.csproj", "UrlShortener.Services/"]

RUN dotnet restore "UrlShortener.MVC/UrlShortener.MVC.csproj"

# Copy toàn bộ để build
COPY . .
WORKDIR "/src/UrlShortener.MVC"
RUN dotnet build "UrlShortener.MVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UrlShortener.MVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.MVC.dll"]
