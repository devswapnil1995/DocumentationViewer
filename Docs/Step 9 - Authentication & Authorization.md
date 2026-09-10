## Authentication in .NET Core

> Authentication answers: "Who are you?"

> Authorization answers: "What are you allowed to do?"

### Authentication vs Authorization

Imagine you call:

```http
GET /api/employees
```

The server needs to know:

### Authentication

```text
Who is making this request?
        ↓
Swapnil
```

### Authorization

```text
Is Swapnil allowed to access employees?
        ↓
Yes
```

So:

```text
Request
   ↓
Authentication
   ↓
Who are you?
   ↓
Authorization
   ↓
What can you access?
   ↓
Endpoint
```

***Interview one-liner***

> Authentication verifies the identity of a user or client, while authorization determines what that authenticated identity is allowed to access.

    
**Authentication schemes**

ASP.NET Core has the concept of an **authentication scheme**.

For example:

```text
JWT Bearer
Cookie
API Key/custom
OpenID Connect
```

The scheme tells ASP.NET Core **how authentication should be performed**.

For APIs, you'll commonly see:

```text
Bearer authentication
```
---

#### `UseAuthentication()` vs `UseAuthorization()`

This is a **very common interview question**.

**`UseAuthentication()`**

It tries to establish the user's identity.

```text
JWT
 ↓
Validate token
 ↓
Create ClaimsPrincipal
 ↓
HttpContext.User
```

**`UseAuthorization()`**

It checks whether that identity has permission.

```text
HttpContext.User
       ↓
Authorization policy
       ↓
Allowed?
```

So:

```text
Authentication
      ↓
"Who are you?"

Authorization
      ↓
"Are you allowed?"
```

---

***`Cookie Authentication`***

> Stores the user’s authentication ticket in an encrypted cookie after login. The browser sends the cookie with each request.

Common for: MVC or Razor Pages applications (server-rendered apps).

**Pros:**
Simple to set up
Works well for traditional web apps

**Cons:**   
Not ideal for APIs or SPAs (cross-domain issues)
Vulnerable if cookies are stolen (mitigate with HttpOnly, Secure, SameSite)

Security Notes:
Always use HTTPS
Enable Data Protection API for encryption
Add CSRF protection


***`JWT (JSON Web Token) Authentication`***

> After login, the server issues a signed token (JWT) that contains claims. Client sends the token in the Authorization header (Bearer <token>).
Common for: REST APIs, SPAs, mobile apps.

**Pros:**
Stateless (no server-side session storage)
Works across domains easily
Good for microservices

**Cons:**
Token theft = access until expiration
Revocation is tricky (need blacklist or short expiry + refresh token)

**Security Notes:**
Use short-lived tokens + refresh tokens
Sign with strong algorithms (RS256, not HS256 if possible)
Store tokens securely (never in localStorage if XSS risk; prefer HttpOnly cookies)


***`OAuth 2.0 / OpenID Connect`***

> Delegates authentication to an Identity Provider (Google, Azure AD, Okta, Auth0, etc.). Uses tokens for API access.

Common for: Enterprise, multi-service systems, SSO.

**Pros:**
Industry standard for SSO and API access
No password storage in your app
Supports external login providers

**Cons:**
More complex to set up
Requires understanding of flows (Authorization Code Flow, Client Credentials Flow, etc.)

**Security Notes:**
Always use Authorization Code Flow with PKCE for SPAs/mobile
Use trusted IdPs with proper token signing


***`Windows Authentication (Kerberos/NTLM)`***

> Uses Windows domain credentials for automatic authentication (Active Directory).

Common for: Intranet apps, corporate environments.

**Pros:**
No need for password input
Secure in internal networks

**Cons:**
Not for public-facing apps
Requires domain setup


***`API Key Authentication`***

> Client sends a pre-shared API key with each request.

Common for: Service-to-service communication, public APIs.

**Pros:**
Simple for machine-to-machine

**Cons:**
Key theft = full access
No built-in expiry

**Security Notes:**
Rotate keys regularly
Restrict scope/IP
Use over HTTPS only


***`Certificate Authentication (Mutual TLS)`***

