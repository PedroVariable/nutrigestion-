# Guía de Deploy — NutriGestión

Despliegue gratis en **Render** (backend .NET + frontend estático) + **UptimeRobot** para mantener el backend despierto.

---

## ⚠️ Antes de subir a GitHub

Tu repo tenía secretos hardcoded en `appsettings.json`. Ya los moví a `appsettings.Development.json` (que debes ignorar en git), pero si **ya hiciste push** a un repo público antes:

1. **Rota la contraseña** del usuario MongoDB en Atlas (Database Access → Edit → Edit Password).
2. **Regenera el JWT Secret** — Render lo va a generar automático con `generateValue: true`.

Asegúrate que tu `.gitignore` incluye:

```
backend/NutriGestion.Api/appsettings.Development.json
backend/NutriGestion.Api/bin/
backend/NutriGestion.Api/obj/
frontend/node_modules/
frontend/dist/
frontend/.env
frontend/.env.production
```

---

## Paso 1 — Subir el proyecto a GitHub

```bash
cd c:\PProyectos\NutriGestion
git init
git add .
git commit -m "Initial commit"
# Crea el repo en github.com (puede ser privado)
git remote add origin https://github.com/<tu-usuario>/nutrigestion.git
git branch -M main
git push -u origin main
```

---

## Paso 2 — Configurar MongoDB Atlas para producción

1. **Network Access** → "Add IP Address" → "Allow Access from Anywhere" (0.0.0.0/0).
   - Render no tiene IPs fijas en el plan free, así que necesitas abrir el acceso.
   - Si quieres más seguridad, sube a un plan pagado de Render que sí da IP estática.

2. Copia tu connection string completa (la que ya tienes en `appsettings.Development.json`).

---

## Paso 3 — Deploy en Render (con Blueprint)

1. Ve a [render.com](https://render.com) y crea cuenta (con GitHub).
2. **Dashboard** → **New +** → **Blueprint**.
3. Conecta tu repo `nutrigestion`.
4. Render detecta `render.yaml` y muestra los dos servicios (`nutrigestion-api` y `nutrigestion-web`).
5. Te pedirá llenar los valores marcados con `sync: false`:
   - `MongoDb__ConnectionString` → pega tu connection string de Atlas.
   - `Cors__AllowedOrigins` → **déjalo vacío por ahora**, lo llenas después del primer deploy.
   - `VITE_API_URL` → **déjalo vacío por ahora**, igual lo llenas después.
6. Click **Apply**.

Espera ~5-10 min al primer build. Render te da dos URLs, algo como:
- `https://nutrigestion-api.onrender.com` (backend)
- `https://nutrigestion-web.onrender.com` (frontend)

---

## Paso 4 — Conectar frontend ↔ backend

Ahora que ya tienes ambas URLs, regresa al dashboard de Render:

1. **nutrigestion-api** → Environment → editar `Cors__AllowedOrigins`:
   ```
   https://nutrigestion-web.onrender.com
   ```
   (sin slash final). Render redespliega automático.

2. **nutrigestion-web** → Environment → editar `VITE_API_URL`:
   ```
   https://nutrigestion-api.onrender.com/api
   ```
   ⚠️ Importante el `/api` al final — sin él las rutas no van a coincidir.

3. Trigger un **Manual Deploy** del frontend para que tome el nuevo `VITE_API_URL` (las vars de Vite se inyectan en build time, no en runtime).

---

## Paso 5 — Keep-alive con UptimeRobot (gratis)

El plan free de Render duerme el backend tras 15 min sin tráfico. Un ping cada 5 min lo mantiene despierto.

1. Crea cuenta en [uptimerobot.com](https://uptimerobot.com).
2. **Add New Monitor**:
   - Type: **HTTP(s)**
   - Friendly Name: `NutriGestión API`
   - URL: `https://nutrigestion-api.onrender.com/health`
   - Monitoring Interval: **5 minutes**
3. Save.

Eso es todo. Tu API no se va a dormir más.

---

## Verificación final

1. Abre `https://nutrigestion-api.onrender.com/health` → debe devolver JSON con `{ "estado": "ok", ... }`.
2. Abre `https://nutrigestion-api.onrender.com/swagger` → debe mostrar Swagger UI.
3. Abre `https://nutrigestion-web.onrender.com` → debe cargar el login del frontend.
4. Inicia sesión y verifica que los datos cargan.

---

## Variables de entorno (referencia)

### Backend (`nutrigestion-api`)
| Variable | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `MongoDb__ConnectionString` | tu Atlas SRV string |
| `MongoDb__DatabaseName` | `nutrigestion` |
| `Jwt__Secret` | (auto-generado por Render) |
| `Jwt__Issuer` | `nutrigestion-api` |
| `Jwt__Audience` | `nutrigestion-web` |
| `Jwt__ExpiresInHours` | `8` |
| `Cors__AllowedOrigins` | URL pública del frontend |

### Frontend (`nutrigestion-web`)
| Variable | Valor |
|---|---|
| `VITE_API_URL` | `https://nutrigestion-api.onrender.com/api` |

---

## Si prefieres Railway en lugar de Render

Railway no se duerme (sin keep-alive) y el deploy es aún más simple, pero usa los $5 USD/mes de crédito gratis (probablemente te alcanza para esta app).

1. [railway.app](https://railway.app) → New Project → Deploy from GitHub.
2. Selecciona el repo, Railway detecta el `Dockerfile` automáticamente.
3. Configura las mismas variables de entorno del Paso 3.
4. En **Settings** → **Networking** → Generate Domain.

Para el frontend en Railway, agrega un segundo servicio del mismo repo apuntando a `/frontend` con build command `npm ci && npm run build` y un servidor estático tipo `npx serve dist`.
