## Angular Fundamentals

**What is Angular?**

> Angular is a TypeScript-based frontend framework developed by Google for building web applications.

It provides a complete ecosystem for building applications:

```text
Angular
│
├── Components
├── Templates
├── Routing
├── Forms
├── HTTP
├── Dependency Injection
├── Signals
├── RxJS
├── Directives
└── Pipes
```

Unlike a simple JavaScript library, Angular gives you many things you need to build a complete application.

---

**Angular Application Structure**

A modern Angular application looks roughly like:

```text
my-angular-app/
│
├── src/
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.css
│   │   └── app.config.ts
│   │
│   ├── main.ts
│   ├── index.html
│   └── styles.css
│
├── angular.json
├── package.json
├── tsconfig.json
└── ...
```

The two files I want you to understand first are:

```text
main.ts
app.config.ts
```

---

**`main.ts`**

This is effectively the **entry point** of your Angular application.

Modern Angular commonly has:

```typescript
bootstrapApplication(
  AppComponent,
  appConfig
);
```

Conceptually:

```text
Browser
   ↓
main.ts
   ↓
bootstrapApplication()
   ↓
AppComponent
   ↓
Angular application starts
```

For example:

```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error(err));
```

---

**What is Bootstrap?**

> Bootstrap means starting/initializing the Angular application

When you write:

```typescript
bootstrapApplication(AppComponent, appConfig);
```

you're essentially saying:

> "Start Angular using `AppComponent` as the root component and use this application configuration."

So:

```text
main.ts
   ↓
bootstrapApplication()
   ↓
AppComponent
```

---

**`Component`**

> A component is the fundamental building block of an Angular UI.

- A @Component decorator that contains some configuration used by Angular.
- An HTML template that controls what renders into the DOM.
- A CSS selector that defines how the component is used in HTML.
- A TypeScript class with behaviors, such as handling user input or making requests to a server.

For example:

```text
Application
│
├── HeaderComponent
├── SidebarComponent
├── ProductListComponent
├── ProductCardComponent
└── FooterComponent
```

Each component generally contains:

```text
Component
│
├── TypeScript → behavior/state
├── HTML       → UI/template
└── CSS        → styling
```

Example:

```typescript
@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent {

  productName = 'Laptop';

  buyProduct() {
    console.log('Product purchased');
  }
}
```

Template:

```html
<h2>{{ productName }}</h2>

<button (click)="buyProduct()">
  Buy
</button>
```

So:

```text
TypeScript
    ↕
Template
```

---

**`Standalone Components`**

This is especially important because you're learning **modern Angular**.

Older Angular applications commonly used:

```text
NgModule
   ↓
Components
```

Modern Angular uses **standalone components** by default.

Example:

```typescript
@Component({
  selector: 'app-product',
  standalone: true,
  imports: [],
  templateUrl: './product.html'
})
export class ProductComponent {
}
```

In newer Angular projects, standalone is the normal approach, so you should understand it well rather than focusing primarily on the older `NgModule` style.

---

**Component Selector**

Suppose:

```typescript
@Component({
  selector: 'app-product'
})
export class ProductComponent {}
```

You can use it in another template:

```html
<app-product></app-product>
```

So:

```text
selector
   ↓
HTML element representing the component
```

---

**Component Template**

You can define HTML externally:

```typescript
@Component({
  templateUrl: './product.html'
})
```

or inline:

```typescript
@Component({
  template: `
    <h2>{{ productName }}</h2>
  `
})
```

External templates are generally easier to maintain for larger components.

---

**Angular Component Tree**

This is important.

Suppose your application has:

```text
AppComponent
│
├── HeaderComponent
│
├── DashboardComponent
│   ├── SalesComponent
│   └── RevenueComponent
│
└── FooterComponent
```

Angular manages this as a **component tree**.

```text
             AppComponent
             /          \
        Header         Dashboard
                       /       \
                   Sales      Revenue
```

This becomes important later when we study:

* Change detection
* Component communication
* Signals
* Lifecycle
* Dependency injection

---

**Angular's Basic Flow**

Suppose the user opens:

```text
https://myapp.com/products
```

Conceptually:

```text
Browser
   ↓
Angular application
   ↓
Router
   ↓
Product component
   ↓
Product service
   ↓
HttpClient
   ↓
.NET Web API
   ↓
Database
```

The response comes back:

```text
Database
   ↓
.NET API
   ↓
HttpClient
   ↓
Service
   ↓
Component state
   ↓
Template
   ↓
UI
```

This architecture will feel familiar because it's similar to the layered .NET architecture we've already discussed.

---

**Angular's Major Building Blocks**

You should now have this mental map:

```text
                    Angular
                       |
       +---------------+---------------+
       |               |               |
   Components       Services        Router
       |               |               |
   Templates           |           Navigation
       |               |
   Data Binding       DI
       |
   Directives
       |
     Pipes
       |
    Signals
       |
     RxJS
       |
    HttpClient
```
------
------

## Component Lifecycle

> A component's lifecycle is the sequence of steps that happen between the component's creation and its destruction. Each step represents a different part of Angular's process for rendering components and checking them for updates over time.

> constructor is a JavaScript/TypeScript class constructor, not an Angular lifecycle hook. Angular invokes it when creating the component instance, and it's commonly used for dependency injection.

| Hook                    |       Frequency | Think                     |
| ----------------------- | --------------: | ------------------------- |
| `constructor`           |            Once | Create class / DI         |
| `ngOnChanges`           | On input change | Input changed             |
| `ngOnInit`              |            Once | Initialize component      |
| `ngDoCheck`             |            Many | Custom change detection   |
| `ngAfterContentInit`    |            Once | Projected content ready   |
| `ngAfterContentChecked` |            Many | Projected content checked |
| `ngAfterViewInit`       |            Once | Component view ready      |
| `ngAfterViewChecked`    |            Many | Component view checked    |
| `afterNextRender`       |     Next render | DOM rendered              |
| `afterEveryRender`      |    Every render | DOM rendered              |
| `ngOnDestroy`           |            Once | Cleanup                   |


- **ngOnChanges** - Called before ngOnInit() (if the component has bound inputs) and whenever one or more data-bound input properties change.

-------------
-------------

## Component selectors

| Type                        | Example        | Matches                     |
| --------------------------- | -------------- | --------------------------- |
| **Element / Type selector** | `app-user`     | HTML element/tag            |
| **Attribute selector**      | `[app-button]` | Element having an attribute |
| **Class selector**          | `.menu-item`   | Element having a CSS class  |

```
Angular Component Selectors
│
├── Element
│     selector: 'app-user'
│     <app-user>
│
├── Attribute
│     selector: '[app-button]'
│     <button app-button>
│
├── Class
│     selector: '.menu-item'
│     <div class="menu-item">
│
├── Combined
│     selector: 'button[app-upload]'
│
├── Multiple
│     selector: 'app-a, [app-a]'
│
└── :not()
      selector: '[app-x]:not(textarea)'

```

