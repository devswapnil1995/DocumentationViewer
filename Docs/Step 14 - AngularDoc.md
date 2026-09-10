## Angular Rendering Strategies

Rendering strategy decides **where and when Angular generates the HTML**. Angular has 3 main strategies: **CSR, SSG, and SSR**. 

**`1. CSR — Client-Side Rendering`**

Angular renders the page **in the browser after JavaScript loads**.

```text
Browser
  ↓
Download JS
  ↓
Angular starts
  ↓
Render HTML
```

**Best for**

* Admin dashboards
* Internal applications
* Highly interactive applications
* Apps where SEO isn't important

Example:

```text
/company-admin/dashboard
```

**Pros**

* Simple
* Minimal server requirements
* Great for interactive applications

**Cons**

* Initial page can take longer to display
* Poorer SEO because content requires JavaScript execution 

**Angular default rendering strategy.** 

---

**`2. SSG — Static Site Generation`**

HTML is generated **at build time**.

```text
Build
 ↓
Generate HTML
 ↓
Deploy
 ↓
User requests page
 ↓
Ready-made HTML
```

**Best for**

* Marketing websites
* Blogs
* Documentation
* Product catalogs with relatively stable content

Example:

```text
/products/laptop-123
```

if the content doesn't change frequently.

**Pros**

* Excellent SEO
* Very fast initial load
* Can be served through CDN
* No server rendering required at request time

**Cons**

* Content changes generally require rebuilding/redeploying
* Not ideal for user-specific or real-time content 

---

**`3. SSR — Server-Side Rendering`**

HTML is generated **on the server for the initial request**.

```text
Browser
   ↓
Server
   ↓
Render Angular HTML
   ↓
Browser receives HTML
   ↓
Hydration
   ↓
Interactive Angular app
```

**Best for**

* E-commerce
* News websites
* Public pages requiring SEO
* Dynamic content

Example:

```text
/product/iphone
```

where price, inventory, or other information changes frequently.

**Pros**

* Excellent SEO
* Fast initial content visibility
* Dynamic content can be generated per request

**Cons**

* Requires a server
* Higher server cost
* Interactivity comes after hydration 

---

### CSR vs SSG vs SSR

|                 | CSR       | SSG                 | SSR         |
| --------------- | --------- | ------------------- | ----------- |
| HTML generated  | Browser   | Build time          | Server      |
| SEO             | ❌ Poor    | ✅ Excellent         | ✅ Excellent |
| Initial load    | Slower    | 🚀 Fastest          | 🚀 Fast     |
| Dynamic data    | ✅         | ⚠️ Limited          | ✅           |
| Server required | Minimal   | No rendering server | Yes         |
| Best for        | Dashboard | Blog/marketing      | E-commerce  |

------
------

## Angular Router Lifecycle & Events

| Event                  | Meaning                                   |
| ---------------------- | ----------------------------------------- |
| `NavigationStart`      | Navigation begins                         |
| `RouteConfigLoadStart` | Lazy route configuration starts loading   |
| `RouteConfigLoadEnd`   | Lazy route configuration finished loading |
| `RoutesRecognized`     | Angular identifies the matching route     |
| `GuardsCheckStart`     | Route guards start                        |
| `GuardsCheckEnd`       | Guard evaluation finishes                 |
| `ResolveStart`         | Resolver starts fetching data             |
| `ResolveEnd`           | Resolver finishes                         |
| `ActivationStart`      | Route component activation starts         |
| `ActivationEnd`        | Route activation finishes                 |
| `NavigationEnd`        | Navigation successfully completed         |
| `NavigationCancel`     | Navigation was cancelled                  |
| `NavigationError`      | Navigation failed                         |
| `NavigationSkipped`    | Navigation was skipped, e.g. same URL     |
| `Scroll`               | Router scroll event                       |

--------
--------

## Angular Forms

Angular provides **two traditional approaches** for forms:

1. **Reactive Forms** — form model is explicitly created in TypeScript.
2. **Template-driven Forms** — form model is implicitly created through template directives.

Angular's current docs also introduce **Signal Forms**, but for interviews and existing Angular applications, Reactive Forms and Template-driven Forms remain essential. 

---

**Reactive vs Template-driven**

