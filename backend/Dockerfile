# ビルド
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# 実行
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Linuxにライブラリをインストール
RUN apt-get update && apt-get install -y libkrb5-3 libgssapi-krb5-2 netcat-openbsd

WORKDIR /app
COPY --from=build /app .

# wait-for-it.sh をコピー
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

# ENTRYPOINT の行以外は ビルド時に実行される。
# ENTRYPOINT 行はコンテナが起動したときに実行される
# DB が起動するまで待ってから backend を起動
ENTRYPOINT ["/wait-for-it.sh", "db", "5432", "dotnet", "MyApi.dll"]