----------
----------

## Angular Inputs

An **input** allows a parent component to pass data to a child component. Angular's modern recommended API is the signal-based `input()` function. 

**`1. Basic Input`**

**Child**

```typescript
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-user',
  template: `Hello {{ name() }}`
})
export class UserComponent {
  name = input<string>();
}
```

**Parent**

```html
<app-user [name]="userName"></app-user>
```

```typescript
userName = 'Swapnil';
```

Flow:

```text
Parent
 userName
    ↓
 [name]
    ↓
Child
 name()
```

`input()` returns an **InputSignal**, so you read it using:

```typescript
name()
```

Input signals are **read-only** from the child. 

---

**`2. Default Value`**

```typescript
age = input(25);
```

If parent doesn't provide `age`, the value is:

```text
25
```

TypeScript automatically infers it as a number. 

---

**`3. Optional Input`**

```typescript
name = input<string>();
```

If the parent doesn't provide it:

```text
name() → undefined
```

Therefore the type is effectively:

```typescript
InputSignal<string | undefined>
```



---

**`4. Required Input`**

Use `input.required()` when the child **must receive a value**.

```typescript
userId = input.required<number>();
```

Parent must provide:

```html
<app-user [userId]="10"></app-user>
```

Otherwise Angular reports an error at **build time**. 

---

**`5. Input with computed()`**

Because `input()` is a signal, you can derive values from it:

```typescript
price = input(100);

priceWithTax = computed(() =>
  this.price() * 1.18
);
```

```text
price()
   ↓
computed()
   ↓
priceWithTax()
```

---

**`6. Input Transform`**

You can transform incoming values:

```typescript
label = input('', {
  transform: (value: string) => value.trim()
});
```

Angular applies the transform when the input is set.

There are also built-in transforms:

```typescript
import { booleanAttribute, numberAttribute } from '@angular/core';

disabled = input(false, {
  transform: booleanAttribute
});

value = input(0, {
  transform: numberAttribute
});
```

Useful when accepting values from HTML-like attributes. 

---

**`7. Input Alias`**

Change the name used in the template:

```typescript
value = input(0, {
  alias: 'sliderValue'
});
```

Parent:

```html
<app-slider [sliderValue]="50"></app-slider>
```

But inside TypeScript, you still use:

```typescript
this.value()
```



---

**`8. Old @Input() API`**

You will definitely see this in existing Angular projects:

```typescript
@Input() name!: string;
```

Modern Angular:

```typescript
name = input<string>();
```

Both are supported, but Angular recommends the **signal-based `input()` for new projects**. 

---

### Input Cheat Sheet

| Requirement    | Syntax                               |
| -------------- | ------------------------------------ |
| Optional input | `input<string>()`                    |
| Default value  | `input(10)`                          |
| Required       | `input.required<number>()`           |
| Derived value  | `computed(() => ...)`                |
| Transform      | `input('', { transform: ... })`      |
| Alias          | `input(0, { alias: 'sliderValue' })` |
| Older code     | `@Input()`                           |

-------------
--------------

## Angular Outputs

An **output** allows a child component to send a custom event/data to its parent.

Modern Angular uses the `output()` function. 

**`1. Basic Output`**

**Child**

```typescript
import { Component, output } from '@angular/core';

@Component({
  selector: 'app-child',
  template: `
    <button (click)="selectUser()">
      Select
    </button>
  `
})
export class ChildComponent {

  userSelected = output<string>();

  selectUser() {
    this.userSelected.emit('Swapnil');
  }
}
```

**Parent**

```html
<app-child
  (userSelected)="onUserSelected($event)">
</app-child>
```

```typescript
onUserSelected(name: string) {
  console.log(name);
}
```

Flow:

```text
Child
  |
  | emit('Swapnil')
  ↓
Parent
  |
  ↓
$event
```

`output()` returns an `OutputEmitterRef`, and `.emit()` raises the custom event. 

---

**`2. Output Without Data`**

If you only need to notify the parent:

```typescript
panelClosed = output<void>();
```

Emit:

```typescript
this.panelClosed.emit();
```

Parent:

```html
<app-panel
  (panelClosed)="savePanelState()">
</app-panel>
```

---

**`3. Passing Data`**

You can emit almost any value:

```typescript
valueChanged = output<number>();

this.valueChanged.emit(7);
```

Or an object:

```typescript
userSelected = output<{ id: number; name: string }>();

this.userSelected.emit({
  id: 10,
  name: 'Swapnil'
});
```

Parent receives it through:

```html
(userSelected)="onUserSelected($event)"
```



---

**`4. Output Alias`**

You can expose a different name in the template:

```typescript
changed = output<number>({
  alias: 'valueChanged'
});
```

Parent:

```html
<app-slider
  (valueChanged)="saveValue($event)">
</app-slider>
```

Inside TypeScript, the property is still:

```typescript
this.changed
```

Aliases should generally be avoided unless there's a good reason, such as avoiding a native DOM event name collision. 

---

**`5. Naming Rules`**

Prefer:

```typescript
userSelected = output();
valueChanged = output();
panelClosed = output();
```

Use **camelCase**.

Avoid:

```typescript
onUserSelected   // ❌
OnUserSelected   // ❌
click            // ❌ can conflict with DOM event
```

Angular specifically recommends avoiding output names that collide with native DOM events and avoiding the `on` prefix. 

---

**`6. Important: Outputs Don't Bubble`**

Angular custom outputs **do not bubble through the DOM** like native browser events.

```text
Parent
  ↑
Child
  ↑
Grandchild
```

An output from `Grandchild` doesn't automatically reach `Parent`.

You need to explicitly handle/re-emit it or use shared state/services for more complex communication. 

---

**`7. Old `@Output()` API`**

You'll see this frequently in existing projects:

```typescript
@Output()
userSelected = new EventEmitter<string>();
```

Then:

```typescript
this.userSelected.emit('Swapnil');
```

It is still fully supported, but for **new Angular code**, prefer:

```typescript
userSelected = output<string>();
```



---

### Input vs Output

|            | Input           | Output                            |
| ---------- | --------------- | --------------------------------- |
| Direction  | Parent → Child  | Child → Parent                    |
| Modern API | `input()`       | `output()`                        |
| Purpose    | Receive data    | Raise event                       |
| Example    | `[user]="user"` | `(selected)="onSelected($event)"` |

> **`input()` is used for parent-to-child data flow, while `output()` is used by a child to emit custom events to its parent.** 

---------------
---------------

## Angular Content Projection — `ng-content`

**Content projection** lets a component accept HTML/content from its parent and render it inside a specific place in its own template. Think of it like Angular's version of a native HTML `<slot>`. 

**`1. Basic Example`**

**Child component**

```typescript
@Component({
  selector: 'app-card',
  template: `
    <div class="card">
      <ng-content />
    </div>
  `
})
export class CardComponent {}
```

**Parent**

```html
<app-card>
  <h2>Product Details</h2>
  <p>This is my product.</p>
</app-card>
```