|             | Reactive Forms                          | Template-driven             |
| ----------- | --------------------------------------- | --------------------------- |
| Form model  | Explicit in TS                          | Implicit in template        |
| Main APIs   | `FormControl`, `FormGroup`, `FormArray` | `ngModel`, `NgForm`         |
| Data flow   | Synchronous                             | Asynchronous                |
| Data model  | Structured / immutable                  | Mutable                     |
| Validation  | Functions                               | Directives                  |
| Testing     | Easier                                  | More dependent on rendering |
| Scalability | ⭐⭐⭐⭐⭐                                   | ⭐⭐⭐                         |
| Best for    | Complex forms                           | Simple forms                |

**Interview answer:**

> Use **Reactive Forms** for complex, scalable, reusable and testable forms. Use **Template-driven Forms** for simple forms with minimal logic. 

---

**Reactive Forms**

The form model is created in the component.

**Simple `FormControl`**

```ts
import { FormControl, ReactiveFormsModule } from '@angular/forms';

export class UserComponent {
  name = new FormControl('');
}
```

```html
<input [formControl]="name">

<p>{{ name.value }}</p>
```

Here:

```text
FormControl
     ↕
   Input
```

The **form model is the source of truth**. 

---

**FormGroup**

Used to group multiple controls.

```ts
userForm = new FormGroup({
  name: new FormControl(''),
  email: new FormControl(''),
  age: new FormControl(0)
});
```

Template:

```html
<form [formGroup]="userForm">

  <input formControlName="name">
  <input formControlName="email">
  <input formControlName="age">

</form>
```

Get values:

```ts
this.userForm.value;
```

Example:

```ts
{
  name: 'Swapnil',
  email: 'swapnil@test.com',
  age: 31
}
```

### Mental model

```text
FormGroup
 ├── name    → FormControl
 ├── email   → FormControl
 └── age     → FormControl
```

---

**FormArray**

Used when the number of controls is **dynamic**.

Example: multiple phone numbers.

```ts
phones = new FormArray([
  new FormControl('')
]);
```

Add:

```ts
this.phones.push(new FormControl(''));
```

Remove:

```ts
this.phones.removeAt(0);
```

Template:

```html
<div formArrayName="phones">
  @for (phone of phones.controls; track $index) {
    <input [formControlName]="$index">
  }
</div>
```

**When to use**

> `FormGroup` → fixed fields

> `FormArray` → dynamic/repeating fields

---

**Common Form Classes**

Angular forms are mainly built around:

| Class                  | Purpose                                              |
| ---------------------- | ---------------------------------------------------- |
| `FormControl`          | One field                                            |
| `FormGroup`            | Group of controls                                    |
| `FormArray`            | Dynamic collection                                   |
| `ControlValueAccessor` | Bridge between Angular forms and custom/DOM controls |



---

**Template-driven Forms**

Uses `FormsModule` and `[(ngModel)]`.

```ts
import { FormsModule } from '@angular/forms';

name = '';
```

```html
<input [(ngModel)]="name">

<p>{{ name }}</p>
```

Angular's `NgModel` directive internally creates and manages a `FormControl`. 

**Form example**

```html
<form #userForm="ngForm" (ngSubmit)="submit(userForm)">

  <input
    name="name"
    [(ngModel)]="name"
    required
  >

  <button type="submit">Save</button>

</form>
```

---

**Reactive Form Validation**

Angular provides built-in validators.

```ts
import { Validators } from '@angular/forms';

userForm = new FormGroup({
  name: new FormControl('', Validators.required),

  email: new FormControl(
    '',
    [Validators.required, Validators.email]
  )
});
```

Check:

```ts
this.userForm.valid
this.userForm.invalid
```

Check individual control:

```ts
this.userForm.controls.email.invalid
```

Common validators:

```ts
Validators.required
Validators.email
Validators.minLength(5)
Validators.maxLength(50)
Validators.min(18)
Validators.max(100)
Validators.pattern(...)
```

---

**`valueChanges`**

Very important for interviews.

`FormControl` and other form controls expose `valueChanges` as an Observable.

```ts
this.userForm.controls.email.valueChanges
  .subscribe(value => {
    console.log(value);
  });
```

Useful for:

* search/autocomplete
* dependent fields
* dynamic validation
* reacting to user input

Example:

```ts
this.search.valueChanges
  .pipe(
    debounceTime(300),
    distinctUntilChanged()
  )
  .subscribe(value => {
    // API call
  });
```

---

**Updating Form Values**

**`setValue()`**

Must provide the complete structure.

```ts
this.userForm.setValue({
  name: 'Swapnil',
  email: 'test@test.com',
  age: 31
});
```

