# Self Interview Q&A: Real-World Experience

## Microfrontends & Architecture

### 1. What microfrontend implementation have you done?

**Answer:**

Our application evolved from a monolithic frontend to a modular microfrontend architecture to support faster development, team independence, and independent deployment.

**Evolution & Architecture:**

```text
Before (Monolithic):
┌─────────────────────────────────────┐
│  AngularJS + Angular Combined       │
│  (All modules in single bundle)     │
│  ~5MB bundle, slow updates          │
└─────────────────────────────────────┘

After (Microfrontends):
┌────────────────────────────────────────────┐
│         single-spa Router (Shell)          │
├──────────┬──────────┬──────────┬───────────┤
│Dashboard │  Orders  │ Inventory│ Analytics │
│ Module   │  Module  │  Module  │  Module   │
│ (Angular)│(Angular) │(Angular) │(Angular)  │
└──────────┴──────────┴──────────┴───────────┘
   Shared Services, Redux Store, UI Library
```

**Key Implementation Details:**

**1. Framework Choice: single-spa**
- Lightweight library for registering and managing multiple SPAs
- Perfect for independently versioned applications
- Allows different frameworks (Angular, React, Vue) to coexist

**2. Registry Configuration (single-spa-config.js)**

```javascript
// ✅ Central configuration for all microfrontends
import { registerApplication, start } from 'single-spa';

registerApplication({
  name: '@myapp/dashboard',
  app: () => System.import('@myapp/dashboard'),
  activeWhen: '/dashboard'
});

registerApplication({
  name: '@myapp/orders',
  app: () => System.import('@myapp/orders'),
  activeWhen: '/orders'
});

registerApplication({
  name: '@myapp/inventory',
  app: () => System.import('@myapp/inventory'),
  activeWhen: '/inventory'
});

start();
```

**3. Independent Deployment Pipeline**
- Each module has **own repository** and **build pipeline**
- Deployed to CDN with **versioning** (e.g., `/dashboard@2.1.0/index.js`)
- Can deploy modules independently without full app rebuild
- Zero-downtime deployments

**4. Dynamic Component Loading (Angular)**

```typescript
// ✅ Custom helper for loading remote components
@Injectable()
export class MicroFrontendLoader {
  private componentCache = new Map();

  async loadComponentFactory(moduleName: string, componentName: string) {
    const cacheKey = `${moduleName}:${componentName}`;

    if (this.componentCache.has(cacheKey)) {
      return this.componentCache.get(cacheKey);
    }

    // Dynamically load remote module
    const module = await import(`${this.getModuleUrl(moduleName)}`);
    const componentFactory = module[componentName];

    this.componentCache.set(cacheKey, componentFactory);
    return componentFactory;
  }

  private getModuleUrl(moduleName: string): string {
    return `/modules/${moduleName}/index.js`;
  }
}

// Usage in container component
export class DashboardContainerComponent implements OnInit {
  @ViewChild('container', { read: ViewContainerRef }) container!: ViewContainerRef;

  constructor(private loader: MicroFrontendLoader) {}

  async ngOnInit() {
    const componentFactory = await this.loader.loadComponentFactory(
      'dashboard',
      'DashboardComponent'
    );
    this.container.createComponent(componentFactory);
  }
}
```

**5. Shared Session & Authentication**

```typescript
// ✅ Shared authentication service available to all modules
@Injectable({
  providedIn: 'root'
})
export class SharedAuthService {
  private sessionData = new BehaviorSubject(null);
  public session$ = this.sessionData.asObservable();

  setSession(userDetails: any) {
    this.sessionData.next(userDetails);
    sessionStorage.setItem('session', JSON.stringify(userDetails));
  }

  getSession(): any {
    return this.sessionData.value || 
           JSON.parse(sessionStorage.getItem('session') || '{}');
  }
}

// Injected in each microfrontend
export class OrdersComponent implements OnInit {
  userSession$ = this.authService.session$;

  constructor(private authService: SharedAuthService) {}
}
```

**6. Lazy Loading & Performance Optimization**