The parent's content gets projected where `<ng-content>` exists:

```text
Parent content
      ↓
<app-card>
      ↓
<ng-content>
      ↓
Card UI
```

Angular calls this **content**, while the HTML defined inside the card component itself is its **view**. 

---

**`2. Multiple Slots`**

You can have multiple projection areas using `select`.

**Child**

```html
<div class="card">
  <div class="title">
    <ng-content select="card-title" />
  </div>

  <div class="body">
    <ng-content select="card-body" />
  </div>
</div>
```

**Parent**

```html
<app-card>
  <card-title>Product</card-title>

  <card-body>
    Product description goes here.
  </card-body>
</app-card>
```

Think:

```text
card-title → title slot
card-body  → body slot
```

The `select` uses CSS-selector-style matching. 

---

**`3. Fallback Content`**

You can provide default content:

```html
<ng-content select="card-title">
  Default Title
</ng-content>
```

If the parent doesn't provide `card-title`, Angular displays:

```text
Default Title
```

---

### Ideal Scenario — When Should I Use It?

The **best scenario** is when you are building a **reusable container/layout component where the outer structure is fixed but the inner content needs to vary**.

**Example: Reusable Card**

You want every card to have:

```text
┌──────────────────────┐
│ Card styling/shadow  │
│                      │
│   CUSTOM CONTENT     │
│                      │
└──────────────────────┘
```

Instead of creating:

```text
ProductCard
OrderCard
UserCard
NotificationCard
```

with duplicate card HTML/CSS, create:

```text
Reusable Card
     +
ng-content
```

Then:

```html
<app-card>
  <app-product-details />
</app-card>
```

or:

```html
<app-card>
  <app-user-details />
</app-card>
```

**Other good scenarios**

* Modal/Dialog containers
* Cards
* Layout wrappers
* Tabs
* Custom panels
* Reusable form sections
* Header/body/footer-style components

---

### Input vs Content Projection

This distinction is **very important**.

**Use `input()` when:**

You are passing **data**.

```html
<app-user [name]="userName" />
```

```text
Parent → data → Child
```

**Use `ng-content` when:**

You are passing **UI/content**.

```html
<app-card>
  <h2>My Product</h2>
  <button>Buy</button>
</app-card>
```

```text
Parent → UI/content → Child
```

Don't conditionally create/remove `<ng-content>` using:

```html
@if (...) {
  <ng-content />
}
```

Angular processes `<ng-content>` at compile time, and projected content is instantiated even if the placeholder is hidden. For conditional content rendering, Angular recommends template fragments instead. 

------
------

## Angular Host Elements

The **host element** is the HTML element that matches a component's selector. Angular creates the component on that element, and the component's template is rendered **inside** it. 

**1. Basic Example**

Component:

```typescript
@Component({
  selector: 'app-profile',
  template: `
    <img src="profile.jpg">
  `
})
export class ProfileComponent {}
```

Usage:

```html
<app-profile></app-profile>
```

Rendered DOM conceptually:

```html
<app-profile>
  <img src="profile.jpg">
</app-profile>
```

So:

```text
<app-profile>
      ↑
 Host Element
```

---

**2. Binding to Host Element**

A component can bind **properties, attributes, styles, and events** directly to its host element using the `host` property.

```typescript
@Component({
  selector: 'app-slider',

  host: {
    'role': 'slider',
    '[attr.aria-valuenow]': 'value',
    '[class.active]': 'isActive()',
    '[tabIndex]': 'disabled ? -1 : 0',
    '(keydown)': 'updateValue($event)'
  },

  template: `
    <span>Slider</span>
  `
})
export class SliderComponent {

  value = 50;
  disabled = false;
  isActive = signal(true);

  updateValue(event: KeyboardEvent) {
    // ...
  }
}
```

The bindings apply to:

```html
<app-slider>
```

not the `<span>` inside it.

---

**3. Why is Host Binding Useful?**

Imagine a reusable button component:

```text
<app-button>
```

You want the **component's host element itself** to have:

```text
role
class
style
tabIndex
events
```

Instead of manually modifying the host element using DOM APIs, define those behaviors declaratively:

```typescript
host: {
  '[class.active]': 'isActive()',
  '(click)': 'onClick()'
}
```

**Ideal scenarios**

* Accessibility attributes
* Host-level CSS classes
* Host styles
* Keyboard events
* Component state reflected on the host
* Reusable UI components

---

**4. `host` vs `@HostBinding` / `@HostListener`**

Modern Angular recommends:

```typescript
@Component({
  host: {
    '[class.active]': 'isActive()',
    '(click)': 'onClick()'
  }
})
```

Older code may use:

```typescript
@HostBinding('class.active')
isActive = true;

@HostListener('click')
onClick() {}
```

**For new code, prefer the `host` property.** Angular documents `@HostBinding` and `@HostListener` as backwards-compatible APIs. 

---

**5. Host Binding Collision**

Suppose component defines:

```typescript
host: {
  'role': 'presentation'
}
```

But parent uses:

```html
<app-profile role="group"></app-profile>
```

Angular has rules for deciding which value wins.

Important rule to remember:

> When both values are dynamic, the component's host binding wins. 

You generally don't need to memorize all collision rules for interviews.

---

**6. Host Element vs Component Template**

This distinction is important:

```html
<app-card>
    <h2>Product</h2>
</app-card>
```

Here:

```text
<app-card>
    ↑
Host element

<h2>Product</h2>
    ↑
Projected content, if using ng-content
```

While:

```typescript
template: `
  <div class="card">
     ...
  </div>
`
```

is the component's **view**.

So:

```text
Component
│
├── Host Element
│     └── <app-card>
│
└── Component View
      └── template HTML
```

----------
----------

## Angular Queries

> Queries let a component find and reference child components, directives, DOM elements, or other values from its template.** Modern Angular query APIs return **signals**.

There are **2 categories**:

```text
Queries
├── View Queries
│   ├── viewChild()
│   └── viewChildren()
│
└── Content Queries
    ├── contentChild()
    └── contentChildren()
```

---

**`1. viewChild()`**

Find **one child inside the component's own template**.

```typescript
@Component({
  template: `
    <app-header />
  `
})
export class AppComponent {
  header = viewChild(HeaderComponent);
}
```

Access it:

```typescript
this.header()
```

If the child doesn't exist, result can be `undefined`. The query automatically updates if the child appears/disappears, for example because of `@if`. 

**Ideal scenario**  

You need to call a method or access a property of a child component.

```typescript
header = viewChild(HeaderComponent);

refreshHeader() {
  this.header()?.refresh();
}
```

---

**`2. viewChildren()`**

Find **multiple children in your own template**.

```typescript
actions = viewChildren(ActionComponent);
```

Template:

```html
<app-action />
<app-action />
<app-action />
```

Then:

```typescript
this.actions()
```

returns an array of matching components. 

---

**`3. contentChild()`**

Find a child supplied as **content by the parent**.

This is closely related to `ng-content`.

