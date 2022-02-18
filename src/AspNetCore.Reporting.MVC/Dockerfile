#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ConnectionStrings:DefaultConnection="Data Source=Database/application.db"

# install System.Drawing native dependencies
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["AspNetCore.Reporting.MVC.csproj", "."]
COPY ["../AspNetCore.Reporting.Common/AspNetCore.Reporting.Common.csproj", "../AspNetCore.Reporting.Common/"]
RUN dotnet restore "./AspNetCore.Reporting.MVC.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AspNetCore.Reporting.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.Reporting.MVC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.Reporting.MVC.dll"]