```typescript
// ✅ Lazy load microfrontends on route activation
const routes: Routes = [
  {
    path: 'dashboard',
    loadChildren: () => import('@myapp/dashboard/dashboard.module')
      .then(m => m.DashboardModule),
    data: { microFrontend: 'dashboard' }
  },
  {
    path: 'orders',
    loadChildren: () => import('@myapp/orders/orders.module')
      .then(m => m.OrdersModule),
    data: { microFrontend: 'orders' }
  }
];
```

**7. Shared Design System & UI Components**

```typescript
// ✅ Shared library with common components
// @myapp/shared-ui package
export * from './components/button/button.component';
export * from './components/modal/modal.component';
export * from './components/table/table.component';
export * from './directives/auth.directive';
export * from './pipes/phone.pipe';

// Used in each microfrontend
import { SharedButtonComponent } from '@myapp/shared-ui';

@Component({
  selector: 'app-orders',
  template: `
    <app-button (click)="submitOrder()">Submit Order</app-button>
  `,
  imports: [SharedButtonComponent]
})
export class OrdersComponent {}
```

**Benefits Achieved:**
✅ Independent team development and deployment
✅ Modules upgraded independently (Angular 12 → 17)
✅ Smaller bundles per module (from 5MB → 1-2MB each)
✅ Faster initial load (lazy load only active module)
✅ Version flexibility (can run multiple module versions)

---

### 2. How did you handle communication between different microfrontends?

**Answer:**

We implemented a **multi-layered communication strategy** combining state management, event buses, and shared services.

**Communication Architecture:**

```text
┌─────────────────────────────────────────────────────┐
│           Shell Application (single-spa)             │
│                                                      │
│  ┌────────────────┐         ┌───────────────────┐  │
│  │ Shared Redux   │◄───────►│ Event Bus          │  │
│  │ Store          │         │ (RxJS Subject)    │  │
│  └────────────────┘         └───────────────────┘  │
│         ▲                           ▲              │
│         │                           │              │
│    ┌────┴─────┐          ┌─────────┴────┐         │
│    │           │          │              │         │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ │Dashboard │ │ Orders   │ │Inventory │ │Analytics │
│ │Module    │ │ Module   │ │ Module   │ │ Module   │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘
└─────────────────────────────────────────────────────┘
```

**Implementation 1: Redux for State Sharing**

```typescript
// ✅ Shared Redux store with actions/reducers
// store/shared.reducer.ts
export const sharedReducer = (state = initialState, action) => {
  switch (action.type) {
    case '[Shared] Set User Session':
      return { ...state, userSession: action.payload };
    case '[Shared] Set Selected Order':
      return { ...state, selectedOrder: action.payload };
    case '[Shared] Set Notifications':
      return { ...state, notifications: action.payload };
    default:
      return state;
  }
};

// store/shared.actions.ts
export class SetUserSession {
  static readonly type = '[Shared] Set User Session';
  constructor(public payload: any) {}
}

export class SetSelectedOrder {
  static readonly type = '[Shared] Set Selected Order';
  constructor(public payload: Order) {}
}

// In any microfrontend
export class DashboardComponent {
  userSession$ = this.store.select(state => state.shared.userSession);
  selectedOrder$ = this.store.select(state => state.shared.selectedOrder);

  constructor(private store: Store) {}

  selectOrder(order: Order) {
    this.store.dispatch(new SetSelectedOrder(order));
  }
}

// Another microfrontend receives the action
export class OrderDetailsComponent implements OnInit {
  order: Order;

  constructor(private store: Store) {}

  ngOnInit() {
    this.store.select(state => state.shared.selectedOrder)
      .subscribe(order => {
        this.order = order;
        this.loadOrderDetails();
      });
  }
}
```

**Implementation 2: Event-Based Communication (RxJS Subjects)**

