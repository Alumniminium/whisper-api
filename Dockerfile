# herstfortress/speech2text:latest

FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy as runtime
WORKDIR /app
COPY --from=build /app/Assets/ /app/Assets
COPY --from=build /app/published-app /app

RUN apt update -y && apt install ffmpeg -y

ENTRYPOINT [ "dotnet", "/app/speech2text.dll" ]