**`patchValue()`**

Updates only specified fields.

```ts
this.userForm.patchValue({
  name: 'Swapnil'
});
```

> `setValue()` → complete form structure required

> `patchValue()` → partial update allowed

---

**Form State**

Important properties:

| State       | Meaning                        |
| ----------- | ------------------------------ |
| `valid`     | Validation passed              |
| `invalid`   | Validation failed              |
| `touched`   | User interacted and left field |
| `untouched` | User hasn't left/interacted    |
| `dirty`     | Value changed                  |
| `pristine`  | Value hasn't changed           |
| `pending`   | Async validation in progress   |

---

**`ControlValueAccessor`**

`ControlValueAccessor` is the bridge between Angular Forms and a custom form control.

Example:

```text
Angular FormControl
       ↕
ControlValueAccessor
       ↕
Custom Component
```

Useful when creating custom controls such as:

* custom dropdown
* date picker
* rich text editor
* custom input component

Angular uses value accessors internally for native controls as well. 

---------
---------

## Angular Signal Forms

**Signal Forms** is Angular's newer form system built on **Signals**. It provides automatic two-way synchronization, type-safe field access, and schema-based validation. 

> **Important:** Signal Forms require **Angular v21+** and are currently best suited for **new applications built with Signals**. For existing applications using Reactive Forms, Angular recommends continuing with Reactive Forms when production stability is important. 

**Why Signal Forms?**

Traditional forms can require separate handling for:

```text
Form State
   ↓
Validation
   ↓
Errors
   ↓
UI synchronization
```

Signal Forms try to simplify this with:

* ✅ Automatic state synchronization
* ✅ Type-safe form fields
* ✅ Centralized validation
* ✅ Signal-based reactive state
* ✅ Less boilerplate

---

**2. Basic Setup**

Signal Forms are included in `@angular/forms`.

Import from:

```ts
import {
  form,
  FormField,
  required,
  email
} from '@angular/forms/signals';
```

`FormField` must be imported into the component when binding fields to HTML inputs. 

---

**Basic Example**

Suppose we have:

```ts
user = signal({
  name: '',
  email: ''
});
```

Create a form:

```ts
userForm = form(this.user);
```

Template:

```html
<input [formField]="userForm.name">

<input [formField]="userForm.email">
```

The important idea is:

```text
Signal Model
     ↕
 Signal Form
     ↕
  HTML Field
```

Changes are synchronized automatically.

---

**Validation**

Validation is defined using a schema.

For example:

```ts
userForm = form(this.user, (schema) => {
  required(schema.name);
  required(schema.email);
  email(schema.email);
});
```

This centralizes validation rules instead of scattering them across the template and component.

Angular provides validation functions such as:

```ts
required
email
```

and the Signal Forms API provides a broader validation system. 

---

### Signal Forms vs Reactive Forms

|                     | Signal Forms          | Reactive Forms                     |
| ------------------- | --------------------- | ---------------------------------- |
| Foundation          | Signals               | RxJS + form controls               |
| Model               | Signal-based          | `FormControl` / `FormGroup`        |
| Binding             | `[formField]`         | `[formControl]`, `formControlName` |
| Validation          | Schema-based          | Validators                         |
| Type safety         | Strong                | Strong with typed forms            |
| Angular requirement | v21+                  | Mature/established                 |
| Best for            | New signal-based apps | Existing/complex production apps   |
| Stability           | Newer                 | More mature                        |

Angular specifically states that Signal Forms work best for new applications built with signals, while Reactive Forms remain a solid choice for existing applications or when production stability guarantees are important. 

--------
--------

## Angular HTTP Client

Angular provides `HttpClient` from `@angular/common/http` for communicating with backend APIs over HTTP. It supports typed responses, error handling, interceptors, and testing utilities. 

---

**Setup `HttpClient`**

In modern standalone Angular, configure it with `provideHttpClient()`:

```ts
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient()
  ]
};
```

Then inject it:

```ts
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';

private http = inject(HttpClient);
```

---

**GET Request**

Suppose .NET API:

```text
GET /api/users
```

Angular:

```ts
interface User {
  id: number;
  name: string;
  email: string;
}

getUsers() {
  return this.http.get<User[]>('/api/users');
}
```

`HttpClient` methods return **Observables**.

```ts
this.getUsers().subscribe(users => {
  console.log(users);
});
```
---

**POST Request**