```typescript
// ✅ Centralized event bus for microfrontend communication
@Injectable({
  providedIn: 'root'
})
export class MicroFrontendEventBus {
  private eventSubject = new Subject<MicroEvent>();
  public events$ = this.eventSubject.asObservable();

  emit(event: MicroEvent) {
    this.eventSubject.next(event);
  }

  onEvent<T>(eventType: string): Observable<T> {
    return this.events$.pipe(
      filter(event => event.type === eventType),
      map(event => event.payload as T)
    );
  }
}

// Define events
export interface MicroEvent {
  type: string;
  payload: any;
  source: string;
  timestamp: Date;
}

// Usage: Dashboard emits order selection event
export class DashboardComponent {
  constructor(private eventBus: MicroFrontendEventBus) {}

  selectOrder(order: Order) {
    this.eventBus.emit({
      type: 'ORDER_SELECTED',
      payload: order,
      source: 'dashboard',
      timestamp: new Date()
    });
  }
}

// Usage: Orders module listens to selection
export class OrdersComponent implements OnInit {
  ngOnInit() {
    this.eventBus.onEvent<Order>('ORDER_SELECTED')
      .subscribe(order => {
        console.log('Order selected from dashboard:', order);
        this.loadOrderDetails(order);
      });
  }

  constructor(private eventBus: MicroFrontendEventBus) {}
}
```

**Implementation 3: Shared Data Service with Session Passing**

```typescript
// ✅ Main app component bridges communication
@Injectable({
  providedIn: 'root'
})
export class SharedDataService {
  private sessionData = new BehaviorSubject<SessionData | null>(null);
  private orderContext = new BehaviorSubject<any>(null);

  session$ = this.sessionData.asObservable().pipe(shareReplay(1));
  orderContext$ = this.orderContext.asObservable().pipe(shareReplay(1));

  constructor(private http: HttpClient) {
    this.initializeSession();
  }

  private initializeSession() {
    this.http.get<SessionData>('/api/session').subscribe(
      session => this.sessionData.next(session)
    );
  }

  updateOrderContext(context: any) {
    this.orderContext.next(context);
  }

  getSessionData(): SessionData | null {
    return this.sessionData.value;
  }
}

// Pass to child microfrontends via inputs/outputs
@Component({
  selector: 'app-shell',
  template: `
    <app-dashboard 
      [sessionData]="session$ | async"
      (orderSelected)="handleOrderSelection($event)">
    </app-dashboard>

    <app-orders 
      [selectedOrder]="selectedOrder"
      (orderUpdate)="handleOrderUpdate($event)">
    </app-orders>
  `
})
export class ShellComponent {
  session$ = this.sharedService.session$;
  selectedOrder: any;

  constructor(private sharedService: SharedDataService) {}

  handleOrderSelection(order: any) {
    this.selectedOrder = order;
    this.sharedService.updateOrderContext({ currentOrder: order });
  }

  handleOrderUpdate(updatedOrder: any) {
    this.sharedService.updateOrderContext({ currentOrder: updatedOrder });
  }
}
```

**Implementation 4: Window-based Global State (Alternative)**

```typescript
// ✅ Global window object for module-level communication
(window as any).__microFrontendState = {
  session: null,
  selectedOrder: null,
  notifications: []
};

// Update from one module
(window as any).__microFrontendState.selectedOrder = order;

// Listen from another
const unsubscribe = setInterval(() => {
  const currentOrder = (window as any).__microFrontendState.selectedOrder;
  if (currentOrder) {
    handleOrderUpdate(currentOrder);
  }
}, 100);
```

**Best Practices Summary:**
| Method | Use Case | Pros | Cons |
|---|---|---|---|
| **Redux** | Complex state, many interactions | Predictable, time-travel debug | More boilerplate |
| **Event Bus** | Loose coupling, async events | Simple, decoupled | Hard to track |
| **Shared Service** | Direct communication | Easy to understand | Tight coupling |
| **Window State** | Quick communication | Very simple | Unstructured, messy |

---

### 3. How to handle shared state management in microfrontends?

**Answer:**

Shared state management is critical for consistency across microfrontends. We implemented a multi-tier approach.

**Architecture:**

