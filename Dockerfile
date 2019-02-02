FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /app
COPY . .
RUN dotnet publish --configuration Release --framework netcoreapp2.2 --output out

FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app
EXPOSE 7/udp
EXPOSE 9/udp
COPY --from=build /app/WakeOnLan/out ./
ENTRYPOINT ["dotnet", "WakeOnLan.dll"]
