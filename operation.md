# Operasyon Kılavuzu

Bu doküman, projeyi **tek komutla** Docker Compose üzerinde ayağa kaldırmak, token almak,
istek atmak, gözlemlemek (Grafana/Loki) ve sorun giderme adımlarını içerir.

> Not: Tüm komutlar depo kök dizininden (`cms-clean-arch-microservices/`) çalıştırılacak şekilde verilmiştir.

---

## 1) Önkoşullar

- **Docker Desktop** (Windows/macOS) veya Docker Engine (Linux)
- **.NET SDK 8** (yalnızca test/geliştirme için)
- (Opsiyonel) **Visual Studio 2022** / Rider

---

## 2) Proje & Dizin Yapısı (özet)

```
/docker
  grafana/
  loki/
  postgres/
  redis/
src/
  BuildingBlocks/Shared.Web/...
  UserService/
    UserService.Api/
    UserService.Application/
    UserService.Infrastructure/
    UserService.Domain/
  ContentService/
    ContentService.Api/
    ContentService.Application/
    ContentService.Infrastructure/
    ContentService.Domain/
tests/
  UserService.Tests/
  ContentService.Tests/
  IntegrationTests/            
docs/
  operations.md
  architecture.md
README.md
.env
docker-compose.yml               # docker/docker-compose.yml olabilir (sizdeki konuma göre)
```

---

## 3) `.env` Örneği

> Proje köküne `.env` dosyası açın (compose bu dosyayı otomatik okur). Değerleri sizdeki yapıyla uyumlu olacak şekilde güncelleyin.

```dotenv
# Postgres
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5433

# Redis
REDIS_PORT=6379

# JWT
JWT_ISSUER=cmspoc
JWT_AUDIENCE=cmspoc.clients
JWT_KEY=ThisIsA32ByteMinSecretKey_ChangeMe_123!

# Opsiyonel
APPLY_MIGRATIONS=true
SEED=true

# API host portları
USER_API_PORT=7146
CONTENT_API_PORT=7099

# Observability
GRAFANA_ADMIN_PASSWORD=admin
GRAFANA_PORT=3000
LOKI_PORT=3100
```

> **Önemli:** `JWT_KEY` en az ~32 byte olmalı. Aynı key **iki serviste de** kullanılmalı (S2S token doğrulaması için).

---

## 4) İlk Çalıştırma (Build + Up)

```bash
# Windows PowerShell veya Bash
docker compose -f docker/docker-compose.yml --env-file .env up --build -d
```

> İlk kez çalıştırmada imajların çekilmesi / restore ve publish süre alabilir.

### Servis URL’leri
- **UserService**: http://localhost:${USER_API_PORT}/swagger  → (varsayılan: http://localhost:7146/swagger)
- **ContentService**: http://localhost:${CONTENT_API_PORT}/swagger → (varsayılan: http://localhost:7099/swagger)
- **Grafana**: http://localhost:${GRAFANA_PORT} → (varsayılan: http://localhost:3000)
- **Loki** (HTTP API): http://localhost:${LOKI_PORT}
- **PostgreSQL**: localhost:${POSTGRES_PORT}  (host’tan bağlanmak için)
- **Redis**: localhost:${REDIS_PORT}

> Her servis için `/health`, `/ready`, `/ping` uçları da mevcuttur.

> Postman Hazır Collection: **[docs/postman_collection.json](docs/postman_collection.json)**

> Context Diyagram: **[docs/system-context-diagram.png](docs/system-context-diagram.png)**
---

## 5) Seed Kullanıcı & Token Alma

Servisler ilk açılışta **migrasyon + seed** çalıştırır (ENV `APPLY_MIGRATIONS=true`, `SEED=true`).

Örnek seed kullanıcılar (UserService):
- **Admin**: `admin@cms.local` / `P@ssw0rd!`  (Rol: `Admin`)
- Ek kullanıcılar: `editor@cms.local`, `user1@cms.local` … (roller değişebilir)

### Token isteği (PowerShell)

```powershell
$body = @{ email="admin@cms.local"; password="P@ssw0rd!" } | ConvertTo-Json
$res  = Invoke-RestMethod -Uri "http://localhost:7146/api/v1/auth/login" -Method Post -ContentType "application/json" -Body $body
$res
# $res.accessToken değerini Authorization header'da Bearer olarak kullanın.
```

### Token isteği (curl)

```bash
curl -s -X POST http://localhost:7146/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@cms.local","password":"P@ssw0rd!"}'
```

---

## 6) Örnek İstekler

### Users (Admin token ile daha fazla uç)

- Liste: `GET http://localhost:7146/api/v1/users?page=1&pageSize=20`
- Detay: `GET http://localhost:7146/api/v1/users/{id}`
- Internal brief (S2S Policy): `GET http://localhost:7146/api/v1/users/{id}/brief`

### Contents

- Liste: `GET http://localhost:7099/api/v1/contents?page=1&pageSize=20&search=...`
- Detay: `GET http://localhost:7099/api/v1/contents/{id}`
- Oluştur (Admin): `POST http://localhost:7099/api/v1/contents`
- Güncelle (Admin): `PUT http://localhost:7099/api/v1/contents/{id}`
- Sil (Admin): `DELETE http://localhost:7099/api/v1/contents/{id}`

> Content listesinde **yazar bilgisi (AuthorDisplayName/Email)**, ContentService → UserService S2S çağrısıyla doldurulur.

---

## 7) Gözlemlenebilirlik (Grafana + Loki)

### Giriş
- URL: `http://localhost:3000`
- Kullanıcı: `admin`
- Parola: `.env → GRAFANA_ADMIN_PASSWORD` (varsayılan: `admin`)

### Loki Data Source
Eğer hazır gelmiyorsa Grafana’da:
1. **Connections → Add new connection → Loki**
2. URL: `http://loki:3100`
3. Save & Test

### Örnek Log Sorguları
- Yalnız UserService: `{app="UserService.Api"}`
- Yalnız ContentService: `{app="ContentService.Api"}`
- Belirli correlation id: `{app="UserService.Api"} | json | CorrelationId="...id..."`

> Serilog konfigürasyonunuzda `WriteTo.GrafanaLoki(lokiUrl, labels: new[] { new LokiLabel{ Key="app", Value="UserService.Api" }, ... })`
etiketi olduğundan emin olun. `Loki:Url` değeriniz compose içinde `http://loki:3100` olmalı.


## 8) Docker Yönetimi

```bash
# Loglar (servis adı 'user-service', 'content-service', 'grafana', 'loki' vs olabilir)
docker compose logs -f user-service
docker compose logs -f content-service

# Durdur
docker compose down

# Durdur + tüm volume’leri sil (temiz başlangıç)
docker compose down -v

# Tek servis yeniden başlat
docker compose restart content-service
```

---
## 9) Testler

```bash
# Tüm testler
dotnet test

# Sadece bir proje
dotnet test tests/UserService.Tests/UserService.Tests.csproj
```

> Integration testler WebApplicationFactory ile çalışır. EF sağlayıcı çakışması yaşamamak için test projeleri DI’de InMemory kullanır (UAT/Prod’da Postgres).

---

## 10) Tam Temizlik ve Taze Kurulum

```bash
docker compose down -v
docker builder prune -f
docker volume prune -f

# ardından yeniden:
docker compose -f docker/docker-compose.yml --env-file .env up --build -d
```