```html
<app-card>
  <app-header />
</app-card>
```

Inside `CardComponent`:

```typescript
header = contentChild(HeaderComponent);
```

Think:

```text
Parent
  ↓
<app-card>
  ↓
projected content
  ↓
contentChild()
```

Unlike `viewChild()`, this searches the component's **content**, not its own template. 

---

**`4. contentChildren()`**

Find multiple projected children.

```typescript
items = contentChildren(MenuItemComponent);
```

Parent:

```html
<app-menu>
  <app-menu-item />
  <app-menu-item />
  <app-menu-item />
</app-menu>
```

Then:

```typescript
this.items()
```

returns the matching children. 

---

**View vs Content**

This is the **most important thing to understand**.

`View`

HTML defined **inside the component's own template**:

```typescript
@Component({
  template: `
    <app-header />
  `
})
```

Use:

```typescript
viewChild()
viewChildren()
```

`Content`

HTML provided **from outside the component**, usually through content projection:

```html
<app-card>
  <app-header />
</app-card>
```

Use:

```typescript
contentChild()
contentChildren()
```
---

**`5. Querying DOM Elements`**

You can also query using a template reference variable:

```html
<button #save>Save</button>
```

```typescript
saveButton = viewChild<ElementRef<HTMLButtonElement>>('save');
```

Then:

```typescript
this.saveButton()?.nativeElement
```

Angular also supports `read` when you want a different value from the matched element. 

---

**`6. Required Query`**

If a child **must exist**, use:

```typescript
header = viewChild.required(HeaderComponent);
```

Instead of:

```typescript
header = viewChild(HeaderComponent);
```

Required queries don't include `undefined` in their type, and Angular reports an error if the target isn't found. 

---

**`7. Old Syntax — Know It for Interviews`**

You'll still see:

```typescript
@ViewChild(HeaderComponent)
header!: HeaderComponent;

@ViewChildren(ActionComponent)
actions!: QueryList<ActionComponent>;

@ContentChild(HeaderComponent)
header!: HeaderComponent;

@ContentChildren(MenuItemComponent)
items!: QueryList<MenuItemComponent>;
```

Modern Angular recommends the **signal-based APIs**:

```typescript
viewChild()
viewChildren()
contentChild()
contentChildren()
```

The decorator APIs are still supported. 

---

### Interview Cheat Sheet

| Need                              | Use                 |
| --------------------------------- | ------------------- |
| One child in own template         | `viewChild()`       |
| Multiple children in own template | `viewChildren()`    |
| One projected child               | `contentChild()`    |
| Multiple projected children       | `contentChildren()` |
| Child  exist                  | `.required()`       |
| DOM/template reference            | `viewChild('ref')`  |
| Parent → Child data               | `input()`           |
| Child → Parent event              | `output()`          |

---------
---------

## Angular Programmatic Rendering

Normally, we render components directly in the template:

```html
<app-user></app-user>
```

**Programmatic rendering** means creating/rendering a component dynamically from code when you don't know beforehand which component should appear. Angular provides two main approaches: `NgComponentOutlet` and `ViewContainerRef`. 

---

**`1. NgComponentOutlet`**

Use it when the **component type changes dynamically**.

Example:

```typescript
@Component({
  template: `
    <ng-container
      *ngComponentOutlet="componentType">
    </ng-container>
  `
})
export class ProfileComponent {

  componentType = AdminProfile;
}
```

You can change:

```typescript
componentType = AdminProfile;
```

to:

```typescript
componentType = UserProfile;
```

and Angular renders the appropriate component. 

**Passing Inputs**

```html
<ng-container
  *ngComponentOutlet="
    componentType;
    inputs: inputs
  ">
</ng-container>
```

```typescript
inputs = {
  username: 'Swapnil',
  role: 'admin'
};
```

---

**`2. ViewContainerRef`**

Use it when you want to **create a component dynamically from TypeScript** at a specific location.

```typescript
private vcr = inject(ViewContainerRef);

showUser() {
  this.vcr.createComponent(UserComponent);
}
```

Angular inserts the dynamically created component into that view container. 

A common template location:

```html
<ng-container #container></ng-container>
```

Then you can obtain/use the corresponding `ViewContainerRef`.

---

**`NgComponentOutlet` vs `ViewContainerRef`**

|            | `NgComponentOutlet`                | `ViewContainerRef`            |
| ---------- | ---------------------------------- | ----------------------------- |
| Approach   | Template                           | TypeScript                    |
| Best for   | Simple dynamic component selection | Programmatic/detailed control |
| Example    | Admin vs User UI                   | Dynamic modal/popup/list      |
| Complexity | Easier                             | More control                  |

**Mental model**

```text
Need dynamic component?
        │
        ├── Template-driven
        │      ↓
        │  NgComponentOutlet
        │
        └── Code-driven
               ↓
          ViewContainerRef
```

---

**Ideal Scenarios**

`1. Dynamic UI based on role`

```text
Admin → AdminDashboard
User  → UserDashboard
```

`NgComponentOutlet` is a good fit.

`2. Dynamic forms`

```text
Question type
    ↓
TextQuestionComponent
DropdownQuestionComponent
DateQuestionComponent
```

`3. Dynamic dialogs/modals`

```text
Button clicked
     ↓
Create ModalComponent
     ↓
Pass data
     ↓
Show modal
```

`ViewContainerRef` is useful here.

`4. Plugin-style UI`

```text
Configuration
     ↓
Determine component
     ↓
Render dynamically
```

--------
--------

## Angular Advanced Component Configuration

**`1. ChangeDetectionStrategy`**

Controls **when Angular checks a component for UI updates**.

```typescript
@Component({
  selector: 'app-user',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `...`
})
export class UserComponent {}
```

**`Default` vs `OnPush`**

| Strategy  | Behavior                                                     |
| --------- | ------------------------------------------------------------ |
| `Default` | Angular checks more broadly when application activity occurs |
| `OnPush`  | Angular checks the component only under specific conditions  |

For `OnPush`, Angular checks when:

* An input changes through template binding
* An event listener in the component runs
* Component is explicitly marked for checking, e.g. `markForCheck()`
* Related mechanisms such as `AsyncPipe` mark it for checking 

**Why use `OnPush`?**

For **performance**, especially in large applications.

```text
Default
Event/API/Timer
      ↓
Broad change detection

OnPush
Event/Input/Explicit mark
      ↓
Check relevant component
```

**Interview:**

> `OnPush` reduces unnecessary change detection and improves performance by limiting when Angular checks a component.

> **Important current-doc note:** The Angular documentation you're reading is now v22 and says `OnPush` is the default strategy since v22. For your Angular 21 preparation, you may still encounter `Default` as the historical/default behavior. 

---

**`2. preserveWhitespaces`**

Angular normally removes/collapses unnecessary whitespace in templates.

```typescript
@Component({
  preserveWhitespaces: true
})
```

Normally, **leave this at the default**.

You rarely need to change it.

**Interview:**