> Client and server exchange SSL/TLS certificates to authenticate each other.

Common for: High-security, machine-to-machine APIs.

**Pros:**
Extremely secure when implemented right
No passwords/tokens

**Cons:**
Harder to manage (certificate issuance, renewal)

**Security Notes:**
Use short-lived certs
Automate renewal with tools like ACM

----------------------

### JWT Authentication

> **JWT — JSON Web Token** — is extremely common for ASP.NET Core Web APIs. A JWT is basically a token containing claims about the authenticated identity.

Conceptually:

```text
JWT
 ├── Header
 ├── Payload
 └── Signature
```

Example payload might contain:

```json
{
  "sub": "123",
  "name": "Swapnil",
  "role": "Admin"
}
```

The server validates the token's signature and other relevant claims.

**Why use JWT?**

Because HTTP APIs are commonly stateless. Instead of the server maintaining a traditional server-side session for every API client:

```text
Client
   ↓
JWT
   ↓
API
```

The token carries identity/claims that the API can validate.

**Configure JWT authentication**

A simplified setup looks like:

```csharp
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-identity-server";
        options.Audience = "my-api";
    });
```

Then in the middleware pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Important:

```text
UseAuthentication()
        ↓
Identifies the caller

UseAuthorization()
        ↓
Checks permissions
```


**JWT request flow**

This is extremely important for interviews.

```text
             LOGIN
               │
               ▼
        Username/Password
               │
               ▼
         Authentication
               │
               ▼
           JWT Token
               │
               ▼
             Client
               │
               │ Authorization: Bearer JWT
               ▼
          ASP.NET Core
               │
               ▼
        JWT Authentication
               │
          Token valid?
          ┌────┴────┐
         Yes        No
          │          │
          ▼          ▼
     User.Identity   401
          │
          ▼
      Authorization
          │
          ▼
       Endpoint
```


**`Claims`**

> Claims are pieces of information about the authenticated identity.

Example:

```json
{
  "sub": "123",
  "name": "Swapnil",
  "role": "Admin",
  "department": "IT"
}
```

These become available through:

```csharp
User
```

For example:

```csharp
var userName = User.Identity?.Name;
```

or:

```csharp
var role = User.FindFirst("role")?.Value;
```

Conceptually:

```text
JWT
 ↓
Claims
 ↓
ClaimsPrincipal
 ↓
HttpContext.User
```
-----------

### OAuth2 and SSO authentication 

> OAuth2 is an authorization framework (not authentication itself). It allows an application (your .NET Core app) to access a user’s resources (profile, email, APIs) on their behalf from another provider (Google, Microsoft, Azure AD, GitHub, etc.) without sharing passwords.

**Actors in OAuth2:**

- Resource Owner (User) → The person logging in.
- Client (Your .NET Core App) → The app that wants user data.
- Authorization Server (IdP e.g., Google, Azure AD, Okta) → Issues tokens after authentication.
- Resource Server (API) → The API you want to access using tokens.

**Tokens:**
- Access Token → Grants access to APIs (short-lived).
- Refresh Token → Gets new access tokens (long-lived).
- ID Token (OIDC extension) → Contains user identity info (used for login).

***What is SSO (Single Sign-On)?***
- SSO is a mechanism that lets a user log in once with a trusted identity provider (IdP) and then access multiple applications without logging in again.
- You log into Microsoft 365 once. Then Outlook, Teams, SharePoint, etc., all work seamlessly.
- This is usually implemented with OAuth2 + OpenID Connect (OIDC).
- OIDC adds authentication (who you are) on top of OAuth2 (which only defines authorization).

**How it works in a .NET Core application**
Here’s the high-level flow if you integrate OAuth2 + OIDC:
- User tries to access your app
- They hit a secured page /dashboard.
- Redirect to Identity Provider (IdP)
- Your app redirects to Azure AD / Google / Okta for login.
   Example: https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?...
