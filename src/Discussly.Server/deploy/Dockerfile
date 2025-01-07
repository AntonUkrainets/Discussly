FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln ./
COPY Discussly.Server/*.csproj ./Discussly.Server/
COPY Discussly.Server.Data/*.csproj ./Discussly.Server.Data/
COPY Discussly.Server.Infrastructure/*.csproj ./Discussly.Server.Infrastructure/
COPY Discussly.Server.SharedKernel/*.csproj ./Discussly.Server.SharedKernel/

RUN dotnet restore "Discussly.Server/Discussly.Server.csproj"
COPY . ./

RUN dotnet build "Discussly.Server/Discussly.Server.csproj" -c Debug -o /app/build
RUN dotnet publish "Discussly.Server/Discussly.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu-core \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Discussly.Server.dll"]