```text
┌─────────────────────────────────────────────┐
│  Global Redux Store (Shared State)           │
│  ├── User Session (set once, shared read)   │
│  ├── Global Notifications                   │
│  ├── Feature Flags                          │
│  └── Cross-module Context                   │
└─────────────────────────────────────────────┘
         ▲
         │ (read & dispatch)
    ┌────┴────────────────────────┐
    │                             │
┌───────────────┐        ┌──────────────────┐
│ Local Stores  │        │ Local Stores     │
│ (Dashboard)   │        │ (Orders Module)  │
└───────────────┘        └──────────────────┘
```

**Implementation 1: Root Redux Store Setup**

```typescript
// ✅ Shared Redux store - accessible to all modules
// store/root.reducer.ts
import { combineReducers } from '@ngrx/store';

export const rootReducer = combineReducers({
  // Shared global state
  shared: sharedReducer,
  session: sessionReducer,
  notifications: notificationsReducer,

  // Feature-specific (loaded dynamically)
  dashboard: dashboardReducer,
  orders: ordersReducer,
  inventory: inventoryReducer
});

// store/shared.state.ts
export interface SharedState {
  userSession: SessionData | null;
  tenantId: string | null;
  userPermissions: string[];
  selectedContext: any;
}

export const initialSharedState: SharedState = {
  userSession: null,
  tenantId: null,
  userPermissions: [],
  selectedContext: null
};

// store/shared.reducer.ts
export const sharedReducer = (
  state = initialSharedState,
  action: Action
): SharedState => {
  switch (action.type) {
    case '[Shared] Initialize Session':
      return { ...state, userSession: (action as any).payload };
    case '[Shared] Set Tenant':
      return { ...state, tenantId: (action as any).payload };
    case '[Shared] Update Permissions':
      return { ...state, userPermissions: (action as any).payload };
    case '[Shared] Set Context':
      return { ...state, selectedContext: (action as any).payload };
    default:
      return state;
  }
};
```

**Implementation 2: Effects for Side Effects**

```typescript
// ✅ Effects to handle async operations
import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError } from 'rxjs/operators';

@Injectable()
export class SharedEffects {
  initializeSession$ = createEffect(() =>
    this.actions$.pipe(
      ofType('[App] Initialize'),
      switchMap(() =>
        this.sessionService.loadSessionData().pipe(
          map(session => ({
            type: '[Shared] Initialize Session',
            payload: session
          })),
          catchError(error => {
            console.error('Session load failed', error);
            return of({})
          })
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private sessionService: SessionService
  ) {}
}
```

**Implementation 3: Selectors for Memoization**

```typescript
// ✅ Memoized selectors for performance
import { createFeatureSelector, createSelector } from '@ngrx/store';

export const selectSharedState = createFeatureSelector<SharedState>('shared');

export const selectUserSession = createSelector(
  selectSharedState,
  state => state.userSession
);

export const selectUserPermissions = createSelector(
  selectSharedState,
  state => state.userPermissions
);

export const selectCanDelete = createSelector(
  selectUserPermissions,
  permissions => permissions.includes('DELETE_ORDERS')
);

// Usage in components
export class OrdersComponent {
  canDelete$ = this.store.select(selectCanDelete);
  userSession$ = this.store.select(selectUserSession);

  constructor(private store: Store) {}
}
```

**Implementation 4: Shared Store Module**

```typescript
// ✅ Shared store module exported to all microfrontends
// shared-store.module.ts
import { NgModule } from '@angular/core';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

@NgModule({
  imports: [
    StoreModule.forFeature('shared', sharedReducer),
    EffectsModule.forFeature([SharedEffects])
  ]
})
export class SharedStoreModule {}

// Each microfrontend imports it
@NgModule({
  imports: [
    SharedStoreModule,
    StoreModule.forFeature('dashboard', dashboardReducer),
    EffectsModule.forFeature([DashboardEffects])
  ]
})
export class DashboardModule {}
```

**Implementation 5: State Hydration/Persistence**