- User logs in
- Enter credentials at IdP (not in your app).
- Authorization Code issued
- After successful login, IdP redirects back to your app with an authorization code.
- Exchange Code for Tokens
- Your app calls the IdP’s token endpoint with the authorization code and client secret.
- IdP responds with:
id_token → JWT containing user claims (used for login).
access_token → Used to call APIs.
refresh_token → To renew access tokens.
.NET Core validates tokens
Middleware validates the signature, issuer, audience, and expiry.
Creates an authenticated ClaimsPrincipal.
User is logged in
The user can now access your app and APIs using the tokens.


**Implementation in .NET Core**
In ASP.NET Core, you usually add OIDC Middleware in Program.cs (or Startup.cs):
```csharp   
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://login.microsoftonline.com/{tenant}/v2.0"; // IdP URL
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.ResponseType = "code"; // Authorization code flow
    options.SaveTokens = true; // Save access + refresh tokens in cookie
    options.Scope.Add("openid");    // for authentication
    options.Scope.Add("profile");   // user profile info
    options.Scope.Add("email");
});
```
Now:
Navigating to a protected controller action [Authorize] → redirects to IdP.
After login, user’s claims (name, email, roles) are available via User.Identity.

**SSO in Action**
If multiple apps (say App1 and App2) are configured with the same IdP:
User logs in once at IdP when accessing App1.
When they open App2, the IdP sees the existing session → issues tokens without asking for login again.
Result = SSO experience.

**Common Providers for OAuth2 + SSO in .NET Core**
- Azure AD / Entra ID (most common in enterprises).
- IdentityServer4 / Duende (self-hosted identity provider).
- Okta / Auth0 / Keycloak.
- Google, Facebook, GitHub (social logins).

### Summary:
- OAuth2 = Authorization → issuing tokens for accessing APIs.
- OIDC = Authentication layer on top of OAuth2 → who the user is.
- SSO = Multiple apps use the same IdP session so login happens only once.
- In .NET Core → you use middleware (AddAuthentication, AddOpenIdConnect) to connect your app with the IdP.


### Refresh Token Flow (for JWT)
```
1. User logs in with credentials
        ↓
2. Server validates → issues:
   - Access Token (short-lived, e.g. 15 min)
   - Refresh Token (long-lived, e.g. 7 days)
        ↓
3. Client stores both
   - Access token → memory / short-term storage
   - Refresh token → httpOnly secure cookie (safer than localStorage)
        ↓
4. Client uses Access Token for API calls (Authorization: Bearer <token>)
        ↓
5. Access token expires (after 15 min) → API returns 401 Unauthorized
        ↓
6. Client sends Refresh Token to a dedicated endpoint (e.g. /api/auth/refresh)
        ↓
7. Server validates refresh token (checks DB / store — is it valid, not revoked, not expired?)
        ↓
8. Server issues NEW access token (and often a new refresh token too — "rotation")
        ↓
9. Client continues using new access token seamlessly — user never notices
```

### Key Concepts to Remember
- Access token = stateless, self-contained (JWT), not stored server-side, can't be revoked easily
- Refresh token = stateful, stored in DB/cache server-side, CAN be revoked (logout, security breach, etc.)
- Token Rotation = every time refresh token is used, issue a new refresh token + invalidate old one → prevents reuse if stolen
- Refresh Token Reuse Detection = if an already-used/rotated refresh token is used again → signals theft → revoke entire token family (all descendant tokens) for that user
- Refresh tokens should be stored in httpOnly, Secure cookies (not localStorage) to prevent XSS theft

> A valid refresh token call typically returns BOTH a new access token AND a new refresh token — if rotation is implemented (which is the recommended/modern approach).
If rotation is NOT implemented, it returns only a new access token, and the same refresh token keeps getting reused until it naturally expires.

>The server returns 401 Unauthorized — it does not issue a new access token. End result: The user's session is fully over. There's no way to silently recover — they must log in again with their credentials to get a fresh access token + refresh token pair.

---------------------
---------------------
## 15 ways to improve ASP.NET Core API performance