> `preserveWhitespaces` controls whether Angular preserves whitespace in component templates. 

---

**`3. CUSTOM_ELEMENTS_SCHEMA`**

By default, Angular reports an error for an unknown HTML element.

Example:

```html
<my-web-component></my-web-component>
```

If this is a **custom web component**, Angular can be configured with:

```typescript
@Component({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <my-web-component></my-web-component>
  `
})
export class AppComponent {}
```

This tells Angular to allow custom elements that it doesn't recognize. 

**Ideal scenario**

When integrating Angular with **Web Components / Custom Elements** from another library.

Don't use it just to hide Angular template errors.

-------
-------

## Angular Custom Elements (Web Components)

**Angular Elements** allow you to convert an Angular component into a **standard Web Component (Custom Element)** that can be used in:

* Angular apps
* React apps
* Vue apps
* Plain HTML/JavaScript apps

using a normal HTML tag. 

---

**Why Do We Need It?**

Normally Angular components only work inside Angular:

```html
<app-user></app-user>
```

But what if another application is built in:

* React
* Vue
* ASP.NET MVC
* Plain HTML

and wants to reuse your component?

Angular Elements solves this.

```html
<user-profile></user-profile>
```

Now it behaves like a native HTML element. 

---

**Basic Flow**

**Step 1: Create Angular Component**

```typescript
@Component({
  selector: 'app-popup',
  template: `
    <h2>{{ message() }}</h2>
  `
})
export class PopupComponent {
  message = input('');
}
```

---

**Step 2: Install Package**

```bash
npm install @angular/elements
```
---

**Step 3: Convert to Custom Element**

```typescript
import { createCustomElement } from '@angular/elements';

const PopupElement =
  createCustomElement(PopupComponent, {
    injector
  });
```

---

**Step 4: Register Element**

```typescript
customElements.define(
  'popup-element',
  PopupElement
);
```
---

**Step 5: Use Like HTML**

```html
<popup-element
  message="Hello">
</popup-element>
```

Browser automatically creates the Angular component instance. 

---

### Input Mapping

Angular Input:

```typescript
message = input('');
```

becomes:

```html
<popup-element
  message="Hello">
</popup-element>
```

Angular automatically maps component inputs to HTML attributes. 

---

### Output Mapping

Angular Output:

```typescript
closed = output<void>();
```

becomes a browser Custom Event:

```javascript
popup.addEventListener(
  'closed',
  () => console.log('Closed')
);
```

The emitted data is available in:

```javascript
event.detail
```
---

### Ideal Scenarios

**1. Share Components Across Frameworks**

```text
Angular Team
     ↓
Date Picker
     ↓
Used by
React / Vue / Angular
```

Most common use case.

---

**2. Design System / Component Library**

```text
Company UI Library
│
├── Button
├── Dialog
├── Grid
└── DatePicker
```

Build once, use everywhere.

---

**3. Micro Frontends**

```text
Main App (React)
      ↓
Angular Widget
      ↓
<customer-dashboard>
```

Embed Angular functionality without converting the whole application.

---

**4. Legacy Application Integration**

```text
ASP.NET MVC
     ↓
Angular Web Component
```

Useful when gradually modernizing old applications.

---

Do **NOT** use the component selector as the custom element name.

Bad:

```typescript
selector: 'app-popup'

customElements.define(
  'app-popup',
  PopupElement
);
```

This can create conflicts and duplicate component instantiation. Use a different custom-element tag name. 

---

### Angular Component vs Angular Element

| Angular Component               | Angular Element                   |
| ------------------------------- | --------------------------------- |
| Angular only                    | Any framework                     |
| Used in Angular templates       | Used as HTML element              |
| `<app-user>`                    | `<user-widget>`                   |
| Angular runtime required in app | Self-bootstrapping custom element |

-----
-----

## Angular Template Binding

**Binding creates a dynamic connection between the component's data and the template.** When the data changes, Angular updates the UI. 

There are **4 things you should master**:

```text
Interpolation
Property Binding
Attribute Binding
Class / Style Binding
```

---

**`1. Interpolation {{ }}`**

Used to display **dynamic text**.

```typescript
name = 'Swapnil';
```

```html
<h2>Hello {{ name }}</h2>
```

Output:

```text
Hello Swapnil
```

With signals:

```typescript
name = signal('Swapnil');
```

```html
<h2>Hello {{ name() }}</h2>
```

Angular tracks signals read in the template and updates the UI when they change. 

**Remember**

```text
{{ value }}
     ↓
Display text
```

---

**`2. Property Binding [ ]`**

Used to set a **DOM property**, component property, or directive property.

```typescript
isDisabled = true;
```

```html
<button [disabled]="isDisabled">
  Save
</button>
```

Here Angular sets the `disabled` **DOM property**.

Another example:

```typescript
imageUrl = '/images/user.png';
```

```html
<img [src]="imageUrl">
```

**Component input**

```html
<app-user [name]="userName"></app-user>
```

Here `[name]` binds to the component's input/property. 

---

**`3. Attribute Binding [attr.]`**

Use this when you specifically need to set an **HTML attribute**, especially when there isn't a corresponding DOM property.

```typescript
role = 'button';
```

```html
<div [attr.role]="role">
  Save
</div>
```

Angular effectively sets:

```html
<div role="button">
```

If the value is `null`, Angular removes the attribute. 

**Remember**

```text
[property]       → DOM/component property

[attr.attribute] → HTML attribute
```

---

**`4. Interpolation vs Property Binding`**

These can sometimes look similar:

```html
<img src="{{ imageUrl }}">
```

and:

```html
<img [src]="imageUrl">
```

For property binding, Angular treats the second form as a property binding directly. Angular also treats interpolation in property/attribute positions as a property binding in this context. 

For interview purposes:

```text
{{ }}  → primarily display dynamic text
[ ]    → bind a property
```

---

**`5. Class Binding`**

Conditionally add/remove a CSS class:

```typescript
isActive = true;
```

```html
<div [class.active]="isActive">
  User