```typescript
// ✅ Persist critical state to localStorage/sessionStorage
export class StateHydrationService {
  constructor(private store: Store) {}

  hydrateFromStorage() {
    const savedSession = sessionStorage.getItem('session');
    const savedPermissions = sessionStorage.getItem('permissions');

    if (savedSession) {
      this.store.dispatch({
        type: '[Shared] Initialize Session',
        payload: JSON.parse(savedSession)
      });
    }

    if (savedPermissions) {
      this.store.dispatch({
        type: '[Shared] Update Permissions',
        payload: JSON.parse(savedPermissions)
      });
    }
  }

  persistState(state: SharedState) {
    sessionStorage.setItem('session', JSON.stringify(state.userSession));
    sessionStorage.setItem('permissions', JSON.stringify(state.userPermissions));
  }
}

// Use in app initialization
export class AppInitializerService {
  constructor(private hydration: StateHydrationService) {}

  initialize() {
    this.hydration.hydrateFromStorage();
    // Load fresh data from server
    this.loadSessionFromServer();
  }
}
```

**Anti-patterns to Avoid:**

| ❌ Anti-pattern | Why it's bad | ✅ Solution |
|---|---|---|
| **Each module has own Redux store** | Data duplication, inconsistency | Single root store with shared feature |
| **Direct component communication** | Tight coupling, hard to maintain | Redux or Event Bus |
| **Storing large objects in state** | Memory bloat, serialization issues | Store IDs, fetch data when needed |
| **Mutable state updates** | Debugging nightmare, change detection issues | Immutable updates with operators |
| **Side effects in reducers** | Unpredictable behavior | Use Effects for async operations |

---

## Framework Upgrades & Migrations

### 4. What challenges did you face while upgrading .NET 7 to .NET 10? How did you implement and plan?

**Answer:**

The upgrade from .NET 7 to .NET 10 was significant, requiring careful planning and phased implementation. Here's our comprehensive approach:

**Pre-Upgrade Analysis:**

```text
Assessment Phase:
├── API Compatibility Check
├── Dependency Analysis (NuGet packages)
├── Performance Baseline Testing
├── Database Migration Path
└── Breaking Changes Review
```

**Challenge 1: Breaking Changes & API Deprecations**

**Problem:**
- `System.Net.Http.HttpClient` changes
- `Startup.cs` removed (Program.cs only)
- `IAsyncEnumerable` behavior changes
-Entity Framework Core query improvements

**Solution:**

```csharp
// Before .NET 7+: Startup.cs + Program.cs
// After .NET 8+: Program.cs only (top-level statements)

// ✅ New Program.cs only
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Build app
var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

**Challenge 2: Nullable Reference Types & Warnings**

**Problem:**
- .NET 8+ enables nullable reference types by default
- Hundreds of compiler warnings
- Breaking changes in API contracts

**Solution:**

```csharp
// Step 1: Enable nullable in csproj file
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>  // Enable nullable reference types
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
</PropertyGroup>

// Step 2: Fix code to handle nullability
// Before
public class UserService
{
    public User GetUser(int id)
    {
        return _context.Users.Find(id);  // Might return null
    }
}

// After
public class UserService
{
    public User? GetUser(int id)  // ? indicates nullable
    {
        return _context.Users.Find(id);
    }

    public User GetUserOrThrow(int id)
    {
        return _context.Users.Find(id) 
            ?? throw new InvalidOperationException("User not found");
    }
}

// Step 3: Update calling code
var user = userService.GetUser(1);
if (user == null)
{
    return NotFound();
}

// Or use null-coalescing
var username = user?.Name ?? "Guest";
```

**Challenge 3: Entity Framework Core 8+ Changes**

**Problem:**
- Complex LINQ queries changed
- Query compilation behavior updated
- Need to test all database access

**Solution:**

```csharp
// Before .NET 8 (may work differently)
var orders = context.Orders
    .AsEnumerable()  // Brings to memory
    .Where(o => o.Items.Sum(i => i.Quantity) > 10)  // Filtered in memory
    .ToList();

// After .NET 8 (optimized)
var orders = context.Orders
    .Where(o => o.Items.Sum(i => i.Quantity) > 10)  // Executes in DB
    .ToList();

