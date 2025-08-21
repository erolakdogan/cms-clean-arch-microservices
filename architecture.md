# CMS PoC – Mimarî Özeti

## Amaç

- 2 mikroservis (.NET 8): **UserService** (kimlik & kullanıcı), **ContentService** (içerik).
- **PostgreSQL** (ayrı DB’ler), **Redis** (okuma önbelleği), **Docker Compose** (tek komutla çalıştırma).
- **CQRS + MediatR**, **FluentValidation**, **API Versioning**, **Swagger/OpenAPI**.
- **JWT**: UserService üretir/validasyon, ContentService doğrular; servisler arası iletişim **typed HttpClient + Polly**.
- Gözlemlenebilirlik: **Serilog → Loki → Grafana**.
- 
## Yüksek Seviye Mimarî (Mermaid)

---
config:
  layout: dagre
---

flowchart LR
    Client[("Swagger/Postman")] --> US["UserService.Api"] & CS["ContentService.Api"]
    US --> PGU[("Postgres: users")]
    CS --> PGC[("Postgres: contents")]
    US <--> RD[("Redis")]
    CS <--> RD
    CS -- UsersClient + JWT --> US
    US -- Serilog --> LOKI[("Loki")]
    LOKI --> GF["Grafana"]
    CS -- Serilog --> LOKI

