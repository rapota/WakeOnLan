FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /app
COPY ./WakeOnLan.sln ./WakeOnLan.sln
COPY ./WakeOnLan/WakeOnLan.csproj ./WakeOnLan/WakeOnLan.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore --configuration Release --output out

FROM microsoft/dotnet:3.0-runtime AS runtime
WORKDIR /app
EXPOSE 7/udp
EXPOSE 9/udp
COPY --from=build /app/WakeOnLan/out ./
ENTRYPOINT ["WakeOnLan.exe"]