// Example: Group by with new syntax
var ordersByCustomer = context.Orders
    .Where(o => o.Status == "Complete")
    .GroupBy(o => o.CustomerId)
    .Select(g => new 
    { 
        CustomerId = g.Key,
        TotalAmount = g.Sum(o => o.Amount),
        OrderCount = g.Count()
    })
    .ToList();
```

**Challenge 4: Dependency Version Conflicts**

**Problem:**
- NuGet packages not compatible with .NET 10
- Major version breaking changes
- Transitive dependency conflicts

**Solution Process:**

```bash
# Step 1: Update .NET target
dotnet nuget update --outdated
# OR manually edit .csproj

# Step 2: Update problematic packages one by one
dotnet package add EntityFrameworkCore --version 10.*
dotnet package add Serilog --version 5.*
dotnet package add Newtonsoft.Json --version 13.*

# Step 3: Check dependency tree
dotnet nuget locals all --clear
dotnet restore

# Step 4: Run tests
dotnet test

# Step 5: Resolve conflicts manually if needed
```

**Challenge 5: Async/Await & Performance Changes**

**Problem:**
- Async behavior changed in .NET 8+
- Some patterns became deprecated
- Performance characteristics changed

**Solution:**

```csharp
// Before .NET 8
public async Task<User> GetUserAsync(int id)
{
    // ❌ Old pattern - creates unnecessary Task
    return await _context.Users.FindAsync(id);
}

// After .NET 8 (optimized)
public async ValueTask<User?> GetUserAsync(int id)
{
    // ✅ Use ValueTask for frequently-called methods
    return await _context.Users.FindAsync(id);
}

// Performance-critical methods
public async ValueTask<bool> UserExistsAsync(int id)
{
    // ValueTask avoids allocation if result is synchronous
    return await _context.Users.AnyAsync(u => u.Id == id);
}
```

**Migration Plan & Timeline:**

```
Phase 1: Preparation (Week 1-2)
├── Audit code for breaking changes
├── Update csproj to .NET 10
├── Run build and identify issues
└── Document all warnings

Phase 2: Entity Framework (Week 2-3)
├── Review and test all database queries
├── Update LINQ expressions
├── Test against staging database
└── Performance test critical queries

Phase 3: Core Infrastructure (Week 3-4)
├── Update authentication/authorization
├── Update logging infrastructure
├── Update DI configuration
├── Update API middleware

Phase 4: Testing & Validation (Week 4-5)
├── Unit test suite
├── Integration test suite  
├── Performance testing
├── Load testing

Phase 5: Deployment (Week 5-6)
├── Deploy to staging
├── Smoke tests & UAT
├── Monitor performance metrics
├── Gradual rollout to production
└── Rollback plan if issues
```

**Verification Checklist:**

```csharp
// ✅ .NET Upgrade Validation
public class UpgradeValidationTests
{
    [Fact]
    public async Task VerifyDatabaseConnectivity()
    {
        var result = await _dbContext.Database.CanConnectAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyAsyncQueries()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .ToListAsync();
        Assert.NotEmpty(orders);
    }

    [Fact]
    public void VerifyNullablityHandling()
    {
        User? user = null;
        var name = user?.Name ?? "Guest";
        Assert.Equal("Guest", name);
    }

    [Fact]
    public async Task VerifyBackgroundServices()
    {
        var service = _serviceProvider.GetRequiredService<IHostedService>();
        Assert.NotNull(service);
    }
}
```

---

### 5. What challenges did you face while upgrading Angular 12 to Angular 17? How did you implement and plan?

**Answer:**

Angular 12 to 17 was a major upgrade spanning new initialization patterns, signals, control flow, and numerous API changes. Here's our detailed implementation:

**Pre-Upgrade Analysis:**

```text
Assessment Phase:
├── Dependency version analysis
├── Breaking changes review
├── Module removal timeline
├── Standalone components evaluation
└── Performance baseline testing
```

**Challenge 1: Old NgModuleBootstrap → Standalone Components**

**Problem:**
- Angular 14+ deprecates NgModule patterns
- Need to support both old and new styles during migration
- Testing infrastructure changed

**Before (Angular 12: NgModule):**
```typescript
// app.module.ts
@NgModule({
  declarations: [AppComponent, DashboardComponent],
  imports: [
    BrowserModule,
    CommonModule,
    HttpClientModule,
    RouterModule.forRoot(routes)
  ],
  providers: [UserService, OrderService],
  bootstrap: [AppComponent]
})
export class AppModule { }

