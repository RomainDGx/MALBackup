# syntax=docker/dockerfile:1

# Create a stage for building the application.
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy source and dependencies.
COPY MALBackup.App/ MALBackup.App/
COPY MALBackup.Model/ MALBackup.Model/

# Publish the application.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish MALBackup.App/MALBackup.App.csproj --use-current-runtime -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

RUN mkdir /output
RUN chown $APP_UID /output
RUN mkdir /Logs
RUN chown $APP_UID /Logs

# Copy everything needed to run the app from the "build" stage.
COPY --from=build /app .

USER $APP_UID

ENTRYPOINT ["dotnet", "MALBackup.App.dll"]
