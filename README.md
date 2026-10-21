# EventEase 🎟️

EventEase is a Blazor WebAssembly (.NET 8) app for managing corporate and social events. You can browse and edit events, take registrations with validation, keep a user session, and check guests in with an attendance tracker.
<img width="1336" height="814" alt="image" src="https://github.com/user-attachments/assets/3cff47c0-bf9d-4462-8e1a-0fce79f37a2b" />

## Features

| Area | What it does |
| --- | --- |
| **Event Card** | Reusable component with two-way binding (`@bind-Event`). Edits happen inline on a draft copy, the title previews live as you type, and the form is validated. |
| **Routing** | `/`, `/events`, `/events/{id:int}`, `/events/{id:int}/register`, `/attendance/{id:int?}`, `/account`. Uses route constraints and a styled not-found page. |
| **Registration Form** | `EditForm` with DataAnnotations plus business rules (capacity, duplicate email, closed events). Gives a confirmation code. |
| **User Sessions** | `SessionState` service saved to `localStorage`. The session expires after 8 hours without activity, and only app-local return URLs are followed. |
| **Attendance Tracker** | Live stats ring, quick check-in by confirmation code, search and status filters, and a virtualized guest list. |

## Getting started

```bash
dotnet run --urls http://localhost:5180
```

Then open http://localhost:5180. Requires the .NET 8 SDK.

## Project structure

```
Components/   EventCard, LiveInputText, NotFoundView
Layout/       MainLayout (nav, session chip, ErrorBoundary)
Models/       Event, Registration, RegistrationModel, UserSession, SignInModel
Pages/        Home, EventList, EventDetails, Register, Attendance, Account
Services/     EventService, RegistrationService, SessionState, BrowserStorage, Text
wwwroot/      index.html, css/app.css (dark glassmorphism design system)
```

## Development summary: how the AI assistant helped

### 1. Foundational Event Card and two-way data binding
- Scaffolded an empty Blazor WASM project and designed the `Event` model with validation attributes, including a custom `[FutureDate]`.
- Built `EventCard.razor` with an `Event` parameter and an `EventChanged` callback, so parents can use `@bind-Event`. On the details page, `@bind-Event:after` saves changes after the card updates the bound value.
- Edits go to a cloned **draft**, so the original event only changes after a valid Save. Added `LiveInputText`, which updates on every keystroke, so the card's title and cover update while you type.

### 2. Routing: implementation and debugging
- Added typed route constraints (`{Id:int}`), so `/events/abc` goes to the not-found page instead of crashing.
- **Bug fixed:** data loaded in `OnInitialized` went stale when moving between `/events/1` and `/events/2`, because Blazor reuses the same component. Loading now happens in `OnParametersSet`.
- **Bug fixed:** the page class `Events` clashed with the injected `Events` service (CS0542). Renamed the page to `EventList.razor` and kept the route the same.
- The `ErrorBoundary` resets whenever the route changes, so an error on one page doesn't stay on screen after you navigate away. The mobile menu also closes on navigation.
- Missing IDs show a "not found" view for that entity. The attendance event picker changes the URL, so views can be deep-linked and back/forward works.

### 3. Performance and input validation
- Event search waits 250 ms after typing stops (`CancellationTokenSource`) before filtering. Filter results are cached with `@bind:after` instead of being recalculated on every render.
- Added `@key` to lists, and `<Virtualize>` to the guest table so only visible rows render.
- Saved state is loaded once in `Program.cs` before the first render. `BrowserStorage` handles corrupted JSON and storage being unavailable without crashing.
- Input is checked in several places: regex rules for names, emails and phone numbers; trimmed and length-limited search terms; enum values parsed with `TryParse`; event validation in `EventService.UpdateAsync`; double-submit guards; and checks against open-redirect `returnUrl` values.

### 4. Advanced features
- **Registration Form:** DataAnnotations plus a `ValidationMessageStore` that shows service-side errors (sold out, duplicate email, seats left) on the right field. Fields are pre-filled from the session, and registering signs you in automatically.
- **State management:** singleton services (`EventService`, `RegistrationService`, `SessionState`) are the single source of truth. Components subscribe to `OnChange` and unsubscribe in `Dispose`, so updates appear everywhere.
- **Attendance Tracker:** attendance-rate ring, booked/checked-in/pending/seats-left stats, code check-in with format validation, undo and remove actions, and a demo-data seeder.

### 5. Verification
- The project builds with 0 warnings and 0 errors.
- Tested in a browser: home page, search and filters, inline editing with validation, not-found routes, registration validation and success, duplicate-registration block, code check-in and duplicate check-in, demo seeding and filters, session persistence after a reload, and sign-out.