// main.ts
platformBrowserDynamic()
  .bootstrapModule(AppModule)
  .catch(err => console.error(err));
```

**After (Angular 17: Standalone):**
```typescript
// app.config.ts - New pattern
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor, loggingInterceptor])
    ),
    // Add services
    UserService,
    OrderService
  ]
};

// main.ts - Simplified
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app.config';
import { AppComponent } from './app.component';

bootstrapApplication(AppComponent, appConfig).catch(console.error);

// app.component.ts - Standalone  
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {}
```

**Migration Strategy:**
```typescript
// Phase 1: Keep both systems during transition
@NgModule({
  imports: [
    NgModelStandaloneComponent,  // Import standalone component
    CommonModule
  ]
})
export class LegacyModule { }

// Phase 2: Convert components gradually
// BEFORE
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent { }

// AFTER
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, SharedModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent { }
```

**Challenge 2: New Control Flow Syntax (if/for/switch)**

**Problem:**
- `*ngIf`, `*ngFor`, `*ngSwitch` are being phased out
- New syntax requires template changes
- Must test all affected templates

**Before (Angular 12):**
```html
<!-- Templates with directives -->
<div *ngIf="user; else noUser">
  <h1>{{ user.name }}</h1>
  <p *ngFor="let order of user.orders">
    {{ order.name }}
  </p>
</div>
<ng-template #noUser>
  <p>No user logged in</p>
</ng-template>

<div [ngSwitch]="status">
  <p *ngSwitchCase="'active'">Active</p>
  <p *ngSwitchCase="'inactive'">Inactive</p>
  <p *ngSwitchDefault>Unknown</p>
</div>
```

**After (Angular 17):**
```html
<!-- New control flow syntax (built-in, no imports needed) -->
@if (user) {
  <h1>{{ user.name }}</h1>
  @for (let order of user.orders) {
    <p>{{ order.name }}</p>
  }
} @else {
  <p>No user logged in</p>
}

@switch (status) {
  @case ('active') { <p>Active</p> }
  @case ('inactive') { <p>Inactive</p> }
  @default { <p>Unknown</p> }
}

<!-- Deferred rendering (new performance feature) -->
@defer (on viewport) {
  <app-heavy-component />
} @placeholder {
  <p>Loading...</p>
}
```

**Automated Migration:**
```bash
# Angular provides automatic migration
ng update @angular/core

# This automatically converts:
# - *ngIf to @if
# - *ngFor to @for
# - But manual review recommended
```

**Challenge 3: Signals Introduction**

**Problem:**
- Signals are new reactive primitive in Angular
- Replaces some RxJS patterns
- New change detection strategy

**Before (RxJS pattern):**
```typescript
@Component({
  selector: 'app-counter',
  template: `
    <p>Count: {{ count$ | async }}</p>
    <button (click)="increment()">+</button>
  `
})
export class CounterComponent {
  private countSubject = new BehaviorSubject(0);
  count$ = this.countSubject.asObservable();

