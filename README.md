# Cache Service

This project uses `IDistributedCache` within a cache service to expose some utility methods.

For more complex scenarios, consider using a specialized library like [FusionCache](https://github.com/ZiggyCreatures/FusionCache).

Please note, this project is just a sample and is not production-ready.

## Distributed Cache

When working with `IDistributedCache` in a .NET application, you can choose from various implementations available in .NET 8, including:
- Memory Cache
- Redis
- SQL Server
- NCache
- Azure CosmosDB

This project is configured to work with Redis if the Redis configuration is present in the app settings. Otherwise, it defaults to using an in-memory `DistributedCache`.

## Project Structure

This project has the following structure:
- **docker**: Contains a Docker Compose file with a configured Redis container.
- **src**: Contains the project source code.
- **test**: Contains the project tests.