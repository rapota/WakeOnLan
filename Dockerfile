FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app
COPY ./WakeOnLan.sln ./WakeOnLan.sln
COPY ./WakeOnLan/WakeOnLan.csproj ./WakeOnLan/WakeOnLan.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore --configuration Release --output out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
EXPOSE 7/udp
EXPOSE 9/udp
COPY --from=build /app/WakeOnLan/out ./
ENTRYPOINT ["WakeOnLan.exe"]