| #  | Optimization                      | What you do                                   |
| -- | --------------------------------- | --------------------------------------------- |
| 1  | **Database queries**              | Optimize SQL/EF queries                       |
| 2  | **Indexes**                       | Add indexes to frequently queried columns     |
| 3  | **Avoid N+1 queries**             | Use proper joins/projections/includes         |
| 4  | **Projection**                    | Select only required columns                  |
| 5  | **`AsNoTracking()`**              | Use for read-only EF queries                  |
| 6  | **Pagination**                    | Don't return thousands/millions of records    |
| 7  | **Async I/O**                     | Use `async/await` for DB/HTTP/file operations |
| 8  | **Caching**                       | Cache frequently requested data/responses     |
| 9  | **Compression**                   | Reduce response payload size                  |
| 10 | **DTOs**                          | Return only required data                     |
| 11 | **Avoid unnecessary allocations** | Reduce object creation/copies                 |
| 12 | **Connection pooling**            | Reuse DB/HTTP connections                     |
| 13 | **HTTP client reuse**             | Use `IHttpClientFactory`                      |
| 14 | **Background processing**         | Move non-critical work out of request         |
| 15 | **Measure/profile**               | Find actual bottleneck before optimizing      |

> For a typical enterprise .NET API, I'd separate the API, Application, Domain, and Infrastructure concerns. Controllers remain thin and handle HTTP concerns. Application contains use cases and business orchestration. Domain contains core business rules and entities. Infrastructure handles EF Core, databases, external services, and other technical concerns. I use interfaces where they provide useful boundaries and DI to compose the implementations
-------------
-------------

## Scaling

> Scaling means increasing your system's capacity so it can handle **more users, requests, data, or workload** without becoming too slow or unavailable.

For an API, imagine:

```text
100 users
   ↓
   API
   ↓
Database
```

Now users increase to:

```text
100,000 users
```

Your single API server may not be enough. You need to **scale**.

There are two main approaches:

```text
                 Scaling
                    |
          +---------+---------+
          |                   |
     Vertical              Horizontal
      Scaling                Scaling
       (Scale Up)            (Scale Out)
```

**`Vertical Scaling — Scale Up`**

You make the **existing server bigger**.

Suppose your API server has:

```text
CPU: 2 cores
RAM: 4 GB
```

You upgrade it to:

```text
CPU: 8 cores
RAM: 32 GB
```

```text
Before:

       API Server
     2 CPU / 4 GB
          ↓
       Database


After:

       API Server
     8 CPU / 32 GB
          ↓
       Database
```

**Advantages**

* Simple
* Usually no application architecture changes
* Easy to implement
* Useful when workload is moderate

**Disadvantages**

* Hardware has a limit
* Can become expensive
* Single point of failure still exists
* Eventually you cannot make the machine bigger

---

**`Horizontal Scaling — Scale Out`**

Instead of making one server bigger, you create **multiple instances** of your API.

```text
                 Load Balancer
                 /     |      \
                /      |       \
               ↓       ↓        ↓
            API 1    API 2    API 3
               \       |       /
                \      |      /
                   Database
```

Suppose one API server handles:

```text
1,000 requests/sec
```

Instead of upgrading it to a huge machine, you can have:

```text
API 1 → 1,000 req/sec
API 2 → 1,000 req/sec
API 3 → 1,000 req/sec
```

Potential capacity:

```text
~3,000 req/sec
```

assuming the rest of the architecture isn't the bottleneck.

---

**What does the Load Balancer do?**

The Load Balancer distributes incoming requests.

```text
Request 1 → API 1
Request 2 → API 2
Request 3 → API 3
Request 4 → API 1
Request 5 → API 2
```

For example:

```text
                 Load Balancer
                       |
        +--------------+--------------+
        ↓              ↓              ↓
      API 1          API 2          API 3
```

Common cloud examples include Azure Application Gateway, Azure Load Balancer, and Azure Front Door, depending on the architecture.

---

**The BIG problem with Horizontal Scaling**

Imagine you have:

```text
API 1
  ↓
IMemoryCache
```

and:

```text
API 2
  ↓
IMemoryCache
```

These are **different memory spaces**.

```text
        Load Balancer
          /       \
         ↓         ↓
      API 1      API 2
       ↓           ↓
   Cache A      Cache B
```

If user requests:

```text
Request 1 → API 1
```

API 1 caches:

