FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /app
COPY ./WakeOnLan.sln ./WakeOnLan.sln
COPY ./WakeOnLan/WakeOnLan.csproj ./WakeOnLan/WakeOnLan.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish --no-restore --configuration Release --framework netcoreapp2.2 --output out

FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app
EXPOSE 7/udp
EXPOSE 9/udp
COPY --from=build /app/WakeOnLan/out ./
ENTRYPOINT ["dotnet", "WakeOnLan.dll"]
