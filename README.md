# SOA_RA1392022

Tourist tour platform, built as a set of .NET microservices behind an API gateway.

## Architecture

| Service | Responsibility | Database | Port (local) |
|---|---|---|---|
| Gateway | Ocelot API gateway, single entry point | — | 5000 |
| Stakeholders.Api | Users, roles, auth (register/login/JWT), profiles | MongoDB | 5001 |
| Blog.Api | Blogs, comments | MongoDB | 5002 |
| Followers.Api | Follow graph, feed filtering, recommendations | Neo4j | 5003 |
| Tour.Api | Tours, key points, position simulator, tour execution | MongoDB | 5004 |
| Purchase.Api | Shopping cart, checkout, purchase tokens | MongoDB | 5005 |

### RPC (gRPC)

- `Blog.Api` → `Followers.Api`: `IsFollowing` check before a comment is allowed.
- `Tour.Api` → `Purchase.Api`: `IsPurchased` check before a tour execution can start.

Proto contracts live in `src/Shared/Protos`.

## Running

```
docker compose up --build
```

This starts MongoDB, Neo4j, all five services, and the gateway. All traffic goes
through the gateway at `http://localhost:5000`.

Services are also individually reachable on their local ports for debugging
(see table above), each exposing `GET /health`.

## Solution structure

```
src/
  Gateway/                 Ocelot API gateway
  Shared/Protos/           gRPC contracts shared by client/server projects
  Services/
    Stakeholders/Stakeholders.Api/
    Blog/Blog.Api/
    Followers/Followers.Api/
    Tour/Tour.Api/
    Purchase/Purchase.Api/
```

## Status

Initial scaffold: project wiring, database connections, auth, and the core
entities/endpoints for requirements 1, 4, 5, 6, 9, 10, 11, 14, 16, 17 are in
place. Image upload handling (local file storage) is not yet implemented —
`ImagePath`/`ImagePaths` fields currently expect a path string.