</div>
```

If `isActive` is `true`:

```html
<div class="active">
```

You can also bind multiple classes:

```html
<div [class]="classes"></div>
```

where:

```typescript
classes = 'active highlighted';
```

Angular also supports arrays and objects for `[class]`. 

---

**`6. Style Binding`**

Bind an individual CSS property:

```typescript
height = 200;
```

```html
<div [style.height.px]="height">
</div>
```

Result:

```html
<div style="height: 200px">
```

You can also do:

```html
<div [style.display]="isVisible ? 'block' : 'none'">
</div>
```

Angular supports individual style properties and multi-style bindings. 

---

> Two-way binding synchronizes a value between the component and the UI. Angular uses [()] syntax; [(ngModel)] is commonly used with forms, while model() enables two-way binding between components.

--------
--------

## Angular Control Flow 

Angular provides built-in template control flow using:

```text
@if / @else
@for
@switch
```

These are the **modern Angular syntax** and are preferred over the older `*ngIf`, `*ngFor`, and `*ngSwitch` style. 

---

**`1. @if`**

Conditionally render content.

```typescript
isLoggedIn = true;
```

```html
@if (isLoggedIn) {
  <p>Welcome!</p>
} @else {
  <p>Please login.</p>
}
```

Multiple conditions:

```html
@if (role === 'admin') {
  <app-admin />
} @else if (role === 'editor') {
  <app-editor />
} @else {
  <app-user />
}
```

**Important**

You can store the result of an expression:

```html
@if (user.profile; as profile) {
  <p>{{ profile.name }}</p>
}
```

Useful when the expression is long or you want to reuse the result. 

---

**`2. @for`**

Used to loop through a collection.

```typescript
users = [
  { id: 1, name: 'Swapnil' },
  { id: 2, name: 'Rahul' }
];
```

```html
@for (user of users; track user.id) {
  <p>{{ user.name }}</p>
}
```

**`track` is very important**

```html
track user.id
```

tells Angular how an item corresponds to a DOM element.

When data changes, Angular can update only the necessary DOM instead of recreating everything. Use a **unique ID** whenever possible. 

```text
users
 ↓
@for
 ↓
track user.id
 ↓
Efficient DOM updates
```

---

**`@for` built-in variables**

Angular provides:

| Variable | Meaning       |
| -------- | ------------- |
| `$index` | Current index |
| `$count` | Total number  |
| `$first` | First item    |
| `$last`  | Last item     |
| `$even`  | Even index    |
| `$odd`   | Odd index     |

Example:

```html
@for (user of users; track user.id; let i = $index) {
  <p>{{ i + 1 }}. {{ user.name }}</p>
}
```
---

`@empty`

Handle an empty collection:

```html
@for (user of users; track user.id) {
  <p>{{ user.name }}</p>
} @empty {
  <p>No users found.</p>
}
```

This is very useful for API-driven lists. 

---

**`3. @switch`**

Use when you have multiple possible values.

```typescript
role = 'admin';
```

```html
@switch (role) {
  @case ('admin') {
    <app-admin />
  }

  @case ('editor') {
    <app-editor />
  }

  @default {
    <app-user />
  }
}
```

Comparison uses strict equality:

```typescript
===
```

There is **no fall-through**, so you don't need `break`. 

You can also have multiple cases for the same block:

```html
@switch (role) {
  @case ('admin')
  @case ('manager') {
    <app-management />
  }

  @default {
    <app-user />
  }
}
```

--------------
---------------

## Angular Pipes

> A **pipe transforms data for display in the template** without changing the original data.

Syntax:

```html
{{ value | pipe }}
```



**`1. Built-in Pipes`**

Common ones:

| Pipe        | Purpose                      | Example                   |
| ----------- | ---------------------------- | ------------------------- |
| `uppercase` | Uppercase text               | `{{ name \| uppercase }}` |
| `lowercase` | Lowercase text               | `{{ name \| lowercase }}` |
| `titlecase` | Title case                   | `{{ name \| titlecase }}` |
| `date`      | Format date                  | `{{ date \| date }}`      |
| `currency`  | Currency                     | `{{ price \| currency }}` |
| `number`    | Number formatting            | `{{ price \| number }}`   |
| `percent`   | Percentage                   | `{{ value \| percent }}`  |
| `json`      | Debug object                 | `{{ user \| json }}`      |
| `async`     | Observable/Promise value     | `{{ users$ \| async }}`   |
| `slice`     | Slice string/array           | `{{ name \| slice:0:5 }}` |
| `keyvalue`  | Object/Map → key/value pairs | `{{ user \| keyvalue }}`  |

Angular provides these through `@angular/common`. 

---

**`2. Passing Parameters`**

Use `:`.

```html
{{ price | currency:'INR' }}

{{ today | date:'dd/MM/yyyy' }}
```

Multiple parameters:

```html
{{ today | date:'hh:mm':'UTC' }}
```
---

**`3. Chaining Pipes`**

You can apply multiple pipes.

```html
{{ name | lowercase | titlecase }}
```

Execution is **left → right**:

```text
name
 ↓
lowercase
 ↓
titlecase
 ↓
display
```

---

**`4. Custom Pipe`**

Use `@Pipe` and implement `PipeTransform`.

```typescript
@Pipe({
  name: 'kebabCase'
})
export class KebabCasePipe implements PipeTransform {

  transform(value: string): string {
    return value.toLowerCase().replace(/ /g, '-');
  }
}
```

Use:

```html
{{ productName | kebabCase }}
```

For:

```text
"My Product"
```

Result:

```text
my-product
```

---

**`5. Pure vs Impure Pipes`**

This is an **important interview question**.

**Pure pipe — default**

```typescript
@Pipe({
  name: 'myPipe',
  pure: true
})
```

Angular re-runs it when the **input value/reference changes**.

For an array:

```typescript
users.push(newUser);
```

The array reference hasn't changed, so a pure pipe doesn't detect that mutation.

But:

```typescript
users = [...users, newUser];
```

creates a new array reference, so Angular detects it.

**Impure pipe**

```typescript
@Pipe({
  name: 'myPipe',
  pure: false
})
```

Angular checks it much more frequently.

⚠️ **Avoid impure pipes unless genuinely necessary** because they can have a significant performance cost. 

---

**`6. AsyncPipe`**

Very important when we study RxJS.

```typescript
users$ = this.userService.getUsers();
```

Instead of:

```typescript
users$.subscribe(...)
```

in the component, you can use:

```html
@for (user of users$ | async; track user.id) {
  <p>{{ user.name }}</p>
}
```

`AsyncPipe` subscribes to an `Observable`/`Promise` and provides its latest value to the template. It also handles subscription cleanup when the component is destroyed. 

--------
--------

## Angular `ng-template`

`<ng-template>` defines a **template fragment** that Angular does **not render immediately**. You can render it later dynamically or programmatically. 

**`1. Basic Example`**

```html
<p>This is visible</p>

<ng-template>
  <p>This is NOT visible yet</p>
</ng-template>
```

The `<p>` inside `ng-template` is only a **template definition**, not rendered DOM. 

Think:

```text
<ng-template>
      ↓
Template definition
      ↓
Render later
```

---

**`2. TemplateRef`**

Give the template a reference:

```html
<ng-template #loading>
  <p>Loading...</p>
</ng-template>
```

Angular represents this template as a `TemplateRef`.

You can get it with:

```typescript
loading = viewChild<TemplateRef<unknown>>('loading');
```

---

**`3. NgTemplateOutlet`**

Use `NgTemplateOutlet` when you want to render a template fragment **from the template**.

```html
<ng-template #loading>
  <p>Loading...</p>
</ng-template>

<ng-container
  [ngTemplateOutlet]="loading">
</ng-container>
```

Result:

```text
Loading...
```

Usually use it with `<ng-container>` so you don't introduce an unnecessary DOM element. 

---

**`4. Passing Data to Template`**

You can pass context:

```html
<ng-template #userTemplate let-name="name">
  <p>Hello {{ name }}</p>