```text
user:123 → data
```

Then:

```text
Request 2 → API 2
```

API 2 doesn't have that cached value.

This is why horizontally scaled applications should avoid depending on **local in-memory state** when that state needs to be shared across instances.

Use something like:

```text
API 1 ──┐
API 2 ──┼──→ Redis
API 3 ──┘
```

instead.

---

**Session State**

Another common problem:

```text
User
 ↓
API 1
 ↓
Session data stored in API 1 memory
```

Next request:

```text
User
 ↓
API 2
```

API 2 doesn't have that session data.

Solutions include:

* Distributed session
* Redis
* Database-backed state
* Or preferably designing APIs to be **stateless** where practical

**Why Stateless APIs are important**

A horizontally scalable API should ideally be:

```text
Request
   ↓
Any API instance
   ↓
Process request
   ↓
Response
```

It shouldn't matter whether the request goes to:

```text
API 1
API 2
API 3
```

For example, don't store important user state only in:

```csharp
private static ...
```

or:

```csharp
IMemoryCache
```

if another instance needs that state.

Instead:

```text
API instances
      |
      +----→ Database
      |
      +----→ Redis
      |
      +----→ Message Broker
```

---

**Auto Scaling**

In cloud environments, horizontal scaling can be automatic.

For example:

```text
Normal traffic
     ↓
   API 2 instances


Traffic increases
     ↓
   API 5 instances


Traffic decreases
     ↓
   API 2 instances
```

This is called **autoscaling**.

You can scale based on metrics such as:

```text
CPU
Memory
Request count
Queue length
Response latency
```

---

**What about the Database?**

This is where interviewers often go deeper.

You may horizontally scale your API:

```text
        Load Balancer
        /     |     \
      API1   API2   API3
        \     |     /
         \    |    /
          Database
```

But now the database may become the bottleneck.

You might need:

```text
Database optimization
        ↓
Indexes
        ↓
Read replicas
        ↓
Caching
        ↓
Partitioning/sharding (when appropriate)
```

So scaling isn't simply:

> "Add more API servers."

You have to identify the **bottleneck**.

---

### Vertical vs Horizontal

|                   | Vertical             | Horizontal              |
| ----------------- | -------------------- | ----------------------- |
| Also called       | Scale Up             | Scale Out               |
| Approach          | Bigger machine       | More machines/instances |
| Complexity        | Lower                | Higher                  |
| Maximum capacity  | Hardware limit       | Can scale much further  |
| Fault tolerance   | Lower                | Higher                  |
| Load balancer     | Usually unnecessary  | Usually needed          |
| Stateless design  | Less critical        | **Very important**      |
| Cloud scalability | Limited              | **Excellent**           |
| Cost              | Can become expensive | Often more flexible     |

---

### Real-world API architecture

For a scalable .NET API, you might have:

```text
                    Internet
                       |
                       ↓
              Load Balancer / Gateway
                       |
          +------------+------------+
          |            |            |
          ↓            ↓            ↓
       .NET API     .NET API     .NET API
       Instance 1   Instance 2   Instance 3
          |            |            |
          +------------+------------+
                       |
              +--------+--------+
              |                 |
              ↓                 ↓
            Redis            Database
              |
              ↓
        Shared Cache
```

And for asynchronous work:

```text
API
 ↓
Message Queue
 ↓
Background Workers
 ↓
Database / External services
```

---

**When would you choose which?**

### Small application

```text
1 API server
   ↓
Database
```

Vertical scaling is often perfectly fine.

---

### Growing application

```text
             Load Balancer
              /          \
            API 1        API 2
               \          /
                Database
```

Horizontal scaling starts becoming useful.

---

### High-scale application

```text
                 Gateway
                    |
          +---------+---------+
          ↓         ↓         ↓
        API 1     API 2     API 3
          |         |         |
          +---------+---------+
                    |
                  Redis
                    |
                 Database
                    |
              Read Replicas
```

You combine:

> **Horizontal scaling + caching + database optimization + async processing + autoscaling**

---

**Vertical = make the server bigger.**
**Horizontal = add more servers.**

------------------------
------------------------