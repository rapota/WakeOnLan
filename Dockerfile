FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY ./WakeOnLan.sln ./WakeOnLan.sln
COPY ./WakeOnLan/WakeOnLan.csproj ./WakeOnLan/WakeOnLan.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore --configuration Release --framework netcoreapp3.1 --output out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
EXPOSE 7/udp
EXPOSE 9/udp
COPY --from=build /app/WakeOnLan/out ./
ENTRYPOINT ["WakeOnLan.exe"]
