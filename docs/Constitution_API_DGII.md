# Prompt Maestro — API SaaS de Facturación Electrónica DGII en .NET 10

## Rol
Actúa como un **arquitecto de software senior, especialista en .NET 10, SaaS B2B, integración tributaria y facturación electrónica DGII de República Dominicana**. Tu objetivo es diseñar una **API SaaS multi-tenant** para vender como servicio a empresas que ya tienen sus propios sistemas de facturación o ERP, de modo que puedan integrarse a la plataforma mediante endpoints bien documentados.

No diseñes una simple API que “recibe JSON y genera XML”. Diseña una **plataforma tributaria completa**, auditable, segura, extensible, certificable y orientada a operación real.

---

## Contexto del producto
El producto será una **API SaaS de facturación electrónica** para empresas que ya tienen sus sistemas implementados. Estas empresas no recibirán el código fuente ni la implementación interna, sino:

- Base URL del servicio
- Endpoints REST
- Documentación técnica
- Contratos de request/response
- Reglas de integración
- Autenticación y autorización
- Webhooks
- Ejemplos de uso
- Ambiente sandbox y producción
- Opcionalmente SDKs o colecciones Postman

La plataforma debe abstraer la complejidad de DGII y exponer una integración simple y robusta.

---

## Objetivo general
Diseñar y especificar una solución completa en **.NET 10** para una API SaaS de facturación electrónica que:

1. Reciba documentos desde sistemas externos.
2. Transforme el modelo canónico interno al formato tributario requerido.
3. Valide reglas fiscales y estructurales antes de enviar.
4. Firme digitalmente el XML.
5. Se autentique ante DGII.
6. Envíe documentos a DGII.
7. Consulte el estado por TrackId o mecanismo equivalente.
8. Maneje aceptación, rechazo, acuse, aprobación comercial, anulación y contingencia.
9. Sea multi-tenant desde el día 1.
10. Sea apta para operar como producto SaaS comercializable.

---

## Restricciones y lineamientos obligatorios

### Tecnologías base
- Backend principal: **.NET 10 / ASP.NET Core Web API**
- Arquitectura: limpia, modular, orientada a dominios y casos de uso
- Persistencia: **PostgreSQL o SQL Server**
- Caché distribuida / locking / datos efímeros: **Hazelcast** como alternativa gratuita y open source
- Jobs en segundo plano: Hangfire, Quartz o colas desacopladas
- Observabilidad: OpenTelemetry
- Almacenamiento de archivos: XML, acuses, PDFs, representaciones impresas y evidencias
- Gestión de secretos y certificados: vault o mecanismo seguro equivalente

### Importante sobre Redis
**No usar Redis**. Sustituirlo por **Hazelcast** o, si el diseño lo amerita, por otra alternativa gratuita y open source como:
- Hazelcast
- Valkey
- KeyDB Community
- NCache Open Source (si aplica)

Pero la propuesta principal debe basarse en **Hazelcast** para:
- caché distribuida
- locking distribuido
- almacenamiento efímero de tokens
- coordinación entre nodos
- control de idempotencia cuando aplique

---

## Principio de diseño clave
El sistema **no debe tratarse como un generador de XML**, sino como un **motor tributario SaaS**.

Debe contemplar:
- cumplimiento fiscal
- operación multiempresa
- seguridad fuerte
- escalabilidad
- resiliencia
- auditoría
- soporte a certificación DGII
- trazabilidad completa
- extensibilidad futura

---

## Alcance funcional mínimo esperado
Diseña la plataforma contemplando estas fases.

### Fase 1
- Emisión de e-CF 31
- Emisión de e-CF 32
- Emisión de e-CF 34
- Validación previa
- Firma digital
- Envío a DGII
- Consulta de estado
- Representación impresa
- Sandbox y producción

### Fase 2
- e-CF 33
- Acuse de recibo
- Aprobación / rechazo comercial
- Anulación de e-NCF
- Resumen de factura de consumo electrónica según reglas aplicables

### Fase 3
- Contingencia
- Comunicación emisor-receptor
- Webhooks avanzados
- Portal de clientes
- Reporterías
- Métricas operativas
- Automatizaciones

---

## Requisitos de arquitectura
Diseña la solución usando una arquitectura por capas o modular que separe claramente:

- **API Layer**: endpoints HTTP, versionado, serialización, validaciones básicas
- **Application Layer**: casos de uso, orquestación, comandos, queries
- **Domain Layer**: reglas fiscales, entidades, value objects, validaciones de negocio
- **Infrastructure Layer**: DGII, firma, persistencia, almacenamiento, Hazelcast, colas, servicios externos
- **Tenant Layer**: resolución de tenant, configuración por empresa, aislamiento lógico
- **Audit/Observability Layer**: logs, métricas, trazas, eventos de auditoría

Evita lógica tributaria dentro de controladores.

---

## Requisitos multi-tenant obligatorios
La plataforma debe ser **multi-tenant real**. Debe aislar por tenant:

- certificados digitales
- configuraciones DGII
- credenciales
- secuencias / tipos de e-CF habilitados
- ambientes de certificación y producción
- webhooks
- almacenamiento documental
- permisos y usuarios
- auditoría y trazabilidad
- branding o configuraciones comerciales si luego aplica

Debes proponer el modelo de aislamiento más adecuado:
- shared database + tenant discriminator
- schema per tenant
- database per tenant

Analiza ventajas, desventajas, costo operativo y escalabilidad.

---

## Modelo canónico obligatorio
La plataforma debe usar un **modelo canónico interno**, desacoplado de los XML tributarios.

No uses el XML de DGII como modelo principal de dominio.

Propón un modelo canónico que incluya como mínimo:
- Issuer
- Buyer
- DocumentHeader
- DocumentType
- Currency
- TaxBreakdown
- Totals
- Lines
- References
- Payments
- Discounts / Charges
- Logistics si aplica
- Metadata de integración

Luego diseña mapeadores/transformadores hacia:
- e-CF 31
- e-CF 32
- e-CF 33
- e-CF 34
- formatos de acuse
- formatos de aprobación/rechazo comercial
- anulación
- resumen de factura de consumo si aplica

---

## Flujo operativo mínimo esperado
Debes modelar el flujo completo de emisión:

1. Cliente autentica contra la API SaaS.
2. Cliente envía documento en JSON según modelo canónico.
3. Sistema resuelve el tenant y su configuración.
4. Sistema valida request técnica y fiscalmente.
5. Sistema genera XML correspondiente.
6. Sistema firma digitalmente el documento.
7. Sistema obtiene token/autenticación DGII.
8. Sistema envía documento a DGII.
9. Sistema registra TrackId o referencia externa.
10. Sistema consulta estado posteriormente.
11. Sistema actualiza estados internos.
12. Sistema notifica al cliente por webhook y/o endpoint de consulta.

El diseño debe ser **asíncrono**, resiliente y apto para reintentos controlados.

---

## Estados del documento
No uses solo estados genéricos como “procesado” o “error”.

Propón una máquina de estados detallada, por ejemplo:
- Draft
- ReceivedFromClient
- ValidationFailed
- Validated
- Signed
- AuthenticationFailed
- Authenticated
- SubmittedToDGII
- TrackIdReceived
- InProcess
- Accepted
- AcceptedWithConditions
- Rejected
- DeliveryPending
- DeliveredToReceiver
- CancelRequested
- Cancelled
- InContingency
- ReconciliationPending

Incluye transiciones válidas, eventos que disparan cambios y cómo auditar cada transición.

---

## Validaciones obligatorias
El sistema debe validar antes de firmar y enviar:

### Validaciones técnicas
- contrato JSON
- campos obligatorios
- formatos
- tipos de datos
- longitudes
- enumeraciones
- referencias cruzadas

### Validaciones fiscales
- tipo de comprobante permitido
- consistencia de totales
- impuestos y redondeos
- emisor / comprador
- numeración
- reglas por tipo de e-CF
- documentos relacionados
- restricciones por monto o tipo de receptor

### Validaciones estructurales
- XML bien formado
- estructura acorde al esquema esperado
- coherencia de nodos y atributos
- compatibilidad con firmado digital

---

## Firma digital
La propuesta debe incluir una estrategia robusta de firma digital.

Debes contemplar:
- almacenamiento seguro de certificados por tenant
- cifrado en reposo
- protección de contraseñas del certificado
- rotación de certificados
- expiración y alertas preventivas
- auditoría de uso del certificado
- separación por ambiente
- manejo de errores de firmado

Explica si la firma debe ocurrir:
- dentro de la plataforma SaaS
- o delegarse a infraestructura del cliente

Y recomienda la opción más viable comercialmente para un SaaS.