  increment() {
    const current = this.countSubject.value;
    this.countSubject.next(current + 1);
  }
}
```

**After (Signals pattern):**
```typescript
@Component({
  selector: 'app-counter',
  standalone: true,
  template: `
    <p>Count: {{ count() }}</p>
    <button (click)="increment()">+</button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CounterComponent {
  count = signal(0);  // Reactive primitive

  increment() {
    this.count.update(c => c + 1);
  }
}

// Advanced signals
export class UserComponent {
  firstName = signal('John');
  lastName = signal('Doe');

  // Computed signal (memoized)
  fullName = computed(() => `${this.firstName()} ${this.lastName()}`);

  // Effect (runs when dependencies change)
  logName = effect(() => {
    console.log(`User: ${this.fullName()}`);
  });
}
```

**Challenge 4: HttpClient & Interceptor Changes**

**Problem:**
- HttpInterceptor interface changed
- Functional interceptors are new pattern
- Injectable patterns updated

**Before (Angular 12):**
```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
    return next.handle(cloned);
  }
}

@NgModule({
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ]
})
export class AppModule { }
```

**After (Angular 15+):**
```typescript
// Functional interceptor (preferred)
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req);
};

// Register in config
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor, loggingInterceptor])
    )
  ]
};
```

**Challenge 5: Routing Changes**

**Problem:**
- RouterModule deprecated
- Router now provided via provideRouter
- Lazy loading syntax simplified

**Before (Angular 12):**
```typescript
const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: 'orders',
    loadChildren: () => import('./orders/orders.module')
      .then(m => m.OrdersModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)]
})
export class AppModule { }
```

**After (Angular 14+):**
```typescript
const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: 'orders',
    loadChildren: () => import('./orders/orders-routes')
      .then(m => m.ordersRoutes)  // Routes array, not module
  }
];

// orders-routes.ts
export const ordersRoutes: Routes = [
  { path: '', component: OrderListComponent },
  { path: ':id', component: OrderDetailComponent }
];

// app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes)
  ]
};
```

**Challenge 6: Major Breaking Changes**

| Feature | Change | Migration |
|---|---|---|
| **CommonModule** | No longer needed in standalone | Remove from imports |
| **FormsModule** | Still needed, must import | `imports: [FormsModule]` |
| **RxJS** | Some operators deprecated | Update to new operator versions |
| **Decorators** | stricter evaluation order | Review decorator application order |
| **Typescript** | Requires 5.2+ | Update TypeScript version |

**Migration Plan & Timeline:**

```
Phase 1: Preparation (Week 1-2)
├── Run ng update @angular/core@17
├── Review and fix breaking changes
├── Update all dependencies
└── Update TypeScript version

Phase 2: Standalone Migration (Week 2-4)
├── Convert components to standalone gradually
├── Update app.config.ts and main.ts
├── Convert feature modules to standalone
└── Test each feature

Phase 3: Control Flow Migration (Week 4-5)
├── Run automated migration (ng update)
├── Review template changes
├── Test user interactions
└── Browser compatibility check

Phase 4: Signals & RxJS Update (Week 5-6)
├── Identify high-value signal conversions
├── Convert components to signals
├── Update change detection strategies
└── Performance testing

Phase 5: Testing & Validation (Week 6-7)
├── Unit test updates
├── E2E test updates
├── Browser compatibility testing
├── Performance benchmarking

Phase 6: Deployment (Week 7-8)
├── Staging deployment
├── Cross-browser testing
├── Performance validation
└── Gradual production rollout
```

**Verification Checklist:**

```typescript
// ✅ Angular Upgrade Validation
describe('Angular 17 Upgrade Validation', () => {

  it('should support standalone components', () => {
    expect(DashboardComponent.ɵcmp).toBeDefined();
  });

  it('should support new control flow syntax', () => {
    const fixture = TestBed.createComponent(TestComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Expected text');
  });

  it('should support signals', () => {
    const count = signal(0);
    expect(count()).toBe(0);
    count.set(5);
    expect(count()).toBe(5);
  });

  it('should support functional interceptors', (done) => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor]))
      ]
    });

    const http = TestBed.inject(HttpClient);
    http.get('/test').subscribe(() => done());
  });

  it('should work with new routing', () => {
    const router = TestBed.inject(Router);
    expect(router.config.length).toBeGreaterThan(0);
  });
});
```

---

## Key Takeaways

| Topic | Key Learning |
|---|---|
| **Microfrontends** | Use single-spa + Redux for state + Event bus for communication |
| **State Management** | Centralized Redux store with memoized selectors |
| **.NET 7→10 Upgrade** | Plan phased migration, handle nullable types, test database queries |
| **Angular 12→17 Upgrade** | Adopt standalone components, signals, and new control flow syntax |