```ts
createUser(user: User) {
  return this.http.post<User>('/api/users', user);
}
```

Usage:

```ts
this.userService.createUser(user)
  .subscribe(response => {
    console.log(response);
  });
```

---

**PUT / PATCH / DELETE**

```ts
// PUT
this.http.put<User>('/api/users/1', user);

// PATCH
this.http.patch<User>('/api/users/1', {
  name: 'Swapnil'
});

// DELETE
this.http.delete<void>('/api/users/1');
```
---

Use an **HTTP interceptor** for cross-cutting concerns such as:

```text
Authorization
Correlation ID
Logging
Error handling
Loading indicator
```

Angular officially supports request/response interception. 

---

**Error Handling**

Use RxJS `catchError()`:

```ts
import { catchError } from 'rxjs';

getUsers() {
  return this.http.get<User[]>('/api/users').pipe(
    catchError(error => {
      console.error(error);
      throw error;
    })
  );
}
```

Common HTTP errors:

```text
400 → Bad Request
401 → Unauthorized
403 → Forbidden
404 → Not Found
500 → Server Error
```

---

**HttpClient is Observable-based**

Important interview point:

```ts
this.http.get<User[]>('/api/users')
```

returns:

```text
Observable<User[]>
```

HTTP requests are generally **cold Observables** — the request is made when the Observable is subscribed to.

```ts
const request$ = this.http.get('/api/users');

// No request yet

request$.subscribe();

// HTTP request happens
```

-----------
-----------

## What is RxJS?

> **RxJS (Reactive Extensions for JavaScript)** is a library for working with **asynchronous and event-based data using Observables**.

Angular uses RxJS heavily for **HTTP calls, router events, forms, and other reactive operations**.

`1. Observable`

> An **Observable** represents a stream of values that can arrive over time.

```ts
import { Observable } from 'rxjs';

const numbers$ = new Observable(observer => {
  observer.next(1);
  observer.next(2);
  observer.next(3);
  observer.complete();
});
```

Subscribe to it:

```ts
numbers$.subscribe(value => {
  console.log(value);
});
```

Output:

```text
1
2
3
```

`$` is a common naming convention indicating an Observable.

---

`2. Why RxJS is useful`

Without RxJS, asynchronous operations can become difficult to manage:

```text
API calls
User input
Timers
WebSocket messages
Router events
Multiple async operations
       ↓
     RxJS
       ↓
Manage streams declaratively
```

---

`3. Operators`

RxJS provides operators to transform and control streams.

---

`4. Observable vs Promise`

| Observable                    | Promise                               |
| ----------------------------- | ------------------------------------- |
| Multiple values               | Usually one value                     |
| Can be cancelled/unsubscribed | Cannot normally be cancelled directly |
| Lazy by default               | Starts immediately                    |
| Many RxJS operators           | `.then()`, `.catch()`                 |
| Excellent for streams         | Good for single async result          |

Example:

```ts
// Promise
Promise<User>

// Observable
Observable<User>
```

Angular `HttpClient` returns **Observables**:

```ts
this.http.get<User>('/api/user');
```

---

`5. Subscription`

An Observable generally starts producing values when subscribed to:

```ts
users$
  .subscribe(users => {
    console.log(users);
  });
```

You can also handle:

```ts
subscribe({
  next: value => {},
  error: error => {},
  complete: () => {}
});
```

---

`6. Important RxJS Operators`

For interviews, know these:

| Operator               | Purpose                                   |
| ---------------------- | ----------------------------------------- |
| `map`                  | Transform values                          |
| `filter`               | Filter values                             |
| `tap`                  | Side effects/debugging                    |
| `switchMap`            | Switch to latest Observable               |
| `mergeMap`             | Run/merge concurrent Observables          |
| `concatMap`            | Execute Observables sequentially          |
| `exhaustMap`           | Ignore new emissions while current runs   |
| `debounceTime`         | Wait for quiet period                     |
| `distinctUntilChanged` | Ignore duplicate consecutive values       |
| `catchError`           | Handle errors                             |
| `finalize`             | Execute cleanup                           |
| `take`                 | Take specific number of values            |
| `takeUntil`            | Stop based on another Observable          |
| `forkJoin`             | Wait for multiple Observables to complete |

`The *Map operators are especially important:`

```text
switchMap  → cancel previous, use latest
mergeMap   → run concurrently
concatMap  → queue sequentially
exhaustMap → ignore new while busy
```

---

`7. RxJS in Angular`

