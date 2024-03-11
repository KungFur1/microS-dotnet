* Info about dotnet installation: `dotnet --info`
* Template projects list: `dotnet new list`
* Create a solution file: `dotnet new sln`
* Create a webapi template project: `dotnet new webapi -o src/MyService`
* Add a project to the solution file: `dotnet sln add src/MyService`

- Start the project, with reload when files change: `dotnet watch`

* View dotnet global installed tools: `dotnet tool list -g`
* Install Entity Framework tools: `dotnet tool install dotnet-ef -g`
* Update Entity Framework tools: `dotnet tool update dotnet-ef -g`

- Create a migration: `dotnet ef migrations add "InitialCreate" -o Migrations`
- Apply migration to the database: `dotnet ef database update`
- Drop database: `dotnet ef database drop`

* Run docker-compose.yml in detached mode (without logs in the console): `docker compose up -d`

- Initialize a git repository: `git init`
- Create a .gitignore file for your solution: `dotnet new gitignore`

* Create a .NET class library: `dotnet new classlib -o src/Contracts`
* Add reference to other projects: `dotnet add reference ../../src/Contracts/`

- Install Duende Identity Server Templates: `dotnet new --install Duende.IdentityServer.Templates`
- Create the Identity Server template project: `dotnet new isaspid -o src/IdentityService`