# CMS Clean Architecture Microservices (User + Content)

PoC: Clean Architecture .NET 8 microservices (User/Content) + PostgreSQL + Docker +  Redis + Loki + Grafana 

- 2 mikroservis (.NET 8): **UserService** (kimlik & kullanıcı), **ContentService** (içerik).
- **PostgreSQL** (ayrı DB’ler), **Redis** (okuma önbelleği), **Docker Compose** (tek komutla çalıştırma).
- **CQRS + MediatR**, **FluentValidation**, **API Versioning**, **Swagger/OpenAPI**.
- **JWT**: UserService üretir/validasyon, ContentService doğrular; servisler arası iletişim **typed HttpClient + Polly**.
- Gözlemlenebilirlik: **Serilog → Loki → Grafana**.

Bu proje, **.NET 8** üzerinde geliştirilmiş, mikroservis mimarisi ile tasarlanmış bir içerik yönetim sistemi örneğidir.
Bir içerik yönetim sistemi (CMS) ürünü base mimari yapı olarak kullanılabilir. İki mikroservis, (UserService, ContentService) içeren **Clean Architecture**   prensipleriyle
tasarlanmış case çalışmasıdır. 2 Ayri veritabani (**Postgres**), cashe yapisi için **Redis**, loglama ve monitör etmek için **Loki ve Grafana** ile birlikte geliştirilmiş, 
**Docker Compose** üzerinden  tek bir komut ile run edilebilir. 

> Operasyon kılavuzu: **[docs/operations.md](docs/operations.md)**

> Postman Hazır Collection: **[docs/postman_collection.json](docs/postman_collection.json)**

> Context Diyagram: **[docs/system-context-diagram.png](docs/system-context-diagram.png)**

## Teknolojiler
- **.NET 8**, .Net, MediatR, FluentValidation,Mapperly
- **EF Core** (PostgreSQL), InMemory (test)
- **Serilog** + **Grafana Loki** (loglama,monitor)
- **Redis** (caching)
- **Docker Compose**
- **Swagger / OpenAPI 3**
- **xUnit, FluentAssertions, Moq** (test)

## Modüller
- `UserService`: Kimlik doğrulama (JWT), kullanıcı CRUD, internal “brief” endpoint.
- `ContentService`: İçerik CRUD, UserService’den yazar bilgisini S2S çağrısıyla zenginleştirir.
- `BuildingBlocks/Shared.Web`: Ortak middleware, caching, security yardımcıları.
- `docker/`: postgres/redis/loki/grafana ayarları ve compose dosyaları.
- `docs/`: operasyon ve mimari dokümanları.
- `tests/`: Unit/Integration testleri.

## Hızlı Başlangıç
1) `.env` dosyasını proje köküne ekleyin (örnek için operations.md’ye bakın).  
2) Çalıştırın:
```bash
docker compose -f docker/docker-compose.yml --env-file .env up --build -d
```
3) Swagger:
- UserService → `http://localhost:7146/swagger`
- ContentService → `http://localhost:7099/swagger`

## Testler
```bash
dotnet test
```
> Postman Hazır Collection: **[docs/postman_collection.json](docs/postman_collection.json)**
> Context Diyagram: **[docs/system-context-diagram.png](docs/system-context-diagram.png)**

## Mimari
- Clean Arch (Api / Application / Infrastructure / Domain)
- CQRS (MediatR), Validasyon (FluentValidation),Mapping (Mapperly) Cross-cutting (Serilog, Middleware)
- S2S güvenlik: aynı JWT ayarlarıyla servisler arası minimal “brief” çağrıları

Detay için: **docs/architecture.md**


