# ビルド
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# 実行
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Linuxにライブラリをインストール
RUN apt-get update && apt-get install -y libkrb5-3

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "MyApi.dll"]