---

## Integración con DGII
El diseño debe contemplar servicios para:
- autenticación
- obtención y renovación de token
- envío de documentos
- consulta de estado
- recepción de respuestas
- manejo de errores externos
- correlación entre documento interno y referencia DGII

Se debe incluir:
- caché de token usando **Hazelcast**
- renovación anticipada del token
- tolerancia a fallos
- circuit breakers si aplica
- reintentos con política segura
- reconciliación ante timeouts o respuestas ambiguas

---

## Idempotencia y concurrencia
Debes contemplar explícitamente:
- idempotency key por documento
- prevención de duplicados
- locking distribuido por tenant + serie + tipo + número
- manejo de reintentos seguros
- deduplicación por externalDocumentId
- reconciliación ante pérdida de respuesta
- consistencia transaccional entre DB y colas/eventos

Usa **Hazelcast** para locking distribuido y coordinación, o justifica otra alternativa gratuita si propones una mejor opción.

---

## Procesamiento asíncrono
La plataforma no debe depender de un flujo 100% síncrono.

Diseña procesamiento basado en:
- cola interna o jobs desacoplados
- workers de emisión
- workers de consulta de estado
- workers de webhooks
- workers de reconciliación
- workers de contingencia

Debes explicar:
- cuándo responder 202 Accepted
- cuándo responder 200 OK
- cómo exponer polling de estado
- cómo evitar doble procesamiento

---

## Webhooks
La solución debe incluir webhooks para notificar eventos como:
- documento aceptado
- documento rechazado
- documento aceptado con condiciones
- acuse recibido
- aprobación comercial
- rechazo comercial
- anulación procesada
- documento en contingencia
- certificado próximo a expirar
- webhook fallido tras reintentos

Incluye:
- firma de webhook
- reintentos exponenciales
- DLQ o estrategia de fallos permanentes
- trazabilidad de entregas
- endpoint de prueba
- registro de intentos

---

## Contingencia
La plataforma debe contemplar contingencia como módulo formal, no como parche.

Debe incluir:
- activación de contingencia por tenant
- motivos de contingencia
- trazabilidad de inicio y fin
- emisión diferida si aplica
- regularización posterior
- colas de reproceso
- evidencias y auditoría
- reglas para documentos emitidos bajo contingencia

---

## Seguridad obligatoria
El diseño debe incluir:
- autenticación robusta del cliente consumidor
- autorización por scopes, roles o permisos
- separación estricta entre tenants
- cifrado en tránsito
- cifrado en reposo
- secretos en vault
- certificados protegidos
- rotación de secretos
- protección contra replay
- rate limiting
- API keys o OAuth2 según recomiendes
- auditoría inmutable
- trazabilidad completa por request
- correlación end-to-end

Propón qué esquema de autenticación usarías para clientes B2B:
- API Key
- OAuth2 Client Credentials
- JWT firmado
- combinación híbrida

Y justifica la elección.

---

## Observabilidad y operación SaaS
Diseña el sistema para operar en producción con paneles y métricas como:
- documentos emitidos por tenant
- documentos aceptados / rechazados
- ratio de error por validación
- tiempos de procesamiento
- tiempos de respuesta DGII
- jobs fallidos
- webhooks fallidos
- certificados próximos a vencer
- tenants sin actividad
- uso de secuencias
- incidentes de contingencia

Incluye:
- logs estructurados
- métricas
- trazas distribuidas
- correlation IDs
- health checks
- readiness / liveness

---

## Onboarding de clientes
Diseña un proceso de onboarding técnico para cada cliente que incluya:
- registro del tenant
- configuración tributaria
- carga y validación de certificado
- configuración de ambientes
- carga de secuencias/autorizaciones
- mapeo ERP -> modelo canónico
- pruebas sandbox
- checklist de salida a producción
- soporte a certificación DGII si corresponde

---

## Entrega comercial al cliente
Recuerda que el cliente final no recibirá el backend ni la implementación interna. Diseña qué debe recibir un cliente integrador:

- documentación OpenAPI / Swagger
- guía de integración
- guía de errores
- colección Postman
- ejemplos de código
- credenciales sandbox
- credenciales producción
- documentación de webhooks
- SLA y límites
- versionado de API
- changelog

---

