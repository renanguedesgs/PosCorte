#!/bin/sh
export ASPNETCORE_URLS="http://+:${PORT:-8080}"
exec dotnet PosCorte.API.dll