</ng-template>

<ng-container
  [ngTemplateOutlet]="userTemplate"
  [ngTemplateOutletContext]="{ name: 'Swapnil' }">
</ng-container>
```

Result:

```text
Hello Swapnil
```

The `let-name="name"` connects the template variable to the context object's `name` property. 

---

**`5. ViewContainerRef`**

You can also render a `TemplateRef` **from TypeScript**:

```typescript
private vcr = inject(ViewContainerRef);

show(template: TemplateRef<unknown>) {
  this.vcr.createEmbeddedView(template);
}
```

So:

```text
TemplateRef
    ↓
ViewContainerRef
    ↓
DOM
```

> Use `ng-template` when you want **reusable or dynamically rendered pieces of HTML**.

**1. Loading / Empty / Error templates**

```text
Data available → show data
No data        → show empty template
Loading        → show loading template
```

**2. Reusable templates**

For example, a table can receive a template defining **how each row should be displayed**.

**3. Dynamic rendering**

```text
Condition
   ↓
Choose template
   ↓
Render selected template
```

**4. Structural directives**

This is an important connection.

Angular internally transforms:

```html
<div *myDirective>
  Hello
</div>
```

conceptually into:

```html
<ng-template myDirective>
  <div>
    Hello
  </div>
</ng-template>
```

Structural directives use `TemplateRef` and `ViewContainerRef` to dynamically render template fragments. 

---

**`ng-template` vs `ng-content`**

Don't confuse these:

|                     | `ng-template`                   | `ng-content`                       |
| ------------------- | ------------------------------- | ---------------------------------- |
| Purpose             | Define template to render later | Project parent's content           |
| Content source      | Template itself                 | Parent                             |
| Render immediately? | ❌ No                            | ✅ Projected when component renders |
| Main use            | Dynamic/reusable templates      | Component composition              |

-------
-------

## Angular `@defer`

> `@defer` is Angular's **deferred/lazy loading** feature. It delays loading code that isn't needed for the initial page, reducing the initial JavaScript bundle and improving initial load performance. 

**1. Basic Usage**

```html
@defer {
  <app-heavy-component />
}
```

Angular puts the deferred component into a **separate lazy chunk** and loads it when the defer trigger occurs. By default, the trigger is **browser idle**. 

Think:

```text
Initial page
    ↓
Load essential code
    ↓
Browser becomes idle
    ↓
Load HeavyComponent
```

---

**2. `@placeholder`**

Show something before the deferred component loads.

```html
@defer {
  <app-heavy-component />
} @placeholder {
  <p>Component will load soon...</p>
}
```

Flow:

```text
Placeholder
    ↓
Loading triggered
    ↓
HeavyComponent
```

Placeholder dependencies are loaded eagerly, so keep them lightweight. 

---

**3. `@loading` and `@error`**

```html
@defer {
  <app-heavy-component />
} @placeholder {
  <p>Preparing...</p>
} @loading {
  <p>Loading...</p>
} @error {
  <p>Failed to load.</p>
}
```

| Block          | Purpose                 |
| -------------- | ----------------------- |
| `@defer`       | Actual deferred content |
| `@placeholder` | Before loading starts   |
| `@loading`     | While loading           |
| `@error`       | If loading fails        |

---

**4. Important Triggers**

You can control **when** the deferred content loads.

`on idle`

Default:

```html
@defer (on idle) {
  <app-chart />
}
```

Loads when the browser is idle.

---

`on viewport`

Excellent for content below the fold:

```html
@defer (on viewport) {
  <app-recommendations />
} @placeholder {
  <p>Recommendations...</p>
}
```

When the placeholder enters the viewport, Angular loads the component. 

**Ideal scenario:**

```text
Product page
   ↓
User scrolls down
   ↓
Recommendations become visible
   ↓
Load recommendations
```

---

`on interaction`

Load when the user interacts:

```html
@defer (on interaction) {
  <app-advanced-search />
} @placeholder {
  <button>Open Advanced Search</button>
}
```

---

`on hover`

```html
@defer (on hover) {
  <app-preview />
} @placeholder {
  <div>Hover to preview</div>
}
```

---

`on timer`

```html
@defer (on timer(5s)) {
  <app-ad />
}
```

Loads after 5 seconds. 

---

`when`

Use your own condition:

```html
@defer (when showChart) {
  <app-chart />
}
```

Once `showChart` becomes truthy, the block loads. This is effectively a **one-time trigger**; it doesn't go back to the placeholder if the condition later becomes false. 

---

**5. Prefetch**

You can **download the code early** but wait to actually display it.

```html
@defer (on interaction; prefetch on idle) {
  <app-details />
} @placeholder {
  <button>View Details</button>
}
```

Meaning:

```text
Browser idle
    ↓
Download Details code

User clicks
    ↓
Show Details immediately
```

This is useful when you can predict that the user is **likely to need something soon**. 

---

**Ideal Scenarios**

Use `@defer` for **large or non-critical UI**.

```text
Dashboard
├── KPI cards          ← Load immediately
├── Chart              ← @defer
├── Large data table   ← @defer
├── Map                ← @defer (on viewport)
└── Reports            ← @defer (on interaction)
```

Especially useful for:

* Charts
* Maps
* Large tables
* Rich editors
* Analytics sections
* Recommendation sections
* Heavy third-party components

---

### `@defer` vs Lazy Route Loading

Don't confuse these:

|         | `@defer`                    | Lazy Route             |
| ------- | --------------------------- | ---------------------- |
| Purpose | Lazy-load part of a page    | Lazy-load a route/page |
| Example | Chart, map, table           | `/reports`             |
| Trigger | Idle, viewport, click, etc. | Navigation             |
| Scope   | Component/template section  | Route                  |

Think:

```text
Lazy Route
    ↓
Lazy-load entire page

@defer
    ↓
Lazy-load part of a page
```
---------
---------

## Angular Directives

A **directive adds behavior to an existing element or component**. It can change appearance, behavior, or how elements fit into the DOM. 

**`1. Types of Directives`**

Angular has 3 primary types:

| Type                     | Purpose                           |
| ------------------------ | --------------------------------- |
| **Component**            | Reusable UI with its own template |
| **Attribute directive**  | Change appearance/behavior        |
| **Structural directive** | Add/remove/change DOM layout      |


The important distinction:

```text
Component
    → "I provide UI"

Attribute Directive
    → "I modify existing UI"

Structural Directive
    → "I modify what exists in the DOM"
```

---

**`2. Attribute Directive`**

An attribute directive changes the **behavior or appearance** of an existing element.

Example: highlight on hover.

```typescript
@Directive({
  selector: '[appHighlight]',
  host: {
    '(mouseenter)': 'isHovered.set(true)',
    '(mouseleave)': 'isHovered.set(false)',
    '[style.background-color]':
      'isHovered() ? "yellow" : null'
  }
})
export class HighlightDirective {
  protected isHovered = signal(false);
}
```

Use:

```html
<p appHighlight>
  Hover over me