Common Angular examples:

`HTTP`

```ts
users$ = this.http.get<User[]>('/api/users');
```

`Router`

```ts
this.router.events.subscribe(event => {
  // router event
});
```

`Reactive Forms`

```ts
this.searchControl.valueChanges.subscribe(value => {
  // react to input
});
```

So:

```text
Angular
 ├── HttpClient      → Observable
 ├── Router Events   → Observable
 ├── Form Changes    → Observable
 └── RxJS            → operators + stream processing
```

---
---

## Subjects in RxJS

> A **Subject** is a special type of Observable that can do **two things**:

1. Be **subscribed to** like an Observable.
2. **Emit values** using `next()` like an Observer.

**1. Basic Subject**

```ts
import { Subject } from 'rxjs';

const subject = new Subject<number>();

subject.subscribe(value => {
  console.log('A:', value);
});

subject.subscribe(value => {
  console.log('B:', value);
});

subject.next(10);
subject.next(20);
```

Output:

```text
A: 10
B: 10
A: 20
B: 20
```

**Key point**

```ts
subject.next(10);
```

broadcasts `10` to all current subscribers.

---

**2. Observable vs Subject**

| Observable                                       | Subject                              |
| ------------------------------------------------ | ------------------------------------ |
| Usually unicast                                  | Multicast                            |
| Consumer listens                                 | Subject can emit                     |
| `subscribe()`                                    | `subscribe()` + `next()`             |
| Each subscription can have independent execution | Subscribers share the same emissions |

Simple interview definition:

> **Observable is a data producer that consumers subscribe to; Subject is both an Observable and an Observer and can multicast values to multiple subscribers.**

---

**3 `BehaviorSubject`**

`BehaviorSubject` is a Subject that:

* Requires an **initial value**
* Stores the **latest value**
* Immediately gives the latest value to a new subscriber

```ts
const user$ = new BehaviorSubject<string>('Guest');

user$.next('Swapnil');

user$.subscribe(value => {
  console.log(value);
});
```

Output:

```text
Swapnil
```

Even though the subscription happened **after** `next()`.

**Very common use**

Sharing current application state:

```ts
private userSubject =
  new BehaviorSubject<User | null>(null);

user$ = this.userSubject.asObservable();
```

Update:

```ts
this.userSubject.next(user);
```

Read current value:

```ts
this.userSubject.value;
```

---

**4. `ReplaySubject`**

`ReplaySubject` stores previous emissions and **replays them to new subscribers**.

```ts
const subject = new ReplaySubject<number>(2);

subject.next(10);
subject.next(20);
subject.next(30);

subject.subscribe(value => {
  console.log(value);
});
```

Output:

```text
20
30
```

Because we configured:

```ts
new ReplaySubject(2)
```

to replay the last **2 values**.

It can also replay values based on a time window.

---

**5. `AsyncSubject`**

`AsyncSubject` emits **only the last value**, and only when the Subject completes.

```ts
const subject = new AsyncSubject<number>();

subject.subscribe(value => {
  console.log(value);
});

subject.next(10);
subject.next(20);
subject.next(30);

subject.complete();
```

Output:

```text
30
```

Think:

```text
10 → 20 → 30 → complete()
                  ↓
                30 only
```

It is less commonly used in normal Angular development.

---

### Four Important Subjects

| Subject           | Initial Value | New Subscriber Gets         |
| ----------------- | ------------- | --------------------------- |
| `Subject`         | ❌             | Future values only          |
| `BehaviorSubject` | ✅             | Latest value                |
| `ReplaySubject`   | ❌             | Previous buffered values    |
| `AsyncSubject`    | ❌             | Last value after completion |

### Easy memory trick

```text
Subject
   ↓
Nothing from past

BehaviorSubject
   ↓
Current value

ReplaySubject
   ↓
Past values

AsyncSubject
   ↓
Last value after complete
```
----
----

## RxJS Operators

Your notes cover **Creation, Transformation, and Filtering operators**. For interview preparation, I would organize them like this:

**1. Creation Operators**

Create Observables from different sources. 

| Operator      | What it does                              | Example use                  |
| ------------- | ----------------------------------------- | ---------------------------- |
| `of()`        | Emits given values                        | Mock data                    |
| `from()`      | Converts array/Promise/iterable           | Convert Promise → Observable |
| `interval()`  | Emits numbers periodically forever        | Polling                      |
| `timer()`     | Delayed/repeated emissions                | Delayed API/polling          |
| `fromEvent()` | Converts events to Observable             | Click/scroll/input           |
| `range()`     | Emits number sequence                     | Pagination/helpers           |
| `defer()`     | Creates Observable lazily on subscription | Dynamic configuration        |