## Diseño de endpoints
Propón endpoints REST versionados, por ejemplo:
- `POST /api/v1/documents`
- `GET /api/v1/documents/{id}`
- `GET /api/v1/documents/{id}/status`
- `POST /api/v1/documents/{id}/cancel`
- `POST /api/v1/webhooks/test`
- `GET /api/v1/catalogs/document-types`
- `GET /api/v1/tenants/me`

Debes definir:
- naming consistente
- contratos request/response
- códigos HTTP correctos
- errores estandarizados
- idempotency key
- correlation id
- headers requeridos

---

## Diseño de respuestas
Las respuestas deben ser consistentes y orientadas a integración B2B.

Propón un estándar para:
- éxito síncrono
- éxito asíncrono
- error de validación
- error de autenticación
- error de autorización
- error de negocio
- error externo DGII
- error interno recuperable
- error interno no recuperable

Incluye ejemplos JSON.

---

## Base de datos y persistencia
Diseña las entidades/tablas principales para:
- tenants
- tenant settings
- certificates
- users / api clients
- documents
- document lines
- document statuses
- DGII submissions
- webhook subscriptions
- webhook deliveries
- audit logs
- outbox/inbox events
- contingency records
- token cache metadata si procede

Explica qué guardar en DB relacional, qué guardar en Hazelcast y qué guardar en almacenamiento de archivos.

---

## Estrategias de consistencia
Debes recomendar patrones como:
- Outbox Pattern
- Inbox Pattern
- Retry Pattern
- Circuit Breaker
- Distributed Lock
- Idempotency Store
- Saga ligera si aplica

Explica cuándo usarlos y por qué.

---

## Escalabilidad y despliegue
La solución debe ser apta para desplegarse en contenedores y escalar horizontalmente.

Debes contemplar:
- múltiples instancias de API
- múltiples workers
- Hazelcast en clúster
- almacenamiento compartido o compatible
- despliegue con Docker
- posible orquestación futura con Kubernetes
- separación por ambientes
- CI/CD

---

## No hagas esto
Evita expresamente estos errores de diseño:
- lógica tributaria en controladores
- XML como modelo central del dominio
- procesamiento 100% síncrono
- falta de idempotencia
- falta de trazabilidad
- mezclar tenants sin aislamiento serio
- guardar certificados de forma insegura
- depender de estados ambiguos
- no prever contingencia
- exponer detalles internos innecesarios al cliente

---

## Entregables esperados
Tu respuesta debe entregarse en formato técnico, claro y accionable.

Incluye obligatoriamente:

1. **Resumen ejecutivo de la solución**
2. **Arquitectura general**
3. **Diagrama textual o explicación de módulos**
4. **Estructura de proyectos en .NET 10**
5. **Definición de capas y responsabilidades**
6. **Modelo multi-tenant recomendado**
7. **Modelo de datos principal**
8. **Modelo canónico sugerido**
9. **Flujos operativos clave**
10. **Máquina de estados del documento**
11. **Diseño de endpoints iniciales**
12. **Contratos request/response de ejemplo**
13. **Estrategia de seguridad**
14. **Estrategia de firma digital**
15. **Integración DGII**
16. **Uso de Hazelcast en la solución**
17. **Estrategia de idempotencia y locking**
18. **Jobs/workers necesarios**
19. **Webhooks y eventos**
20. **Contingencia**
21. **Observabilidad**
22. **Onboarding del cliente**
23. **Roadmap MVP -> V2 -> V3**
24. **Riesgos técnicos y mitigaciones**
25. **Recomendaciones finales de implementación**

---

## Nivel de detalle esperado
La respuesta debe ser:
- profunda
- concreta
- orientada a implementación real
- pensada para SaaS B2B
- alineada con buenas prácticas empresariales
- lista para servir como blueprint técnico del proyecto

No des una respuesta superficial. No te limites a listar tecnologías. Diseña la solución de forma coherente y operable.

---

## Extra: preferencia de estilo de respuesta
Cuando respondas:
- usa secciones claras
- propone nombres de proyectos y carpetas
- incluye ejemplos realistas
- justifica decisiones técnicas
- explica trade-offs
- sugiere MVP primero, sin perder visión de largo plazo
- prioriza mantenibilidad, seguridad y escalabilidad

---

## Instrucción final
Con todo lo anterior, genera una **propuesta técnica integral** para construir esta API SaaS de facturación electrónica en **.NET 10**, usando **Hazelcast** como reemplazo gratuito/open source de Redis para caché distribuida, locking y coordinación.