</p>
```

Now any element with:

```html
appHighlight
```

gets the same behavior. 

**Ideal scenario**

When you want to **reuse behavior** across existing elements:

```text
Tooltip
Autofocus
Highlight
Permission behavior
Custom styling
Keyboard behavior
```

---

**`3. Structural Directive`**

Structural directives control **whether/how elements are added or removed from the DOM**.

Historically:

```html
<div *ngIf="isLoggedIn">
  Welcome
</div>
```

or:

```html
<div *ngFor="let user of users">
  {{ user.name }}
</div>
```

Modern Angular uses built-in control flow:

```html
@if (isLoggedIn) {
  <div>Welcome</div>
}

@for (user of users; track user.id) {
  <div>{{ user.name }}</div>
}
```

So for **new Angular code**, learn `@if` / `@for` first. Structural directives remain important because you'll encounter them in existing applications.

---

**`4. Directive vs Component`**

This is a common interview question.

**Component**

```typescript
@Component({
  selector: 'app-user',
  template: `
    <h2>User Profile</h2>
  `
})
```

It **owns/provides UI**.

**Directive**

```typescript
@Directive({
  selector: '[appHighlight]'
})
```

It **adds behavior to existing UI**.

```text
Component
   ↓
Own template/UI

Directive
   ↓
Enhances existing element/component
```

Angular specifically recommends a component when you need to render your own markup or manage a piece of UI with its own template. 

-------------
---------------

## Angular Routing

> Routing decides which component should be displayed for a given URL, without doing a full page reload.

Think:

```text
URL
 ↓
Router
 ↓
Matching Route
 ↓
Component
 ↓
<router-outlet>
```

---

**1. Define Routes**

```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'users',
    component: UsersComponent
  },
  {
    path: 'products',
    component: ProductsComponent
  }
];
```

Now:

```text
/users     → UsersComponent
/products  → ProductsComponent
```

---

**2. `router-outlet`**

This is the **placeholder where Angular renders the active route component**.

```html
<nav>
  <a routerLink="/users">Users</a>
  <a routerLink="/products">Products</a>
</nav>

<router-outlet />
```

If URL is:

```text
/products
```

Angular renders:

```text
<router-outlet>
      ↓
ProductsComponent
```

Routes, outlets, and links are the three core building blocks of Angular routing. 

---

**3. `routerLink`**

Used for navigation from the template.

```html
<a routerLink="/users">Users</a>
```

Instead of:

```html
<a href="/users">Users</a>
```

`routerLink` lets Angular Router handle the navigation without a full page reload.

You can also bind it:

```html
<a [routerLink]="['/users', userId]">
  View User
</a>
```

---

**`4. Route Parameters`**

Useful when the URL contains an identifier.

```typescript
{
  path: 'users/:id',
  component: UserComponent
}
```

URL:

```text
/users/101
```

Read it using `ActivatedRoute`:

```typescript
private route = inject(ActivatedRoute);

userId = this.route.snapshot.paramMap.get('id');
```

Mental model:

```text
/users/:id
     ↓
/users/101
     ↓
id = 101
```

---

**`5. Query Parameters`**

Useful for filters, sorting, pagination, etc.

URL:

```text
/products?category=laptop&page=2
```

Navigate:

```typescript
this.router.navigate(
  ['/products'],
  {
    queryParams: {
      category: 'laptop',
      page: 2
    }
  }
);
```

Read:

```typescript
this.route.queryParams.subscribe(params => {
  console.log(params['category']);
});
```

---

**`6. Programmatic Navigation`**

Instead of clicking a link:

```typescript
private router = inject(Router);

openUser() {
  this.router.navigate(['/users', 101]);
}
```

Useful after:

```text
Login successful
      ↓
Navigate to dashboard
```

or:

```text
Form submitted
      ↓
Navigate to confirmation page
```

---

**`7. Nested Routes`

You can have routes inside routes.

```text
/dashboard
    ├── /overview
    ├── /orders
    └── /settings
```

Example:

```typescript
{
  path: 'dashboard',
  component: DashboardComponent,
  children: [
    {
      path: 'orders',
      component: OrdersComponent
    },
    {
      path: 'settings',
      component: SettingsComponent
    }
  ]
}
```

The parent component needs its own:

```html
<router-outlet />
```

for the child route.

---

**`8. Wildcard Route`**

Handle unknown URLs:

```typescript
{
  path: '**',
  component: NotFoundComponent
}
```

Example:

```text
/random-url
     ↓
NotFoundComponent
```

Usually keep it **last** because route matching order matters.

---

**`9. Lazy Loading`**

Instead of loading every component when the application starts:

```typescript
{
  path: 'admin',
  loadComponent: () =>
    import('./admin/admin.component')
      .then(m => m.AdminComponent)
}
```

Angular loads the component when the route is needed.

For larger features:

```typescript
{
  path: 'admin',
  loadChildren: () =>
    import('./admin/admin.routes')
      .then(m => m.ADMIN_ROUTES)
}
```

This helps reduce the initial application bundle.

----------------
----------------

## Angular Route Loading Strategies

Angular has **2 main route loading strategies**:

```text
Route Loading
├── Eager
└── Lazy
```

The choice affects your **initial bundle size and application startup performance**. 

---

**`1. Eager Loading`**

If you directly reference a component in the route:

```typescript
import { HomeComponent } from './home.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent
  }
];
```

Angular includes that component in the initial JavaScript bundle. 

```text
Application starts
      ↓
Download JS
      ↓
HomeComponent already available
```

**Use when**

Usually for your **main landing page** or small, frequently used pages.

---

**`2. Lazy Loading`**

Load the component **only when the user navigates to that route**.

`loadComponent`

```typescript
export const routes: Routes = [
  {
    path: 'reports',
    loadComponent: () =>
      import('./reports/reports.component')
  }
];
```

Angular creates a separate JS chunk and downloads it when `/reports` becomes active. 

```text
Initial application
      ↓
Reports code NOT downloaded
      ↓
User opens /reports
      ↓
Download reports chunk
      ↓
Render ReportsComponent
```

---

3. `loadChildren`

Used to lazy-load a **route tree/child routes**.

```typescript
export const routes: Routes = [
  {
    path: 'admin',
    loadChildren: () =>
      import('./admin/admin.routes')
  }
];
```

Good for large features:

```text
/admin
   ├── users
   ├── products
   ├── reports
   └── settings
```

The entire admin route configuration can be loaded only when needed. 

---

**`loadComponent` vs `loadChildren`**

|          | `loadComponent` | `loadChildren`       |
| -------- | --------------- | -------------------- |
| Loads    | One component   | Route tree           |
| Good for | Individual page | Large feature/module |
| Example  | `/login`        | `/admin/*`           |

Mental model:

```text
One lazy page
     ↓
loadComponent()

Entire feature with routes
     ↓
loadChildren()
```
-------------
-------------