**Most important**

```ts
of(1, 2, 3)
```

→ emits `1`, `2`, `3`.

```ts
from([1, 2, 3])
```

→ also emits `1`, `2`, `3`, but the important difference is that `from()` converts an existing iterable/Promise into an Observable.

---

**2. Transformation Operators**

These change or flatten emitted values.

`map`

Transforms every value. 

```ts
of(1, 2, 3).pipe(
  map(x => x * 10)
);
```

```text
1 → 10
2 → 20
3 → 30
```

**Use:** Transform API response.

---

`switchMap`

Switches to the latest inner Observable and unsubscribes from the previous one. 

```ts
search$.pipe(
  switchMap(query => this.http.get(`/api/search?q=${query}`))
);
```

**Use:** Search/autocomplete.

```text
A ────────X
           \
B ───────────────→ Result B
```

**Remember:** `switchMap` = **latest wins**.

---

`mergeMap`

Runs inner Observables concurrently. 

```ts
ids$.pipe(
  mergeMap(id => this.http.get(`/api/users/${id}`))
);
```

**Use:** Multiple independent API calls.

**Remember:** `mergeMap` = **parallel**.

---

`concatMap`

Waits for the current Observable to complete before starting the next. 

```ts
files$.pipe(
  concatMap(file => upload(file))
);
```

**Use:** Upload/process items **one by one**.

**Remember:** `concatMap` = **queue**.

---

`exhaustMap`

Ignores new emissions while the current Observable is running. 

```ts
submitClick$.pipe(
  exhaustMap(() => this.http.post('/api/order', order))
);
```

**Use:** Prevent double form submission.

**Remember:** `exhaustMap` = **ignore while busy**.


| Operator     | Behavior        | Typical use          |
| ------------ | --------------- | -------------------- |
| `switchMap`  | Cancel previous | Search               |
| `mergeMap`   | Parallel        | Independent requests |
| `concatMap`  | Sequential      | Ordered operations   |
| `exhaustMap` | Ignore new      | Submit button        |

### Easy memory trick

```text
switchMap  → Latest
mergeMap   → Parallel
concatMap  → Queue
exhaustMap → Busy → Ignore
```

---

**3. Filtering Operators**

These control **which values pass through**.

| Operator               | Meaning                             |
| ---------------------- | ----------------------------------- |
| `filter`               | Only matching values                |
| `take`                 | First N values                      |
| `first`                | First matching value                |
| `last`                 | Last matching value                 |
| `takeWhile`            | Continue while condition is true    |
| `skip`                 | Skip first N                        |
| `debounceTime`         | Wait for silence                    |
| `distinctUntilChanged` | Ignore consecutive duplicates       |
| `throttleTime`         | Emit, then ignore for time window   |
| `auditTime`            | Emit latest value after time window |

Your file covers these filtering operators in this section. 

---

**`debounceTime`**

Very important for search.

```ts
search.valueChanges.pipe(
  debounceTime(500)
);
```

Waits until the user stops typing.

```text
S → Sw → Swa → Swap → [500ms silence] → API
```

---

**`distinctUntilChanged`**

Prevents consecutive duplicate values.

```ts
of(1, 1, 2, 2, 3).pipe(
  distinctUntilChanged()
);
```

Result:

```text
1
2
3
```

Useful with form values/search. 

---

**`throttleTime` vs `debounceTime`**

This is commonly asked.

`debounceTime`

**Wait until activity stops.**

```text
Typing: ███████████____
                 ↓
               emit
```

Use for:

> Search/autocomplete.

`throttleTime`

**Emit immediately, then limit frequency.**

```text
█____█____█____█
↓
emit
```

Use for:

> Scroll/mousemove/resize events.

Your notes describe `throttleTime` as emitting the first value and ignoring others during the time window. 

---

**`auditTime`**

Waits for the time window and emits the **latest** value.

```text
Values: 1 2 3 4 5
        └──1 sec──┘
                 ↓
                 5
```

Useful for scroll tracking. 

> takeUntil uses another Observable as a signal to stop a subscription, while Angular's takeUntilDestroyed() automatically completes the Observable when the Angular context is destroyed, reducing manual cleanup code.
-------
--------