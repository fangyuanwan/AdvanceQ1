
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 5000

ENV RABBITMQ_HOST localhost
ENV RABBITMQ_PORT 5672
ENV RABBITMQ_USER guest
ENV RABBITMQ_PASSWORD guest
ENV RABBITMQ_VHOST /

ENV ASPNETCORE_URLS=http://+:5000

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["TaskManagement.csproj", "./"]
RUN dotnet restore "TaskManagement.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TaskManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.dll"]
