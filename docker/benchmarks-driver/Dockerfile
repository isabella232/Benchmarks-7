FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# copy everything
COPY . ./

# build
WORKDIR /app/src/BenchmarksDriver
RUN dotnet publish -c Release -o /app/out

# create runtime image
FROM microsoft/dotnet:runtime
WORKDIR /app
COPY --from=build-env /app/out ./
ENTRYPOINT ["dotnet", "BenchmarksDriver.dll", "--trace-output", "/traces"]
